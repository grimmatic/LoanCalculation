using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanCalculation.Models.Entities;

[Table("urunler")]
public class Urun
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("ad")]
    public string Ad { get; set; } = null!;

    [MaxLength(10)]
    [Column("kod")]
    public string? Kod { get; set; }

    [Column("aktif")]
    public bool Aktif { get; set; } = true;
}