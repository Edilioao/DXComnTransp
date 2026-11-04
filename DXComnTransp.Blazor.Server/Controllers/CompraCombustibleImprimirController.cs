using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.Persistent.Base;
using DXComnTransp.Blazor.Server.Services;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Botón "Imprimir Compra" (reporte DevExpress) para Blazor Server.
///
/// Flujo:
///   1. Construye el XtraReport y lo exporta a PDF (síncrono).
///   2. Guarda el PDF en ReporteCompraPdfCache (vigencia 15 min) y
///      obtiene un token.
///   3. Pide al servicio VisorReporteService de ESTE circuito que
///      avise al componente Components/VisorReporte.razor, que muestra
///      el PDF en un DxPopup.
///
/// POR QUÉ ASÍ (esto es lo que la evidencia del log validó):
///   - Con el visor renderizado por Blazor (sin tocar ventanas del
///     navegador), la acción ejecutó repetidas veces seguidas en la
///     lista: 4 ejecuciones en la misma sesión sin bloquearse.
///   - En cambio, abrir una pestaña nueva desde JavaScript dejó el
///     circuito en un estado donde el POPUP de acciones de DevoExpress
///     dejó de responder: la primera ejecución salía perfecta y todas
///     las siguientes (lista y detalle) ni siquiera llegaban al
///     servidor ("Acción Imprimir ejecutada" nunca más en el log),
///     aunque los clics en las filas sí funcionaban.
///   - El DxPopup es el mismo componente que usan los diálogos de XAF,
///     así que se pinta encima de la vista activa, incluida la de
///     detalle, y no hay overlays propios que tapan la pantalla.
///
/// La impresión (pestaña nueva) se hace con un ENLACE real dentro del
/// visor, nunca con window.open ni interop: los navegadores no bloquean
/// enlaces y así no toca nada del estado del circuito.
/// </summary>
public class CompraCombustibleImprimirController : ViewController
{
    private const string ImprimirActionId = "ImprimirCompraCombustible";

    private readonly SimpleAction imprimirAction;

    public CompraCombustibleImprimirController()
    {
        TargetObjectType = typeof(CompraCombustible);

        // Any: el detalle puede abrirse como vista raíz o como ventana
        // emergente (anidada). Con Nesting.Root el controlador NO se
        // activaba en el detalle emergente, y por eso el botón no
        // existía ahí.
        TargetViewNesting = Nesting.Any;

        imprimirAction = new SimpleAction(
            this,
            ImprimirActionId,
            PredefinedCategory.RecordEdit)
        {
            Caption = "Imprimir Compra",
            ImageName = "Action_Printing_Print",
            ToolTip = "Muestra el PDF (DevExpress) de esta compra",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        // OJO: NO suscribir Execute aquí. XAF reutiliza la misma
        // instancia del controlador al cambiar de vista (list <-> detalle)
        // y nos llama OnActivated/OnDeactivated en cada cambio. Si
        // suscribimos solo en el constructor y desuscribimos en
        // OnDeactivated, la acción queda sin manejador para siempre
        // (el clic llega al servidor y no hace nada). El patrón de
        // DevExpress es suscribir en OnActivated y desuscribir en
        // OnDeactivated.
    }

    private void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        DiagnosticoReportes.Registrar(
            $"Acción Imprimir ejecutada vista={View?.GetType().Name} id={View?.Id}");

        CompraCombustible compra = ObtenerCompraSeleccionada();

        if (compra == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar una compra para imprimir.");
        }

        // Una compra nueva (aún sin guardar) no existe en la BD:
        // no hay nada que imprimir todavía.
        if (compra.Id == 0)
        {
            throw new UserFriendlyException(
                "Guarde la compra antes de imprimirla.");
        }

        // ---------- Generar el PDF (síncrono: si falla, XAF lo muestra) ----------
        string url;

        try
        {
            url = GenerarPdf(compra);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Exception error = ex;
            while (error.InnerException != null)
                error = error.InnerException;

            throw new UserFriendlyException(
                "Error al generar el reporte: " + error.Message);
        }

        // ---------- Mostrarlo en el visor de ESTE circuito ----------
        AbrirReporte(url);
    }

    /// <summary>
    /// Muestra el PDF en el visor del circuito que hizo clic.
    ///
    /// VisorReporteService es Scoped (una instancia por circuito): avisa
    /// al componente VisorReporte.razor (montado en _Host.cshtml con
    /// render-mode="Server"), que abre el DxPopup con el PDF.
    ///
    /// No se usa JavaScript interop aquí a propósito.
    /// </summary>
    private void AbrirReporte(string url)
    {
        IServiceProvider proveedor =
            (Application as BlazorApplication)?.ServiceProvider;

        if (proveedor?.GetService(typeof(VisorReporteService)) is VisorReporteService visor)
        {
            visor.Abrir(url);

            DiagnosticoReportes.Registrar(
                $"VisorReporteService.Abrir OK inst={visor.GetHashCode()} url=" + url);

            return;
        }

        DiagnosticoReportes.Registrar(
            "VisorReporteService no disponible. url=" + url);
    }

    /// <summary>
    /// Carga la compra en un ObjectSpace nuevo, construye el reporte,
    /// exporta a PDF y lo guarda en la caché. Devuelve la URL del PDF.
    /// 100% síncrono: si algo falla, la excepción la muestra XAF.
    /// </summary>
    private string GenerarPdf(CompraCombustible compra)
    {
        IObjectSpace objectSpace =
            Application.CreateObjectSpace(typeof(CompraCombustible));

        try
        {
            CompraCombustible compraCompleta = objectSpace
                .GetObjectsQuery<CompraCombustible>()
                .FirstOrDefault(c => c.Id == compra.Id);

            if (compraCompleta == null)
            {
                throw new UserFriendlyException(
                    "No se pudo cargar la compra para el reporte.");
            }

            Configuracion configuracion = objectSpace
                .GetObjectsQuery<Configuracion>()
                .OrderBy(c => c.Id)
                .FirstOrDefault();

            CompraCombustibleReport report = CompraCombustibleReport.Crear(
                compraCompleta,
                configuracion);

            using var memoria = new MemoryStream();
            report.ExportToPdf(memoria);
            byte[] pdf = memoria.ToArray();

            if (pdf == null || pdf.Length == 0)
            {
                throw new UserFriendlyException(
                    "El reporte se generó vacío. Verifique los datos de la compra.");
            }

            string nombreArchivo =
                $"compra-{compraCompleta.Consecutivo:D6}.pdf";

            string token = ReporteCompraPdfCache.Guardar(pdf, nombreArchivo);

            DiagnosticoReportes.Registrar(
                $"PDF generado token={token} bytes={pdf.Length} " +
                $"compra={compraCompleta.Id}");

            return $"/reportes/compra-combustible-pdf/{token}";
        }
        finally
        {
            objectSpace.Dispose();
        }
    }

    private CompraCombustible ObtenerCompraSeleccionada()
    {
        if (View is DetailView vistaDetalle &&
            vistaDetalle.CurrentObject is CompraCombustible compraDetalle)
        {
            return compraDetalle;
        }

        if (View is ListView vistaLista &&
            vistaLista.SelectedObjects != null &&
            vistaLista.SelectedObjects.Count == 1 &&
            vistaLista.SelectedObjects[0] is CompraCombustible compraLista)
        {
            return compraLista;
        }

        return null;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // -= antes de += : a prueba de activaciones repetidas sin
        // desactivación previa (nunca quedan dos manejadores colgados).
        imprimirAction.Execute -= ImprimirAction_Execute;
        imprimirAction.Execute += ImprimirAction_Execute;

        DiagnosticoReportes.Registrar(
            $"Controller Imprimir activado vista={View?.GetType().Name} id={View?.Id}");
    }

    protected override void OnDeactivated()
    {
        imprimirAction.Execute -= ImprimirAction_Execute;
        base.OnDeactivated();
    }
}
