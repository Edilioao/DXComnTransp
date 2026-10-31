using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos/Vehículos")]
[DefaultProperty(nameof(Matricula))]
[Table("Vehiculos")]
public class Vehiculo
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
    // MATRÍCULA
    // ============================================================

    [RuleRequiredField(
        "VehiculoMatriculaObligatoria",
        DefaultContexts.Save,
        "Debe indicar la matrícula del vehículo."
    )]
    [RuleUniqueValue(
        "VehiculoMatriculaUnica",
        DefaultContexts.Save,
        "Ya existe un vehículo con esta matrícula."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(20)]
    [ModelDefault("Caption", "Matrícula")]
    public virtual string Matricula { get; set; }


    // ============================================================
    // TIPO DE VEHÍCULO
    // ============================================================

    [RuleValueComparison(
        "VehiculoTipoVehiculoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el tipo de vehículo."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdTipoVehiculo { get; set; }

    [ForeignKey(nameof(IdTipoVehiculo))]
    [ModelDefault("Caption", "Tipo de Vehículo")]
    public virtual TipoVehiculo TipoVehiculo { get; set; }


    // ============================================================
    // ÍNDICE REAL
    // ============================================================

    [RuleValueComparison(
        "VehiculoIndiceRealMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El índice real debe ser mayor que cero."
    )]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("Caption", "Índice Real")]
    public virtual decimal IndiceReal { get; set; }


    // ============================================================
    // CAPACIDAD DEL TANQUE
    // ============================================================

    [RuleValueComparison(
        "VehiculoCapTanqueMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "La capacidad del tanque debe ser mayor que cero."
    )]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("Caption", "Capacidad Tanque")]
    public virtual decimal CapTanqueComb { get; set; }


    // ============================================================
    // COMBUSTIBLE
    // ============================================================

    [RuleValueComparison(
        "VehiculoCombustibleMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el combustible."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdCombustible { get; set; }

    [ForeignKey(nameof(IdCombustible))]
    [ModelDefault("Caption", "Combustible")]
    public virtual TipoCombustible Combustible { get; set; }


    // ============================================================
    // TIPO DE SERVICIO
    // ============================================================

    [RuleValueComparison(
        "VehiculoTipoServicioMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el tipo de servicio."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdTipoServicio { get; set; }

    [ForeignKey(nameof(IdTipoServicio))]
    [ModelDefault("Caption", "Tipo de Servicio")]
    public virtual TipoServicio TipoServicio { get; set; }


    // ============================================================
    // INDICATIVO
    // ============================================================

    [Size(100)]
    [ModelDefault("Caption", "Indicativo")]
    public virtual string Indicativo { get; set; }


    // ============================================================
    // NÚMERO DE CARROCERÍA
    // ============================================================

    [RuleRequiredField(
        "VehiculoNoCarroceriaObligatorio",
        DefaultContexts.Save,
        "Debe indicar el número de carrocería."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "No. Carrocería")]
    public virtual string NoCarroceria { get; set; }


    // ============================================================
    // NÚMERO DE MOTOR
    // ============================================================

    [RuleRequiredField(
        "VehiculoNoMotorObligatorio",
        DefaultContexts.Save,
        "Debe indicar el número de motor."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "No. Motor")]
    public virtual string NoMotor { get; set; }

    // ============================================================
    // DEPÓSITO
    // ============================================================

    [RuleValueComparison(
        "VehiculoDepositoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el depósito."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdDeposito { get; set; }

    [ForeignKey(nameof(IdDeposito))]
    [RuleRequiredField(
        "VehiculoDepositoObligatorio",
        DefaultContexts.Save,
        "Debe seleccionar el depósito."
    )]
    [ModelDefault("Caption", "Depósito")]
    public virtual Deposito Deposito { get; set; }


    // ============================================================
    // DOCUMENTOS ESCANEADOS
    // ============================================================

    [DevExpress.ExpressApp.DC.Aggregated]
    [InverseProperty(nameof(DocumentoVehiculo.Vehiculo))]
    [ModelDefault("Caption", "Documentos")]
    public virtual IList<DocumentoVehiculo> Documentos { get; set; }
        = new List<DocumentoVehiculo>();
}