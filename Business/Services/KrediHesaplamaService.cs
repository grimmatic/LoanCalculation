using LoanCalculation.Models.Entities;
using LoanCalculation.Business.Interfaces;

namespace LoanCalculation.Business.Services;

public class KrediHesaplamaService : IKrediHesaplamaService
{
    public async Task<List<OdemePlani>> HesaplaAsync(KrediHesaplamaIstek istek, CancellationToken ct)
    {
        var aylikOran = (double)istek.FaizOrani  / 100d; // Aylık faiz oranı (yıllık faiz oranı / 12 / 100)
        var n = istek.Vade;
        var P = (double)istek.Tutar;

        var aylikOdeme = aylikOran == 0
            ? P / n
            : P * (aylikOran / (1 - Math.Pow(1 + aylikOran, -n)));

        var bakiye = P;
        var plan = new List<OdemePlani>(n);
        var baslangic = istek.BaslangicTarihi ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        for (int i = 1; i <= n; i++)
        {
            var faiz = bakiye * aylikOran;
            var anapara = aylikOdeme - faiz;
            bakiye -= anapara;

            plan.Add(new OdemePlani
            {
                TaksitNo = i,
                TaksitTutari = Math.Round((decimal)aylikOdeme, 2),
                AnaPara = Math.Round((decimal)anapara, 2),
                Faiz = Math.Round((decimal)faiz, 2),
                KalanBakiye = Math.Round((decimal)Math.Max(bakiye, 0), 2),
                VadeTarihi = baslangic.AddMonths(i)
            });
        }

        return await Task.FromResult(plan);
    }
}