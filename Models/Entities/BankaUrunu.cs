using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("banka_urunleri")]
public class BankaUrunu
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("banka_id")]
    public int BankaId { get; set; }

    [Column("urun_tipi_id")]
    public int UrunTipiId { get; set; }

    [Column("faiz_orani", TypeName = "numeric(5,2)")]
    public decimal FaizOrani { get; set; }

    [Column("min_tutar", TypeName = "numeric(12,2)")]
    public decimal MinTutar { get; set; }

    [Column("max_tutar", TypeName = "numeric(12,2)")]
    public decimal MaxTutar { get; set; }

    [Column("min_vade")]
    public int MinVade { get; set; }

    [Column("max_vade")]
    public int MaxVade { get; set; }

    [MaxLength(200)]
    [Column("kampanya_adi")]
    public string? KampanyaAdi { get; set; }

    [Column("aktif")]
    public bool Aktif { get; set; } = true;

    public virtual Banka Banka { get; set; } = null!;
    public virtual UrunTipi UrunTipi { get; set; } = null!;
    public virtual ICollection<Hesaplama> Hesaplamalar { get; set; } = new List<Hesaplama>();
}