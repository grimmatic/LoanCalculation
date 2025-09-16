using LoanCalculation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/urunler")]
public class UrunlerController : ControllerBase
{
    private readonly AppDbContext _db;
    public UrunlerController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAktifUrunler(CancellationToken ct) =>
        Ok(await _db.Urunler.AsNoTracking().Where(u => u.Aktif).ToListAsync(ct));
}