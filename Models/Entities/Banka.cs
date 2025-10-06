using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("bankalar")]
public class Banka
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("ad")]
    public string Ad { get; set; } = null!;

    [Column("logo_url")]
    public string? LogoUrl { get; set; }

    [Column("aktif")]
    public bool Aktif { get; set; } = true;
    public virtual ICollection<BankaUrunu> BankaUrunleri { get; set; } = new List<BankaUrunu>();

}