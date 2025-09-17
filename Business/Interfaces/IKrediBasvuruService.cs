using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public record KrediBasvuruIstek(
    string Email,
    string AdSoyad, 
    int BankaId,
    int UrunId,
    decimal KrediTutari,
    int KrediVadesi,
    DateOnly? BaslangicTarihi = null);

public interface IKrediBasvuruService
{
    Task<(Hesaplama hesaplama, List<OdemePlani> plan)> BasvurVeKaydetAsync(KrediBasvuruIstek istek, CancellationToken ct);
}