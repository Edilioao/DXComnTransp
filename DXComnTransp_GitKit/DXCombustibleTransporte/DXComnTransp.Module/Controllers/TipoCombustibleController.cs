using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DXComnTransp.Module.BusinessObjects;

namespace DXComnTransp.Module.Controllers;

public class TipoCombustibleController : ObjectViewController<DetailView, TipoCombustible>
{
    private const string IdReadOnlyKey = "TipoCombustibleIdReadOnly";

    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is TipoCombustible combustible &&
            ObjectSpace.IsNewObject(combustible))
        {
            combustible.Id = ObtenerSiguienteId();
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
        if (View.CurrentObject is not TipoCombustible combustible)
            return;

        if (View.FindItem(nameof(TipoCombustible.Id)) is PropertyEditor editor)
        {
            bool esNuevo = ObjectSpace.IsNewObject(combustible);

            editor.AllowEdit[IdReadOnlyKey] = esNuevo;
        }
    }

    private int ObtenerSiguienteId()
    {
        int? ultimoId = ObjectSpace
            .GetObjectsQuery<TipoCombustible>()
            .Select(x => (int?)x.Id)
            .Max();

        return (ultimoId ?? 0) + 1;
    }
}