using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Modelos))]
[Table("Modelos")]
public class Modelo
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "ModeloNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre del modelo."
    )]
    [RuleUniqueValue(
        "ModeloNombreUnico",
        DefaultContexts.Save,
        "Ya existe un modelo con este nombre."
    )]
    [Required]
    [Size(100)]
    public virtual string Modelos { get; set; }
}