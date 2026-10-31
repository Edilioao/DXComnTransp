
using System;
using System.IO;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.XtraReports.UI;

namespace DXComnTransp.Module.Controllers
{
    public class DuplicarReporteController
        : ObjectViewController<ListView, ReportDataV2>
    {
        private readonly SimpleAction duplicarReporteAction;

        public DuplicarReporteController()
        {
            duplicarReporteAction = new SimpleAction(
                this,
                "DuplicarReporte",
                PredefinedCategory.Edit)
            {
                Caption = "Duplicar reporte",
                ImageName = "Action_Copy",
                SelectionDependencyType =
                    SelectionDependencyType.RequireSingleObject
            };

            duplicarReporteAction.Execute +=
                DuplicarReporteAction_Execute;
        }

        private void DuplicarReporteAction_Execute(
            object sender,
            SimpleActionExecuteEventArgs e)
        {
            ReportDataV2 original =
                View.CurrentObject as ReportDataV2;

            if (original == null)
                return;

            if (original.Content == null ||
                original.Content.Length == 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "El reporte seleccionado no contiene un diseño para copiar.",
                    InformationType.Warning);

                return;
            }

            try
            {
                // Nombre base de la copia.
                string nombreBase =
                    (original.DisplayName ?? "Reporte").Trim();

                string nombreCopia =
                    nombreBase + " - Copia";

                int numero = 2;

                // Evitar nombres duplicados.
                while (ObjectSpace.GetObjectsQuery<ReportDataV2>()
                    .Any(r => r.DisplayName == nombreCopia))
                {
                    nombreCopia =
                        nombreBase + " - Copia " + numero;

                    numero++;
                }

                // Crear un registro nuevo.
                ReportDataV2 copia =
                    ObjectSpace.CreateObject<ReportDataV2>();

                // Cargar el diseño original.
                using (MemoryStream entrada =
                    new MemoryStream(original.Content))
                {
                    XtraReport reporte = new XtraReport();

                    reporte.LoadLayoutFromXml(entrada);

                    // Cambiar también el nombre dentro del diseño.
                    reporte.DisplayName = nombreCopia;

                    using (MemoryStream salida =
                        new MemoryStream())
                    {
                        reporte.SaveLayoutToXml(salida);

                        copia.Content = salida.ToArray();
                    }

                    // Liberar el reporte cargado.
                    reporte.Dispose();
                }

                // Nombre que muestra XAF en la lista.
                copia.DisplayName = nombreCopia;

                // Conservar la configuración de inserción.
                copia.IsInplaceReport =
                    original.IsInplaceReport;

                // Guardar el registro nuevo.
                ObjectSpace.CommitChanges();

                View.Refresh();

                Application.ShowViewStrategy.ShowMessage(
                    "Reporte duplicado correctamente: " +
                    nombreCopia,
                    InformationType.Success);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "No se pudo duplicar el reporte: " +
                    ex.Message,
                    InformationType.Error);
            }
        }
    }
}
