using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Persistent.Base;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.XtraReports.Web.WebDocumentViewer.Native.Services;

namespace DXComnTransp.Module.Controllers;

// ============================================================
// IMPRIMIR COMPRA DE COMBUSTIBLE  (XAF 22.1, Win + Blazor)
//
// - Action "Imprimir Compra" en DetailView y ListView
//   (requiere seleccionar una sola compra).
// - Construye el XtraReport ad-hoc (sin ReportDataV2).
// - Muestra el preview vía ReportServiceController.ShowPreview(XtraReport),
//   invocado por reflexión para funcionar en WinReportServiceController
//   y en BlazorReportServiceController sin referencia directa.
// ============================================================
public class ImprimirCompraCombustibleController : ViewController
{
    private const string ImprimirActionId = "ImprimirCompraCombustible";

    private readonly SimpleAction imprimirAction;

    public ImprimirCompraCombustibleController()
    {
        TargetObjectType = typeof(CompraCombustible);
        TargetViewNesting = Nesting.Root;

        imprimirAction = new SimpleAction(
            this,
            ImprimirActionId,
            PredefinedCategory.Reports)
        {
            Caption = "Imprimir Compra",
            ImageName = "Action_Printing_Print",
            ToolTip = "Imprimir / Previsualizar esta compra de combustible",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        imprimirAction.Execute += ImprimirAction_Execute;
    }

    // ============================================================
    // EJECUCIÓN
    // ============================================================
    private void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        CompraCombustible compra = GetCompraSeleccionada();

        if (compra == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar una compra para imprimir.");
        }

        // ========================================================
        // CARGAR DATOS DE EMPRESA (Configuracion)
        // ========================================================
        Configuracion cfg = null;
        using (IObjectSpace cfgOS = Application.CreateObjectSpace(typeof(Configuracion)))
        {
            cfg = cfgOS.GetObjectsQuery<Configuracion>().FirstOrDefault();
        }

        // ========================================================
        // RECARGAR LA COMPRA (CON DETALLES) EN UN OBJECTSPACE NUEVO
        //
        // Esto evita problemas con el tracking de EF Core al pasarla
        // como DataSource del reporte.
        // ========================================================
        CompraCombustible compraReporte;
        IObjectSpace reportOS = Application.CreateObjectSpace(typeof(CompraCombustible));
        compraReporte = reportOS
            .GetObjectsQuery<CompraCombustible>()
            .FirstOrDefault(c => c.Id == compra.Id);

        if (compraReporte == null)
        {
            reportOS.Dispose();
            throw new UserFriendlyException(
                "No se pudo cargar la compra para el reporte.");
        }

        // Forzar la materialización de los Detalles
        _ = compraReporte.Detalles?.Count;

        // ========================================================
        // CONSTRUIR EL REPORTE
        // ========================================================
        var report = new CompraCombustibleReport();

        report.SetEmpresa(
            cfg?.Nombre ?? "",
            cfg?.Nit ?? "",
            cfg?.Slogan ?? "",
            cfg?.Telefonos ?? "",
            cfg?.Correo ?? "",
            cfg?.Logo);

        report.DataSource = new[] { compraReporte };
        report.DisplayName =
            $"Compra Combustible #{compraReporte.Consecutivo} - Fact. {compraReporte.NoFactura}";

        //// ========================================================
        //// MOSTRAR PREVIEW MEDIANTE REPORTS V2
        //// ========================================================
        //var reportsController =
        //    Frame.GetController<ReportServiceController>();

        //if (reportsController == null)
        //{
        //    reportOS.Dispose();

        //    throw new UserFriendlyException(
        //        "No se encontró ReportServiceController. " +
        //        "Verifique que ReportsV2 esté habilitado.");
        //}

        //try
        //{
        //    var reportStorage =
        //        Application.ServiceProvider.GetRequiredService<IReportStorage>();

        //    // Preparar el reporte para Reports V2.
        //    ReportsModuleV2 reportsModule =
        //        ReportsModuleV2.FindReportsModule(Application.Modules);

        //    if (reportsModule == null)
        //    {
        //        throw new InvalidOperationException(
        //            "No se encontró el módulo ReportsV2.");
        //    }

        //    reportsModule.ReportsDataSourceHelper.SetupBeforePrint(report);

        //    // Registrar la instancia dinámica y obtener su handle.
        //    string handle = reportStorage.CreateNewReportHandle();
        //    reportStorage.SetNewData(report, handle);

        //    // Abrir la vista previa mediante XAF.
        //    reportsController.ShowPreview(handle);
        //}
        //catch (Exception ex)
        //{
        //    reportOS.Dispose();

        //    Exception error = ex;
        //    while (error.InnerException != null)
        //        error = error.InnerException;

        //    throw new UserFriendlyException(
        //        "Error al mostrar el reporte: " + error.Message);
        //}
    }

    // ============================================================
    // RESOLVER LA COMPRA (sirve para DetailView y ListView)
    // ============================================================
    private CompraCombustible GetCompraSeleccionada()
    {
        if (View is DetailView dv &&
            dv.CurrentObject is CompraCombustible compraDv)
        {
            return compraDv;
        }

        if (View is ListView lv &&
            lv.SelectedObjects != null &&
            lv.SelectedObjects.Count == 1 &&
            lv.SelectedObjects[0] is CompraCombustible compraLv)
        {
            return compraLv;
        }

        return null;
    }

    // ============================================================
    // INVOCA ShowPreview(XtraReport) EN LA IMPLEMENTACIÓN DE PLATAFORMA
    //
    // - WinReportServiceController y BlazorReportServiceController
    //   exponen el método ShowPreview(XtraReport) o
    //   ShowPreview(XtraReport, IPrintingSystemBase).
    // - Buscamos el método por reflexión para no referenciar
    //   assemblies de plataforma en el proyecto Module.
    // ============================================================
    
}
