using LoanCalculation.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/kredi-hesapla")]
public class KrediHesaplamaController : ControllerBase
{
    private readonly IKrediHesaplamaService _svc;
    public KrediHesaplamaController(IKrediHesaplamaService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Hesapla([FromBody] KrediHesaplamaIstek istek, CancellationToken ct)
    {
        var plan = await _svc.HesaplaAsync(istek, ct);
        
        var aylikOdeme = plan.FirstOrDefault()?.TaksitTutari ?? 0;
        var toplamOdeme = plan.Sum(p => p.TaksitTutari);
        var toplamFaiz = plan.Sum(p => p.Faiz);
        
        return Ok(new { 
            plan,
            ozet = new {
                krediTutari = istek.Tutar,
                vade = istek.Vade,
                faizOrani = istek.FaizOrani,
                aylikOdeme,
                toplamOdeme,
                toplamFaiz
            }
        });
    }
}