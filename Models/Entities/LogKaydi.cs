using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LoanCalculation.Models.Entities;

[Table("loglar")]
public class LogKaydi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [MaxLength(10)]
    [Column("seviye", TypeName = "varchar(10)")]
    public string Seviye { get; set; } = "Info";

    [Column("mesaj", TypeName = "text")]
    public string Mesaj { get; set; } = null!;

    [Column("detay", TypeName = "jsonb")]
    public JsonDocument? Detay { get; set; }

    [Column("olusturma_tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
}