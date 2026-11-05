using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

// Detalle de la entrada de combustible. Mapeado a la tabla legada
// RecepcionDetalle (el resumen es EntradaCombustible -> RecepcionResumen).
// No tiene [DefaultClassOptions]: solo se ve dentro del detalle del
// resumen, igual que CompraCombustibleDetalle.
[Table("RecepcionDetalle")]
public class EntradaCombustibleDetalle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }


    // ============================================================
    // ENTRADA (RESUMEN)
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdRecepcion { get; set; }

    [ForeignKey(nameof(IdRecepcion))]
    [ModelDefault("Caption", "Entrada")]
    public virtual EntradaCombustible Entrada { get; set; }


    // ============================================================
    // COMBUSTIBLE (IdProducto -> TipoCombustible)
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDetalleProductoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el combustible."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdProducto { get; set; }

    [ForeignKey("IdProducto")]
    [ModelDefault("Caption", "Combustible")]
    public virtual TipoCombustible Producto { get; set; }


    // ============================================================
    // UNIDAD DE MEDIDA
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDetalleMedidaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar la unidad de medida."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdMedida { get; set; }

    [ForeignKey("IdMedida")]
    [ModelDefault("Caption", "Unidad de Medida")]
    public virtual UnidadMedida Medida { get; set; }


    // ============================================================
    // PRECIO
    //
    // REGLA del usuario: NO es editable. El sistema lo sugiere solo
    // en OnChanged del controller: precio del INVENTARIO en ese
    // depósito; si no hay inventario, el PCosto del catálogo
    // TipoCombustible. La columna calculada de la BD es el IMPORTE.
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDetallePrecioMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El precio debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "Precio")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.0000}")]
    [ModelDefault("AllowEdit", "False")]
    public virtual decimal Precio { get; set; }


    // ============================================================
    // TASA DE CAMBIO
    //
    // La tasa con la que convierte el precio de la moneda de la entrada
    // a la moneda local (CUP). Por defecto toma la del resumen.
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDetalleTasaCambioMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "La tasa de cambio debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "Tasa de Cambio")]
    [ModelDefault("DisplayFormat", "{0:#,##0.0000}")]
    [ModelDefault("EditMask", "n4")]
    public virtual decimal TasaCambio { get; set; } = 1m;


    // ============================================================
    // CANTIDAD
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDetalleCantidadMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "La cantidad debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "Cantidad")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public virtual decimal Cantidad { get; set; }


    // ============================================================
    // IMPORTE
    //
    // Columna calculada en SQL: Precio * TasaCambio * Cantidad.
    // Solo lectura: la BD la calcula.
    // ============================================================

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [ModelDefault("Caption", "Importe")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    [ModelDefault("AllowEdit", "False")]
    public virtual decimal Importe { get; set; }
}
