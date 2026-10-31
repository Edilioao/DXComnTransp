using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos/Vehículos")]
[DefaultProperty(nameof(NombreChofer))]
[Table("Choferes")]
public class Chofer
{
    // ============================================================
    // ID
    // ============================================================

    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long Id { get; set; }


    // ============================================================
    // CARNÉ DE IDENTIDAD
    // ============================================================

    [RuleRequiredField(
        "ChoferCIdentidadObligatorio",
        DefaultContexts.Save,
        "Debe indicar el carné de identidad."
    )]
    [RuleUniqueValue(
        "ChoferCIdentidadUnico",
        DefaultContexts.Save,
        "Ya existe un chofer con este carné de identidad."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(30)]
    [ModelDefault("Caption", "Carné de Identidad")]
    public virtual string CIdentidad { get; set; }


    // ============================================================
    // NOMBRE
    // ============================================================

    [RuleRequiredField(
        "ChoferNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el nombre del chofer."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "Nombre del Chofer")]
    public virtual string NombreChofer { get; set; }


    // ============================================================
    // LICENCIAS
    // ============================================================

    [RuleRequiredField(
        "ChoferLicenciasObligatorias",
        DefaultContexts.Save,
        "Debe indicar las licencias del chofer."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "Licencias")]
    public virtual string Licencias { get; set; }


    // ============================================================
    // TELÉFONO
    // ============================================================

    [Size(100)]
    [ModelDefault("Caption", "Teléfono")]
    public virtual string Telefono { get; set; }


    // ============================================================
    // ACTIVO
    // ============================================================

    [ModelDefault("Caption", "Activo")]
    public virtual bool Activo { get; set; } = true;
}