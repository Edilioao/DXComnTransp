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
/// Reporte DevExpress (XtraReport) de una compra de combustible.
/// Réplica del formato del reporte HTML (CompraCombustibleReporteHtml).
///
/// Diseño "a prueba de errores":
///  - NO usa entidades de EF Core como DataSource (evita lazy-loading y
///    problemas de contexto). Todos los datos se copian a texto/DTO
///    ANTES de renderizar.
///  - NO usa ReportDataV2 ni almacenamiento de reportes: es ad-hoc.
///  - Única fuente de datos: una lista de DTO planos con propiedades
///    simples (int/string/decimal).
///
/// Uso (desde controllers de plataforma):
///     var report = CompraCombustibleReport.Crear(compra, configuracion);
///     report.DisplayName = "...";
///     // Win:  new ReportPrintTool(report).ShowPreviewDialog();
///     // Web:  report.ExportToPdf(stream);
/// </summary>
[DisplayName("Compra de Combustible")]
public class CompraCombustibleReport : XtraReport
{
    private static readonly CultureInfo Cultura =
        CultureInfo.GetCultureInfo("es-MX");

    // Paleta (idéntica al HTML)
    private static readonly Color Azul = Color.FromArgb(20, 52, 92);        // #14345c
    private static readonly Color AzulClaro = Color.FromArgb(232, 238, 247); // #e8eef7
    private static readonly Color GrisTexto = Color.FromArgb(85, 85, 85);   // #555555
    private static readonly Color GrisBorde = Color.FromArgb(210, 216, 224);
    private static readonly Color CremaCaja = Color.FromArgb(250, 251, 253);

    private static readonly Font FuenteBase = new Font("Segoe UI", 9F);
    private static readonly Font FuenteNegrita = new Font("Segoe UI", 9F, FontStyle.Bold);
    private static readonly Font FuenteTitulo = new Font("Segoe UI", 14F, FontStyle.Bold);

    private const float AnchoUtil = 1020F; // Carta 1100 - márgenes 40+40

    // ---------- DTO plano para el detalle ----------
    public class FilaDetalle
    {
        public int Numero { get; set; }
        public string Combustible { get; set; }
        public string UnidadMedida { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Importe { get; set; }
    }

    private CompraCombustibleReport()
    {
        PaperKind = PaperKind.Letter;
        Margins = new Margins(40, 40, 40, 40);
        Font = FuenteBase;
    }

    // ============================================================
    // FÁBRICA
    // ============================================================
    public static CompraCombustibleReport Crear(
        CompraCombustible compra,
        Configuracion configuracion)
    {
        if (compra == null)
            throw new ArgumentNullException(nameof(compra));

        var report = new CompraCombustibleReport();
        report.Cargar(compra, configuracion);
        return report;
    }

    // ============================================================
    // CARGA DE DATOS Y CONSTRUCCIÓN
    //
    // Todo se materializa aquí: una vez terminado, el reporte no
    // toca entidades ni la base de datos.
    // ============================================================
    private void Cargar(CompraCombustible compra, Configuracion cfg)
    {
        // ---------- Estado ----------
        bool cancelada = compra.Cancelada;
        bool terminada = compra.Terminada && !cancelada;
        string estado = cancelada
            ? "CANCELADA"
            : terminada ? "TERMINADA" : "EN PROCESO";
        Color colorEstado = cancelada
            ? Color.FromArgb(150, 30, 40)
            : terminada ? Color.FromArgb(27, 122, 62) : Color.FromArgb(178, 108, 10);

        // ---------- Detalles (copiados a DTO) ----------
        List<CompraCombustibleDetalle> detalles =
            compra.Detalles?
                .OrderBy(d => d.Id)
                .ToList()
            ?? new List<CompraCombustibleDetalle>();

        var filas = new List<FilaDetalle>();
        int numero = 0;
        foreach (CompraCombustibleDetalle d in detalles)
        {
            numero++;
            filas.Add(new FilaDetalle
            {
                Numero = numero,
                Combustible = Texto(d.Producto?.Combustible),
                UnidadMedida = Texto(d.Medida?.UM),
                Cantidad = d.Cantidad,
                Precio = d.Precio,
                Importe = d.Importe
            });
        }

        decimal importeProductos = filas.Sum(f => f.Importe);
        decimal cantidadTotal = filas.Sum(f => f.Cantidad);
        decimal total = importeProductos + compra.Flete + compra.THCD + compra.Seguro;

        string empresa = Texto(cfg?.Nombre, "Empresa");
        string iniciales = Iniciales(empresa);

        // ========================================================
        // BANDAS
        // ========================================================
        Bands.AddRange(new Band[]
        {
            new TopMarginBand { HeightF = 20F },
            ConstruirEncabezado(compra, cfg, empresa, iniciales, estado, colorEstado,
            detalles.Count, importeProductos, total),
            ConstruirCabeceraTabla(),
            ConstruirDetalle(),
            ConstruirPie(compra, empresa, importeProductos, cantidadTotal, filas.Count == 0),
            ConstruirPiePagina(empresa),
            new BottomMarginBand { HeightF = 20F }
        });

        DataSource = filas;
    }

    // ============================================================
    // REPORT HEADER: EMPRESA + TÍTULO/FOLIO/ESTADO + DATOS + RESUMEN
    // ============================================================
    private ReportHeaderBand ConstruirEncabezado(
     CompraCombustible compra,
     Configuracion cfg,
     string empresa,
     string iniciales,
     string estado,
     Color colorEstado,
     int cantidadDetalles,      // <-- nuevo
     decimal importeProductos,  // <-- nuevo
     decimal total)             // <-- nuevo
    {
        var band = new ReportHeaderBand();
        var controles = new List<XRControl>();

        // ---------- LOGO ----------
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
                using var ms = new System.IO.MemoryStream(cfg.Logo);
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

        // ---------- EMPRESA ----------
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
        if (!string.IsNullOrWhiteSpace(cfg?.CuentaBancaria)) info.Add("Cuenta bancaria: " + cfg.CuentaBancaria.Trim());
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

        // ---------- LÍNEA ----------
        controles.Add(new XRLine
        {
            BoundsF = new RectangleF(0, 104, AnchoUtil, 2),
            LineWidth = 1.5F,
            ForeColor = Azul
        });

        // ---------- TÍTULO + FOLIO + ESTADO ----------
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, 110, 500, 14),
            Text = "Comprobante interno",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 120, 120)
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, 124, 500, 26),
            Text = "Compra de combustible",
            Font = FuenteTitulo,
            ForeColor = Azul
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(760, 110, 90, 14),
            Text = "No.",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(120, 120, 120),
            TextAlignment = TextAlignment.MiddleRight
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(855, 106, 165, 24),
            Text = compra.Consecutivo.ToString("D6", Cultura),
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight
        });
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(865, 134, 155, 20),
            Text = estado,
            BackColor = colorEstado,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlignment = TextAlignment.MiddleCenter,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 1, 1)
        });

        float y = 162F;

        // ---------- DATOS GENERALES ----------
        controles.Add(SeccionTitulo("Datos generales", y));
        y += 20F;

        var datos = new List<KeyValuePair<string, string>>
        {
            Par("Fecha de compra", Fecha(compra.Fecha)),
            Par("Fecha de factura", Fecha(compra.FechaFactura)),
            Par("No. factura", Texto(compra.NoFactura)),
            Par("Proveedor", Texto(compra.Proveedor?.Nombre)),
            Par("Moneda", Moneda(compra.Moneda)),
            Par("Cantidad de detalles", cantidadDetalles.ToString("N0", Cultura))
        };
        if (compra.FechaTerminada.HasValue)
            datos.Add(Par("Terminada el", FechaHora(compra.FechaTerminada.Value)));
        if (compra.FechaCancelada.HasValue)
            datos.Add(Par("Cancelada el", FechaHora(compra.FechaCancelada.Value)));

        for (int i = 0; i < datos.Count; i += 2)
        {
            controles.Add(CeldaTexto(0F, y, 130F, 18F, datos[i].Key, negrita: true));
            controles.Add(CeldaTexto(130F, y, 380F, 18F, datos[i].Value));
            if (i + 1 < datos.Count)
            {
                controles.Add(CeldaTexto(520F, y, 130F, 18F, datos[i + 1].Key, negrita: true));
                controles.Add(CeldaTexto(650F, y, 370F, 18F, datos[i + 1].Value));
            }
            y += 18F;
        }

        y += 8F;

        // ---------- RESUMEN DE IMPORTES ----------
        controles.Add(SeccionTitulo("Resumen de importes", y));
        y += 20F;

        controles.AddRange(FilaImporte(y, "Importe de productos", Dinero(importeProductos), false)); y += 18F;
        controles.AddRange(FilaImporte(y, "Flete", Dinero(compra.Flete), false)); y += 18F;
        controles.AddRange(FilaImporte(y, "THCD", Dinero(compra.THCD), false)); y += 18F;
        controles.AddRange(FilaImporte(y, "Seguro", Dinero(compra.Seguro), false)); y += 18F;
        controles.AddRange(FilaImporte(y, "Total de la compra", Dinero(total), true)); y += 22F;

        band.HeightF = y;
        band.Controls.AddRange(controles.ToArray());
        return band;
    }

    // ============================================================
    // PAGE HEADER: CABECERA DE LA TABLA DE DETALLE (se repite)
    // ============================================================
    private PageHeaderBand ConstruirCabeceraTabla()
    {
        var band = new PageHeaderBand { HeightF = 22F };

        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, 0, AnchoUtil, 22)
        };
        var fila = new XRTableRow();

        CeldaFija(fila, "#", 45F, derecho: true, cabecera: true);
        CeldaFija(fila, "Combustible", 425F, derecho: false, cabecera: true);
        CeldaFija(fila, "U/M", 80F, derecho: true, cabecera: true);
        CeldaFija(fila, "Cantidad", 150F, derecho: true, cabecera: true);
        CeldaFija(fila, "Precio", 160F, derecho: true, cabecera: true);
        CeldaFija(fila, "Importe", 160F, derecho: true, cabecera: true);

        tabla.Rows.Add(fila);
        band.Controls.Add(tabla);
        return band;
    }

    // ============================================================
    // DETAIL: UNA FILA POR DETALLE (DTO plano)
    // ============================================================
    private DetailBand ConstruirDetalle()
    {
        var band = new DetailBand { HeightF = 20F };

        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, 0, AnchoUtil, 20)
        };
        var fila = new XRTableRow();

        CeldaExpresion(fila, "[Numero]", 45F, "{0:N0}", derecho: true);
        CeldaExpresion(fila, "[Combustible]", 425F, null, derecho: false);
        CeldaExpresion(fila, "[UnidadMedida]", 80F, null, derecho: true);
        CeldaExpresion(fila, "[Cantidad]", 150F, "{0:N2}", derecho: true);
        CeldaExpresion(fila, "[Precio]", 160F, "{0:$ #,##0.0000}", derecho: true);
        CeldaExpresion(fila, "[Importe]", 160F, "{0:$ #,##0.00}", derecho: true);

        tabla.Rows.Add(fila);
        band.Controls.Add(tabla);
        return band;
    }

    // ============================================================
    // REPORT FOOTER: TOTALES + COMENTARIO + FIRMAS
    // ============================================================
    private ReportFooterBand ConstruirPie(
        CompraCombustible compra,
        string empresa,
        decimal importeProductos,
        decimal cantidadTotal,
        bool sinDetalles)
    {
        var band = new ReportFooterBand();
        var controles = new List<XRControl>();

        float y = 2F;

        // ---------- Fila de totales (alineada con la tabla) ----------
        var tabla = new XRTable
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 22)
        };
        var fila = new XRTableRow();

        var celdaTotales = new XRTableCell
        {
            WidthF = 550F,
            Text = "Totales",
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        };
        fila.Cells.Add(celdaTotales);

        var celdaCantidad = new XRTableCell
        {
            WidthF = 150F,
            Text = Numero(cantidadTotal, 2),
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        };
        fila.Cells.Add(celdaCantidad);

        var celdaVacia = new XRTableCell
        {
            WidthF = 160F,
            Text = "",
            BackColor = AzulClaro,
            Borders = BorderSide.All,
            BorderColor = GrisBorde
        };
        fila.Cells.Add(celdaVacia);

        var celdaImporte = new XRTableCell
        {
            WidthF = 160F,
            Text = Dinero(importeProductos),
            Font = FuenteNegrita,
            BackColor = AzulClaro,
            ForeColor = Azul,
            TextAlignment = TextAlignment.MiddleRight,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(4, 4, 2, 2)
        };
        fila.Cells.Add(celdaImporte);

        tabla.Rows.Add(fila);
        controles.Add(tabla);
        y += 24F;

        if (sinDetalles)
        {
            controles.Add(new XRLabel
            {
                BoundsF = new RectangleF(0, y, AnchoUtil, 18),
                Text = "Esta compra no tiene detalles registrados.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = GrisTexto,
                TextAlignment = TextAlignment.MiddleCenter
            });
            y += 22F;
        }

        // ---------- Comentario ----------
        controles.Add(new XRLabel
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 16),
            Text = "Comentario",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Azul
        });
        y += 18F;

        string comentario = string.IsNullOrWhiteSpace(compra.Comentario)
            ? "Sin comentarios."
            : compra.Comentario.Trim();

        var caja = new XRLabel
        {
            BoundsF = new RectangleF(0, y, AnchoUtil, 34),
            Text = comentario,
            Font = FuenteBase,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = CremaCaja,
            Borders = BorderSide.All,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 3, 3),
            Multiline = true,
            CanGrow = true
        };
        controles.Add(caja);
        y += 44F;

        // ---------- Firmas ----------
        string[] firmas = { "Elaborado por", "Revisado por", "Recibido por" };
        float anchoFirma = 280F;
        float separacion = (AnchoUtil - (firmas.Length * anchoFirma)) / (firmas.Length - 1);

        for (int i = 0; i < firmas.Length; i++)
        {
            float x = i * (anchoFirma + separacion);
            controles.Add(new XRLine
            {
                BoundsF = new RectangleF(x, y + 30, anchoFirma, 1),
                ForeColor = Color.FromArgb(150, 150, 150)
            });
            controles.Add(new XRLabel
            {
                BoundsF = new RectangleF(x, y + 33, anchoFirma, 16),
                Text = firmas[i],
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = GrisTexto,
                TextAlignment = TextAlignment.MiddleCenter
            });
        }
        y += 52F;

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
    // HELPERS DE CONSTRUCCIÓN
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

    private static XRLabel CeldaTexto(
        float x, float y, float w, float h, string texto, bool negrita = false)
    {
        return new XRLabel
        {
            BoundsF = new RectangleF(x, y, w, h),
            Text = texto,
            Font = negrita ? FuenteNegrita : FuenteBase,
            ForeColor = negrita ? GrisTexto : Color.FromArgb(40, 40, 40),
            TextAlignment = TextAlignment.MiddleLeft,
            Multiline = false
        };
    }

    private static XRControl[] FilaImporte(
        float y, string etiqueta, string valor, bool esTotal)
    {
        Font fuente = esTotal
            ? new Font("Segoe UI", 11F, FontStyle.Bold)
            : FuenteBase;

        Color fondo = esTotal ? AzulClaro : Color.Transparent;
        Color frente = esTotal ? Azul : Color.FromArgb(40, 40, 40);
        float alto = esTotal ? 22F : 18F;
        var bordes = esTotal ? BorderSide.All : BorderSide.None;

        var lblEtiqueta = new XRLabel
        {
            BoundsF = new RectangleF(0, y, 770F, alto),
            Text = etiqueta,
            Font = fuente,
            BackColor = fondo,
            ForeColor = frente,
            Borders = bordes,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 2, 2),
            TextAlignment = TextAlignment.MiddleLeft
        };

        var lblValor = new XRLabel
        {
            BoundsF = new RectangleF(770F, y, 250F, alto),
            Text = valor,
            Font = fuente,
            BackColor = fondo,
            ForeColor = frente,
            Borders = bordes,
            BorderColor = GrisBorde,
            Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 2, 2),
            TextAlignment = TextAlignment.MiddleRight
        };

        return new XRControl[] { lblEtiqueta, lblValor };
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

    // ============================================================
    // FORMATO (idéntico al HTML)
    // ============================================================
    private static KeyValuePair<string, string> Par(string k, string v)
        => new KeyValuePair<string, string>(k, v);

    private static string Dinero(decimal valor, int decimales = 2)
        => "$ " + valor.ToString("N" + decimales, Cultura);

    private static string Numero(decimal valor, int decimales)
        => valor.ToString("N" + decimales, Cultura);

    private static string Fecha(DateTime valor)
        => valor == default ? "—" : valor.ToString("dd/MM/yyyy", Cultura);

    private static string FechaHora(DateTime valor)
        => valor.ToString("dd/MM/yyyy HH:mm", Cultura);

    private static string Moneda(Moneda moneda)
    {
        if (moneda == null)
            return "—";

        string nombre = Texto(moneda.Nombre, "—");

        return moneda.TasaCambio > 0 && moneda.TasaCambio != 1m
            ? nombre + " (tasa " + moneda.TasaCambio.ToString("N4", Cultura) + ")"
            : nombre;
    }

    private static string Texto(string valor, string defecto = "—")
        => string.IsNullOrWhiteSpace(valor) ? defecto : valor.Trim();

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
