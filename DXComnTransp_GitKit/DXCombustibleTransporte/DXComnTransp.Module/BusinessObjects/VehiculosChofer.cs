using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos/Vehículos")]
[Table("VehiculosChofer")]
public class VehiculoChofer
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
    // VEHÍCULO
    // ============================================================

    [RuleValueComparison(
        "VehiculoChoferVehiculoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el vehículo."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long IdVehiculo { get; set; }

    [ForeignKey(nameof(IdVehiculo))]
    [ModelDefault("Caption", "Vehículo")]
    public virtual Vehiculo Vehiculo { get; set; }


    // ============================================================
    // CHOFER PRINCIPAL
    // ============================================================

    [RuleValueComparison(
        "VehiculoChoferChoferMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el chofer principal."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long IdChofer { get; set; }

    [ForeignKey(nameof(IdChofer))]
    [ModelDefault("Caption", "Chofer")]
    public virtual Chofer Chofer { get; set; }


    // ============================================================
    // CHOFER SEGUNDO
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long? IdChoferSdo { get; set; }

    [ForeignKey(nameof(IdChoferSdo))]
    [ModelDefault("Caption", "Segundo Chofer")]
    public virtual Chofer ChoferSdo { get; set; }


    // ============================================================
    // CHOFER TERCERO
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long? IdChoferTro { get; set; }

    [ForeignKey(nameof(IdChoferTro))]
    [ModelDefault("Caption", "Tercer Chofer")]
    public virtual Chofer ChoferTro { get; set; }


    // ============================================================
    // CAMPOS DE SISTEMA SQL SERVER
    // ============================================================

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual DateTime SysStartTime { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual DateTime SysEndTime { get; set; }
}