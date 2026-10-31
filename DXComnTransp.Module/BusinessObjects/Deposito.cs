using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DXComnTransp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Catálogos")]
[DefaultProperty(nameof(Nombre))]
[ModelDefault("Caption", "Depósito")]
[Table("Depositos")]
public class Deposito
{
    [System.ComponentModel.DataAnnotations.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [RuleUniqueValue(
        "DepositoIdUnico",
        DefaultContexts.Save,
        "El código del depósito ya existe. Debe utilizar otro."
    )]
    public virtual int Id { get; set; }

    [RuleRequiredField(
    "DepositoNombreObligatorio",
    DefaultContexts.Save,
    "Debe indicar el nombre del depósito."
)]
    [RuleUniqueValue(
    "DepositoNombreUnico",
    DefaultContexts.Save,
    "Ya existe un depósito con este nombre."
)]
    [System.ComponentModel.DataAnnotations.Required]
    [Size(100)]
    [Column("Deposito")]
    public virtual string Nombre { get; set; }

    public virtual bool Interno { get; set; }

    public virtual bool Cerrado { get; set; }
}