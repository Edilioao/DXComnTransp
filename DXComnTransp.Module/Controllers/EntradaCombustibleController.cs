using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DXComnTransp.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DXComnTransp.Module.Controllers;

// ============================================================
// "Terminar Entrada" / "Cancelar Entrada" para EntradaCombustible
// (funciona en Blazor y Win: corre en el módulo compartido).
//
// Mismas reglas que CompraCombustibleController:
//   - Terminar: valida resumen y detalles, marca Terminada y
//     ACTUALIZA EL INVENTARIO (promedio ponderado) en la MISMA
//     transición de guardado (un solo CommitChanges: atómico).
//   - Cancelar: solo si está Terminada y no Cancelada; marca
//     Cancelada y REVERTE el inventario.
//   - Una entrada Terminada o Cancelada queda rojo/verde en la
//     lista, sin edición, sin eliminación.
//
// La BD legada además tiene triggers (encriptados) en estas tablas:
//   TR_RecepcionResumen_DisponibilidadCombustible y
//   TR_RecepcionDetalle_Proteccion; este código debe COMBINAR con
// ellos, no duplicarlos.
//
// IMPORTANTE (lección de CompraCombustibleImprimirController):
// suscribir los Execute en OnActivated (con -=; +=) y desuscribir
// SOLO en OnDeactivated. XAF reutiliza la instancia del controller
// al cambiar de vista; si se suscribe solo en el constructor y se
// desengancha en OnDeactivated, las acciones quedan sordas.
// ============================================================
public class EntradaCombustibleController
    : ObjectViewController<DetailView, EntradaCombustible>
{
    private const string EstadoReadOnlyKey =
        "EntradaCombustibleEstadoReadOnly";

    private const string ConsecutivoReadOnlyKey =
        "EntradaCombustibleConsecutivoReadOnly";

    private const string EliminarBloqueadoKey =
        "EntradaCombustibleEliminarBloqueado";

    private const string TipoInventarioCombustible = "COMBUSTIBLE";

    private SimpleAction terminarAction;
    private SimpleAction cancelarAction;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public EntradaCombustibleController()
    {
        // ========================================================
        // TERMINAR ENTRADA
        // (los manejadores se agregan en OnActivated, ver arriba)
        // ========================================================

        terminarAction = new SimpleAction(
            this,
            "TerminarEntradaCombustible",
            PredefinedCategory.Edit)
        {
            Caption = "Terminar Entrada",
            ImageName = "State_Task_Completed",

            ConfirmationMessage =
                "¿Está seguro de que desea terminar esta entrada?\n\n" +
                "La existencia y el precio del inventario del depósito " +
                "se actualizarán y la entrada no podrá modificarse."
        };


        // ========================================================
        // CANCELAR ENTRADA
        // ========================================================

        cancelarAction = new SimpleAction(
            this,
            "CancelarEntradaCombustible",
            PredefinedCategory.Edit)
        {
            Caption = "Cancelar Entrada",
            ImageName = "Action_Delete",

            ConfirmationMessage =
                "¿Está seguro de que desea cancelar esta entrada?\n\n" +
                "La existencia y el precio del inventario revertirán " +
                "con lo que entró esta entrada."
        };
    }


    // ============================================================
    // ACTIVAR
    // ============================================================

    protected override void OnActivated()
    {
        base.OnActivated();

        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

        //- antes de += : a prueba de activaciones repetidas (nunca
        // quedan dos manejadores colgados).
        terminarAction.Execute -= TerminarAction_Execute;
        terminarAction.Execute += TerminarAction_Execute;

        cancelarAction.Execute -= CancelarAction_Execute;
        cancelarAction.Execute += CancelarAction_Execute;

        ActualizarEstado();
        ActualizarEliminacion();
    }


    // ============================================================
    // CAMBIO DE PROPIEDADES
    // ============================================================

    private void ObjectSpace_ObjectChanged(
        object sender,
        ObjectChangedEventArgs e)
    {
        if (e.Object is EntradaCombustible entradaCambiada)
        {
            // Cambio de moneda del resumen: sugerir su tasa de cambio
            // (el usuario luego puede ajustar a mano).
            if (e.PropertyName == nameof(EntradaCombustible.Moneda) &&
                entradaCambiada.Moneda != null)
            {
                entradaCambiada.TasaCambio =
                    entradaCambiada.Moneda.TasaCambio;
            }

            ActualizarEstado();
            ActualizarEliminacion();
        }
        else if (e.Object is EntradaCombustibleDetalle detalleCambiada)
        {
            // Elegir el combustible de la línea: sugerir el precio de
            // costo (regla del usuario): primero el precio del
            // INVENTARIO de ese depósito; si no hay fila de inventario,
            // el PCosto del catálogo TipoCombustible.
            if (e.PropertyName ==
                nameof(EntradaCombustibleDetalle.Producto))
            {
                SugerirPrecioDetalle(detalleCambiada);
            }
        }
    }


    // ============================================================
    // AUTOCOMPLETADO DE PRECIO AL ELEGIR EL COMBUSTIBLE
    //
    // Regla acordada con el usuario:
    //   1) precio del inventario (depósito de la entrada +
    //      tipo COMBUSTIBLE + producto de la línea), si existe;
    //   2) si no hay inventario: PCosto del catálogo.
    //
    // Nunca pisa un precio que el usuario ya haya escrito > 0.
    // ============================================================

    private void SugerirPrecioDetalle(
        EntradaCombustibleDetalle detalle)
    {
        if (detalle == null ||
            detalle.Precio > 0)
        {
            // Ya tiene precio: no se toca.
            return;
        }

        EntradaCombustible entrada =
            detalle.Entrada ??
            (View?.CurrentObject as EntradaCombustible);

        decimal precioSugerido = 0;

        if (detalle.IdProducto > 0 &&
            entrada != null &&
            entrada.IdDeposito > 0)
        {
            Inventario inventario = ObjectSpace
                .GetObjectsQuery<Inventario>()
                .FirstOrDefault(x =>
                    x.Deposito.Id == entrada.IdDeposito &&
                    x.Producto.Id == detalle.IdProducto);

            if (inventario != null)
            {
                precioSugerido = inventario.Precio;
            }
        }

        if (precioSugerido <= 0 &&
            detalle.Producto != null)
        {
            precioSugerido = detalle.Producto.PCosto;
        }

        if (precioSugerido > 0)
        {
            detalle.Precio = precioSugerido;
        }

        // Tasa del detalle: hereda la del resumen si la línea está en 0.
        if (detalle.TasaCambio == 0 &&
            entrada != null &&
            entrada.TasaCambio > 0)
        {
            detalle.TasaCambio = entrada.TasaCambio;
        }
    }


    // ============================================================
    // ACTUALIZAR ESTADO
    // ============================================================

    private void ActualizarEstado()
    {
        if (View?.CurrentObject is not EntradaCombustible entrada)
            return;

        bool bloqueada =
            entrada.Terminada ||
            entrada.Cancelada;


        // ========================================================
        // BLOQUEAR TODO EL RESUMEN
        // ========================================================

        View.AllowEdit.SetItemValue(
            EstadoReadOnlyKey,
            !bloqueada
        );


        // ========================================================
        // CONSECUTIVO
        //
        // SIEMPRE ES SOLO LECTURA: lo genera el sistema.
        // ========================================================

        if (View.FindItem(
                nameof(EntradaCombustible.Consecutivo))
            is PropertyEditor consecutivoEditor)
        {
            consecutivoEditor.AllowEdit.SetItemValue(
                ConsecutivoReadOnlyKey,
                false
            );
        }


        // ========================================================
        // TERMINAR
        // ========================================================

        terminarAction.Active.SetItemValue(
            EstadoReadOnlyKey,
            !entrada.Terminada &&
            !entrada.Cancelada
        );


        // ========================================================
        // CANCELAR
        // ========================================================

        cancelarAction.Active.SetItemValue(
            EstadoReadOnlyKey,
            entrada.Terminada &&
            !entrada.Cancelada
        );
    }


    // ============================================================
    // CONTROLAR ELIMINACIÓN EN DETAILVIEW
    // ============================================================

    private void ActualizarEliminacion()
    {
        if (View?.CurrentObject is not EntradaCombustible entrada)
            return;

        DeleteObjectsViewController deleteController =
            Frame.GetController<DeleteObjectsViewController>();

        if (deleteController == null)
            return;

        bool puedeEliminar =
            !entrada.Terminada &&
            !entrada.Cancelada;

        deleteController.DeleteAction.Active.SetItemValue(
            EliminarBloqueadoKey,
            puedeEliminar
        );
    }


    // ============================================================
    // TERMINAR ENTRADA
    // ============================================================

    private void TerminarAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not EntradaCombustible entrada)
            return;

        try
        {
            // ========================================================
            // VALIDAR ESTADO
            // ========================================================

            if (entrada.Terminada)
            {
                throw new UserFriendlyException(
                    "La entrada ya está terminada.");
            }

            if (entrada.Cancelada)
            {
                throw new UserFriendlyException(
                    "La entrada está cancelada y no puede ser terminada.");
            }

            // ========================================================
            // VALIDAR DATOS
            // ========================================================

            ValidarResumen(entrada);
            ValidarDetalles(entrada);

            // ========================================================
            // MARCAR COMO TERMINADA Y ACTUALIZAR EL INVENTARIO
            //
            // Ambas cosas en el MISMO ObjectSpace: un solo
            // CommitChanges guarda la entrada Y el inventario
            // atómicamente (los triggers legados de disponibilidad
            // ven una transición consistente).
            // ========================================================

            entrada.Terminada = true;
            entrada.FechaTerminada = DateTime.Now;

            ActualizarInventarioConEntrada(entrada);

            // Guardar en SQL Server.
            ObjectSpace.CommitChanges();

            // ========================================================
            // ACTUALIZAR LA INTERFAZ
            // ========================================================

            View.Refresh();

            // Recalcular los estados después del refresco.
            ActualizarEstado();
            ActualizarEliminacion();

            // Confirmar al usuario que la operación terminó.
            Application.ShowViewStrategy.ShowMessage(
                "La entrada fue terminada correctamente. " +
                "El inventario ya quedó actualizado y no se " +
                "puede modificar la entrada.",
                InformationType.Success);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(
                "Error al terminar la entrada: " + ex.Message);
        }
    }


    // ============================================================
    // CANCELAR ENTRADA
    // ============================================================

    private void CancelarAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not EntradaCombustible entrada)
        {
            throw new UserFriendlyException(
                "No se encontró la entrada que desea cancelar.");
        }

        // La entrada debe estar terminada antes de cancelarla.
        if (!entrada.Terminada)
        {
            throw new UserFriendlyException(
                "La entrada debe estar terminada antes de poder " +
                "cancelarla.");
        }

        if (entrada.Cancelada)
        {
            throw new UserFriendlyException(
                "La entrada ya está cancelada.");
        }

        try
        {
            // Registrar la cancelación y REVERTIR el inventario en
            // el mismo guardado (atómico).
            entrada.Cancelada = true;
            entrada.FechaCancelada = DateTime.Now;

            RevertirInventarioDeEntrada(entrada);

            // Guardar los cambios en la base de datos.
            ObjectSpace.CommitChanges();
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(
                "No se pudo cancelar la entrada. " +
                "Verifique los datos y la conexión con la base de " +
                "datos. Detalle: " + ex.Message);
        }

        // Actualizar el estado de las acciones y el bloqueo.
        ActualizarEstado();
        ActualizarEliminacion();

        // Refrescar los editores de la vista.
        View.Refresh();

        // Informar que la operación terminó correctamente.
        Application.ShowViewStrategy.ShowMessage(
            "La entrada se canceló correctamente y el inventario " +
            "reversó su movimiento.",
            InformationType.Success);
    }


    // ============================================================
    // VALIDAR RESUMEN
    // ============================================================

    private void ValidarResumen(EntradaCombustible entrada)
    {
        if (entrada.Fecha == default)
        {
            throw new UserFriendlyException(
                "Debe indicar la fecha de la entrada.");
        }


        if (string.IsNullOrWhiteSpace(entrada.NoFactura))
        {
            throw new UserFriendlyException(
                "Debe indicar el número de factura.");
        }


        if (entrada.IdDeposito <= 0 ||
            entrada.Deposito == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar el depósito.");
        }


        if (entrada.IdProveedor <= 0 ||
            entrada.Proveedor == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar el proveedor.");
        }


        if (entrada.IdMoneda <= 0 ||
            entrada.Moneda == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar la moneda.");
        }


        if (entrada.TasaCambio <= 0)
        {
            throw new UserFriendlyException(
                "La tasa de cambio debe ser mayor que cero.");
        }
    }


    // ============================================================
    // VALIDAR DETALLES
    // ============================================================

    private void ValidarDetalles(EntradaCombustible entrada)
    {
        if (entrada.Detalles == null ||
            entrada.Detalles.Count == 0)
        {
            throw new UserFriendlyException(
                "No se puede terminar la entrada porque no tiene " +
                "detalles.");
        }


        int numeroDetalle = 0;


        foreach (EntradaCombustibleDetalle detalle in entrada.Detalles)
        {
            numeroDetalle++;


            if (detalle.IdProducto <= 0 ||
                detalle.Producto == null)
            {
                throw new UserFriendlyException(
                    $"El detalle {numeroDetalle} no tiene combustible.");
            }


            if (detalle.IdMedida <= 0 ||
                detalle.Medida == null)
            {
                throw new UserFriendlyException(
                    $"El detalle {numeroDetalle} no tiene unidad de " +
                    "medida.");
            }


            if (detalle.Precio <= 0)
            {
                throw new UserFriendlyException(
                    $"El precio del detalle {numeroDetalle} " +
                    "debe ser mayor que cero.");
            }


            if (detalle.TasaCambio <= 0)
            {
                throw new UserFriendlyException(
                    $"La tasa de cambio del detalle {numeroDetalle} " +
                    "debe ser mayor que cero.");
            }


            if (detalle.Cantidad <= 0)
            {
                throw new UserFriendlyException(
                    $"La cantidad del detalle {numeroDetalle} " +
                    "debe ser mayor que cero.");
            }
        }
    }


    // ============================================================
    // ACTUALIZAR INVENTARIO CON LAS LÍNEAS DE LA ENTRADA
    //
    // Por cada detalle: obtiene (o crea) la fila de Inventario del
    // (Depósito de la entrada, COMBUSTIBLE, combustible del detalle)
    // y aplica promedio ponderado:
    //
    //   nuevoPrecio = (existenciaActual*precioActual
    //                  + cantidad*precioEntrada)
    //                 / (existenciaActual + cantidad)
    //
    // precioEntrada = Precio * TasaCambio del detalle (moneda local),
    // que es el mismo valor que la columna calculada Importe usa por
    // unidad (Importe / Cantidad).
    //
    // Si el inventario no existía (existencia 0), el promedio
    // naturalmente queda con el precio de la línea entrante: la
    // primera entrada define el precio inicial.
    // ============================================================

    private void ActualizarInventarioConEntrada(
        EntradaCombustible entrada)
    {
        TipoInventario tipoCombustible =
            ObtenerTipoInventarioCombustible();

        // Si la misma entrada trae dos líneas del MISMO combustible,
        // ambas deben impactar la MISMA fila de inventario dentro de
        // este guardado (una consulta a la BD no ve filas creadas y
        // todavía sin guardar; este diccionario lo garantiza).
        Dictionary<int, Inventario> inventariosUsados =
            new Dictionary<int, Inventario>();

        foreach (EntradaCombustibleDetalle detalle in entrada.Detalles)
        {
            Inventario inventario;

            if (!inventariosUsados.TryGetValue(
                    detalle.IdProducto,
                    out inventario))
            {
                inventario = ObtenerOCrearInventario(
                    entrada.IdDeposito,
                    entrada.Deposito,
                    tipoCombustible,
                    detalle.Producto);

                inventariosUsados[detalle.IdProducto] = inventario;
            }

            // Promedio ponderado (con inventario recién creado los
            // valores anteriores son 0: queda el precio de entrada).
            decimal precioEntrada = detalle.Precio * detalle.TasaCambio;
            decimal existenciaAnterior = inventario.Existencia;
            decimal precioAnterior = inventario.Precio;

            decimal nuevaExistencia =
                existenciaAnterior + detalle.Cantidad;

            inventario.Precio =
                nuevaExistencia == 0
                    ? precioEntrada
                    : ((existenciaAnterior * precioAnterior) +
                       (detalle.Cantidad * precioEntrada)) /
                      nuevaExistencia;

            inventario.Existencia = nuevaExistencia;
            inventario.Actualizado = DateTime.Now;
        }
    }


    // ============================================================
    // REVERTIR EL INVENTARIO AL CANCELAR
    //
    // Quita la cantidad que entró y "des-pondera" el precio:
    //
    //   precioRevertido = (existActual*precioActual
    //                      - cantidad*precioEntrada)
    //                     / (existActual - cantidad)
    //
    // Guardas: si la existencia después de revertir queda <= 0 se
    // deja en 0 (y el precio no cambia: no hay con qué ponderar).
    // ============================================================

    private void RevertirInventarioDeEntrada(
        EntradaCombustible entrada)
    {
        foreach (EntradaCombustibleDetalle detalle in entrada.Detalles)
        {
            decimal precioEntrada = detalle.Precio * detalle.TasaCambio;

            Inventario inventario =
                ObjectSpace
                    .GetObjectsQuery<Inventario>()
                    .FirstOrDefault(x =>
                        x.Deposito.Id == entrada.IdDeposito &&
                        x.Producto.Id == detalle.IdProducto);

            if (inventario == null)
            {
                // No debería pasar (la entrada terminada debió crear
                // la fila), pero mejor avisar con detalle.
                throw new UserFriendlyException(
                    "No existe inventario para el depósito y el " +
                    $"combustible del detalle {detalle.Producto?.Combustible}. " +
                    "No se puede revertir la entrada.");
            }

            decimal existencaActual = inventario.Existencia;
            decimal precioActual = inventario.Precio;

            decimal nuevaExistencia =
                existencaActual - detalle.Cantidad;

            if (nuevaExistencia > 0 &&
                existencaActual > 0)
            {
                // "Des-ponderar": restar la contribución de la línea
                // entrante y volver a calcular el promedio de lo que
                // queda.
                inventario.Precio =
                    ((existencaActual * precioActual) -
                     (detalle.Cantidad * precioEntrada)) /
                    nuevaExistencia;
            }

            inventario.Existencia =
                nuevaExistencia > 0 ? nuevaExistencia : 0;
            inventario.Actualizado = DateTime.Now;
        }
    }


    // ============================================================
    // TIPO DE INVENTARIO "COMBUSTIBLE" (Id 1 en la BD legada)
    // ============================================================

    private TipoInventario ObtenerTipoInventarioCombustible()
    {
        TipoInventario tipo = ObjectSpace
            .GetObjectsQuery<TipoInventario>()
            .FirstOrDefault(x => x.TpoInventario == TipoInventarioCombustible);

        if (tipo == null)
        {
            throw new UserFriendlyException(
                $"En el catálogo Tipo de Inventario no existe " +
                $"'{TipoInventarioCombustible}'. Corríjalo en el " +
                "catálogo antes de terminar la entrada.");
        }

        return tipo;
    }


    // ============================================================
    // OBTENER (O CREAR) LA FILA DE INVENTARIO
    // ============================================================

    private Inventario ObtenerOCrearInventario(
        int idDeposito,
        Deposito deposito,
        TipoInventario tipoInventario,
        TipoCombustible producto)
    {
        Inventario inventario = ObjectSpace
            .GetObjectsQuery<Inventario>()
            .FirstOrDefault(x =>
                x.Deposito.Id == idDeposito &&
                x.TipoInventario.Id == tipoInventario.Id &&
                x.Producto.Id == producto.Id);

        if (inventario == null)
        {
            inventario = ObjectSpace.CreateObject<Inventario>();

            // Mismo ObjectSpace: asigno las referencias directas, sin
            // clonar nada.
            inventario.Deposito = deposito;
            inventario.TipoInventario = tipoInventario;
            inventario.Producto = producto;
            inventario.Existencia = 0;
            inventario.Precio = 0;
            inventario.Actualizado = DateTime.Now;
        }

        return inventario;
    }


    // ============================================================
    // DESACTIVAR
    // ============================================================

    protected override void OnDeactivated()
    {
        ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;

        terminarAction.Execute -= TerminarAction_Execute;
        cancelarAction.Execute -= CancelarAction_Execute;

        base.OnDeactivated();
    }
}
