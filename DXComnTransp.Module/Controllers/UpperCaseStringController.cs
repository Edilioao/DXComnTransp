using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DXComnTransp.Module.Controllers;

public class UpperCaseStringController : ObjectViewController<DetailView, object>
{
    private static readonly HashSet<string> CamposExcluidos =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Comentario",
            "Comentarios",
            "ComentarioInterno",
            "ComentariosInternos",
            "Nota",
            "Notas",
            "Observacion",
            "Observaciones",
            "DescripcionLarga"
        };

    protected override void OnActivated()
    {
        base.OnActivated();

        ObjectSpace.Committing += ObjectSpace_Committing;
    }

    private void ObjectSpace_Committing(object sender, EventArgs e)
    {
        if (View.CurrentObject == null)
            return;

        ConvertirStringsAMayusculas(View.CurrentObject);
    }

    private void ConvertirStringsAMayusculas(object objeto)
    {
        if (objeto == null)
            return;

        Type tipo = objeto.GetType();

        PropertyInfo[] propiedades = tipo.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance);

        foreach (PropertyInfo propiedad in propiedades)
        {
            if (!propiedad.CanRead || !propiedad.CanWrite)
                continue;

            if (propiedad.PropertyType != typeof(string))
                continue;

            if (CamposExcluidos.Contains(propiedad.Name))
                continue;

            string valor = propiedad.GetValue(objeto) as string;

            if (string.IsNullOrEmpty(valor))
                continue;

            string nuevoValor = valor.ToUpper();

            if (valor != nuevoValor)
            {
                propiedad.SetValue(objeto, nuevoValor);
            }
        }
    }

    protected override void OnDeactivated()
    {
        ObjectSpace.Committing -= ObjectSpace_Committing;

        base.OnDeactivated();
    }
}