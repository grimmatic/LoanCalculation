using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace LoanCalculation.Models;

[Table("hesaplamalar")]
public class Hesaplama
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("urun_id")]
    public int UrunId { get; set; }

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

    [Column("kullanici_ip", TypeName = "inet")]
    public IPAddress? KullaniciIp { get; set; }

    [Column("hesaplama_tarihi")]
    public DateTime HesaplamaTarihi { get; set; } = DateTime.UtcNow;
}