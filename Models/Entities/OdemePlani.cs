using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("odeme_plani")]
public class OdemePlani
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("basvuru_id")]
    public int BasvuruId { get; set; }

    [Column("taksit_no")]
    public int TaksitNo { get; set; }

    [Column("taksit_tutari", TypeName = "numeric")]
    public decimal TaksitTutari { get; set; }

    [Column("ana_para", TypeName = "numeric")]
    public decimal AnaPara { get; set; }

    [Column("faiz", TypeName = "numeric")]
    public decimal Faiz { get; set; }

    [Column("kalan_bakiye", TypeName = "numeric")]
    public decimal KalanBakiye { get; set; }

    [Column("vade_tarihi")]
    public DateOnly VadeTarihi { get; set; }
}