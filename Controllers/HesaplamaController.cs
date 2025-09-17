using LoanCalculation.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/hesaplama")]
public class HesaplamaController : ControllerBase
{
    private readonly IKrediHesaplamaService _svc;
    public HesaplamaController(IKrediHesaplamaService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Hesapla([FromBody] HesaplamaIstek istek, CancellationToken ct)
    {
        var (hesaplama, plan) = await _svc.HesaplaVeKaydetAsync(istek, ct);
        return Ok(new { hesaplama, plan });
    }
}