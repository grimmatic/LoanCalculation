using LoanCalculation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanCalculation.Controllers;

[ApiController]
[Route("api/bankalar")]
public class BankalarController : ControllerBase
{
    private readonly AppDbContext _db;
    public BankalarController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAktifBankalar(CancellationToken ct) =>
        Ok(await _db.Bankalar.AsNoTracking().Where(b => b.Aktif).ToListAsync(ct));
}