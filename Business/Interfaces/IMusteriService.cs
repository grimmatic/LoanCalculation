using LoanCalculation.Models.Entities;

namespace LoanCalculation.Business.Interfaces;

public interface IMusteriService
{
    Task<Musteri?> GetMusteriByEmailAsync(string email);
    Task<Musteri> CreateMusteriAsync(string email, string sifre);
    Task<bool> ValidateMusteriAsync(string email, string sifre);
    Task<List<Banka>> GetMusteriBankalarAsync(int musteriId);
    Task<bool> AddMusteriToBankaAsync(int musteriId, int bankaId);
    Task<bool> RemoveMusteriFromBankaAsync(int musteriId, int bankaId);
    Task<bool> IsMusteriMemberOfBankaAsync(int musteriId, int bankaId);
    Task<bool> HasMusteriAppliedToBankaUrunAsync(int musteriId, int bankaUrunId);
}
