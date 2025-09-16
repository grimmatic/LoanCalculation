using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json;

namespace LoanCalculation.Models;

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

    // jsonb -> JsonDocument (veya string kullanabilirsiniz)
    [Column("detay", TypeName = "jsonb")]
    public JsonDocument? Detay { get; set; }

    [Column("kullanici_ip", TypeName = "inet")]
    public IPAddress? KullaniciIp { get; set; }

    [Column("olusturma_tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
}