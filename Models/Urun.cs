using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models;

[Table("urunler")]
public class Urun
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("ad")]
    public string Ad { get; set; } = null!;

    // Örn: yıllık faiz oranı yüzde olarak (örn 24.5)
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

    [Column("aktif")]
    public bool Aktif { get; set; }
}