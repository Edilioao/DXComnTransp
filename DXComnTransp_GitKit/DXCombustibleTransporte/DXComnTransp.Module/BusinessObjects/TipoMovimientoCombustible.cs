using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[Table("TipoMovimientoCombustible")]
public class TipoMovimientoCombustible
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }

    [RuleRequiredField(
        "TipoMovimientoCombustibleCodigoObligatorio",
        DefaultContexts.Save,
        "Debe indicar el código del movimiento."
    )]
    [RuleUniqueValue(
        "TipoMovimientoCombustibleCodigoUnico",
        DefaultContexts.Save,
        "Ya existe un movimiento con este código."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(30)]
    public virtual string Codigo { get; set; }

    [RuleRequiredField(
        "TipoMovimientoCombustibleDescripcionObligatoria",
        DefaultContexts.Save,
        "Debe indicar la descripción del movimiento."
    )]
    [RuleUniqueValue(
        "TipoMovimientoCombustibleDescripcionUnica",
        DefaultContexts.Save,
        "Ya existe un movimiento con esta descripción."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    public virtual string Descripcion { get; set; }

    public virtual bool EsEntrada { get; set; }

    public virtual bool EsSalida { get; set; }

    public virtual bool AfectaDisponibilidad { get; set; }

    public virtual bool Activo { get; set; } = true;
}