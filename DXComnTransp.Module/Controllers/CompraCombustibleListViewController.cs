using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DXComnTransp.Module.BusinessObjects;

namespace DXComnTransp.Module.Controllers;

public class CompraCombustibleListViewController
    : ObjectViewController<ListView, CompraCombustible>
{
    private const string EliminarBloqueadoKey =
        "CompraCombustibleEliminarBloqueadoListView";

    // ============================================================
    // ACTIVAR
    // ============================================================

    protected override void OnActivated()
    {
        base.OnActivated();

        View.SelectionChanged += View_SelectionChanged;
        ActualizarEliminacion();
    }


    // ============================================================
    // CAMBIO DE SELECCIÓN
    // ============================================================

    private void View_SelectionChanged(
        object sender,
        EventArgs e)
    {
        ActualizarEliminacion();
    }


    // ============================================================
    // CONTROLAR ELIMINACIÓN
    // ============================================================

    private void ActualizarEliminacion()
    {
        DeleteObjectsViewController deleteController =
            Frame.GetController<DeleteObjectsViewController>();

        if (deleteController == null)
            return;


        // ========================================================
        // SIN SELECCIÓN
        // ========================================================

        if (View.SelectedObjects.Count == 0)
        {
            deleteController.DeleteAction.Active.SetItemValue(
                EliminarBloqueadoKey,
                true
            );

            return;
        }


        // ========================================================
        // REVISAR TODAS LAS COMPRAS SELECCIONADAS
        // ========================================================

        foreach (object objeto in View.SelectedObjects)
        {
            if (objeto is not CompraCombustible compra)
                continue;


            if (compra.Terminada ||
                compra.Cancelada)
            {
                deleteController.DeleteAction.Active.SetItemValue(
                    EliminarBloqueadoKey,
                    false
                );

                return;
            }
        }


        // ========================================================
        // TODAS PUEDEN ELIMINARSE
        // ========================================================

        deleteController.DeleteAction.Active.SetItemValue(
            EliminarBloqueadoKey,
            true
        );
    }


    // ============================================================
    // DESACTIVAR
    // ============================================================

    protected override void OnDeactivated()
    {
        View.SelectionChanged -= View_SelectionChanged;
        base.OnDeactivated();
    }

}