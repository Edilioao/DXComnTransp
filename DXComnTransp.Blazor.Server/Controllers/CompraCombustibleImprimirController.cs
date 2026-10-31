using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.AspNetCore;
using DevExpress.Persistent.Base;
using DXComnTransp.Blazor.Server.Services;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.IO;
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Botón "Imprimir Compra" (reporte DevExpress) para Blazor Server.
/// Construye el XtraReport, lo exporta a PDF, lo guarda en caché y lo
/// abre en una pestaña nueva del navegador (reutiliza dxAbrirReporte).
///
/// Funciona en DetailView y en ListView (requiere una sola compra seleccionada).
/// </summary>
public class CompraCombustibleImprimirController : ViewController
{
    private const string ImprimirActionId = "ImprimirCompraCombustible";

    private readonly SimpleAction imprimirAction;

    public CompraCombustibleImprimirController()
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
            ToolTip = "Genera el PDF (DevExpress) de esta compra",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        imprimirAction.Execute += ImprimirAction_Execute;
    }

    private async void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        CompraCombustible compra = ObtenerCompraSeleccionada();

        if (compra == null)
        {
            throw new UserFriendlyException(
                "Debe seleccionar una compra para imprimir.");
        }

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

            // ---------- Exportar a PDF ----------
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
            string url = $"/reportes/compra-combustible-pdf/{token}";

            // ---------- Abrir en pestaña nueva ----------
            IServiceProvider provider =
                (Application as AspNetCoreApplication)?.ServiceProvider;

            if (provider == null)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "No se pudo abrir el navegador para mostrar el reporte.",
                    InformationType.Error);
                return;
            }

            IJSRuntime js = provider.GetService<IJSRuntime>();

            if (js != null)
            {
                await js.InvokeVoidAsync("dxAbrirReporte", url);
            }
            else
            {
                NavigationManager navigation =
                    provider.GetService<NavigationManager>();

                if (navigation == null)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "No se pudo abrir el navegador para mostrar el reporte.",
                        InformationType.Error);
                    return;
                }

                navigation.NavigateTo(url, forceLoad: true);
            }
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

    protected override void OnDeactivated()
    {
        imprimirAction.Execute -= ImprimirAction_Execute;
        base.OnDeactivated();
    }
}
