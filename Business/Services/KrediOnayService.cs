using LoanCalculation.Models.Entities;
using LoanCalculation.Data;
using LoanCalculation.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Business.Services;

public class KrediOnayService : IKrediOnayService
{
    private readonly AppDbContext _db;

    public KrediOnayService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> OnayDurumuHesaplaAsync(Basvuru basvuru)
    {
        // Temel kontroller
        if (basvuru.Gelir < 5000) return "Red";

        // Kredi türüne göre farklı algoritmalar uygula
        var urun = await _db.Urunler.FirstOrDefaultAsync(u => u.Id == basvuru.UrunId);
        var urunAdi = urun?.Ad?.ToLower() ?? "";

        // Gelir/Kredi oranı hesapla
        var gelirKrediOrani = basvuru.Gelir > 0 ? basvuru.KrediTutari / basvuru.Gelir : 0;

        // Aylık ödeme/Gelir oranı hesapla
        var aylikOdemeGelirOrani = basvuru.Gelir > 0 ? basvuru.AylikOdeme / basvuru.Gelir : 0;

        // Kredi türüne göre onay kriterleri
        switch (urunAdi)
        {
            case "konut kredisi":
            case "konut":
                return KonutKredisiOnayAlgoritmasi(gelirKrediOrani, aylikOdemeGelirOrani, basvuru);

            case "ihtiyaç kredisi":
            case "ihtiyac":
                return IhtiyacKredisiOnayAlgoritmasi(gelirKrediOrani, aylikOdemeGelirOrani, basvuru);

            case "taşıt kredisi":
            case "tasit":
            case "araba kredisi":
                return TasitKredisiOnayAlgoritmasi(gelirKrediOrani, aylikOdemeGelirOrani, basvuru);

            case "ticari kredi":
            case "ticari":
                return TicariKredisiOnayAlgoritmasi(gelirKrediOrani, aylikOdemeGelirOrani, basvuru);

            default:
                return GenelKrediOnayAlgoritmasi(gelirKrediOrani, aylikOdemeGelirOrani, basvuru);
        }
    }

    private string KonutKredisiOnayAlgoritmasi(decimal gelirKrediOrani, decimal aylikOdemeGelirOrani, Basvuru basvuru)
    {
        if (gelirKrediOrani > 8 || aylikOdemeGelirOrani > 0.5m || basvuru.Vade > 360) return "Red";

        if (basvuru.Gelir >= 25000 && gelirKrediOrani <= 5 && aylikOdemeGelirOrani <= 0.35m) return "Onay";

        if (basvuru.Gelir >= 15000 && gelirKrediOrani <= 6 && aylikOdemeGelirOrani <= 0.4m) return "İnceleme";

        return "İnceleme";
    }

    private string IhtiyacKredisiOnayAlgoritmasi(decimal gelirKrediOrani, decimal aylikOdemeGelirOrani, Basvuru basvuru)
    {
        if (gelirKrediOrani > 3 || aylikOdemeGelirOrani > 0.4m || basvuru.Vade > 60) return "Red";

        if (basvuru.Gelir >= 20000 && gelirKrediOrani <= 2 && aylikOdemeGelirOrani <= 0.25m) return "Onay";

        if (basvuru.Gelir >= 12000 && gelirKrediOrani <= 2.5m && aylikOdemeGelirOrani <= 0.3m) return "İnceleme";

        return "İnceleme";
    }

    private string TasitKredisiOnayAlgoritmasi(decimal gelirKrediOrani, decimal aylikOdemeGelirOrani, Basvuru basvuru)
    {
        if (gelirKrediOrani > 5 || aylikOdemeGelirOrani > 0.45m || basvuru.Vade > 84) return "Red";

        if (basvuru.Gelir >= 25000 && gelirKrediOrani <= 3 && aylikOdemeGelirOrani <= 0.3m) return "Onay";

        if (basvuru.Gelir >= 15000 && gelirKrediOrani <= 4 && aylikOdemeGelirOrani <= 0.35m) return "İnceleme";

        return "İnceleme";
    }

    private string TicariKredisiOnayAlgoritmasi(decimal gelirKrediOrani, decimal aylikOdemeGelirOrani, Basvuru basvuru)
    {
        // Ticari kredi: En esnek kriterler ama yüksek gelir gereksinimi
        if (gelirKrediOrani > 6 || aylikOdemeGelirOrani > 0.5m || basvuru.Vade > 120) return "Red";

        // Güçlü profil: Gelir ≥50K, oran ≤4x, ödeme ≤%35
        if (basvuru.Gelir >= 50000 && gelirKrediOrani <= 4 && aylikOdemeGelirOrani <= 0.35m) return "Onay";

        // Orta profil: Gelir ≥30K, oran ≤5x, ödeme ≤%40
        if (basvuru.Gelir >= 30000 && gelirKrediOrani <= 5 && aylikOdemeGelirOrani <= 0.4m) return "İnceleme";

        return "İnceleme";
    }

    private string GenelKrediOnayAlgoritmasi(decimal gelirKrediOrani, decimal aylikOdemeGelirOrani, Basvuru basvuru)
    {
        if (gelirKrediOrani > 4 || aylikOdemeGelirOrani > 0.4m || basvuru.Vade > 60) return "Red";

        if (basvuru.Gelir >= 20000 && gelirKrediOrani <= 2.5m && aylikOdemeGelirOrani <= 0.25m) return "Onay";

        if (basvuru.Gelir >= 12000 && gelirKrediOrani <= 3 && aylikOdemeGelirOrani <= 0.3m) return "İnceleme";

        return "İnceleme";
    }

    public async Task<bool> BasvuruOnaylaAsync(int basvuruId)
    {
        var basvuru = await _db.Basvurular.FindAsync(basvuruId);
        if (basvuru == null) return false;

        basvuru.OnayDurumu = "Onay";

        _db.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Kredi başvurusu #{basvuruId} onaylandı."
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BasvuruReddetAsync(int basvuruId, string redNedeni)
    {
        var basvuru = await _db.Basvurular.FindAsync(basvuruId);
        if (basvuru == null) return false;

        basvuru.OnayDurumu = "Red";

        _db.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Kredi başvurusu #{basvuruId} reddedildi. Sebep: {redNedeni}"
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
