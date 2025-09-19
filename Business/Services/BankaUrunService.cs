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
            .Include(bu => bu.UrunTipi)
            .Where(bu => bu.BankaId == bankaId && bu.Aktif)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunTipiId,
                bu.UrunTipi.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade,
                bu.KampanyaAdi
            ))
            .ToListAsync(ct);
    }

    public async Task<BankaUrunDto?> GetBankaUrunuAsync(int id, CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.UrunTipi)
            .Where(bu => bu.Id == id && bu.Aktif)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunTipiId,
                bu.UrunTipi.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade,
                bu.KampanyaAdi
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<BankaUrunDto>> GetAllAktifUrunlerAsync(CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.UrunTipi)
            .Where(bu => bu.Aktif && bu.Banka.Aktif && bu.UrunTipi.Aktif)
            .OrderBy(bu => bu.Banka.Ad)
            .ThenBy(bu => bu.UrunTipi.Ad)
            .Select(bu => new BankaUrunDto(
                bu.Id,
                bu.BankaId,
                bu.Banka.Ad,
                bu.UrunTipiId,
                bu.UrunTipi.Ad,
                bu.FaizOrani,
                bu.MinTutar,
                bu.MaxTutar,
                bu.MinVade,
                bu.MaxVade,
                bu.KampanyaAdi
            ))
            .ToListAsync(ct);
    }

    public async Task<BankaUrunu?> GetBankaUrunuEntityAsync(int id, CancellationToken ct)
    {
        return await _db.BankaUrunleri
            .AsNoTracking()
            .Include(bu => bu.Banka)
            .Include(bu => bu.UrunTipi)
            .FirstOrDefaultAsync(bu => bu.Id == id && bu.Aktif, ct);
    }
}