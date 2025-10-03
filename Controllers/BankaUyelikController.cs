using Microsoft.AspNetCore.Mvc;
using LoanCalculation.Business.Interfaces;
using LoanCalculation.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/banka-uyelik")]
public class BankaUyelikController : ControllerBase
{
    private readonly IMusteriService _musteriService;
    private readonly AppDbContext _context;

    public BankaUyelikController(IMusteriService musteriService, AppDbContext context)
    {
        _musteriService = musteriService;
        _context = context;
    }

    [HttpGet("available-banks")]
    public async Task<IActionResult> GetAvailableBanks()
    {
        var musteriIdStr = HttpContext.Session.GetString("MusteriId");
        if (string.IsNullOrEmpty(musteriIdStr))
        {
            return Unauthorized(new { message = "Oturum bulunamadı." });
        }

        var musteriId = int.Parse(musteriIdStr);

        // Müşterinin üye olmadığı bankaları getir
        var availableBanks = await _context.Bankalar
            .Where(b => b.Aktif &&
                       !_context.MusteriBankalar
                           .Any(mb => mb.MusteriId == musteriId && mb.BankaId == b.Id && mb.Aktif))
            .Select(b => new { b.Id, b.Ad, b.Kod })
            .ToListAsync();

        return Ok(availableBanks);
    }

    [HttpGet("my-banks")]
    public async Task<IActionResult> GetMyBanks()
    {
        var musteriIdStr = HttpContext.Session.GetString("MusteriId");
        if (string.IsNullOrEmpty(musteriIdStr))
        {
            return Unauthorized(new { message = "Oturum bulunamadı." });
        }

        var musteriId = int.Parse(musteriIdStr);
        var bankalar = await _musteriService.GetMusteriBankalarAsync(musteriId);

        var result = bankalar.Select(b => new
        {
            b.Id,
            b.Ad,
            b.Kod,
            uyelikTarihi = _context.MusteriBankalar
                .First(mb => mb.MusteriId == musteriId && mb.BankaId == b.Id)
                .UyelikTarihi
        }).ToList();

        return Ok(result);
    }

    [HttpPost("join/{bankaId}")]
    public async Task<IActionResult> JoinBank(int bankaId)
    {
        var musteriIdStr = HttpContext.Session.GetString("MusteriId");
        if (string.IsNullOrEmpty(musteriIdStr))
        {
            return Unauthorized(new { message = "Oturum bulunamadı." });
        }

        var musteriId = int.Parse(musteriIdStr);

        // Banka var mı kontrol et
        var banka = await _context.Bankalar.FindAsync(bankaId);
        if (banka == null || !banka.Aktif)
        {
            return NotFound(new { message = "Banka bulunamadı." });
        }

        // Zaten üye mi kontrol et
        var isAlreadyMember = await _musteriService.IsMusteriMemberOfBankaAsync(musteriId, bankaId);
        if (isAlreadyMember)
        {
            return BadRequest(new { message = "Bu bankaya zaten üyesiniz." });
        }

        var success = await _musteriService.AddMusteriToBankaAsync(musteriId, bankaId);
        if (success)
        {
            return Ok(new { message = $"{banka.Ad} bankasına başarıyla üye oldunuz." });
        }

        return BadRequest(new { message = "Üyelik işlemi başarısız." });
    }

    [HttpDelete("leave/{bankaId}")]
    public async Task<IActionResult> LeaveBank(int bankaId)
    {
        var musteriIdStr = HttpContext.Session.GetString("MusteriId");
        if (string.IsNullOrEmpty(musteriIdStr))
        {
            return Unauthorized(new { message = "Oturum bulunamadı." });
        }

        var musteriId = int.Parse(musteriIdStr);

        var banka = await _context.Bankalar.FindAsync(bankaId);
        if (banka == null)
        {
            return NotFound(new { message = "Banka bulunamadı." });
        }

        var isMember = await _musteriService.IsMusteriMemberOfBankaAsync(musteriId, bankaId);
        if (!isMember)
        {
            return BadRequest(new { message = "Bu bankaya üye değilsiniz." });
        }

        var success = await _musteriService.RemoveMusteriFromBankaAsync(musteriId, bankaId);
        if (success)
        {
            return Ok(new { message = $"{banka.Ad} bankasından ayrıldınız." });
        }

        return BadRequest(new { message = "Bu bankaya üye değilsiniz." });
    }
}
