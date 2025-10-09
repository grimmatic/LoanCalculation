using Microsoft.AspNetCore.Mvc;
using LoanCalculation.Business.Interfaces;
using LoanCalculation.Models.Entities;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMusteriService _musteriService;

    public AuthController(IMusteriService musteriService)
    {
        _musteriService = musteriService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Sifre))
            {
                return BadRequest(new { message = "Email ve şifre gereklidir." });
            }

            if (request.Sifre.Length < 6)
            {
                return BadRequest(new { message = "Şifre en az 6 karakter olmalıdır." });
            }

            // Email format kontrolü
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Geçerli bir email adresi giriniz." });
            }

            // Email zaten var mı kontrol et
            var existingMusteri = await _musteriService.GetMusteriByEmailAsync(request.Email);
            if (existingMusteri != null)
            {
                return BadRequest(new { message = "Bu email adresi zaten kayıtlı." });
            }

            // Tarih varsa UTC'ye çevir
            DateTime? dogumTarihiUtc = null;
            if (request.DogumTarihi.HasValue)
            {
                dogumTarihiUtc = DateTime.SpecifyKind(request.DogumTarihi.Value, DateTimeKind.Utc);
            }

            var musteri = await _musteriService.CreateMusteriAsync(
                request.Email,
                request.Sifre,
                request.AdSoyad,
                request.Telefon,
                dogumTarihiUtc,
                request.TcKimlikNo);

            HttpContext.Session.SetString("MusteriId", musteri.Id.ToString());
            HttpContext.Session.SetString("Email", musteri.Email);

            return Ok(new
            {
                message = "Kayıt başarılı!",
                musteriId = musteri.Id,
                email = musteri.Email,
                adSoyad = musteri.AdSoyad,
                telefon = musteri.Telefon,
                tcKimlikNo = musteri.TcKimlikNo,
                dogumTarihi = musteri.DogumTarihi,
                bankalar = new List<object>()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kayıt sırasında bir hata oluştu.", error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Sifre))
            {
                return BadRequest(new { message = "Email ve şifre gereklidir." });
            }

            var isValid = await _musteriService.ValidateMusteriAsync(request.Email, request.Sifre);
            if (!isValid)
            {
                return Unauthorized(new { message = "Email veya şifre hatalı." });
            }

            var musteri = await _musteriService.GetMusteriByEmailAsync(request.Email);
            if (musteri == null)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }

            HttpContext.Session.SetString("MusteriId", musteri.Id.ToString());
            HttpContext.Session.SetString("Email", musteri.Email);

            return Ok(new
            {
                message = "Giriş başarılı!",
                musteriId = musteri.Id,
                email = musteri.Email,
                adSoyad = musteri.AdSoyad,
                telefon = musteri.Telefon,
                tcKimlikNo = musteri.TcKimlikNo,
                dogumTarihi = musteri.DogumTarihi,
                bankalar = musteri.MusteriBankalar.Where(mb => mb.Aktif).Select(mb => new
                {
                    id = mb.Banka.Id,
                    ad = mb.Banka.Ad,
                    uyelikTarihi = mb.UyelikTarihi
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu." });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Çıkış başarılı!" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var musteriIdStr = HttpContext.Session.GetString("MusteriId");
        if (string.IsNullOrEmpty(musteriIdStr))
        {
            return Unauthorized(new { message = "Oturum bulunamadı." });
        }

        var email = HttpContext.Session.GetString("Email");
        var musteri = await _musteriService.GetMusteriByEmailAsync(email!);

        if (musteri == null)
        {
            return Unauthorized(new { message = "Kullanıcı bulunamadı." });
        }

        return Ok(new
        {
            musteriId = musteri.Id,
            email = musteri.Email,
            adSoyad = musteri.AdSoyad,
            telefon = musteri.Telefon,
            tcKimlikNo = musteri.TcKimlikNo,
            kayitTarihi = musteri.KayitTarihi,
            bankalar = musteri.MusteriBankalar.Where(mb => mb.Aktif).Select(mb => new
            {
                id = mb.Banka.Id,
                ad = mb.Banka.Ad,
                uyelikTarihi = mb.UyelikTarihi
            }).ToList()
        });
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class SignUpRequest
{
    public string Email { get; set; } = null!;
    public string? AdSoyad { get; set; }
    public string? Telefon { get; set; }
    public DateTime? DogumTarihi { get; set; }
    public string? TcKimlikNo { get; set; }
    public string Sifre { get; set; } = null!;
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Sifre { get; set; } = null!;
}
