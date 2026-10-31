using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DXComnTransp.Win.Controllers;

/// <summary>
/// Botón "Imprimir Reporte" en el DetailView de la compra (versión WinForms).
/// Genera el HTML y lo abre en el navegador predeterminado para imprimirlo.
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

    private void ImprimirAction_Execute(
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

            string carpeta = Path.Combine(
                Path.GetTempPath(),
                "DXComnTransp",
                "Reportes");

            Directory.CreateDirectory(carpeta);

            string archivo = Path.Combine(
                carpeta,
                $"compra-{compra.Consecutivo:D6}-{Guid.NewGuid():N}.html");

            File.WriteAllText(archivo, html, Encoding.UTF8);

            Process.Start(new ProcessStartInfo
            {
                FileName = archivo,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                "No se pudo generar el reporte: " + ex.Message,
                InformationType.Error);
        }
    }
}
