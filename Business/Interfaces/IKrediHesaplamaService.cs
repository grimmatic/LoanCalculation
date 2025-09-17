using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public record KrediHesaplamaIstek(decimal Tutar, int Vade, decimal FaizOrani, DateOnly? BaslangicTarihi = null);

public interface IKrediHesaplamaService
{
    Task<List<OdemePlani>> HesaplaAsync(KrediHesaplamaIstek istek, CancellationToken ct);
}