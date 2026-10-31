using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Operacion))]
[Table("TipoServicios")]
public class TipoServicio
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "TipoServicioOperacionObligatoria",
        DefaultContexts.Save,
        "Debe indicar la operación del servicio."
    )]
    [RuleUniqueValue(
        "TipoServicioOperacionUnica",
        DefaultContexts.Save,
        "Ya existe un tipo de servicio con esta operación."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    public virtual string Operacion { get; set; }
}