using LoanCalculation.Models.Entities;
using LoanCalculation.Data;
using LoanCalculation.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Business.Services;

public class KrediBasvuruService : IKrediBasvuruService
{
    private readonly AppDbContext _db;
    public KrediBasvuruService(AppDbContext db) => _db = db;

    public async Task<(Hesaplama hesaplama, List<OdemePlani> plan)> BasvurVeKaydetAsync(KrediBasvuruIstek istek, CancellationToken ct)
    {
        var urun = await _db.Urunler.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == istek.UrunId && u.Aktif, ct)
            ?? throw new InvalidOperationException("Ürün bulunamadı veya aktif değil.");

        if (istek.KrediTutari < urun.MinTutar || istek.KrediTutari > urun.MaxTutar)
            throw new InvalidOperationException("Kredi tutarı ürün aralığı dışında.");
        if (istek.KrediVadesi < urun.MinVade || istek.KrediVadesi > urun.MaxVade)
            throw new InvalidOperationException("Kredi vadesi ürün aralığı dışında.");

        var aylikOran = (double)urun.FaizOrani / 100d; // Aylık faiz oranı yıllık için / 12d eklenebilir
        var n = istek.KrediVadesi;
        var P = (double)istek.KrediTutari;

        var aylikOdeme = aylikOran == 0
            ? P / n
            : P * (aylikOran / (1 - Math.Pow(1 + aylikOran, -n)));

        var hesaplama = new Hesaplama
        {
            UrunId = urun.Id,
            KrediTutari = istek.KrediTutari,
            Vade = istek.KrediVadesi,
            FaizOrani = urun.FaizOrani,
            AylikOdeme = Math.Round((decimal)aylikOdeme, 2),
            ToplamOdeme = Math.Round((decimal)(aylikOdeme * n), 2),
            HesaplamaTarihi = DateTime.UtcNow
        };

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
            Mesaj = $"Kredi başvurusu #{hesaplama.Id} - Email: {istek.Email}, Ad Soyad: {istek.AdSoyad}, Ürün: {urun.Ad}, Tutar: {istek.KrediTutari}, Vade: {n} ay"
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (hesaplama, plan);
    }
}