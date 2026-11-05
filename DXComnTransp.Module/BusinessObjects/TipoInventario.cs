using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(TpoInventario))]
[Table("TipoInventario")]
public class TipoInventario
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [RuleUniqueValue(
        "TipoInventarioIdUnico",
        DefaultContexts.Save,
        "El código del tipo de inventario ya existe. Debe utilizar otro."
    )]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "TipoInventarioNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el tipo de inventario."
    )]
    [RuleUniqueValue(
        "TipoInventarioNombreUnico",
        DefaultContexts.Save,
        "Ya existe un tipo de inventario con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    public virtual string TpoInventario { get; set; }
}