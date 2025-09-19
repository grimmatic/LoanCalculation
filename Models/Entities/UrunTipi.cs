using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("urun_tipleri")]
public class UrunTipi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("ad")]
    public string Ad { get; set; } = null!;

    [MaxLength(50)]
    [Column("kod")]
    public string? Kod { get; set; }

    [Column("aciklama")]
    public string? Aciklama { get; set; }

    [Column("aktif")]
    public bool Aktif { get; set; } = true;

    // Navigation properties
    public virtual ICollection<BankaUrunu> BankaUrunleri { get; set; } = new List<BankaUrunu>();
}