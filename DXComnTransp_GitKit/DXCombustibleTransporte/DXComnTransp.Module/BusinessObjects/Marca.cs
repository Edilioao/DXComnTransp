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
[Table("Marcas")]
public class Marca
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "MarcaNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre de la marca."
    )]
    [RuleUniqueValue(
        "MarcaNombreUnico",
        DefaultContexts.Save,
        "Ya existe una marca con este nombre."
    )]
    [Required]
    [Size(100)]
    [Column("Marca")]
    public virtual string Nombre { get; set; }
}