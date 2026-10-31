using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Nombre))]
[Table("Configuracion")]
public class Configuracion
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long Id { get; set; }

    [RuleRequiredField(
        "ConfiguracionNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre de la empresa.")]
    [ModelDefault("Caption", "Nombre de la empresa")]
    [Size(200)]
    public virtual string Nombre { get; set; }

    [ModelDefault("Caption", "Slogan")]
    [Size(250)]
    public virtual string Slogan { get; set; }

    [ImageEditor]
    [ModelDefault("Caption", "Logo")]
    public virtual byte[] Logo { get; set; }

    [RuleRequiredField(
        "ConfiguracionNITObligatorio",
        DefaultContexts.Save,
        "Debe indicar el NIT.")]
    [ModelDefault("Caption", "NIT")]
    [Size(50)]
    public virtual string Nit { get; set; }

    [ModelDefault("Caption", "REEUP")]
    [Size(50)]
    public virtual string Reeup { get; set; }

    [ModelDefault("Caption", "Cuenta bancaria")]
    [Size(100)]
    public virtual string CuentaBancaria { get; set; }

    [ModelDefault("Caption", "Correo electrónico")]
    [Size(160)]
    public virtual string Correo { get; set; }

    [ModelDefault("Caption", "Teléfonos")]
    public virtual string Telefonos { get; set; }

    [RuleRequiredField(
        "ConfiguracionTasaVigenteObligatoria",
        DefaultContexts.Save,
        "Debe indicar la tasa vigente.")]
    [ModelDefault("Caption", "Tasa vigente")]
    [ModelDefault("DisplayFormat", "{0:N4}")]
    public virtual decimal? TasaVigente { get; set; } = 1;

    [ModelDefault("Caption", "Ruta de respaldo BD")]
    [Size(250)]
    public virtual string PathSalvaBD { get; set; }
}
