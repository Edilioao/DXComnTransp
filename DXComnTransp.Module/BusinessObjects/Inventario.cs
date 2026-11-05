using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.ConditionalAppearance;

namespace DXComnTransp.Module.BusinessObjects;

// ============================================================
// INVENTARIO DE COMBUSTIBLE
//
// Mapeado a la tabla legada Inventarios (que ya existe en la BD):
// una fila por cada combinacion Deposito + Tipo de Inventario +
// Producto (combustible). Solo se ADMINISTRA con las entradas
// (la existencia sube con el promedio ponderado), no se edita
// a mano: por eso esta Marcado como no editable.
//
// El precio es el PROMEDIO PONDERADO en moneda local:
//   nuevoPrecio = (existenciaActual*precioActual + cantEntrada*precioEntrada)
//                 / (existenciaActual + cantEntrada)
// ============================================================

[DefaultClassOptions]
[NavigationItem("Combustible")]
[DefaultProperty(nameof(Descripcion))]
[ModelDefault("Caption", "Inventario")]
[Table("Inventarios")]
[Appearance(
    "InventarioSoloLectura",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Context = "DetailView",
    Enabled = false
)]
public class Inventario
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual long Id { get; set; }


    // ============================================================
    // DEPOSITO
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdDeposito { get; set; }

    [ForeignKey(nameof(IdDeposito))]
    [ModelDefault("Caption", "Depósito")]
    public virtual Deposito Deposito { get; set; }


    // ============================================================
    // TIPO DE INVENTARIO (COMBUSTIBLE, LUBRICANTES, ...)
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdTipoInv { get; set; }

    [ForeignKey(nameof(IdTipoInv))]
    [ModelDefault("Caption", "Tipo de Inventario")]
    public virtual TipoInventario TipoInventario { get; set; }


    // ============================================================
    // PRODUCTO (COMBUSTIBLE)
    // ============================================================

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdProducto { get; set; }

    [ForeignKey("IdProducto")]
    [ModelDefault("Caption", "Combustible")]
    public virtual TipoCombustible Producto { get; set; }


    // ============================================================
    // PRECIO PROMEDIO PONDERADO (moneda local)
    // ============================================================

    [ModelDefault("Caption", "Precio")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.0000}")]
    [ModelDefault("EditMask", "n4")]
    public virtual decimal Precio { get; set; }


    // ============================================================
    // EXISTENCIA
    // ============================================================

    [ModelDefault("Caption", "Existencia")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public virtual decimal Existencia { get; set; }


    // ============================================================
    // ACTUALIZADO
    // ============================================================

    [ModelDefault("Caption", "Actualizado")]
    public virtual DateTime Actualizado { get; set; }


    [NotMapped]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    public string Descripcion
    {
        get
        {
            return $"{Producto?.Combustible} " +
                   $"({TipoInventario?.TpoInventario} - {Deposito?.Nombre})";
        }
    }
}
