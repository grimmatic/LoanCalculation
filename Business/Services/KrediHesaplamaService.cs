using LoanCalculation.Models.Entities;
using LoanCalculation.Business.Interfaces;
using LoanCalculation.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Business.Services;

public class KrediHesaplamaService : IKrediHesaplamaService
{
    private readonly AppDbContext _db;

    public KrediHesaplamaService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<List<OdemePlani>> HesaplaAsync(KrediHesaplamaIstek istek, CancellationToken ct)
    {
        if (istek.BankaUrunId.HasValue)
        {
            var bankaUrunu = await _db.BankaUrunleri.FindAsync([istek.BankaUrunId.Value], ct);
            if (bankaUrunu != null)
            {
                if (istek.Tutar < bankaUrunu.MinTutar || istek.Tutar > bankaUrunu.MaxTutar)
                    throw new InvalidOperationException($"Kredi tutarı {bankaUrunu.MinTutar:N0} - {bankaUrunu.MaxTutar:N0} ₺ aralığında olmalıdır.");

                if (istek.Vade < bankaUrunu.MinVade || istek.Vade > bankaUrunu.MaxVade)
                    throw new InvalidOperationException($"Kredi vadesi {bankaUrunu.MinVade} - {bankaUrunu.MaxVade} ay aralığında olmalıdır.");
            }
        }

        var aylikOran = (double)istek.FaizOrani  / 100d; // Aylık faiz oranı (yıllık faiz oranı / 12 / 100)
        var n = istek.Vade;
        var P = (double)istek.Tutar;

        var aylikOdeme = aylikOran == 0
            ? P / n
            : P * (aylikOran / (1 - Math.Pow(1 + aylikOran, -n)));

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

        var aylikOdemeTutari = plan.FirstOrDefault()?.TaksitTutari ?? 0;
        var toplamOdemeTutari = plan.Sum(p => p.TaksitTutari);

        _db.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Kredi hesaplama - Tutar: {istek.Tutar:N2} ₺, Vade: {istek.Vade} ay, Faiz: %{istek.FaizOrani}, " +
                   $"Aylık Ödeme: {aylikOdemeTutari:N2} ₺, Toplam Ödeme: {toplamOdemeTutari:N2} ₺"
        });

        await _db.SaveChangesAsync(ct);

        return await Task.FromResult(plan);
    }
}