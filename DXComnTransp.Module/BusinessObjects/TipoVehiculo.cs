using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;
[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(DescripcionCompleta))]
[Table("TipoVehiculos")]
public class TipoVehiculo
{
    // ============================================================
    // ID
    // ============================================================

    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int Id { get; set; }


    // ============================================================
    // TIPO DE VEHÍCULO
    // ============================================================

    [RuleRequiredField(
        "TipoVehiculoNombreObligatorio",
        DefaultContexts.Save,
        "Debe indicar el tipo de vehículo."
    )]
    [RuleUniqueValue(
        "TipoVehiculoNombreUnico",
        DefaultContexts.Save,
        "Ya existe un tipo de vehículo con este nombre."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [Column("TipoVehiculo")]
    [ModelDefault("Caption", "Tipo de Vehículo")]
    public virtual string TipoVehiculoNombre { get; set; }


    // ============================================================
    // MARCA
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoMarcaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar la marca."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdMarca { get; set; }

    [ForeignKey(nameof(IdMarca))]
    [ModelDefault("Caption", "Marca")]
    public virtual Marca Marca { get; set; }


    // ============================================================
    // MODELO
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoModeloMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el modelo."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdModelo { get; set; }

    [ForeignKey(nameof(IdModelo))]
    [ModelDefault("Caption", "Modelo")]
    public virtual Modelo Modelo { get; set; }


    // ============================================================
    // ÍNDICE NORMADO
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoIndiceNormadoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El índice normado debe ser mayor que cero."
    )]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("Caption", "Índice Normado")]
    public virtual decimal IndiceNormado { get; set; }


    // ============================================================
    // BATERÍA
    // ============================================================

    [RuleRequiredField(
        "TipoVehiculoBateriaObligatoria",
        DefaultContexts.Save,
        "Debe indicar la batería."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "Batería")]
    public virtual string Bateria { get; set; }


    // ============================================================
    // NEUMÁTICOS
    // ============================================================

    [RuleRequiredField(
        "TipoVehiculoNeumaticosObligatorios",
        DefaultContexts.Save,
        "Debe indicar los neumáticos."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [ModelDefault("Caption", "Neumáticos")]
    public virtual string Neumaticos { get; set; }


    // ============================================================
    // CAPACIDAD DEL TANQUE
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoCapTanqueMayorCero",
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
    // NÚMERO DE PLAZAS
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoNoPlazasMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "El número de plazas debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "No. Plazas")]
    public virtual int NoPlazas { get; set; }


    // ============================================================
    // CAPACIDAD DE CARGA
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoCapCargaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "La capacidad de carga debe ser mayor que cero."
    )]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("Caption", "Capacidad de Carga")]
    public virtual decimal CapCarga { get; set; }


    // ============================================================
    // COMBUSTIBLE
    // ============================================================

    [RuleValueComparison(
        "TipoVehiculoCombustibleMayorCero",
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

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    [ModelDefault("Caption", "Tipo de Vehículo")]
    public string DescripcionCompleta
    {
        get
        {
            string tipo = TipoVehiculoNombre ?? "";
            string marca = Marca?.Nombre ?? "";
            string modelo = Modelo?.Modelos ?? "";

            if (!string.IsNullOrWhiteSpace(marca) &&
                !string.IsNullOrWhiteSpace(modelo))
            {
                return $"{tipo} - {marca} ({modelo})";
            }

            if (!string.IsNullOrWhiteSpace(marca))
            {
                return $"{tipo} - {marca}";
            }

            return tipo;
        }
    }
}