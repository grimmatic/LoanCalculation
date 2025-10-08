using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("basvurular")]
public class Basvuru
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("banka_urun_id")]
    public int? BankaUrunId { get; set; }

    [Column("musteri_id")]
    public int? MusteriId { get; set; }

    [Column("urun_id")]
    public int? UrunId { get; set; }

    [Column("kredi_tutari", TypeName = "numeric")]
    public decimal KrediTutari { get; set; }

    [Column("vade")]
    public int Vade { get; set; }

    [Column("faiz_orani", TypeName = "numeric")]
    public decimal FaizOrani { get; set; }

    [Column("aylik_odeme", TypeName = "numeric")]
    public decimal AylikOdeme { get; set; }

    [Column("toplam_odeme", TypeName = "numeric")]
    public decimal ToplamOdeme { get; set; }

    [Column("gelir", TypeName = "numeric")]
    public decimal Gelir { get; set; }

    [Column("onay_durumu")]
    public string OnayDurumu { get; set; } = "Beklemede";

    [Column("basvuru_tarihi")]
    public DateTime BasvuruTarihi { get; set; } = DateTime.UtcNow;

    public virtual BankaUrunu? BankaUrunu { get; set; }
    public virtual Musteri? Musteri { get; set; }
}
