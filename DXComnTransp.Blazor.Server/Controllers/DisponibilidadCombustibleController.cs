using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.Persistent.Base;
using DXComnTransp.Blazor.Server.Services;
using DXComnTransp.Module.BusinessObjects;
using DXComnTransp.Module.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DXComnTransp.Blazor.Server.Controllers;

/// <summary>
/// Acción "Disponibilidad Combustible" (Blazor): genera el reporte
/// PDF de DISPONIBILIDAD POR TIPO DE COMBUSTIBLE y lo muestra en el
/// mismo visor (DxPopup) con el mismo mecanismo verificado de las
/// compras y las entradas.
///
/// Disponible por combustible =
///     Comprado  = suma de CANTIDADES de compras NO canceladas
///     Recibido  = suma de CANTIDADES de entradas TERMINADAS NO
///                 canceladas
///     Disponible= Comprado - Recibido
///
/// La acción vive en las vistas de Inventario (lista y detalle,
/// Nesting.Any): es el lugar natural del control de inventario.
/// Es independiente de la selección (reporte de TODOS los tipos).
/// </summary>
public class DisponibilidadCombustibleController : ViewController
{
    private const string AccionId = "DisponibilidadCombustible";

    private readonly SimpleAction reporteAction;

    public DisponibilidadCombustibleController()
    {
        TargetObjectType = typeof(Inventario);
        TargetViewNesting = Nesting.Any;

        reporteAction = new SimpleAction(
            this,
            AccionId,
            PredefinedCategory.Reports)
        {
            Caption = "Disponibilidad Combustible",
            ImageName = "BO_Report",
            ToolTip = "Reporte de disponibilidad por tipo de combustible " +
                      "(comprado - recibido)"
        };
    }

    private void ReporteAction_Execute(
        object sender,
        SimpleActionExecuteEventArgs e)
    {
        string url;

        try
        {
            url = GenerarPdfDisponibilidad();
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
                "Error al generar el reporte de disponibilidad: " + error.Message);
        }

        IServiceProvider proveedor =
            (Application as BlazorApplication)?.ServiceProvider;

        if (proveedor?.GetService(typeof(VisorReporteService)) is VisorReporteService visor)
        {
            visor.Abrir(url);

            DiagnosticoReportes.Registrar(
                $"Disponibilidad.Abrir OK inst={visor.GetHashCode()} url=" + url);
            return;
        }

        DiagnosticoReportes.Registrar(
            "VisorReporteService no disponible (disponibilidad). url=" + url);
    }

    private string GenerarPdfDisponibilidad()
    {
        IObjectSpace objectSpace =
            Application.CreateObjectSpace(typeof(Inventario));

        try
        {
            // ---------- Catálogo de combustibles ----------
            List<TipoCombustible> combustibles = objectSpace
                .GetObjectsQuery<TipoCombustible>()
                .OrderBy(c => c.Combustible)
                .ToList();

            // ---------- Comprado (compras NO canceladas) ----------
            Dictionary<int, decimal> comprado = objectSpace
                .GetObjectsQuery<CompraCombustibleDetalle>()
                .Where(d => d.Compra.Cancelada == false)
                .GroupBy(d => d.IdProducto)
                .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToDictionary(x => x.Producto, x => x.Cantidad);

            // ---------- Recibido (entradas terminadas NO canceladas) ----------
            Dictionary<int, decimal> recibido = objectSpace
                .GetObjectsQuery<EntradaCombustibleDetalle>()
                .Where(d => d.Entrada.Terminada == true &&
                            d.Entrada.Cancelada == false)
                .GroupBy(d => d.IdProducto)
                .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToDictionary(x => x.Producto, x => x.Cantidad);

            // ---------- Filas del reporte ----------
            var filas = new List<ReporteDisponibilidadCombustible.FilaDisponibilidad>();

            foreach (TipoCombustible combustible in combustibles)
            {
                decimal cantComprada =
                    comprado.TryGetValue(combustible.Id, out decimal c)
                        ? c : 0m;
                decimal cantRecibida =
                    recibido.TryGetValue(combustible.Id, out decimal r)
                        ? r : 0m;

                filas.Add(new ReporteDisponibilidadCombustible.FilaDisponibilidad
                {
                    Combustible = combustible.Combustible,
                    Comprado = cantComprada,
                    Recibido = cantRecibida,
                    Disponible = cantComprada - cantRecibida
                });
            }

            Configuracion configuracion = objectSpace
                .GetObjectsQuery<Configuracion>()
                .OrderBy(c => c.Id)
                .FirstOrDefault();

            ReporteDisponibilidadCombustible report =
                ReporteDisponibilidadCombustible.Crear(filas, configuracion);

            using var memoria = new MemoryStream();
            report.ExportToPdf(memoria);
            byte[] pdf = memoria.ToArray();

            if (pdf == null || pdf.Length == 0)
            {
                throw new UserFriendlyException(
                    "El reporte de disponibilidad se generó vacío.");
            }

            string token = ReporteCompraPdfCache.Guardar(
                pdf,
                "disponibilidad-combustible.pdf");

            DiagnosticoReportes.Registrar(
                $"PDF disponibilidad generado token={token} bytes={pdf.Length} " +
                $"filas={filas.Count}");

            return $"/reportes/compra-combustible-pdf/{token}";
        }
        finally
        {
            objectSpace.Dispose();
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // -= antes de += : a prueba de activaciones repetidas.
        reporteAction.Execute -= ReporteAction_Execute;
        reporteAction.Execute += ReporteAction_Execute;
    }

    protected override void OnDeactivated()
    {
        reporteAction.Execute -= ReporteAction_Execute;
        base.OnDeactivated();
    }
}
