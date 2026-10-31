using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(UM))]
[Table("UnidadMedida")]
public class UnidadMedida
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "UnidadMedidaNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar la unidad de medida."
    )]
    [RuleUniqueValue(
        "UnidadMedidaNombreUnica",
        DefaultContexts.Save,
        "Ya existe una unidad de medida con este código."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(8)]
    public virtual string UM { get; set; }
}