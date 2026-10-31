using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Nombre))]
[ImageName("BO_List")]
[ModelDefault("Caption", "Tipo de documento")]
[Table("TipoDocumentoVehiculo")]
public class TipoDocumentoVehiculo
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "TipoDocumentoVehiculoNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre del tipo de documento."
    )]
    [RuleUniqueValue(
        "TipoDocumentoVehiculoNombreUnico",
        DefaultContexts.Save,
        "Ya existe un tipo de documento con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "Tipo de documento")]
    public virtual string Nombre { get; set; }

    [ModelDefault("Caption", "Requiere vencimiento")]
    public virtual bool RequiereVencimiento { get; set; }

    [ModelDefault("Caption", "Activo")]
    public virtual bool Activo { get; set; } = true;
}
