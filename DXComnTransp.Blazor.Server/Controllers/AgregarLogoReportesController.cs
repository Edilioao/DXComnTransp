using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.XtraReports.UI;
using DXComnTransp.Module.BusinessObjects;
using System.IO;
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

public class AgregarLogoReportesController
    : ObjectViewController<ListView, ReportDataV2>
{
    private SimpleAction agregarLogoAction;
    private SimpleAction agregarLogoTodosAction;

    public AgregarLogoReportesController()
    {
        agregarLogoAction = new SimpleAction(
            this,
            "AgregarLogoReporte",
            PredefinedCategory.View)
        {
            Caption = "Agregar Logo a Reporte",
            ImageName = "Action_Insert",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        agregarLogoAction.Execute += AgregarLogoAction_Execute;

        agregarLogoTodosAction = new SimpleAction(
            this,
            "AgregarLogoTodosReportes",
            PredefinedCategory.View)
        {
            Caption = "Agregar Logo a Todos los Reportes",
            ImageName = "Action_Apply",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        agregarLogoTodosAction.Execute += AgregarLogoTodosAction_Execute;
    }

    private void AgregarLogoAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not ReportDataV2 reporte)
        {
            Application.ShowViewStrategy.ShowMessage(
                "Seleccione un reporte.",
                InformationType.Warning);
            return;
        }

        AgregarLogoAReporte(reporte);
    }

    private void AgregarLogoTodosAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        try
        {
            // Obtener la configuración con el logo
            Configuracion configuracion = ObjectSpace.GetObjectsQuery<Configuracion>()
                .FirstOrDefault();

            if (configuracion == null || configuracion.Logo == null || configuracion.Logo.Length == 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "No hay logo cargado en la configuración.",
                    InformationType.Warning);
                return;
            }

            // Obtener todos los reportes
            var reportes = ObjectSpace.GetObjectsQuery<ReportDataV2>().ToList();
            int modificados = 0;
            int omitidos = 0;

            foreach (var reporte in reportes)
            {
                if (reporte.Content == null || reporte.Content.Length == 0)
                {
                    omitidos++;
                    continue;
                }

                if (AgregarLogoAReporteInterno(reporte, configuracion.Logo))
                {
                    modificados++;
                }
                else
                {
                    omitidos++;
                }
            }

            ObjectSpace.CommitChanges();
            View.Refresh();

            Application.ShowViewStrategy.ShowMessage(
                $"Proceso completado:\n" +
                $"- Reportes modificados: {modificados}\n" +
                $"- Reportes omitidos (ya tenían logo o sin contenido): {omitidos}",
                InformationType.Success);
        }
        catch (System.Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                "Error al agregar el logo a todos los reportes: " + ex.Message,
                InformationType.Error);
        }
    }

    private void AgregarLogoAReporte(ReportDataV2 reporte)
    {
        try
        {
            // Obtener la configuración con el logo
            Configuracion configuracion = ObjectSpace.GetObjectsQuery<Configuracion>()
                .FirstOrDefault();

            if (configuracion == null || configuracion.Logo == null || configuracion.Logo.Length == 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "No hay logo cargado en la configuración.",
                    InformationType.Warning);
                return;
            }

            if (reporte.Content == null || reporte.Content.Length == 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "El reporte no tiene contenido.",
                    InformationType.Warning);
                return;
            }

            if (AgregarLogoAReporteInterno(reporte, configuracion.Logo))
            {
                ObjectSpace.CommitChanges();
                View.Refresh();

                Application.ShowViewStrategy.ShowMessage(
                    "Logo agregado correctamente al reporte en la esquina superior izquierda.",
                    InformationType.Success);
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage(
                    "El reporte ya tiene un control de imagen.",
                    InformationType.Info);
            }
        }
        catch (System.Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                "Error al agregar el logo: " + ex.Message,
                InformationType.Error);
        }
    }

    private bool AgregarLogoAReporteInterno(ReportDataV2 reporte, byte[] logoBytes)
    {
        // Cargar el reporte
        XtraReport xtraReport = new XtraReport();
        using (MemoryStream stream = new MemoryStream(reporte.Content))
        {
            xtraReport.LoadLayoutFromXml(stream);
        }

        // Verificar si ya tiene un control de logo
        bool tieneLogo = xtraReport.Bands.OfType<ReportHeaderBand>()
            .Any(b => b.Controls.OfType<XRPictureBox>().Any());

        if (tieneLogo)
        {
            return false;
        }

        // Agregar el logo al ReportHeaderBand
        ReportHeaderBand headerBand = xtraReport.Bands.OfType<ReportHeaderBand>().FirstOrDefault();
        if (headerBand == null)
        {
            headerBand = new ReportHeaderBand();
            xtraReport.Bands.Add(headerBand);
        }

        // Crear control de imagen para el logo
        XRPictureBox logoPictureBox = new XRPictureBox
        {
            Name = "logoPictureBox",
            Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage,
            WidthF = 150,
            HeightF = 80,
            LocationF = new DevExpress.Utils.PointFloat(0, 0)
        };

        // Cargar la imagen desde los bytes
        using (MemoryStream imageStream = new MemoryStream(logoBytes))
        {
            logoPictureBox.Image = System.Drawing.Image.FromStream(imageStream);
        }

        headerBand.Controls.Add(logoPictureBox);

        // Guardar el reporte modificado
        using (MemoryStream stream = new MemoryStream())
        {
            xtraReport.SaveLayoutToXml(stream);
            reporte.Content = stream.ToArray();
        }

        return true;
    }
}
