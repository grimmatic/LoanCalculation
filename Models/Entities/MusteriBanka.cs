using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("musteri_bankalar")]
public class MusteriBanka
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("musteri_id")]
    public int MusteriId { get; set; }

    [Column("banka_id")]
    public int BankaId { get; set; }

    [Column("uyelik_tarihi")]
    public DateTime UyelikTarihi { get; set; } = DateTime.UtcNow;

    [Column("aktif")]
    public bool Aktif { get; set; } = true;

    // Navigation properties
    public virtual Musteri Musteri { get; set; } = null!;
    public virtual Banka Banka { get; set; } = null!;
}
