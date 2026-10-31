using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace DXComnTransp.Module.Reports;

// ============================================================
// REPORTE: COMPRA DE COMBUSTIBLE  (XAF 22.1 + EFCore)
//
// Diseñado 100% en código (no requiere .resx designer).
// Uso:
//     var r = new CompraCombustibleReport();
//     r.SetEmpresa(cfg.Nombre, cfg.Nit, cfg.Slogan,
//                  cfg.Telefonos, cfg.Correo, cfg.Logo);
//     r.DataSource = new[] { compra };
//     // Mostrar preview...
// ============================================================
[DisplayName("Compra de Combustible")]
public class CompraCombustibleReport : XtraReport
{
    // ---------- Controles referenciados dinámicamente ----------
    private XRPictureBox picLogo;
    private XRLabel lblEmpresa;
    private XRLabel lblSlogan;
    private XRLabel lblNit;
    private XRLabel lblContacto;

    // ---------- Estilos ----------
    private readonly XRControlStyle styleHeader;
    private readonly XRControlStyle styleCell;
    private readonly XRControlStyle styleCellRight;
    private readonly XRControlStyle styleLabel;
    private readonly XRControlStyle styleTotal;

    public CompraCombustibleReport()
    {
        // ========================================================
        // CONFIGURACIÓN DE PÁGINA
        // ========================================================
        PaperKind = PaperKind.Letter;
        Margins = new Margins(40, 40, 40, 40);
        Font = new Font("Segoe UI", 9F);

        // ========================================================
        // ESTILOS
        // ========================================================
        styleHeader = new XRControlStyle
        {
            Name = "styleHeader",
            BackColor = Color.FromArgb(44, 62, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            Padding = new PaddingInfo(4, 4, 2, 2),
            BorderColor = Color.FromArgb(44, 62, 80),
            Borders = BorderSide.All
        };
        styleCell = new XRControlStyle
        {
            Name = "styleCell",
            Font = new Font("Segoe UI", 9F),
            Padding = new PaddingInfo(4, 4, 2, 2),
            BorderColor = Color.FromArgb(220, 220, 220),
            Borders = BorderSide.All
        };
        styleCellRight = new XRControlStyle
        {
            Name = "styleCellRight",
            Font = new Font("Segoe UI", 9F),
            Padding = new PaddingInfo(4, 4, 2, 2),
            BorderColor = Color.FromArgb(220, 220, 220),
            Borders = BorderSide.All,
            TextAlignment = TextAlignment.MiddleRight
        };
        styleLabel = new XRControlStyle
        {
            Name = "styleLabel",
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(85, 85, 85)
        };
        styleTotal = new XRControlStyle
        {
            Name = "styleTotal",
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            TextAlignment = TextAlignment.MiddleRight
        };
        StyleSheet.AddRange(new[]
        {
            styleHeader, styleCell, styleCellRight, styleLabel, styleTotal
        });

        // ========================================================
        // BANDAS
        //
        // IMPORTANTE: DetailReportBand (sub-reporte de Detalles)
        // debe ir como banda del REPORTE, NO dentro de Controls
        // de otro band. XtraReports lo renderiza automáticamente
        // debajo del master gracias a DataMember="Detalles".
        // ========================================================
        Bands.AddRange(new Band[]
        {
            BuildTopMargin(),
            BuildReportHeader(),
            BuildDetailBand(),
            BuildDetailReportBand(),
            BuildReportFooter(),
            BuildPageFooter(),
            BuildBottomMargin()
        });
    }

    // ============================================================
    // SETEAR DATOS DE EMPRESA (desde el Controller)
    // ============================================================
    public void SetEmpresa(
        string nombre,
        string nit,
        string slogan,
        string telefonos,
        string correo,
        byte[] logo)
    {
        lblEmpresa.Text = nombre ?? "";
        lblSlogan.Text = slogan ?? "";
        lblNit.Text = string.IsNullOrEmpty(nit) ? "" : "NIT: " + nit;

        string contacto = "";
        if (!string.IsNullOrEmpty(telefonos)) contacto = "Tel: " + telefonos;
        if (!string.IsNullOrEmpty(correo))
            contacto += (contacto.Length > 0 ? "   ·   " : "") + correo;
        lblContacto.Text = contacto;

        if (logo != null && logo.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(logo);
                picLogo.Image = Image.FromStream(ms);
            }
            catch { /* logo inválido, se omite */ }
        }
    }

    // ============================================================
    // MARGENES
    // ============================================================
    private TopMarginBand BuildTopMargin() => new TopMarginBand { HeightF = 20F };
    private BottomMarginBand BuildBottomMargin() => new BottomMarginBand { HeightF = 20F };

    // ============================================================
    // REPORT HEADER - DATOS DE LA EMPRESA + TÍTULO
    // ============================================================
    private ReportHeaderBand BuildReportHeader()
    {
        var band = new ReportHeaderBand { HeightF = 115F };

        picLogo = new XRPictureBox
        {
            Name = "picLogo",
            BoundsF = new RectangleF(0, 0, 90, 90),
            Sizing = ImageSizeMode.ZoomImage
        };

        lblEmpresa = new XRLabel
        {
            Name = "lblEmpresa",
            BoundsF = new RectangleF(100, 0, 520, 24),
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80)
        };

        lblSlogan = new XRLabel
        {
            Name = "lblSlogan",
            BoundsF = new RectangleF(100, 26, 520, 16),
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 120, 120)
        };

        lblNit = new XRLabel
        {
            Name = "lblNit",
            BoundsF = new RectangleF(100, 46, 520, 14),
            Font = new Font("Segoe UI", 9F)
        };

        lblContacto = new XRLabel
        {
            Name = "lblContacto",
            BoundsF = new RectangleF(100, 60, 520, 14),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(85, 85, 85)
        };

        var lineaSep = new XRLine
        {
            Name = "lineaSep",
            BoundsF = new RectangleF(0, 90, 620, 2),
            LineWidth = 1.5F,
            ForeColor = Color.FromArgb(44, 62, 80)
        };

        var lblTitulo = new XRLabel
        {
            Name = "lblTitulo",
            BoundsF = new RectangleF(0, 94, 620, 20),
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            TextAlignment = TextAlignment.MiddleCenter,
            Text = "COMPRA DE COMBUSTIBLE"
        };

        band.Controls.AddRange(new XRControl[]
        {
            picLogo, lblEmpresa, lblSlogan, lblNit, lblContacto, lineaSep, lblTitulo
        });
        return band;
    }

    // ============================================================
    // DETAIL - UNA PASADA POR COMPRA
    // ============================================================
    private DetailBand BuildDetailBand()
    {
        var band = new DetailBand();

        float y = 0F;

        // -------- RESUMEN DE LA COMPRA --------
        band.Controls.AddRange(BuildResumenRow(
            "Consecutivo:", "[Consecutivo]",
            "No. Factura:", "[NoFactura]", y));
        y += 20F;

        band.Controls.AddRange(BuildResumenRow(
            "Fecha:", "[Fecha]",
            "Fecha Factura:", "[FechaFactura]", y,
            fmt1: "{0:dd/MM/yyyy}", fmt2: "{0:dd/MM/yyyy}"));
        y += 20F;

        band.Controls.AddRange(BuildResumenRow(
            "Proveedor:", "[Proveedor.Nombre]",
            "Moneda:", "[Moneda.Nombre]", y));
        y += 20F;

        band.Controls.AddRange(BuildResumenRow(
            "Estado:",
            "Iif([Cancelada], 'CANCELADA', Iif([Terminada], 'TERMINADA', 'PENDIENTE'))",
            "Fecha Estado:",
            "Iif([Cancelada], [FechaCancelada], Iif([Terminada], [FechaTerminada], ?))",
            y, fmt2: "{0:dd/MM/yyyy HH:mm}"));
        y += 30F;

        // -------- CABECERA DE LA TABLA DE DETALLES --------
        var header = CreateHeaderTable(y);
        band.Controls.Add(header);
        y += 18F;

        band.HeightF = y + 2F;

        
        return band;
    }

    private DetailReportBand BuildDetailReportBand()
    {
        var detailReport = new DetailReportBand
        {
            Name = "DetailReportDetalles",
            DataMember = "Detalles",
            Level = 0
        };

        var detalleBand = new DetailBand
        {
            HeightF = 18F,
            Name = "DetalleCompraDetalle"
        };

        detalleBand.Controls.Add(CreateDetalleRow());
        detailReport.Bands.Add(detalleBand);

        var footerDetalles = new ReportFooterBand
        {
            HeightF = 24F,
            Name = "FooterDetalles"
        };

        footerDetalles.Controls.AddRange(BuildDetallesFooter());
        detailReport.Bands.Add(footerDetalles);

        return detailReport;
    }

    // ============================================================
    // HELPERS: FILA RESUMEN
    // ============================================================
    private XRControl[] BuildResumenRow(
        string lbl1, string valExpr1,
        string lbl2, string valExpr2,
        float y,
        string fmt1 = null, string fmt2 = null)
    {
        var l1 = new XRLabel { BoundsF = new RectangleF(0F, y, 90F, 18F), StyleName = "styleLabel", Text = lbl1 };
        var v1 = new XRLabel { BoundsF = new RectangleF(90F, y, 230F, 18F) };
        if (!string.IsNullOrEmpty(fmt1)) v1.TextFormatString = fmt1;
        v1.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", valExpr1));

        var l2 = new XRLabel { BoundsF = new RectangleF(330F, y, 90F, 18F), StyleName = "styleLabel", Text = lbl2 };
        var v2 = new XRLabel { BoundsF = new RectangleF(420F, y, 200F, 18F) };
        if (!string.IsNullOrEmpty(fmt2)) v2.TextFormatString = fmt2;
        v2.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", valExpr2));
        return new XRControl[] { l1, v1, l2, v2 };
    }

    // ============================================================
    // CABECERA DE LA TABLA DE DETALLES
    // ============================================================
    private XRTable CreateHeaderTable(float y)
    {
        var table = new XRTable
        {
            BoundsF = new RectangleF(0F, y, 620F, 18F)
        };
        var row = new XRTableRow();
        AddCell(row, "Combustible", 240F, styleHeader);
        AddCell(row, "U/M", 80F, styleHeader);
        AddCell(row, "Cantidad", 100F, styleHeader);
        AddCell(row, "Precio", 100F, styleHeader);
        AddCell(row, "Importe", 100F, styleHeader);
        table.Rows.Add(row);
        return table;
    }

    // ============================================================
    // FILA DE DETALLE
    // ============================================================
    private XRTable CreateDetalleRow()
    {
        var table = new XRTable
        {
            BoundsF = new RectangleF(0F, 0F, 620F, 18F)
        };
        var row = new XRTableRow();

        AddExpressionCell(row, "[Producto.Nombre]", 240F, styleCell);
        AddExpressionCell(row, "[Medida.Nombre]", 80F, styleCell);
        AddExpressionCell(row, "[Cantidad]", 100F, styleCellRight, "{0:n2}");
        AddExpressionCell(row, "[Precio]", 100F, styleCellRight, "{0:$ #,##0.0000}");
        AddExpressionCell(row, "[Importe]", 100F, styleCellRight, "{0:$ #,##0.00}");

        table.Rows.Add(row);
        return table;
    }

    // ============================================================
    // FOOTER DEL SUB-REPORTE DE DETALLES
    // ============================================================
    private XRControl[] BuildDetallesFooter()
    {
        var lblCantidad = new XRLabel
        {
            BoundsF = new RectangleF(320F, 0F, 100F, 20F),
            StyleName = "styleTotal",
            TextFormatString = "{0:n2}",
            Summary = new XRSummary
            {
                Running = SummaryRunning.Report,
                Func = SummaryFunc.Sum,
                IgnoreNullValues = true
            }
        };
        lblCantidad.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Cantidad]"));

        var lblSub = new XRLabel
        {
            BoundsF = new RectangleF(420F, 0F, 100F, 20F),
            StyleName = "styleTotal",
            Text = "Subtotal:"
        };

        var lblSubVal = new XRLabel
        {
            BoundsF = new RectangleF(520F, 0F, 100F, 20F),
            StyleName = "styleTotal",
            TextFormatString = "{0:$ #,##0.00}",
            Summary = new XRSummary
            {
                Running = SummaryRunning.Report,
                Func = SummaryFunc.Sum,
                IgnoreNullValues = true
            }
        };
        lblSubVal.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Importe]"));

        return new XRControl[] { lblCantidad, lblSub, lblSubVal };
    }

    // ============================================================
    // REPORT FOOTER - TOTALES + COMENTARIO
    // ============================================================
    private ReportFooterBand BuildReportFooter()
    {
        var band = new ReportFooterBand { HeightF = 180F };

        AddTotalRow(band, 0F, "Subtotal productos:", "[ImporteProductos]", "{0:$ #,##0.00}");
        AddTotalRow(band, 22F, "Flete:", "[Flete]", "{0:$ #,##0.00}");
        AddTotalRow(band, 44F, "THCD:", "[THCD]", "{0:$ #,##0.00}");
        AddTotalRow(band, 66F, "Seguro:", "[Seguro]", "{0:$ #,##0.00}");

        var linTot = new XRLine
        {
            BoundsF = new RectangleF(320F, 92F, 300F, 2F),
            LineWidth = 1.5F,
            ForeColor = Color.FromArgb(44, 62, 80)
        };
        band.Controls.Add(linTot);

        var lblTotalLbl = new XRLabel
        {
            BoundsF = new RectangleF(320F, 96F, 180F, 24F),
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            TextAlignment = TextAlignment.MiddleRight,
            Text = "TOTAL:"
        };
        var lblTotalVal = new XRLabel
        {
            BoundsF = new RectangleF(500F, 96F, 120F, 24F),
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            TextAlignment = TextAlignment.MiddleRight,
            TextFormatString = "{0:$ #,##0.00}"
        };
        lblTotalVal.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ImporteTotal]"));
        band.Controls.AddRange(new XRControl[] { lblTotalLbl, lblTotalVal });

        // Nota / Comentario
        var lblNotaTitulo = new XRLabel
        {
            BoundsF = new RectangleF(0F, 130F, 620F, 16F),
            StyleName = "styleLabel",
            Text = "Nota / Comentario"
        };
        var sepNota = new XRLine
        {
            BoundsF = new RectangleF(0F, 146F, 620F, 1F),
            ForeColor = Color.FromArgb(200, 200, 200)
        };
        var lblNotaVal = new XRLabel
        {
            BoundsF = new RectangleF(0F, 150F, 620F, 30F),
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.FromArgb(60, 60, 60),
            CanGrow = true,
            Multiline = true
        };
        lblNotaVal.ExpressionBindings.Add(new ExpressionBinding(
            "BeforePrint",
            "Text",
            "Iif(IsNullOrEmpty([Comentario]), '— Sin comentarios —', [Comentario])"));
        band.Controls.AddRange(new XRControl[] { lblNotaTitulo, sepNota, lblNotaVal });

        return band;
    }

    private void AddTotalRow(Band band, float y, string label, string valueExpr, string fmt)
    {
        var lbl = new XRLabel
        {
            BoundsF = new RectangleF(320F, y, 180F, 20F),
            StyleName = "styleTotal",
            Text = label
        };
        var val = new XRLabel
        {
            BoundsF = new RectangleF(500F, y, 120F, 20F),
            StyleName = "styleTotal",
            TextFormatString = fmt
        };
        val.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", valueExpr));
        band.Controls.AddRange(new XRControl[] { lbl, val });
    }

    // ============================================================
    // PAGE FOOTER
    // ============================================================
    private PageFooterBand BuildPageFooter()
    {
        var band = new PageFooterBand { HeightF = 24F };

        var sep = new XRLine
        {
            BoundsF = new RectangleF(0F, 0F, 620F, 1F),
            ForeColor = Color.FromArgb(200, 200, 200)
        };
        var lblFecha = new XRLabel
        {
            BoundsF = new RectangleF(0F, 4F, 310F, 18F),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray
        };
        lblFecha.ExpressionBindings.Add(new ExpressionBinding(
            "BeforePrint",
            "Text",
            "Concat('Impreso: ', FormatString('{0:dd/MM/yyyy HH:mm}', Now()))"));

        var lblPag = new XRPageInfo
        {
            BoundsF = new RectangleF(310F, 4F, 310F, 18F),
            PageInfo = PageInfo.NumberOfTotal,
            Format = "Página {0} de {1}",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray,
            TextAlignment = TextAlignment.MiddleRight
        };

        band.Controls.AddRange(new XRControl[] { sep, lblFecha, lblPag });
        return band;
    }

    // ============================================================
    // HELPERS DE CELDA
    // ============================================================
    private static void AddCell(XRTableRow row, string text, float width, XRControlStyle style)
    {
        var cell = new XRTableCell
        {
            WidthF = width,
            StyleName = style.Name,
            Text = text
        };
        row.Cells.Add(cell);
    }

    private static void AddExpressionCell(
        XRTableRow row,
        string expression,
        float width,
        XRControlStyle style,
        string fmt = null)
    {
        var cell = new XRTableCell
        {
            WidthF = width,
            StyleName = style.Name
        };
        if (!string.IsNullOrEmpty(fmt))
            cell.TextFormatString = fmt;
        cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expression));
        row.Cells.Add(cell);
    }
}
