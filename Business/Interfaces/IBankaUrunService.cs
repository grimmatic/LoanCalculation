using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public record BankaUrunDto(
    int Id,
    int BankaId,
    string BankaAdi,
    int UrunId,
    string UrunAdi,
    decimal FaizOrani,
    decimal MinTutar,
    decimal MaxTutar,
    int MinVade,
    int MaxVade);

public interface IBankaUrunService
{
  Task<List<BankaUrunDto>> GetBankaUrunleriAsync(int bankaId, CancellationToken ct);
  Task<BankaUrunDto?> GetBankaUrunuAsync(int id, CancellationToken ct);
  Task<List<BankaUrunDto>> GetAllAktifUrunlerAsync(CancellationToken ct);
  Task<BankaUrunu?> GetBankaUrunuEntityAsync(int id, CancellationToken ct);
}