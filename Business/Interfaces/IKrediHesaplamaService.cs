using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public record HesaplamaIstek(int UrunId, decimal Tutar, int Vade, DateOnly? BaslangicTarihi = null);

public interface IKrediHesaplamaService
{
    Task<(Hesaplama hesaplama, List<OdemePlani> plan)> HesaplaVeKaydetAsync(HesaplamaIstek istek, CancellationToken ct);
}