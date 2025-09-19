using LoanCalculation.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/banka-urunleri")]
public class BankaUrunleriController : ControllerBase
{
    private readonly IBankaUrunService _service;
    
    public BankaUrunleriController(IBankaUrunService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAllUrunler(CancellationToken ct)
    {
        var urunler = await _service.GetAllAktifUrunlerAsync(ct);
        return Ok(urunler);
    }

    [HttpGet("banka/{bankaId}")]
    public async Task<IActionResult> GetBankaUrunleri(int bankaId, CancellationToken ct)
    {
        var urunler = await _service.GetBankaUrunleriAsync(bankaId, ct);
        return Ok(urunler);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUrun(int id, CancellationToken ct)
    {
        var urun = await _service.GetBankaUrunuAsync(id, ct);
        if (urun == null)
            return NotFound(new { message = "Ürün bulunamadı" });
        return Ok(urun);
    }
}