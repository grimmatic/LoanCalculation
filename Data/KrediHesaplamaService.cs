using LoanCalculation.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Data;

public class KrediHesaplamaService : IKrediHesaplamaService
{
    private readonly AppDbContext _db;
    public KrediHesaplamaService(AppDbContext db) => _db = db;

    public async Task<(Hesaplama hesaplama, List<OdemePlani> plan)> HesaplaVeKaydetAsync(HesaplamaIstek istek, CancellationToken ct)
    {
        var urun = await _db.Urunler.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == istek.UrunId && u.Aktif, ct)
            ?? throw new InvalidOperationException("Ürün bulunamadı veya aktif değil.");

        if (istek.Tutar < urun.MinTutar || istek.Tutar > urun.MaxTutar)
            throw new InvalidOperationException("Tutar ürün aralığı dışında.");
        if (istek.Vade < urun.MinVade || istek.Vade > urun.MaxVade)
            throw new InvalidOperationException("Vade ürün aralığı dışında.");

        // Aylık faiz oranı ve taksit tutarı
        var aylikOran = (double)urun.FaizOrani / 12d / 100d;
        var n = istek.Vade;
        var P = (double)istek.Tutar;

        var aylikOdeme = aylikOran == 0
            ? P / n
            : P * (aylikOran / (1 - Math.Pow(1 + aylikOran, -n)));

        var hesaplama = new Hesaplama
        {
            UrunId = urun.Id,
            KrediTutari = istek.Tutar,
            Vade = istek.Vade,
            FaizOrani = urun.FaizOrani,
            AylikOdeme = Math.Round((decimal)aylikOdeme, 2),
            ToplamOdeme = Math.Round((decimal)(aylikOdeme * n), 2),
            HesaplamaTarihi = DateTime.UtcNow
        };

        // İtfa planı
        var bakiye = P;
        var plan = new List<OdemePlani>(n);
        var baslangic = istek.BaslangicTarihi ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        for (int i = 1; i <= n; i++)
        {
            var faiz = bakiye * aylikOran;
            var anapara = aylikOdeme - faiz;
            bakiye -= anapara;

            plan.Add(new OdemePlani
            {
                TaksitNo = i,
                TaksitTutari = Math.Round((decimal)aylikOdeme, 2),
                AnaPara = Math.Round((decimal)anapara, 2),
                Faiz = Math.Round((decimal)faiz, 2),
                KalanBakiye = Math.Round((decimal)Math.Max(bakiye, 0), 2),
                VadeTarihi = baslangic.AddMonths(i)
            });
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        _db.Hesaplamalar.Add(hesaplama);
        await _db.SaveChangesAsync(ct);

        foreach (var p in plan) p.HesaplamaId = hesaplama.Id;
        _db.OdemePlanlari.AddRange(plan);

        _db.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Hesaplama #{hesaplama.Id} (ürün:{urun.Ad}, vade:{n}, tutar:{istek.Tutar}) oluşturuldu."
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (hesaplama, plan);
    }
}