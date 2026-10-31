using DXComnTransp.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace DXComnTransp.Module.Reports;

/// <summary>
/// Construye el reporte imprimible (HTML) de una compra de combustible.
/// El HTML es autocontenido: incluye estilos, logo y botón de impresión.
/// </summary>
public static class CompraCombustibleReporteHtml
{
    private static readonly CultureInfo Cultura =
        CultureInfo.GetCultureInfo("es-MX");

    // ============================================================
    // PUNTO DE ENTRADA
    // ============================================================

    public static string Build(
        CompraCombustible compra,
        Configuracion configuracion,
        string generadoPor = null)
    {
        if (compra == null)
            throw new ArgumentNullException(nameof(compra));

        string empresa = Texto(configuracion?.Nombre, "Empresa");
        string iniciales = Iniciales(empresa);

        bool cancelada = compra.Cancelada;
        bool terminada = compra.Terminada && !cancelada;

        string estado = cancelada
            ? "Cancelada"
            : terminada ? "Terminada" : "En proceso";

        string claseEstado = cancelada
            ? "cancelada"
            : terminada ? "terminada" : "proceso";

        List<CompraCombustibleDetalle> detalles =
            compra.Detalles?
                .OrderBy(d => d.Id)
                .ToList()
            ?? new List<CompraCombustibleDetalle>();

        decimal importeProductos = detalles.Sum(d => d.Importe);
        decimal cantidadTotal = detalles.Sum(d => d.Cantidad);
        decimal total =
            importeProductos +
            compra.Flete +
            compra.THCD +
            compra.Seguro;

        var html = new StringBuilder(32 * 1024);

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"es\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\" />");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"<title>Compra de combustible {compra.Consecutivo:D6}</title>");
        html.AppendLine(Estilos);
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // --------------------------------------------------------
        // BARRA DE ACCIONES (no se imprime)
        // --------------------------------------------------------
        html.AppendLine("<div class=\"barra no-print\">");
        html.AppendLine("<button type=\"button\" class=\"btn primary\" onclick=\"window.print()\">&#128424; Imprimir</button>");
        html.AppendLine("<button type=\"button\" class=\"btn\" onclick=\"dxCerrarReporte()\">Cerrar</button>");
        html.AppendLine("</div>");

        html.AppendLine("<main class=\"hoja\">");

        // --------------------------------------------------------
        // ENCABEZADO: LOGO + EMPRESA
        // --------------------------------------------------------
        html.AppendLine(Encabezado(configuracion, empresa, iniciales));

        // --------------------------------------------------------
        // TÍTULO / FOLIO / ESTADO
        // --------------------------------------------------------
        html.AppendLine("<div class=\"titulo\">");
        html.AppendLine("<div>");
        html.AppendLine("<span class=\"etiqueta\">Comprobante interno</span>");
        html.AppendLine("<h2>Compra de combustible</h2>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"folio\">");
        html.AppendLine("<span class=\"rotulo\">No.</span>");
        html.AppendLine($"<span class=\"num\">{compra.Consecutivo:D6}</span>");
        html.AppendLine($"<span class=\"estado {claseEstado}\">{Html(estado)}</span>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");

        // --------------------------------------------------------
        // DATOS GENERALES
        // --------------------------------------------------------
        var datos = new List<KeyValuePair<string, string>>
        {
            Dato("Fecha de compra", Html(Fecha(compra.Fecha))),
            Dato("Fecha de factura", Html(Fecha(compra.FechaFactura))),
            Dato("No. factura", Html(Texto(compra.NoFactura))),
            Dato("Proveedor", Html(Texto(compra.Proveedor?.Nombre))),
            Dato("Moneda", Html(Moneda(compra.Moneda))),
            Dato("Cantidad de detalles", Html(detalles.Count.ToString(Cultura))),
        };

        if (compra.FechaTerminada.HasValue)
        {
            datos.Add(Dato(
                "Terminada el",
                Html(FechaHora(compra.FechaTerminada.Value))));
        }

        if (compra.FechaCancelada.HasValue)
        {
            datos.Add(Dato(
                "Cancelada el",
                Html(FechaHora(compra.FechaCancelada.Value))));
        }

        html.AppendLine("<section>");
        html.AppendLine("<h3>Datos generales</h3>");
        html.AppendLine("<table class=\"datos\">");
        for (int i = 0; i < datos.Count; i += 2)
        {
            html.Append("<tr>");
            html.Append($"<td class=\"lbl\">{Html(datos[i].Key)}</td>");
            html.Append($"<td class=\"val\">{datos[i].Value}</td>");

            if (i + 1 < datos.Count)
            {
                html.Append($"<td class=\"lbl\">{Html(datos[i + 1].Key)}</td>");
                html.Append($"<td class=\"val\">{datos[i + 1].Value}</td>");
            }
            else
            {
                html.Append("<td class=\"lbl\"></td><td class=\"val\"></td>");
            }

            html.AppendLine("</tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("</section>");

        // --------------------------------------------------------
        // RESUMEN DE IMPORTES (ARRIBA)
        // --------------------------------------------------------
        html.AppendLine("<section>");
        html.AppendLine("<h3>Resumen de importes</h3>");
        html.AppendLine("<table class=\"importes\">");
        html.AppendLine($"<tr><td class=\"lbl\">Importe de productos</td><td class=\"num\">{Dinero(importeProductos)}</td></tr>");
        html.AppendLine($"<tr><td class=\"lbl\">Flete</td><td class=\"num\">{Dinero(compra.Flete)}</td></tr>");
        html.AppendLine($"<tr><td class=\"lbl\">THCD</td><td class=\"num\">{Dinero(compra.THCD)}</td></tr>");
        html.AppendLine($"<tr><td class=\"lbl\">Seguro</td><td class=\"num\">{Dinero(compra.Seguro)}</td></tr>");
        html.AppendLine($"<tr class=\"total\"><td class=\"lbl\">Total de la compra</td><td class=\"num\">{Dinero(total)}</td></tr>");
        html.AppendLine("</table>");
        html.AppendLine("</section>");

        // --------------------------------------------------------
        // DETALLE (DEBAJO DEL RESUMEN)
        // --------------------------------------------------------
        html.AppendLine("<section>");
        html.AppendLine("<h3>Detalle de la compra</h3>");
        html.AppendLine("<table class=\"detalle\">");
        html.AppendLine("<thead>");
        html.AppendLine("<tr>");
        html.AppendLine("<th class=\"col-num\">#</th>");
        html.AppendLine("<th>Combustible</th>");
        html.AppendLine("<th class=\"col-um\">U/M</th>");
        html.AppendLine("<th class=\"col-num\">Cantidad</th>");
        html.AppendLine("<th class=\"col-num\">Precio</th>");
        html.AppendLine("<th class=\"col-num\">Importe</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");

        if (detalles.Count == 0)
        {
            html.AppendLine("<tr><td colspan=\"6\" class=\"sin-datos\">Esta compra no tiene detalles registrados.</td></tr>");
        }
        else
        {
            int numero = 0;
            foreach (CompraCombustibleDetalle detalle in detalles)
            {
                numero++;
                html.AppendLine("<tr>");
                html.AppendLine($"<td class=\"col-num\">{numero}</td>");
                html.AppendLine($"<td>{Html(Texto(detalle.Producto?.Combustible))}</td>");
                html.AppendLine($"<td class=\"col-um\">{Html(Texto(detalle.Medida?.UM))}</td>");
                html.AppendLine($"<td class=\"col-num\">{Numero(detalle.Cantidad, 2)}</td>");
                html.AppendLine($"<td class=\"col-num\">{Dinero(detalle.Precio, 4)}</td>");
                html.AppendLine($"<td class=\"col-num\">{Dinero(detalle.Importe)}</td>");
                html.AppendLine("</tr>");
            }
        }

        html.AppendLine("</tbody>");
        html.AppendLine("<tfoot>");
        html.AppendLine($"<tr><td colspan=\"3\">Totales</td><td class=\"col-num\">{Numero(cantidadTotal, 2)}</td><td></td><td class=\"col-num\">{Dinero(importeProductos)}</td></tr>");
        html.AppendLine("</tfoot>");
        html.AppendLine("</table>");
        html.AppendLine("</section>");

        // --------------------------------------------------------
        // COMENTARIO / NOTA FINAL
        // --------------------------------------------------------
        html.AppendLine("<section class=\"comentario\">");
        html.AppendLine("<h3>Comentario</h3>");
        html.AppendLine($"<div class=\"caja\">{Comentario(compra.Comentario)}</div>");
        html.AppendLine("</section>");

        // --------------------------------------------------------
        // FIRMAS
        // --------------------------------------------------------
        html.AppendLine("<section class=\"firmas\">");
        html.AppendLine("<div>Elaborado por</div>");
        html.AppendLine("<div>Revisado por</div>");
        html.AppendLine("<div>Recibido por</div>");
        html.AppendLine("</section>");

        // --------------------------------------------------------
        // PIE
        // --------------------------------------------------------
        string generado =
            "Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", Cultura);

        if (!string.IsNullOrWhiteSpace(generadoPor))
            generado += " por " + Html(generadoPor);

        html.AppendLine("<footer class=\"pie\">");
        html.AppendLine($"<span>{generado}</span>");
        html.AppendLine($"<span>{Html(empresa)} · Documento interno</span>");
        html.AppendLine("</footer>");

        html.AppendLine("</main>");

        // --------------------------------------------------------
        // SCRIPT DE CIERRE
        // --------------------------------------------------------
        html.AppendLine("<script>");
        html.AppendLine("function dxCerrarReporte() {");
        html.AppendLine("  window.close();");
        html.AppendLine("  setTimeout(function () { if (!window.closed) { history.back(); } }, 150);");
        html.AppendLine("}");
        html.AppendLine("</script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    // ============================================================
    // ENCABEZADO
    // ============================================================

    private static string Encabezado(
        Configuracion configuracion,
        string empresa,
        string iniciales)
    {
        var html = new StringBuilder();

        html.AppendLine("<header class=\"encabezado\">");
        html.AppendLine($"<div class=\"logo\">{Logo(configuracion?.Logo, iniciales)}</div>");
        html.AppendLine("<div class=\"empresa\">");
        html.AppendLine($"<h1>{Html(empresa)}</h1>");

        if (!string.IsNullOrWhiteSpace(configuracion?.Slogan))
        {
            html.AppendLine($"<p class=\"slogan\">{Html(configuracion.Slogan)}</p>");
        }

        if (!string.IsNullOrWhiteSpace(configuracion?.Nit))
        {
            html.AppendLine($"<p class=\"linea\">NIT: {Html(configuracion.Nit)}</p>");
        }

        if (!string.IsNullOrWhiteSpace(configuracion?.Reeup))
        {
            html.AppendLine($"<p class=\"linea\">REEUP: {Html(configuracion.Reeup)}</p>");
        }

        if (!string.IsNullOrWhiteSpace(configuracion?.CuentaBancaria))
        {
            html.AppendLine($"<p class=\"linea\">Cuenta bancaria: {Html(configuracion.CuentaBancaria)}</p>");
        }

        if (!string.IsNullOrWhiteSpace(configuracion?.Telefonos))
        {
            html.AppendLine($"<p class=\"linea\">Teléfonos: {Html(configuracion.Telefonos)}</p>");
        }

        if (!string.IsNullOrWhiteSpace(configuracion?.Correo))
        {
            html.AppendLine($"<p class=\"linea\">{Html(configuracion.Correo)}</p>");
        }

        html.AppendLine("</div>");
        html.AppendLine("</header>");

        return html.ToString();
    }

    private static string Logo(byte[] logo, string iniciales)
    {
        if (logo == null || logo.Length == 0)
        {
            return $"<div class=\"logo-placeholder\">{Html(iniciales)}</div>";
        }

        string mime = DetectarMime(logo);

        return "<img src=\"data:" + mime + ";base64," +
               Convert.ToBase64String(logo) +
               "\" alt=\"Logo de la empresa\" />";
    }

    private static string DetectarMime(byte[] bytes)
    {
        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 &&
            bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            return "image/png";
        }

        if (bytes.Length >= 3 &&
            bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (bytes.Length >= 6 &&
            bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
        {
            return "image/gif";
        }

        if (bytes.Length >= 2 &&
            bytes[0] == 0x42 && bytes[1] == 0x4D)
        {
            return "image/bmp";
        }

        return "image/png";
    }

    // ============================================================
    // FORMATO
    // ============================================================

    private static KeyValuePair<string, string> Dato(string etiqueta, string valor)
    {
        return new KeyValuePair<string, string>(etiqueta, valor);
    }

    private static string Dinero(decimal valor, int decimales = 2)
    {
        return "$ " + valor.ToString("N" + decimales, Cultura);
    }

    private static string Numero(decimal valor, int decimales)
    {
        return valor.ToString("N" + decimales, Cultura);
    }

    private static string Fecha(DateTime valor)
    {
        return valor == default
            ? "—"
            : valor.ToString("dd/MM/yyyy", Cultura);
    }

    private static string FechaHora(DateTime valor)
    {
        return valor.ToString("dd/MM/yyyy HH:mm", Cultura);
    }

    private static string Moneda(Moneda moneda)
    {
        if (moneda == null)
            return "—";

        string nombre = Texto(moneda.Nombre, "—");

        return moneda.TasaCambio > 0 && moneda.TasaCambio != 1m
            ? $"{nombre} (tasa {moneda.TasaCambio.ToString("N4", Cultura)})"
            : nombre;
    }

    private static string Comentario(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return "<span class=\"vacio\">Sin comentarios.</span>";
        }

        return Html(valor)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", "<br />");
    }

    private static string Texto(string valor, string defecto = "—")
    {
        return string.IsNullOrWhiteSpace(valor) ? defecto : valor.Trim();
    }

    private static string Html(string valor)
    {
        return WebUtility.HtmlEncode(valor ?? string.Empty);
    }

    private static string Iniciales(string texto)
    {
        string[] partes = (texto ?? string.Empty)
            .Split(new[] { ' ', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return "?";

        if (partes.Length == 1)
        {
            return partes[0]
                .Substring(0, Math.Min(2, partes[0].Length))
                .ToUpperInvariant();
        }

        return (partes[0][0].ToString() + partes[1][0].ToString())
            .ToUpperInvariant();
    }

    // ============================================================
    // ESTILOS
    // ============================================================

    private const string Estilos = @"
<style>
    :root {
        --azul: #14345c;
        --azul-claro: #e8eef7;
        --gris: #5b6b7f;
        --borde: #d7dee8;
        --verde: #0f7b4f;
        --rojo: #b42318;
        --ambar: #a15c00;
    }

    * { box-sizing: border-box; }

    html, body { margin: 0; padding: 0; }

    body {
        font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        font-size: 11.5px;
        line-height: 1.35;
        color: #1f2937;
        background: #eef1f5;
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
    }

    .barra {
        position: sticky;
        top: 0;
        z-index: 10;
        display: flex;
        justify-content: flex-end;
        gap: 8px;
        padding: 10px 18px;
        background: #0f1f33;
    }

    .barra .btn {
        border: 0;
        border-radius: 6px;
        padding: 9px 16px;
        font-size: 12px;
        font-weight: 600;
        font-family: inherit;
        cursor: pointer;
        background: #e8eef7;
        color: #14345c;
    }

    .barra .btn:hover { filter: brightness(0.95); }

    .barra .btn.primary {
        background: #1d6fe0;
        color: #fff;
    }

    .hoja {
        width: 210mm;
        min-height: 297mm;
        margin: 18px auto 40px;
        padding: 14mm 15mm;
        background: #fff;
        box-shadow: 0 12px 32px rgba(15, 23, 42, 0.16);
    }

    /* ---------------- Encabezado ---------------- */

    .encabezado {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 18px;
        border-bottom: 3px solid var(--azul);
        padding-bottom: 12px;
    }

    .logo img {
        max-width: 175px;
        max-height: 82px;
        object-fit: contain;
        display: block;
    }

    .logo-placeholder {
        width: 78px;
        height: 78px;
        border-radius: 14px;
        background: var(--azul);
        color: #fff;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 26px;
        font-weight: 700;
        letter-spacing: 1px;
    }

    .empresa {
        flex: 1;
        text-align: right;
    }

    .empresa h1 {
        margin: 0;
        font-size: 19px;
        line-height: 1.15;
        color: var(--azul);
        letter-spacing: 0.3px;
        text-transform: uppercase;
    }

    .empresa .slogan {
        margin: 3px 0 6px;
        color: var(--gris);
        font-style: italic;
    }

    .empresa .linea {
        margin: 1px 0;
        color: #334155;
        font-size: 11px;
    }

    /* ---------------- Título ---------------- */

    .titulo {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        margin: 16px 0 4px;
        padding: 10px 14px;
        background: var(--azul-claro);
        border-left: 5px solid var(--azul);
    }

    .titulo .etiqueta {
        display: block;
        font-size: 9.5px;
        text-transform: uppercase;
        letter-spacing: 1.2px;
        color: var(--gris);
    }

    .titulo h2 {
        margin: 1px 0 0;
        font-size: 16px;
        color: var(--azul);
        text-transform: uppercase;
        letter-spacing: 0.4px;
    }

    .folio { text-align: right; }

    .folio .rotulo {
        display: block;
        font-size: 9.5px;
        text-transform: uppercase;
        letter-spacing: 1px;
        color: var(--gris);
    }

    .folio .num {
        display: block;
        font-size: 20px;
        font-weight: 700;
        color: var(--azul);
        line-height: 1.1;
        margin-bottom: 3px;
    }

    .estado {
        display: inline-block;
        padding: 2px 10px;
        border-radius: 999px;
        font-size: 9.5px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.6px;
    }

    .estado.terminada { background: #e6f4ec; color: var(--verde); border: 1px solid #b7e1c9; }
    .estado.cancelada { background: #fdecec; color: var(--rojo); border: 1px solid #f5c2c0; }
    .estado.proceso { background: #fff4e0; color: var(--ambar); border: 1px solid #f3d9a4; }

    /* ---------------- Secciones y tablas ---------------- */

    h3 {
        font-size: 11.5px;
        text-transform: uppercase;
        letter-spacing: 0.8px;
        color: var(--azul);
        margin: 18px 0 6px;
        padding-bottom: 3px;
        border-bottom: 1px solid var(--borde);
    }

    table { width: 100%; border-collapse: collapse; }

    .datos td {
        padding: 5px 8px;
        border-bottom: 1px solid #eef2f7;
        vertical-align: top;
    }

    .datos td.lbl {
        width: 16%;
        color: var(--gris);
        font-weight: 600;
        white-space: nowrap;
    }

    .datos td.val {
        width: 34%;
        color: #111827;
        font-weight: 600;
    }

    .importes {
        width: 62%;
        margin-left: auto;
    }

    .importes td {
        padding: 5px 8px;
        border-bottom: 1px solid #eef2f7;
    }

    .importes td.lbl { color: var(--gris); }

    .importes td.num {
        text-align: right;
        white-space: nowrap;
        font-variant-numeric: tabular-nums;
    }

    .importes tr.total td {
        background: var(--azul);
        color: #fff;
        font-weight: 700;
        font-size: 13px;
        border: none;
    }

    /* ---------------- Detalle ---------------- */

    .detalle thead th {
        background: var(--azul);
        color: #fff;
        text-align: left;
        padding: 7px 8px;
        font-size: 10.5px;
        text-transform: uppercase;
        letter-spacing: 0.4px;
    }

    .detalle tbody td {
        padding: 6px 8px;
        border-bottom: 1px solid #e8edf4;
        vertical-align: top;
    }

    .detalle tbody tr:nth-child(even) { background: #f7f9fc; }

    .detalle .col-num,
    .detalle .col-um {
        text-align: right;
        white-space: nowrap;
        font-variant-numeric: tabular-nums;
    }

    .detalle thead .col-num,
    .detalle thead .col-um { text-align: right; }

    .detalle tfoot td {
        padding: 7px 8px;
        font-weight: 700;
        background: var(--azul-claro);
        border-top: 2px solid var(--azul);
    }

    .detalle tfoot td:first-child {
        text-transform: uppercase;
        letter-spacing: 0.5px;
        font-size: 10.5px;
        color: var(--azul);
    }

    .sin-datos {
        padding: 12px 8px;
        color: var(--gris);
        font-style: italic;
        text-align: center;
    }

    /* ---------------- Comentario ---------------- */

    .comentario .caja {
        min-height: 42px;
        padding: 10px 12px;
        border: 1px solid var(--borde);
        border-left: 4px solid var(--azul);
        background: #fbfcfe;
        line-height: 1.5;
    }

    .comentario .vacio {
        color: #94a3b8;
        font-style: italic;
    }

    /* ---------------- Firmas y pie ---------------- */

    .firmas {
        display: flex;
        gap: 26px;
        margin-top: 36px;
    }

    .firmas div {
        flex: 1;
        border-top: 1px solid #94a3b8;
        padding-top: 5px;
        text-align: center;
        color: var(--gris);
        font-size: 9.5px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    .pie {
        margin-top: 22px;
        padding-top: 6px;
        border-top: 1px solid var(--borde);
        color: #94a3b8;
        font-size: 9.5px;
        display: flex;
        justify-content: space-between;
        gap: 12px;
    }

    /* ---------------- Impresión ---------------- */

    @page {
        size: A4;
        margin: 12mm;
    }

    @media print {
        body { background: #fff; }

        .no-print { display: none !important; }

        .hoja {
            width: auto;
            min-height: 0;
            margin: 0;
            padding: 0;
            box-shadow: none;
        }

        h3 { page-break-after: avoid; }

        .detalle tr { page-break-inside: avoid; }

        thead { display: table-header-group; }

        .firmas { page-break-inside: avoid; }
    }
</style>
";
}
