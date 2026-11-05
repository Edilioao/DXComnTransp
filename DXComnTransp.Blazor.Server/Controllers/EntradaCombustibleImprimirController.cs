using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.Persistent.Base;
using DXComnTransp.Blazor.Server.Services;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using System;
using System.IO;
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Botón "Imprimir Entrada" para Blazor Server: reporte DevExpress de
/// una entrada de combustible. Es el ESPEJO exacto del flujo de
/// CompraCombustibleImprimirController, que quedó verificado:
///
///   1. Genera el XtraReport a DTO planos y lo exporta a PDF.
///   2. Guarda el PDF en ReporteCompraPdfCache (15 min, máx 10).
///   3. Abre el Visor (DxPopup) usando VisorReporteService de ESTE
///      circuito. Cero JavaScript interop (la impresión es un enlace
///      real dentro del visor).
///
/// ReporteEntradaCombustible.cs genera el formato (idéntico al de la
/// compra): funciona desde el ListView y desde el DetailView.
/// </summary>
public class EntradaCombustibleImprimirController : ViewController
{
    private const string ImprimirActionId = "ImprimirEntradaCombustible";

    private readonly SimpleAction imprimirAction;

    public EntradaCombustibleImprimirController()
    {
        TargetObjectType = typeof(EntradaCombustible);

        // Any: lista (raíz) y detalle (raíz o anidada).
        TargetViewNesting = Nesting.Any;

        imprimirAction = new SimpleAction(
            this,
            ImprimirActionId,
            PredefinedCategory.RecordEdit)
        {
            Caption = "Imprimir Entrada",
            ImageName = "Action_Printing_Print",
            ToolTip = "Muestra el PDF (DevExpress) de esta entrada",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        // Manejador: se suscribe en OnActivated (no aquí). XAF reutiliza
        // la instancia del controller entre vistas.
    }

    private void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        DiagnosticoReportes.Registrar(
            $"Acción Imprimir Entrada ejecutada vista={View?.GetType().Name} id={View?.Id}");

        EntradaCombustible entrada = ObtenerEntradaSeleccionada();

        if (entrada == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar una entrada para imprimir.");
        }

        if (entrada.Id == 0)
        {
            throw new UserFriendlyException(
                "Guarde la entrada antes de imprimirla.");
        }

        string url;

        try
        {
            url = GenerarPdf(entrada);
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

        AbrirReporte(url);
    }

    private void AbrirReporte(string url)
    {
        IServiceProvider proveedor =
            (Application as BlazorApplication)?.ServiceProvider;

        if (proveedor?.GetService(typeof(VisorReporteService)) is VisorReporteService visor)
        {
            visor.Abrir(url);

            DiagnosticoReportes.Registrar(
                $"VisorReporteService.Abrir OK (entrada) inst={visor.GetHashCode()} url=" + url);

            return;
        }

        DiagnosticoReportes.Registrar(
            "VisorReporteService no disponible (entrada). url=" + url);
    }

    /// <summary>
    /// Carga la entrada en un ObjectSpace nuevo, construye el reporte,
    /// exporta a PDF y lo guarda en la caché. Devuelve la URL.
    /// </summary>
    private string GenerarPdf(EntradaCombustible entrada)
    {
        IObjectSpace objectSpace =
            Application.CreateObjectSpace(typeof(EntradaCombustible));

        try
        {
            EntradaCombustible entradaCompleta = objectSpace
                .GetObjectsQuery<EntradaCombustible>()
                .FirstOrDefault(c => c.Id == entrada.Id);

            if (entradaCompleta == null)
            {
                throw new UserFriendlyException(
                    "No se pudo cargar la entrada para el reporte.");
            }

            Configuracion configuracion = objectSpace
                .GetObjectsQuery<Configuracion>()
                .OrderBy(c => c.Id)
                .FirstOrDefault();

            ReporteEntradaCombustible report = ReporteEntradaCombustible.Crear(
                entradaCompleta,
                configuracion);

            using var memoria = new MemoryStream();
            report.ExportToPdf(memoria);
            byte[] pdf = memoria.ToArray();

            if (pdf == null || pdf.Length == 0)
            {
                throw new UserFriendlyException(
                    "El reporte se generó vacío. Verifique los datos de la entrada.");
            }

            string nombreArchivo =
                $"entrada-{entradaCompleta.Consecutivo:D6}.pdf";

            string token = ReporteCompraPdfCache.Guardar(pdf, nombreArchivo);

            DiagnosticoReportes.Registrar(
                $"PDF entrada generado token={token} bytes={pdf.Length} " +
                $"entrada={entradaCompleta.Id}");

            return $"/reportes/compra-combustible-pdf/{token}";
        }
        finally
        {
            objectSpace.Dispose();
        }
    }

    private EntradaCombustible ObtenerEntradaSeleccionada()
    {
        if (View is DetailView vistaDetalle &&
            vistaDetalle.CurrentObject is EntradaCombustible entradaDetalle)
        {
            return entradaDetalle;
        }

        if (View is ListView vistaLista &&
            vistaLista.SelectedObjects != null &&
            vistaLista.SelectedObjects.Count == 1 &&
            vistaLista.SelectedObjects[0] is EntradaCombustible entradaLista)
        {
            return entradaLista;
        }

        return null;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // -= antes de += : a prueba de activaciones repetidas.
        imprimirAction.Execute -= ImprimirAction_Execute;
        imprimirAction.Execute += ImprimirAction_Execute;

        DiagnosticoReportes.Registrar(
            $"Controller Imprimir Entrada activado vista={View?.GetType().Name} id={View?.Id}");
    }

    protected override void OnDeactivated()
    {
        imprimirAction.Execute -= ImprimirAction_Execute;
        base.OnDeactivated();
    }
}
