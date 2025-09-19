using LoanCalculation.Models.Entities;
using LoanCalculation.Data;
using LoanCalculation.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Business.Services;

public class BankaUrunService : IBankaUrunService
{
    private readonly AppDbContext _db;
    
    public BankaUrunService(AppDbContext db) => _db = db;

    public async Task<List<BankaUrunDto>> GetBankaUrunleriAsync(int bankaId, CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.Urun)
            .Where(bu => bu.BankaId == bankaId && bu.Aktif)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunId,
                bu.Urun.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade
            ))
            .ToListAsync(ct);
    }

    public async Task<BankaUrunDto?> GetBankaUrunuAsync(int id, CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.Urun)
            .Where(bu => bu.Id == id && bu.Aktif)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunId,
                bu.Urun.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<BankaUrunDto>> GetAllAktifUrunlerAsync(CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.Urun)
            .Where(bu => bu.Aktif && bu.Banka.Aktif && bu.Urun.Aktif)
            .OrderBy(bu => bu.Banka.Ad)
            .ThenBy(bu => bu.Urun.Ad)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunId,
                bu.Urun.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade
            ))
            .ToListAsync(ct);
    }

    public async Task<BankaUrunu?> GetBankaUrunuEntityAsync(int id, CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.Urun)
            .FirstOrDefaultAsync(bu => bu.Id == id && bu.Aktif, ct);
    }
}