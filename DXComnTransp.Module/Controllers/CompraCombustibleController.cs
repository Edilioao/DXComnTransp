using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DXComnTransp.Module.BusinessObjects;
using System;

namespace DXComnTransp.Module.Controllers;

public class CompraCombustibleController
    : ObjectViewController<DetailView, CompraCombustible>
{
    private const string EstadoReadOnlyKey =
        "CompraCombustibleEstadoReadOnly";

    private const string ConsecutivoReadOnlyKey =
        "CompraCombustibleConsecutivoReadOnly";

    private const string EliminarBloqueadoKey =
        "CompraCombustibleEliminarBloqueado";

    private SimpleAction terminarAction;
    private SimpleAction cancelarAction;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public CompraCombustibleController()
    {
        // ========================================================
        // TERMINAR COMPRA
        // ========================================================

        terminarAction = new SimpleAction(
            this,
            "TerminarCompraCombustible",
            PredefinedCategory.Edit)
        {
            Caption = "Terminar Compra",
            ImageName = "State_Task_Completed",

            ConfirmationMessage =
                "¿Está seguro de que desea terminar esta compra?\n\n" +
                "Una vez terminada, la compra no podrá modificarse."
        };
        // Manejador: se suscribe en OnActivated (no aquí), porque XAF
        // reutiliza la instancia del controller entre vistas.


        // ========================================================
        // CANCELAR COMPRA
        // ========================================================

        cancelarAction = new SimpleAction(
            this,
            "CancelarCompraCombustible",
            PredefinedCategory.Edit)
        {
            Caption = "Cancelar Compra",
            ImageName = "Action_Delete",

            ConfirmationMessage =
                "¿Está seguro de que desea cancelar esta compra?\n\n" +
                "Una compra cancelada no podrá modificarse."
        };
        // Manejadores: se suscriben en OnActivated (no aquí).
    }


    // ============================================================
    // ACTIVAR
    // ============================================================

    protected override void OnActivated()
    {
        base.OnActivated();

        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

        // -= antes de += : la instancia del controller se REUTILIZA al
        // cambiar de vista; suscribir solo en el constructor deja la
        // acción sorda tras la primera desactivación (lección
        // aprendida con CompraCombustibleImprimirController).
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
        if (e.Object is CompraCombustible)
        {
            ActualizarEstado();
            ActualizarEliminacion();
        }
    }


    // ============================================================
    // ACTUALIZAR ESTADO
    // ============================================================

    private void ActualizarEstado()
    {
        if (View?.CurrentObject is not CompraCombustible compra)
            return;

        bool bloqueada =
            compra.Terminada ||
            compra.Cancelada;


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
        // SIEMPRE ES SOLO LECTURA.
        // LO GENERA SQL MEDIANTE EL TRIGGER.
        // ========================================================

        if (View.FindItem(
                nameof(CompraCombustible.Consecutivo))
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
            !compra.Terminada &&
            !compra.Cancelada
        );


        // ========================================================
        // CANCELAR
        // ========================================================

        cancelarAction.Active.SetItemValue(
            EstadoReadOnlyKey,
            compra.Terminada &&
            !compra.Cancelada
        );
    }


    // ============================================================
    // CONTROLAR ELIMINACIÓN EN DETAILVIEW
    // ============================================================

    private void ActualizarEliminacion()
    {
        if (View?.CurrentObject is not CompraCombustible compra)
            return;

        DeleteObjectsViewController deleteController =
            Frame.GetController<DeleteObjectsViewController>();

        if (deleteController == null)
            return;

        bool puedeEliminar =
            !compra.Terminada &&
            !compra.Cancelada;

        deleteController.DeleteAction.Active.SetItemValue(
            EliminarBloqueadoKey,
            puedeEliminar
        );
    }


    // ============================================================
    // TERMINAR COMPRA
    // ============================================================


    private void TerminarAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not CompraCombustible compra)
            return;

        try
        {
            // ========================================================
            // VALIDAR ESTADO
            // ========================================================

            if (compra.Terminada)
            {
                throw new UserFriendlyException(
                    "La compra ya está terminada.");
            }

            if (compra.Cancelada)
            {
                throw new UserFriendlyException(
                    "La compra está cancelada y no puede ser terminada.");
            }

            // ========================================================
            // VALIDAR DATOS
            // ========================================================

            ValidarResumen(compra);
            ValidarDetalles(compra);

            // ========================================================
            // MARCAR COMO TERMINADA
            // ========================================================

            compra.Terminada = true;
            compra.FechaTerminada = DateTime.Now;

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
                "La compra fue terminada correctamente. " +
                "Ya no se puede modificar.",
                InformationType.Success);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(
                "Error al terminar la compra: " + ex.Message);
        }
    }


    // ============================================================
    // VALIDAR RESUMEN
    // ============================================================

    private void ValidarResumen(CompraCombustible compra)
    {
        if (compra.Fecha == default)
        {
            throw new UserFriendlyException(
                "Debe indicar la fecha de la compra."
            );
        }


        if (compra.FechaFactura == default)
        {
            throw new UserFriendlyException(
                "Debe indicar la fecha de la factura."
            );
        }


        if (string.IsNullOrWhiteSpace(compra.NoFactura))
        {
            throw new UserFriendlyException(
                "Debe indicar el número de factura."
            );
        }


        if (compra.IdProveedor <= 0 ||
            compra.Proveedor == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar el proveedor."
            );
        }


        if (compra.IdMoneda <= 0 ||
            compra.Moneda == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar la moneda."
            );
        }
    }


    // ============================================================
    // VALIDAR DETALLES
    // ============================================================

    private void ValidarDetalles(CompraCombustible compra)
    {
        if (compra.Detalles == null ||
            compra.Detalles.Count == 0)
        {
            throw new UserFriendlyException(
                "No se puede terminar la compra porque no tiene detalles."
            );
        }


        int numeroDetalle = 0;


        foreach (CompraCombustibleDetalle detalle in compra.Detalles)
        {
            numeroDetalle++;


            if (detalle.IdProducto <= 0 ||
                detalle.Producto == null)
            {
                throw new UserFriendlyException(
                    $"El detalle {numeroDetalle} no tiene combustible."
                );
            }


            if (detalle.IdMedida <= 0 ||
                detalle.Medida == null)
            {
                throw new UserFriendlyException(
                    $"El detalle {numeroDetalle} no tiene unidad de medida."
                );
            }


            if (detalle.Cantidad <= 0)
            {
                throw new UserFriendlyException(
                    $"La cantidad del detalle {numeroDetalle} " +
                    "debe ser mayor que cero."
                );
            }


            if (detalle.Importe <= 0)
            {
                throw new UserFriendlyException(
                    $"El importe del detalle {numeroDetalle} " +
                    "debe ser mayor que cero."
                );
            }
        }
    }


    // ============================================================
    // CANCELAR COMPRA
    // ============================================================


    private void CancelarAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not CompraCombustible compra)
        {
            throw new UserFriendlyException(
                "No se encontró la compra que desea cancelar.");
        }

        // La compra debe estar terminada antes de cancelarla.
        if (!compra.Terminada)
        {
            throw new UserFriendlyException(
                "La compra debe estar terminada antes de poder cancelarla.");
        }

        if (compra.Cancelada)
        {
            throw new UserFriendlyException(
                "La compra ya está cancelada.");
        }

        try
        {
            // Registrar la cancelación.
            compra.Cancelada = true;
            compra.FechaCancelada = DateTime.Now;

            // Guardar los cambios en la base de datos.
            ObjectSpace.CommitChanges();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(
                "No se pudo cancelar la compra. " +
                "Verifique los datos y la conexión con la base de datos. " +
                "Detalle: " + ex.Message);
        }

        // Actualizar el estado de las acciones y el bloqueo de eliminación.
        ActualizarEstado();
        ActualizarEliminacion();

        // Refrescar los editores de la vista.
        View.Refresh();

        // Informar que la operación terminó correctamente.
        Application.ShowViewStrategy.ShowMessage(
            "La compra se canceló correctamente.",
            InformationType.Success);
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