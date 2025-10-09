using Microsoft.EntityFrameworkCore;
using LoanCalculation.Data;
using LoanCalculation.Models.Entities;
using LoanCalculation.Business.Interfaces;

namespace LoanCalculation.Business.Services;

public class MusteriService : IMusteriService
{
    private readonly AppDbContext _context;

    public MusteriService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Musteri?> GetMusteriByEmailAsync(string email)
    {
        return await _context.Musteriler
            .Include(m => m.MusteriBankalar)
            .ThenInclude(mb => mb.Banka)
            .FirstOrDefaultAsync(m => m.Email == email && m.Aktif);
    }

    public async Task<Musteri> CreateMusteriAsync(string email, string sifre, string? adSoyad = null, string? telefon = null, DateTime? dogumTarihi = null, string? tcKimlikNo = null)
    {
        var musteri = new Musteri
        {
            Email = email.ToLowerInvariant(),
            AdSoyad = adSoyad ?? "",
            Telefon = telefon,
            DogumTarihi = dogumTarihi,
            TcKimlikNo = tcKimlikNo,
            Sifre = HashPassword(sifre),
            KayitTarihi = DateTime.UtcNow,
            Aktif = true
        };

        _context.Musteriler.Add(musteri);
        await _context.SaveChangesAsync();
        return musteri;
    }

    public async Task<bool> ValidateMusteriAsync(string email, string sifre)
    {
        var musteri = await _context.Musteriler
            .FirstOrDefaultAsync(m => m.Email == email.ToLowerInvariant() && m.Aktif);

        if (musteri == null)
            return false;

        return VerifyPassword(sifre, musteri.Sifre);
    }

    public async Task<List<Banka>> GetMusteriBankalarAsync(int musteriId)
    {
        return await _context.MusteriBankalar
            .Where(mb => mb.MusteriId == musteriId && mb.Aktif)
            .Include(mb => mb.Banka)
            .Select(mb => mb.Banka)
            .ToListAsync();
    }

    public async Task<bool> AddMusteriToBankaAsync(int musteriId, int bankaId)
    {
        // Zaten üye mi kontrol et
        var existingMembership = await _context.MusteriBankalar
            .FirstOrDefaultAsync(mb => mb.MusteriId == musteriId && mb.BankaId == bankaId);

        if (existingMembership != null)
        {
            if (!existingMembership.Aktif)
            {
                existingMembership.Aktif = true;
                existingMembership.UyelikTarihi = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var banka = await _context.Bankalar.FindAsync(bankaId);
                var musteri = await _context.Musteriler.FindAsync(musteriId);
                _context.Loglar.Add(new LogKaydi
                {
                    Seviye = "Info",
                    Mesaj = $"Müşteri banka üyeliği yeniden aktifleştirildi - Müşteri: {musteri?.Email}, Banka: {banka?.Ad}"
                });
                await _context.SaveChangesAsync();
                return true;
            }
            return false; // Zaten aktif üye
        }

        var musteriBanka = new MusteriBanka
        {
            MusteriId = musteriId,
            BankaId = bankaId,
            UyelikTarihi = DateTime.UtcNow,
            Aktif = true
        };

        _context.MusteriBankalar.Add(musteriBanka);
        await _context.SaveChangesAsync();

        var bankaInfo = await _context.Bankalar.FindAsync(bankaId);
        var musteriInfo = await _context.Musteriler.FindAsync(musteriId);
        _context.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Müşteri banka üyeliği oluşturuldu - Müşteri: {musteriInfo?.Email}, Banka: {bankaInfo?.Ad}"
        });
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveMusteriFromBankaAsync(int musteriId, int bankaId)
    {
        var musteriBanka = await _context.MusteriBankalar
            .FirstOrDefaultAsync(mb => mb.MusteriId == musteriId && mb.BankaId == bankaId);

        if (musteriBanka == null)
            return false;

        musteriBanka.Aktif = false;
        await _context.SaveChangesAsync();

        // Log kaydı ekle
        var banka = await _context.Bankalar.FindAsync(bankaId);
        var musteri = await _context.Musteriler.FindAsync(musteriId);
        _context.Loglar.Add(new LogKaydi
        {
            Seviye = "Info",
            Mesaj = $"Müşteri banka üyeliğinden ayrıldı - Müşteri: {musteri?.Email}, Banka: {banka?.Ad}"
        });
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsMusteriMemberOfBankaAsync(int musteriId, int bankaId)
    {
        return await _context.MusteriBankalar
            .AnyAsync(mb => mb.MusteriId == musteriId && mb.BankaId == bankaId && mb.Aktif);
    }

    public async Task<bool> HasMusteriAppliedToBankaUrunAsync(int musteriId, int bankaUrunId)
    {
        return await _context.Basvurular
            .AnyAsync(h => h.MusteriId == musteriId && h.BankaUrunId == bankaUrunId);
    }

    private string HashPassword(string password)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        return hashedInput == hashedPassword;
    }
}
