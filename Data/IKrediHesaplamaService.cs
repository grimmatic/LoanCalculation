using LoanCalculation.Models;

namespace LoanCalculation.Data;

public record HesaplamaIstek(int UrunId, decimal Tutar, int Vade, DateOnly? BaslangicTarihi = null);

public interface IKrediHesaplamaService
{
    Task<(Hesaplama hesaplama, List<OdemePlani> plan)> HesaplaVeKaydetAsync(HesaplamaIstek istek, CancellationToken ct);
}