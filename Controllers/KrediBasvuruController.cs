using LoanCalculation.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/kredi-basvuru")]
public class KrediBasvuruController : ControllerBase
{
    private readonly IKrediBasvuruService _svc;
    public KrediBasvuruController(IKrediBasvuruService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Basvur([FromBody] KrediBasvuruIstek istek, CancellationToken ct)
    {
        var (hesaplama, plan) = await _svc.BasvurVeKaydetAsync(istek, ct);
        return Ok(new { 
            message = "Kredi başvurunuz başarıyla alındı.",
            hesaplamaId = hesaplama.Id,
            hesaplama, 
            plan 
        });
    }
}