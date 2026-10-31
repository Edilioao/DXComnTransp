using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[Appearance(
    "DocumentoVehiculoVencido",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Criteria = "FechaVencimiento is not null And FechaVencimiento < LocalDateTimeToday()",
    Context = "ListView",
    FontColor = "DarkRed"
)]
[DefaultClassOptions]
[NavigationItem("Catálogos")]
[FileAttachment(nameof(Archivo))]
[ImageName("BO_FileAttachment")]
[DefaultProperty(nameof(DescripcionCompleta))]
[ModelDefault("Caption", "Documento de vehículo")]
[Table("DocumentosVehiculo")]
public class DocumentoVehiculo
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long Id { get; set; }

    [RuleValueComparison(
        "DocumentoVehiculoVehiculoMayorCero",
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
    [RuleRequiredField(
        "DocumentoVehiculoVehiculoObligatorio",
        DefaultContexts.Save,
        "Debe seleccionar el vehículo."
    )]
    [ModelDefault("Caption", "Vehículo")]
    public virtual Vehiculo Vehiculo { get; set; }

    [RuleValueComparison(
        "DocumentoVehiculoTipoMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el tipo de documento."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdTipoDocumento { get; set; }

    [ForeignKey(nameof(IdTipoDocumento))]
    [RuleRequiredField(
        "DocumentoVehiculoTipoObligatorio",
        DefaultContexts.Save,
        "Debe seleccionar el tipo de documento."
    )]
    [DataSourceCriteria("Activo = true")]
    [ModelDefault("Caption", "Tipo de documento")]
    public virtual TipoDocumentoVehiculo TipoDocumento { get; set; }

    [Size(200)]
    [ModelDefault("Caption", "Descripción")]
    public virtual string Descripcion { get; set; }

    [ModelDefault("Caption", "Fecha de emisión")]
    public virtual DateTime? FechaEmision { get; set; }

    [RuleRequiredField(
        "DocumentoVehiculoFechaVencimientoObligatoria",
        DefaultContexts.Save,
        "Debe indicar la fecha de vencimiento.",
        TargetCriteria = "TipoDocumento.RequiereVencimiento = true"
    )]
    [ModelDefault("Caption", "Fecha de vencimiento")]
    public virtual DateTime? FechaVencimiento { get; set; }

    [RuleRequiredField(
        "DocumentoVehiculoArchivoObligatorio",
        DefaultContexts.Save,
        "Debe adjuntar el archivo escaneado."
    )]
    [ExpandObjectMembers(ExpandObjectMembers.Never)]
    [FileTypeFilter("Documentos e imágenes", 1, "*.pdf", "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.tif", "*.tiff", "*.doc", "*.docx")]
    [ModelDefault("Caption", "Archivo")]
    public virtual FileData Archivo { get; set; }

    [Size(500)]
    [ModelDefault("Caption", "Observaciones")]
    public virtual string Observaciones { get; set; }

    [NotMapped]
    [VisibleInListView(true)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [ModelDefault("Caption", "Vencido")]
    public bool Vencido
    {
        get
        {
            return FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;
        }
    }

    [NotMapped]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    [ModelDefault("Caption", "Documento")]
    public string DescripcionCompleta
    {
        get
        {
            string tipo = TipoDocumento?.Nombre ?? "Documento";
            string detalle = !string.IsNullOrWhiteSpace(Descripcion)
                ? Descripcion
                : Archivo?.FileName;
            return string.IsNullOrWhiteSpace(detalle) ? tipo : $"{tipo} - {detalle}";
        }
    }
}
