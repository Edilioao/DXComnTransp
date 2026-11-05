using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DXComnTransp.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;

namespace DXComnTransp.Module.Reports;

/// <summary>
/// Reporte DevExpress (XtraReport) de DISPONIBILIDAD DE COMBUSTIBLE
/// por tipo de combustible (toda la empresa, sin desglose por depot):
///
///     Comprado  = compras NO canceladas
///     Recibido  = entradas terminadas NO canceladas
///     Disponible= Comprado - Recibido
///
/// Misma estética del reporte de compra y mismo diseño "a prueba de
/// errores": NO usa entidades EF como DataSource; recibe DTO planos.
/// </summary>
[DisplayName("Disponibilidad de Combustible")]
public class ReporteDisponibilidadCombustible : XtraReport
{
    private static readonly CultureInfo Cultura =
        CultureInfo.GetCultureInfo("es-MX");

    private static readonly Color Azul = Color.FromArgb(20, 52, 92);
    private static readonly Color AzulClaro = Color.FromArgb(232, 238, 247);
    private static readonly Color GrisTexto = Color.FromArgb(85, 85, 85);
    private static readonly Color GrisBorde = Color.FromArgb(210, 216, 224);

    private static readonly Font FuenteBase = new Font("Segoe UI", 9F);
    private static readonly Font FuenteNegrita = new Font("Segoe UI", 9F, FontStyle.Bold);
    private static readonly Font FuenteTitulo = new Font("Segoe UI", 14F, FontStyle.Bold);

    private const float AnchoUtil = 1020F;

    // ---------- DTO plano ----------
    public class FilaDisponibilidad
    {
        public string Combustible { get; set; }
        public decimal Comprado { get; set; }
        public decimal Recibido { get; set; }
        public decimal Disponible { get; set; }
    }

    private ReporteDisponibilidadCombustible()
    {
        PaperKind = PaperKind.Letter;
        Landscape = true;
        Margins = new Margins(40, 40, 40, 40);
        Font = FuenteBase;
    }

    // ============================================================
    // FÁBRICA
    // ============================================================
    public static ReporteDisponibilidadCombustible Crear(
        IList<FilaDisponibilidad> filas,
        Configuracion configuracion)
    {
        if (filas == null)
            throw new ArgumentNullException(nameof(filas));

        var report = new ReporteDisponibilidadCombustible
        {
            DataSource = filas
        };
        report.Cargar(filas, configuracion);
        return report;
    }

    private string EmpresaActual { get; set; }

    private void Cargar(IList<FilaDisponibilidad> filas, Configuracion cfg)
    {
        string empresa = string.IsNullOrWhiteSpace(cfg?.Nombre)
            ? "Empresa"
            : cfg.Nombre.Trim();
        EmpresaActual = empresa;

        decimal totalComprado = filas.Sum(f => f.Comprado);
        decimal totalRecibido = filas.Sum(f => f.Recibido);
        decimal totalDisponible = filas.Sum(f => f.Disponible);

        Bands.AddRange(new Band[]
        {
            new TopMarginBand { HeightF = 20F },
            ConstruirEncabezado(empresa, cfg, filas.Count),
            ConstruirCabeceraTabla(),
            ConstruirDetalle(),
            ConstruirPie(totalComprado, totalRecibido, totalDisponible, filas.Count == 0),
            ConstruirPiePagina(empresa),
            new BottomMarginBand { HeightF = 20F }
        });
    }

    // ============================================================
    // REPORT HEADER
    // ============================================================
    private ReportHeaderBand ConstruirEncabezado(
        string empresa,
        Configuracion cfg,
        int cantidadFilas)
    {
        var band = new ReportHeaderBand();
        var controles = new List<XRControl>();

        bool hayLogo = cfg?.Logo != null && cfg.Logo.Length > 0;
        if (hayLogo)
        {
            var pic = new XRPictureBox
            {
                BoundsF = new RectangleF(0, 0, 100, 100),
                Sizing = ImageSizeMode.ZoomImage
            };
            try
            {
                var ms = new System.IO.MemoryStream(cfg.Logo);
                pic.Image = Image.FromStream(ms);
            }
            catch
            {
                pic.Image = null;
            }
            controles.Add(pic);
        }
        else
        {
            string iniciales = Iniciales(empresa);
            controles.Add(new XRLabel
            {
                BoundsF = new RectangleF(0, 0, 100, 100),
                Text = iniciales,
                BackColor = AzulClaro,
                ForeColor = Azul,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                TextAlignment = TextAlignment.MiddleCenter
            });
        }

        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(115, 0, 600, 28),
            Text = empresa,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleLeft
        });

        var info = new List<string>();
        if (!string.IsNullOrWhiteSpace(cfg?.Slogan)) info.Add(cfg.Slogan.Trim());
        if (!string.IsNullOrWhiteSpace(cfg?.Nit)) info.Add("NIT: " + cfg.Nit.Trim());
        if (!string.IsNullOrWhiteSpace(cfg?.Reeup)) info.Add("REEUP: " + cfg.Reeup.Trim());
        if (!string.IsNullOrWhiteSpace(cfg?.Telefonos)) info.Add("Teléfonos: " + cfg.Telefonos.Trim());
        if (!string.IsNullOrWhiteSpace(cfg?.Correo)) info.Add(cfg.Correo.Trim());

        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(115, 30, 600, 68),
            Text = string.Join(Environment.NewLine, info),
            Font = FuenteBase,
            ForeColor = GrisTexto,
            Multiline = true,
            CanGrow = true,
            TextAlignment = TextAlignment.MiddleLeft
        });

        controles.Add(new XRLine
        {
            BoundsF = new RectangleF(0, 104, AnchoUtil, 2),
            LineWidth = 1.5F,
            ForeColor = Azul
        });

        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, 110, 600, 14),
            Text = "Reporte gerencial",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 120, 120)
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, 124, 600, 26),
            Text = "Disponibilidad de combustible",
            Font = FuenteTitulo,
            ForeColor = Azul
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(620, 110, 400, 40),
            Text = "Comprado (compras no canceladas)\n" +
                   "Recibido (entradas terminadas no canceladas)\n" +
                   "Disponible = Comprado - Recibido",
            Font = new Font("Segoe UI", 8F),
            ForeColor = GrisTexto,
            Multiline = true,
            TextAlignment = TextAlignment.MiddleRight
        });

        float y = 162F;

        controles.Add(SeccionTitulo("Por tipo de combustible", y));
        y += 22F;

        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, y, 400, 16),
            Text = "Tipos de combustible en el reporte: " +
                   cantidadFilas.ToString("N0", Cultura),
            Font = FuenteBase,
            ForeColor = GrisTexto
        });

        band.HeightF = y + 20F;
        band.Controls.AddRange(controles.ToArray());
        return band;
    }

    // ============================================================
    // PAGE HEADER: CABECERA DE LA TABLA
    // ============================================================
    private PageHeaderBand ConstruirCabeceraTabla()
    {
        var band = new PageHeaderBand { HeightF = 22F };

        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, 0, AnchoUtil, 22)
        };
        var fila = new XRTableRow();

        CeldaFija(fila, "Combustible", 420F, derecho: false, cabecera: true);
        CeldaFija(fila, "Comprado", 200F, derecho: true, cabecera: true);
        CeldaFija(fila, "Recibido", 200F, derecho: true, cabecera: true);
        CeldaFija(fila, "Disponible", 200F, derecho: true, cabecera: true);

        tabla.Rows.Add(fila);
        band.Controls.Add(tabla);
        return band;
    }

    // ============================================================
    // DETAIL: UNA FILA POR TIPO DE COMBUSTIBLE
    // ============================================================
    private DetailBand ConstruirDetalle()
    {
        var band = new DetailBand { HeightF = 20F };

        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, 0, AnchoUtil, 20)
        };
        var fila = new XRTableRow();

        CeldaExpresion(fila, "[Combustible]", 420F, null, derecho: false);
        CeldaExpresion(fila, "[Comprado]", 200F, "{0:N2}", derecho: true);
        CeldaExpresion(fila, "[Recibido]", 200F, "{0:N2}", derecho: true);
        CeldaExpresion(fila, "[Disponible]", 200F, "{0:N2}", derecho: true);

        tabla.Rows.Add(fila);
        band.Controls.Add(tabla);
        return band;
    }

    // ============================================================
    // REPORT FOOTER: TOTALES
    // ============================================================
    private ReportFooterBand ConstruirPie(
        decimal totalComprado,
        decimal totalRecibido,
        decimal totalDisponible,
        bool sinFilas)
    {
        var band = new ReportFooterBand();
        var controles = new List<XRControl>();

        float y = 2F;

        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 22)
        };
        var fila = new XRTableRow();

        fila.Cells.Add(new XRTableCell
        {
            WidthF = 420F,
            Text = "Totales",
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        });
        fila.Cells.Add(new XRTableCell
        {
            WidthF = 200F,
            Text = totalComprado.ToString("N2", Cultura),
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        });
        fila.Cells.Add(new XRTableCell
        {
            WidthF = 200F,
            Text = totalRecibido.ToString("N2", Cultura),
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        });
        fila.Cells.Add(new XRTableCell
        {
            WidthF = 200F,
            Text = totalDisponible.ToString("N2", Cultura),
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        });

        tabla.Rows.Add(fila);
        controles.Add(tabla);
        y += 30F;

        if (sinFilas)
        {
            controles.Add(new XRLabel
            {
                BoundsF = new RectangleF(0, y, AnchoUtil, 18),
                Text = "No hay datos de compras ni de entradas para " +
                       "calcular la disponibilidad.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = GrisTexto,
                TextAlignment = TextAlignment.MiddleCenter
            });
            y += 22F;
        }

        // Nota de alcance: cantidades en la unidad de medida original de
        // cada documento (no homogeneizada).
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 32),
            Text = "Nota: las cantidades corresponden a la unidad de " +
                   "medida registrado en cada documento. Este reporte " +
                   "refleja el corte al momento de generarlo.",
            Font = new Font("Segoe UI", 8F, FontStyle.Italic),
            ForeColor = GrisTexto,
            Multiline = true,
            CanGrow = true
        });
        y += 40F;

        band.HeightF = y;
        band.Controls.AddRange(controles.ToArray());
        return band;
    }

    // ============================================================
    // PAGE FOOTER
    // ============================================================
    private PageFooterBand ConstruirPiePagina(string empresa)
    {
        var band = new PageFooterBand { HeightF = 22F };

        band.Controls.Add(new XRLine
        {
            BoundsF = new RectangleF(0, 0, AnchoUtil, 1),
            ForeColor = Color.FromArgb(200, 200, 200)
        });

        band.Controls.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, 3, 400, 16),
            Text = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", Cultura),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray
        });

        band.Controls.Add(new XRLabel
        {
            BoundsF = new RectangleF(400, 3, 350, 16),
            Text = empresa + " · Documento interno",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray,
            TextAlignment = TextAlignment.MiddleCenter
        });

        band.Controls.Add(new XRPageInfo
        {
            BoundsF = new RectangleF(750, 3, 270, 16),
            PageInfo = PageInfo.NumberOfTotal,
            Format = "Página {0} de {1}",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray,
            TextAlignment = TextAlignment.MiddleRight
        });

        return band;
    }

    // ============================================================
    // HELPERS
    // ============================================================
    private static XRLabel SeccionTitulo(string texto, float y)
    {
        return new XRLabel
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 17),
            Text = texto,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Azul
        };
    }

    private static void CeldaFija(
        XRTableRow fila, string texto, float ancho, bool derecho, bool cabecera)
    {
        fila.Cells.Add(new XRTableCell
        {
            WidthF = ancho,
            Text = texto,
            Font = cabecera ? FuenteNegrita : FuenteBase,
            BackColor = cabecera ? Azul : Color.White,
            ForeColor = cabecera ? Color.White : Color.FromArgb(40, 40, 40),
            TextAlignment = derecho ? TextAlignment.MiddleRight : TextAlignment.MiddleLeft,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        });
    }

    private static void CeldaExpresion(
        XRTableRow fila, string expresion, float ancho, string formato, bool derecho)
    {
        var celda = new XRTableCell
        {
            WidthF = ancho,
            Font = FuenteBase,
            ForeColor = Color.FromArgb(40, 40, 40),
            TextAlignment = derecho ? TextAlignment.MiddleRight : TextAlignment.MiddleLeft,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        };

        if (!string.IsNullOrEmpty(formato))
            celda.TextFormatString = formato;

        celda.ExpressionBindings.Add(
            new ExpressionBinding("BeforePrint", "Text", expresion));

        fila.Cells.Add(celda);
    }

    private static string Iniciales(string texto)
    {
        string[] partes = (texto ?? string.Empty)
            .Split(new[] { ' ', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return "?";

        if (partes.Length == 1)
            return partes[0]
                .Substring(0, Math.Min(2, partes[0].Length))
                .ToUpperInvariant();

        return (partes[0][0].ToString() + partes[1][0].ToString())
            .ToUpperInvariant();
    }
}
