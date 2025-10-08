using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("musteriler")]
public class Musteri
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Required, MaxLength(100)]
    [Column("sifre")]
    public string Sifre { get; set; } = null!;

    [Column("kayit_tarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    [Column("aktif")]
    public bool Aktif { get; set; } = true;

    // Navigation properties
    public virtual ICollection<MusteriBanka> MusteriBankalar { get; set; } = new List<MusteriBanka>();
    public virtual ICollection<Basvuru> Basvurular { get; set; } = new List<Basvuru>();
}
