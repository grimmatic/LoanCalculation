using LoanCalculation.Models.Entities;

public interface IBankaUrunService
{
  Task<List<BankaUrunDto>> GetBankaUrunleriAsync(int bankaId, CancellationToken ct);
    Task<BankaUrunDto?> GetBankaUrunuAsync(int id, CancellationToken ct);
    Task<List<BankaUrunDto>> GetAllAktifUrunlerAsync(CancellationToken ct);
    Task<BankaUrunu?> GetBankaUrunuEntityAsync(int id, CancellationToken ct);
}

public record BankaUrunDto(
    int Id,
    int BankaId,
    string BankaAdi,
    int UrunTipiId,
    string UrunAdi,
    decimal FaizOrani,
    decimal MinTutar,
    decimal MaxTutar,
    int MinVade,
    int MaxVade,
    string? KampanyaAdi);