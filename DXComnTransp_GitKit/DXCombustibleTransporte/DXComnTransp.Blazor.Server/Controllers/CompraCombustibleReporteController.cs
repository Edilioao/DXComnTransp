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
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Agrega el botón "Imprimir Reporte" en el DetailView de la compra.
/// Genera el HTML y lo abre en una pestaña nueva del navegador.
/// </summary>
public class CompraCombustibleReporteController
    : ObjectViewController<DetailView, CompraCombustible>
{
    private readonly SimpleAction imprimirAction;

    public CompraCombustibleReporteController()
    {
        imprimirAction = new SimpleAction(
            this,
            "ImprimirReporteCompraCombustible",
            PredefinedCategory.Reports)
        {
            Caption = "Imprimir Reporte",
            ImageName = "Action_Printing",
            ToolTip = "Genera el reporte imprimible de la compra."
        };

        imprimirAction.Execute += ImprimirAction_Execute;
    }

    private async void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        if (View?.CurrentObject is not CompraCombustible compra)
            return;

        try
        {
            Configuracion configuracion = ObjectSpace
                .GetObjectsQuery<Configuracion>()
                .OrderBy(c => c.Id)
                .FirstOrDefault();

            string html = CompraCombustibleReporteHtml.Build(
                compra,
                configuracion);

            string token = ReporteCompraCache.Guardar(html);
            string url = $"/reportes/compra-combustible/{token}";

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
                // Abre el reporte en una pestaña nueva. Si el navegador
                // bloquea la ventana, el script navega en la misma pestaña.
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
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                "No se pudo generar el reporte: " + ex.Message,
                InformationType.Error);
        }
    }
}
