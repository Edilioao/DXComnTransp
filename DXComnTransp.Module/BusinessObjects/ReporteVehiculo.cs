using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using System.ComponentModel.DataAnnotations;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
public class ReporteVehiculo
{
    [Key]
    public long Id { get; set; }

    public string? Matricula { get; set; }
    public string? Indicativo { get; set; }
    public string? TipoVehiculo { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? TipoServicio { get; set; }
    public string? Deposito { get; set; }
    public string? Combustible { get; set; }

    public decimal? IndiceReal { get; set; }
    public decimal? CapacidadTanque { get; set; }
    public string? NoCarroceria { get; set; }
    public string? NoMotor { get; set; }

    public decimal? IndiceNormado { get; set; }
    public string? Bateria { get; set; }
    public string? Neumaticos { get; set; }
    public int? NoPlazas { get; set; }
    public decimal? CapCarga { get; set; }
    public decimal? CapacidadTanqueNormada { get; set; }

    [FieldSize(500)]
    public string? Choferes { get; set; }


    [System.ComponentModel.Browsable(true)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    [ModelDefault("Caption", "Tipo de Vehículo")]
    public string DescripcionCompleta
    {
        get
        {
            string tipo = TipoVehiculo ?? "";
            string marca = Marca ?? "";
            string modelo = Modelo ?? "";

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