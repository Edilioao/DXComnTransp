using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Nombre))]
[Table("Monedas")]
public class Moneda
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "MonedaNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre de la moneda."
    )]
    [RuleUniqueValue(
        "MonedaNombreUnico",
        DefaultContexts.Save,
        "Ya existe una moneda con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(6)]
    [Column("Moneda")]
    public virtual string Nombre { get; set; }

    [RuleValueComparison(
    "MonedaTasaCambioMayorCero",
    DefaultContexts.Save,
    ValueComparisonType.GreaterThan,
    0,
    "La tasa de cambio debe ser mayor que cero."
)]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    [ModelDefault("EditMask", "\\$ #,##0.00")]
    public virtual decimal TasaCambio { get; set; } = 1m;
}