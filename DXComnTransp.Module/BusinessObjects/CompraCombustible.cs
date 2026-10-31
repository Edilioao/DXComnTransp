using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DevExpress.ExpressApp.ConditionalAppearance;

namespace DXComnTransp.Module.BusinessObjects;

[Appearance(
    "CompraCombustibleTerminada",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Criteria = "Terminada = true And Cancelada = false",
    Context = "ListView",
    BackColor = "Honeydew",
    FontColor = "DarkGreen"
)]
[Appearance(
    "CompraCombustibleCancelada",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Criteria = "Cancelada = true",
    Context = "ListView",
    BackColor = "MistyRose",
    FontColor = "DarkRed"
)]

[DefaultClassOptions]
[NavigationItem("Combustible")]
[DefaultProperty(nameof(DescripcionCompra))]
[Table("ComprasCombustibleResumen")]
public class CompraCombustible
{
    // ============================================================
    // ID
    // ============================================================

    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }


    // ============================================================
    // FECHA
    // ============================================================

    [RuleRequiredField(
        "CompraCombustibleFechaObligatoria",
        DefaultContexts.Save,
        "Debe indicar la fecha de la compra."
    )]
    [ModelDefault("Caption", "Fecha")]
    public virtual DateTime Fecha { get; set; }


    // ============================================================
    // CONSECUTIVO
    // ============================================================

    [VisibleInListView(true)]
    [VisibleInDetailView(true)]
    [ModelDefault("Caption", "Consecutivo")]
    public virtual int Consecutivo { get; set; }


    // ============================================================
    // FACTURA
    // ============================================================

    [RuleRequiredField(
        "CompraCombustibleFechaFacturaObligatoria",
        DefaultContexts.Save,
        "Debe indicar la fecha de la factura."
    )]
    [ModelDefault("Caption", "Fecha Factura")]
    public virtual DateTime FechaFactura { get; set; }

    [RuleRequiredField(
        "CompraCombustibleNoFacturaObligatorio",
        DefaultContexts.Save,
        "Debe indicar el número de factura."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(60)]
    [ModelDefault("Caption", "No. Factura")]
    public virtual string NoFactura { get; set; }


    // ============================================================
    // PROVEEDOR
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleProveedorMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el proveedor."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdProveedor { get; set; }

    [ForeignKey(nameof(IdProveedor))]
    [ModelDefault("Caption", "Proveedor")]
    public virtual Proveedor Proveedor { get; set; }


    // ============================================================
    // MONEDA
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleMonedaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar la moneda."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdMoneda { get; set; }

    [ForeignKey(nameof(IdMoneda))]
    [ModelDefault("Caption", "Moneda")]
    public virtual Moneda Moneda { get; set; }


    // ============================================================
    // GASTOS
    // ============================================================

    [ModelDefault("Caption", "Flete")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal Flete { get; set; }

    [ModelDefault("Caption", "THCD")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal THCD { get; set; }

    [ModelDefault("Caption", "Seguro")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal Seguro { get; set; }


    // ============================================================
    // ESTADO
    // ============================================================

    [ModelDefault("Caption", "Terminada")]
    public virtual bool Terminada { get; set; }

    [VisibleInListView(false)]
    [ModelDefault("Caption", "Fecha Terminada")]
    public virtual DateTime? FechaTerminada { get; set; }

    [ModelDefault("Caption", "Cancelada")]
    public virtual bool Cancelada { get; set; }

    [VisibleInListView(false)]
    [ModelDefault("Caption", "Fecha Cancelada")]
    public virtual DateTime? FechaCancelada { get; set; }


    // ============================================================
    // COMENTARIO
    // ============================================================

    [ModelDefault("Caption", "Comentario")]
    [Size(500)]
    public virtual string Comentario { get; set; }


    // ============================================================
    // DETALLES
    // ============================================================

    [DevExpress.ExpressApp.DC.Aggregated]
    [ModelDefault("Caption", "Detalles")]
    public virtual IList<CompraCombustibleDetalle> Detalles { get; set; }
    = new List<CompraCombustibleDetalle>();

    // ============================================================
    // IMPORTE PRODUCTOS
    // ============================================================

    [NotMapped]
    [VisibleInListView(true)]
    [ModelDefault("Caption", "Importe Productos")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal ImporteProductos
    {
        get
        {
            if (Detalles == null)
                return 0;

            return Detalles.Sum(x => x.Importe);
        }
    }


    // ============================================================
    // IMPORTE TOTAL
    // ============================================================

    [NotMapped]
    [VisibleInListView(true)]
    [ModelDefault("Caption", "Importe Total")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal ImporteTotal
    {
        get
        {
            return ImporteProductos + Flete + THCD + Seguro;
        }
    }


    [NotMapped]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    [ModelDefault("Caption", "Compra")]
    public string DescripcionCompra
    {
        get
        {
            return $"{Consecutivo} - {NoFactura}";
        }
    }

}