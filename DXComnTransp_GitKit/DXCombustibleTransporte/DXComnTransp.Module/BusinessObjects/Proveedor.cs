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
[Table("Proveedores")]
public class Proveedor
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "ProveedorNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre del proveedor."
    )]
    [RuleUniqueValue(
        "ProveedorNombreUnico",
        DefaultContexts.Save,
        "Ya existe un proveedor con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(500)]
    [Column("Proveedor")]
    public virtual string Nombre { get; set; }

    public virtual bool Activo { get; set; } = true;
}