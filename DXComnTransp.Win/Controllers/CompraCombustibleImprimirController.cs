using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.XtraReports.UI;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using System;
using System.Linq;

namespace DXComnTransp.Win.Controllers;

/// <summary>
/// Botón "Imprimir Compra" (reporte DevExpress) para la compra de combustible.
/// Construye el XtraReport y muestra la vista previa nativa de Windows
/// (ReportPrintTool), reemplazando el controller roto del módulo.
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
            PredefinedCategory.RecordEdit)
        {
            Caption = "Imprimir Compra",
            ImageName = "Action_Printing_Print",
            ToolTip = "Vista previa del reporte DevExpress de esta compra",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };

        imprimirAction.Execute += ImprimirAction_Execute;
    }

    private void ImprimirAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
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

        // ========================================================
        // ObjectSpace nuevo: la compra y la configuración se cargan
        // ahí y todo el contenido del reporte se materializa antes
        // de renderizar (el reporte no vuelve a tocar la BD).
        // ========================================================
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

            report.DisplayName =
                $"Compra {compraCompleta.Consecutivo:D6} - Fact. {compraCompleta.NoFactura}";

            // Vista previa nativa (modal). Al cerrarla seguimos aquí.
            var herramienta = new ReportPrintTool(report);
            herramienta.ShowPreviewDialog();
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
