
using System;
using System.Globalization;
using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Models;
using DXComnTransp.Module.BusinessObjects;

namespace DXComnTransp.Blazor.Server.Controllers
{
    public class CompraCombustibleResumenController
        : ObjectViewController<ListView, CompraCombustible>
    {
        private DxGridModel modeloCuadricula;

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            DesuscribirEvento();

            if (View is ListView &&
                View.Editor is DxGridListEditor editor)
            {
                modeloCuadricula = editor.GetGridAdapter().GridModel;

                if (modeloCuadricula != null)
                {
                    modeloCuadricula.CustomizeSummaryDisplayText
                        += PersonalizarTextoResumen;
                }
            }
        }

        private void PersonalizarTextoResumen(
            GridCustomizeSummaryDisplayTextEventArgs e)
        {
            if (e.Item.SummaryType == GridSummaryItemType.Sum &&
                e.Value != null)
            {
                e.DisplayText = string.Format(
                    CultureInfo.CurrentCulture,
                    "$ {0:N2}",
                    e.Value);
            }
        }

        private void DesuscribirEvento()
        {
            if (modeloCuadricula != null)
            {
                modeloCuadricula.CustomizeSummaryDisplayText
                    -= PersonalizarTextoResumen;

                modeloCuadricula = null;
            }
        }

        protected override void OnDeactivated()
        {
            DesuscribirEvento();
            base.OnDeactivated();
        }
    }
}
