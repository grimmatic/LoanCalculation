using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public record KrediBasvuruIstek(
    string Email,
    string AdSoyad,
    int? BankaUrunId,
    decimal KrediTutari,
    int KrediVadesi,
    decimal Gelir,
    DateOnly? BaslangicTarihi = null);

public interface IKrediBasvuruService
{
    Task<(Basvuru basvuru, List<OdemePlani> plan)> BasvurVeKaydetAsync(KrediBasvuruIstek istek, CancellationToken ct, int? musteriId = null);
    Task<BankaUrunu?> GetBankaUrunuAsync(int bankaUrunId);
    Task<List<object>> GetMusteriApplicationsAsync(int musteriId);
}