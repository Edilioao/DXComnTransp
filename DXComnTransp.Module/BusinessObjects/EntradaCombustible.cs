using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DevExpress.ExpressApp.ConditionalAppearance;

namespace DXComnTransp.Module.BusinessObjects;

[Appearance(
    "EntradaCombustibleTerminada",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Criteria = "Terminada = true And Cancelada = false",
    Context = "ListView",
    BackColor = "Honeydew",
    FontColor = "DarkGreen"
)]
[Appearance(
    "EntradaCombustibleCancelada",
    AppearanceItemType = "ViewItem",
    TargetItems = "*",
    Criteria = "Cancelada = true",
    Context = "ListView",
    BackColor = "MistyRose",
    FontColor = "DarkRed"
)]

[DefaultClassOptions]
[NavigationItem("Combustible")]
[DefaultProperty(nameof(DescripcionEntrada))]
[ModelDefault("Caption", "Entrada de Combustible")]
[Table("RecepcionResumen")]
public class EntradaCombustible
    : DevExpress.ExpressApp.IXafEntityObject,
      DevExpress.ExpressApp.IObjectSpaceLink
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
    // FECHA
    // ============================================================

    [RuleRequiredField(
        "EntradaCombustibleFechaObligatoria",
        DefaultContexts.Save,
        "Debe indicar la fecha de la entrada."
    )]
    [ModelDefault("Caption", "Fecha")]
    public virtual DateTime Fecha { get; set; }


    // ============================================================
    // CONSECUTIVO
    //
    // Se genera al guardar (ReciboCombustible.OnSaving) igual que el
    // trigger lo hacia en las compras. Solo lectura para el usuario.
    // ============================================================

    [VisibleInListView(true)]
    [VisibleInDetailView(true)]
    [ModelDefault("Caption", "Consecutivo")]
    public virtual int Consecutivo { get; set; }


    // ============================================================
    // DEPOSITO
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleDepositoMayorCero",
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
    [ModelDefault("Caption", "Depósito")]
    public virtual Deposito Deposito { get; set; }


    // ============================================================
    // PROVEEDOR
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleProveedorMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar el proveedor."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdProveedor { get; set; }

    [ForeignKey(nameof(IdProveedor))]
    [ModelDefault("Caption", "Proveedor")]
    public virtual Proveedor Proveedor { get; set; }


    // ============================================================
    // FACTURA
    // ============================================================

    [RuleRequiredField(
        "EntradaCombustibleNoFacturaObligatoria",
        DefaultContexts.Save,
        "Debe indicar el número de factura."
    )]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(50)]
    [ModelDefault("Caption", "No. Factura")]
    public virtual string NoFactura { get; set; }


    // ============================================================
    // MONEDA
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleMonedaMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "Debe seleccionar la moneda."
    )]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public virtual int IdMoneda { get; set; }

    [ForeignKey(nameof(IdMoneda))]
    [ModelDefault("Caption", "Moneda")]
    public virtual Moneda Moneda { get; set; }


    // ============================================================
    // TASA DE CAMBIO DEL RESUMEN
    //
    // La que se usa por defecto en las lineas del detalle (el importe
    // del detalle es Precio * TasaCambio * Cantidad).
    // ============================================================

    [RuleValueComparison(
        "EntradaCombustibleTasaCambioMayorCero",
        DefaultContexts.Save,
        ValueComparisonType.GreaterThan,
        0,
        "La tasa de cambio debe ser mayor que cero."
    )]
    [ModelDefault("Caption", "Tasa de Cambio")]
    [ModelDefault("DisplayFormat", "{0:#,##0.0000}")]
    [ModelDefault("EditMask", "n4")]
    public virtual decimal TasaCambio { get; set; } = 1m;


    // ============================================================
    // ESTADO (mismas reglas que la compra)
    // ============================================================

    [ModelDefault("Caption", "Terminada")]
    public virtual bool Terminada { get; set; }

    [VisibleInListView(false)]
    [ModelDefault("Caption", "Fecha Terminada")]
    public virtual DateTime? FechaTerminada { get; set; }

    [ModelDefault("Caption", "Cancelada")]
    public virtual bool Cancelada { get; set; }

    [VisibleInListView(false)]
    [ModelDefault("Caption", "Fecha Cancelada")]
    public virtual DateTime? FechaCancelada { get; set; }


    // ============================================================
    // COMENTARIO
    // ============================================================

    [ModelDefault("Caption", "Comentario")]
    [Size(4000)]
    public virtual string Comentario { get; set; }


    // ============================================================
    // DETALLES
    // ============================================================

    [DevExpress.ExpressApp.DC.Aggregated]
    [ModelDefault("Caption", "Detalles")]
    public virtual IList<EntradaCombustibleDetalle> Detalles { get; set; }
    = new List<EntradaCombustibleDetalle>();


    // ============================================================
    // IMPORTES CALCULADOS
    // ============================================================

    [NotMapped]
    [VisibleInListView(true)]
    [ModelDefault("Caption", "Importe Productos")]
    [ModelDefault("DisplayFormat", "{0:$ #,##0.00}")]
    public virtual decimal ImporteProductos
    {
        get
        {
            if (Detalles == null)
                return 0;

            return Detalles.Sum(x => x.Importe);
        }
    }

    [NotMapped]
    [VisibleInListView(true)]
    [ModelDefault("Caption", "Cantidad Total")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public virtual decimal CantidadTotal
    {
        get
        {
            if (Detalles == null)
                return 0;

            return Detalles.Sum(x => x.Cantidad);
        }
    }


    [NotMapped]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(true)]
    [ModelDefault("Caption", "Entrada")]
    public string DescripcionEntrada
    {
        get
        {
            return $"{Consecutivo} - {NoFactura}";
        }
    }


    // ============================================================
    // IXafEntityObject
    //
    // XAF llama OnCreated al crear el objeto desde la UI, OnLoaded al
    // cargarlo y OnSaving en cada guardado.
    // ============================================================

    public void OnCreated()
    {
        Fecha = DateTime.Now;
        TasaCambio = 1m;
        Terminada = false;
        Cancelada = false;
    }

    public void OnLoaded()
    {
        // Nada: los valores vienen de la BD.
    }

    public void OnSaving()
    {
        // Consecutivo secuencial para entradas nuevas (idem lo que el
        // trigger hacia en compras). Solo si hay ObjectSpace y no
        // asignado: deja el valor maniobrado a mano.
        DevExpress.ExpressApp.IObjectSpace espacio =
            ((DevExpress.ExpressApp.IObjectSpaceLink)this).ObjectSpace;

        if (Id == 0 && Consecutivo == 0 && espacio != null)
        {
            int consecutivoMaximo = espacio
                .GetObjectsQuery<EntradaCombustible>()
                .Where(x => x.Id > 0)
                .Select(x => (int?)x.Consecutivo)
                .Max() ?? 0;

            Consecutivo = consecutivoMaximo + 1;
        }
    }

    // ============================================================
    // IObjectSpaceLink
    //
    // XAF asigna el ObjectSpace del objeto: sirve para consultar
    // datos en OnSaving. Implementación EXPLICITA de la interfaz a
    // propósito: si la propiedad es pública, aparece como columna /
    // campo "Object Space" en las vistas (basura para el usuario) y
    // EF intenta mapearla.
    // ============================================================

    private DevExpress.ExpressApp.IObjectSpace _objectSpace;

    DevExpress.ExpressApp.IObjectSpace
        DevExpress.ExpressApp.IObjectSpaceLink.ObjectSpace
    {
        get => _objectSpace;
        set => _objectSpace = value;
    }
}

