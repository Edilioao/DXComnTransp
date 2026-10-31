using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Combustible")]
[Table("ComprasCombustibleDetalle")]
public class CompraCombustibleDetalle
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }


    // ============================================================
    // COMPRA
    // ============================================================
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdCompra { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(IdCompra))]
    [ModelDefault("Caption", "Compra")]
    public virtual CompraCombustible Compra { get; set; }

    // ============================================================
    // COMBUSTIBLE
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleDetalleProductoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el combustible."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdProducto { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("IdProducto")]
    [ModelDefault("Caption", "Combustible")]
    public virtual TipoCombustible Producto { get; set; }


    // ============================================================
    // UNIDAD DE MEDIDA
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleDetalleMedidaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar la unidad de medida."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdMedida { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("IdMedida")]
    [ModelDefault("Caption", "Unidad de Medida")]
    public virtual UnidadMedida Medida { get; set; }

    // ============================================================
    // PRECIO
    // ============================================================

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [ModelDefault("Caption", "Precio")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.0000}")]
    [ModelDefault("EditMask", "n4")]
    [ModelDefault("AllowEdit", "False")]
    public virtual decimal Precio { get; set; }



    // ============================================================
    // CANTIDAD
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleDetalleCantidadMayorCero",
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
    // ============================================================

    [RuleValueComparison(
        "CompraCombustibleDetalleImporteMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El importe debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "Importe")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal Importe { get; set; }
}