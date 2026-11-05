using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DXComnTransp.Module.BusinessObjects;
using System;
using System.Linq;

namespace DXComnTransp.Module.Controllers;

// ============================================================
// Reglas de eliminación en la LISTA de entradas de combustible
// (idéntico a CompraCombustibleListViewController): no se puede
// eliminar ninguna entrada seleccionada que esté Terminada o
// Cancelada.
// ============================================================
public class EntradaCombustibleListViewController
    : ObjectViewController<ListView, EntradaCombustible>
{
    private const string EliminarBloqueadoKey =
        "EntradaCombustibleEliminarBloqueadoListView";

    // ============================================================
    // ACTIVAR
    // ============================================================

    protected override void OnActivated()
    {
        base.OnActivated();

        View.SelectionChanged -= View_SelectionChanged;
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
        // REVISAR TODAS LAS ENTRADAS SELECCIONADAS
        // ========================================================

        foreach (object objeto in View.SelectedObjects)
        {
            if (objeto is not EntradaCombustible entrada)
                continue;


            if (entrada.Terminada ||
                entrada.Cancelada)
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
