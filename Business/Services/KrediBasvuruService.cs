using LoanCalculation.Models.Entities;
using LoanCalculation.Data;
using LoanCalculation.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Business.Services;

public class KrediBasvuruService : IKrediBasvuruService
{
    private readonly AppDbContext _db;
    private readonly IKrediOnayService _onayService;
    
    public KrediBasvuruService(AppDbContext db, IKrediOnayService onayService)
    {
        _db = db;
        _onayService = onayService;
    }

    public async Task<(Basvuru basvuru, List<OdemePlani> plan)> BasvurVeKaydetAsync(KrediBasvuruIstek istek, CancellationToken ct, int? musteriId = null)
    {
        BankaUrunu? bankaUrunu = null;
        
        if (istek.BankaUrunId.HasValue)
        {
            bankaUrunu = await _db.BankaUrunleri
                .AsNoTracking()
                .Include(bu => bu.Banka)
                .Include(bu => bu.Urun)
                .FirstOrDefaultAsync(bu => bu.Id == istek.BankaUrunId && bu.Aktif, ct)
                ?? throw new InvalidOperationException("Banka ürünü bulunamadı veya aktif değil.");
        }

        if (bankaUrunu != null)
        {
            if (istek.KrediTutari < bankaUrunu.MinTutar || istek.KrediTutari > bankaUrunu.MaxTutar)
                throw new InvalidOperationException($"Kredi tutarı {bankaUrunu.MinTutar:N0} - {bankaUrunu.MaxTutar:N0} ₺ aralığında olmalıdır.");

            if (istek.KrediVadesi < bankaUrunu.MinVade || istek.KrediVadesi > bankaUrunu.MaxVade)
                throw new InvalidOperationException($"Kredi vadesi {bankaUrunu.MinVade} - {bankaUrunu.MaxVade} ay aralığında olmalıdır.");
        }

        var aylikOran = bankaUrunu != null ? (double)bankaUrunu.FaizOrani / 100d : 0; // Aylık faiz oranı
        var n = istek.KrediVadesi;
        var P = (double)istek.KrediTutari;

        var aylikOdeme = aylikOran == 0
            ? P / n
            : P * (aylikOran / (1 - Math.Pow(1 + aylikOran, -n)));

        var basvuru = new Basvuru
        {
            BankaUrunId = bankaUrunu?.Id,
            MusteriId = musteriId,
            UrunId = bankaUrunu?.UrunId,
            KrediTutari = istek.KrediTutari,
            Vade = istek.KrediVadesi,
            FaizOrani = bankaUrunu?.FaizOrani ?? 0,
            AylikOdeme = Math.Round((decimal)aylikOdeme, 2),
            ToplamOdeme = Math.Round((decimal)(aylikOdeme * n), 2),
            Gelir = istek.Gelir,
            OnayDurumu = "Beklemede",
            BasvuruTarihi = DateTime.UtcNow
        };

        // Onay algoritmasını çalıştır
        basvuru.OnayDurumu = await _onayService.OnayDurumuHesaplaAsync(basvuru);

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

        _db.Basvurular.Add(basvuru);
        await _db.SaveChangesAsync(ct);

        foreach (var p in plan) p.BasvuruId = basvuru.Id;
        _db.OdemePlanlari.AddRange(plan);

        _db.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Kredi başvurusu #{basvuru.Id} - Email: {istek.Email}, Ad Soyad: {istek.AdSoyad}, " +
                   $"Banka: {(bankaUrunu?.Banka?.Ad ?? "Manuel")}, Ürün: {(bankaUrunu?.Urun?.Ad ?? "Manuel")}, " +
                   $"Faiz: %{(bankaUrunu?.FaizOrani ?? 0)}, Tutar: {istek.KrediTutari:N2}, Vade: {n} ay, Gelir: {istek.Gelir:N2}, " +
                   $"Onay Durumu: {basvuru.OnayDurumu}"
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (basvuru, plan);
    }

    public async Task<BankaUrunu?> GetBankaUrunuAsync(int bankaUrunId)
    {
        return await _db.BankaUrunleri
            .Include(bu => bu.Banka)
            .Include(bu => bu.Urun)
            .FirstOrDefaultAsync(bu => bu.Id == bankaUrunId && bu.Aktif);
    }

    public async Task<List<object>> GetMusteriApplicationsAsync(int musteriId)
    {
        var applications = await _db.Basvurular
            .Where(h => h.MusteriId == musteriId && h.BankaUrunId != null)
            .Include(h => h.BankaUrunu)
                .ThenInclude(bu => bu!.Banka)
            .Include(h => h.BankaUrunu)
                .ThenInclude(bu => bu!.Urun)
            .OrderByDescending(h => h.BasvuruTarihi)
            .Select(h => new
            {
                Id = h.Id,
                BankaAdi = h.BankaUrunu!.Banka!.Ad,
                UrunAdi = h.BankaUrunu!.Urun!.Ad,
                KrediTutari = h.KrediTutari,
                KrediVadesi = h.Vade,
                BasvuruTarihi = h.BasvuruTarihi,
                OnayDurumu = h.OnayDurumu
            })
            .ToListAsync();

        return applications.Cast<object>().ToList();
    }
}