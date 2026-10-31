using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DXComnTransp.Module.BusinessObjects;

namespace DXComnTransp.Module.Controllers;

public class DepositoController : ObjectViewController<DetailView, Deposito>
{
    private const string IdReadOnlyKey = "DepositoIdReadOnly";

    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.CurrentObject is Deposito deposito &&
            ObjectSpace.IsNewObject(deposito))
        {
            deposito.Id = ObtenerSiguienteId();
            deposito.Interno = true;
            deposito.Cerrado = false;
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
        if (View.CurrentObject is not Deposito deposito)
            return;

        if (View.FindItem(nameof(Deposito.Id)) is PropertyEditor editor)
        {
            bool esNuevo = ObjectSpace.IsNewObject(deposito);

            // Nuevo = editable
            // Existente = bloqueado
            editor.AllowEdit[IdReadOnlyKey] = esNuevo;
        }
    }

    private int ObtenerSiguienteId()
    {
        int? ultimoId = ObjectSpace
            .GetObjectsQuery<Deposito>()
            .Select(x => (int?)x.Id)
            .Max();

        return (ultimoId ?? 0) + 1;
    }
}