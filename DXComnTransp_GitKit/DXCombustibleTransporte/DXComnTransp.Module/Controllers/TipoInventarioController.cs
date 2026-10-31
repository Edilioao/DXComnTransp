using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DXComnTransp.Module.BusinessObjects;

namespace DXComnTransp.Module.Controllers;

public class TipoInventarioController : ObjectViewController<DetailView, TipoInventario>
{
    private const string IdReadOnlyKey = "TipoInventarioIdReadOnly";

    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is TipoInventario tipoInventario &&
            ObjectSpace.IsNewObject(tipoInventario))
        {
            tipoInventario.Id = ObtenerSiguienteId();
        }

        AplicarEstadoId();
    }

    protected override void OnViewControlsCreated()
    {
        base.OnViewControlsCreated();
        AplicarEstadoId();
    }

    private void AplicarEstadoId()
    {
        if (View.CurrentObject is not TipoInventario tipoInventario)
            return;

        if (View.FindItem(nameof(TipoInventario.Id)) is PropertyEditor editor)
        {
            bool esNuevo = ObjectSpace.IsNewObject(tipoInventario);

            editor.AllowEdit[IdReadOnlyKey] = esNuevo;
        }
    }

    private int ObtenerSiguienteId()
    {
        int? ultimoId = ObjectSpace
            .GetObjectsQuery<TipoInventario>()
            .Select(x => (int?)x.Id)
            .Max();

        return (ultimoId ?? 0) + 1;
    }
}