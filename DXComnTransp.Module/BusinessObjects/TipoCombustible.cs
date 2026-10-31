using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Combustible))]
[Table("TipoCombustible")]
public class TipoCombustible
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [RuleUniqueValue(
        "TipoCombustibleIdUnico",
        DefaultContexts.Save,
        "El código del tipo de combustible ya existe. Debe utilizar otro."
    )]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "TipoCombustibleNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el tipo de combustible."
    )]
    [RuleUniqueValue(
        "TipoCombustibleNombreUnico",
        DefaultContexts.Save,
        "Ya existe un tipo de combustible con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(60)]
    public virtual string Combustible { get; set; }

    [RuleValueComparison(
    "TipoCombustiblePCostoMayorCero",
    DefaultContexts.Save,
    ValueComparisonType.GreaterThan,
    0,
    "El precio de costo debe ser mayor que cero."
)]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    [ModelDefault("EditMask", "\\$ #,##0.00")]
    public virtual decimal PCosto { get; set; }

    [RuleValueComparison(
        "TipoCombustiblePVentaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El precio de venta debe ser mayor que cero."
    )]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    [ModelDefault("EditMask", "\\$ #,##0.00")]
    public virtual decimal PVenta { get; set; }
}