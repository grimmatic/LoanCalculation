using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public interface IKrediOnayService
{
    Task<string> OnayDurumuHesaplaAsync(Basvuru basvuru);
    Task<bool> BasvuruOnaylaAsync(int basvuruId);
    Task<bool> BasvuruReddetAsync(int basvuruId, string redNedeni);
}


