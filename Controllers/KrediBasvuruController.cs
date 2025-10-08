using LoanCalculation.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/kredi-basvuru")]
public class KrediBasvuruController : ControllerBase
{
    private readonly IKrediBasvuruService _svc;
    private readonly IMusteriService _musteriService;
    
    public KrediBasvuruController(IKrediBasvuruService svc, IMusteriService musteriService)
    {
        _svc = svc;
        _musteriService = musteriService;
    }

    [HttpPost]
    public async Task<IActionResult> Basvur([FromBody] KrediBasvuruIstek istek, CancellationToken ct)
    {
        try
        {
            // Müşteri giriş yapmış mı kontrol et
            var musteriIdStr = HttpContext.Session.GetString("MusteriId");
            if (string.IsNullOrEmpty(musteriIdStr))
            {
                return Unauthorized(new { message = "Başvuru yapmak için giriş yapmalısınız." });
            }

            var musteriId = int.Parse(musteriIdStr);

            // Müşteri bu bankaya üye mi kontrol et
            if (istek.BankaUrunId.HasValue)
            {
                // BankaUrunId'den banka bilgisini al
                var bankaUrunu = await _svc.GetBankaUrunuAsync(istek.BankaUrunId.Value);
                if (bankaUrunu == null)
                {
                    return BadRequest(new { message = "Seçilen banka ürünü bulunamadı." });
                }

                var isMember = await _musteriService.IsMusteriMemberOfBankaAsync(musteriId, bankaUrunu.BankaId);
                if (!isMember)
                {
                    return BadRequest(new { message = "Bu bankaya başvuru yapmak için önce banka müşterisi olmalısınız." });
                }

                // Daha önce aynı krediye başvurmuş mu kontrol et
                var hasApplied = await _musteriService.HasMusteriAppliedToBankaUrunAsync(musteriId, istek.BankaUrunId.Value);
                if (hasApplied)
                {
                    return BadRequest(new { message = "Bu kredi ürününe daha önce başvuru yapmışsınız." });
                }
            }

            var (basvuru, plan) = await _svc.BasvurVeKaydetAsync(istek, ct, musteriId);
            return Ok(new {
                message = "Kredi başvurunuz başarıyla alındı.",
                basvuruId = basvuru.Id,
                basvuru,
                plan
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bir hata oluştu.", detail = ex.Message });
        }
    }

    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        try
        {
            // Müşteri giriş yapmış mı kontrol et
            var musteriIdStr = HttpContext.Session.GetString("MusteriId");
            if (string.IsNullOrEmpty(musteriIdStr))
            {
                return Unauthorized(new { message = "Geçmiş başvurularınızı görmek için giriş yapmalısınız." });
            }

            var musteriId = int.Parse(musteriIdStr);
            var applications = await _svc.GetMusteriApplicationsAsync(musteriId);
            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bir hata oluştu.", detail = ex.Message });
        }
    }
}