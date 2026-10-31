using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.ComponentModel;
using System.Reflection;

namespace DXComnTransp.Module.Controllers;

public class DeleteConfirmationController : ViewController<ListView>
{
    private DeleteObjectsViewController deleteController;
    private string mensajeOriginal;

    public DeleteConfirmationController()
    {
        TargetViewType = ViewType.ListView;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        deleteController = Frame.GetController<DeleteObjectsViewController>();

        if (deleteController == null)
            return;

        // Guardar el mensaje original de XAF
        mensajeOriginal = deleteController.DeleteAction.ConfirmationMessage;

        View.SelectionChanged += View_SelectionChanged;

        ActualizarMensaje();
    }

    private void View_SelectionChanged(object sender, EventArgs e)
    {
        ActualizarMensaje();
    }

    private void ActualizarMensaje()
    {
        if (deleteController == null)
            return;

        int cantidad = View.SelectedObjects.Count;

        if (cantidad <= 0)
        {
            deleteController.DeleteAction.ConfirmationMessage =
                mensajeOriginal;

            return;
        }

        // Nombre de la página/lista que muestra XAF
        string nombrePagina = View.Caption;

        if (string.IsNullOrWhiteSpace(nombrePagina))
            nombrePagina = "registros";

        // ============================================================
        // UN SOLO REGISTRO
        // ============================================================

        if (cantidad == 1)
        {
            object objeto = View.SelectedObjects[0];

            string identificacion = ObtenerIdentificacion(objeto);

            if (!string.IsNullOrWhiteSpace(identificacion))
            {
                deleteController.DeleteAction.ConfirmationMessage =
                    $"¿Desea eliminar el registro \"{identificacion}\" " +
                    $"de {nombrePagina}?\n\n" +
                    "Esta operación no se puede deshacer.";
            }
            else
            {
                deleteController.DeleteAction.ConfirmationMessage =
                    $"¿Desea eliminar el registro seleccionado de {nombrePagina}?\n\n" +
                    "Esta operación no se puede deshacer.";
            }
        }

        // ============================================================
        // VARIOS REGISTROS
        // ============================================================

        else
        {
            deleteController.DeleteAction.ConfirmationMessage =
                $"¿Desea eliminar los {cantidad} registros seleccionados " +
                $"de {nombrePagina}?\n\n" +
                "Esta operación no se puede deshacer.";
        }
    }

    private string ObtenerIdentificacion(object objeto)
    {
        if (objeto == null)
            return null;

        Type tipo = objeto.GetType();

        // ============================================================
        // 1. Utilizar [DefaultProperty]
        // ============================================================

        DefaultPropertyAttribute atributo =
            tipo.GetCustomAttribute<DefaultPropertyAttribute>();

        if (atributo != null &&
            !string.IsNullOrWhiteSpace(atributo.Name))
        {
            PropertyInfo propiedadDefault =
                tipo.GetProperty(atributo.Name);

            if (propiedadDefault != null)
            {
                object valor = propiedadDefault.GetValue(objeto);

                if (valor != null &&
                    !string.IsNullOrWhiteSpace(valor.ToString()))
                {
                    return valor.ToString();
                }
            }
        }

        // ============================================================
        // 2. Buscar propiedades habituales
        // ============================================================

        string[] propiedades =
        {
            "Nombre",
            "Descripcion",
            "Descripción",
            "Codigo",
            "Código",
            "NombreCompleto"
        };

        foreach (string nombre in propiedades)
        {
            PropertyInfo propiedad =
                tipo.GetProperty(nombre);

            if (propiedad == null)
                continue;

            object valor = propiedad.GetValue(objeto);

            if (valor != null &&
                !string.IsNullOrWhiteSpace(valor.ToString()))
            {
                return valor.ToString();
            }
        }

        return null;
    }

    protected override void OnDeactivated()
    {
        if (deleteController != null)
        {
            View.SelectionChanged -= View_SelectionChanged;

            if (!string.IsNullOrWhiteSpace(mensajeOriginal))
            {
                deleteController.DeleteAction.ConfirmationMessage =
                    mensajeOriginal;
            }

            deleteController = null;
        }

        base.OnDeactivated();
    }
}