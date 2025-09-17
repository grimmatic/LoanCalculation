using Microsoft.EntityFrameworkCore;
using LoanCalculation.Models.Entities;

namespace LoanCalculation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Banka> Bankalar => Set<Banka>();
    public DbSet<Hesaplama> Hesaplamalar => Set<Hesaplama>();
    public DbSet<OdemePlani> OdemePlanlari => Set<OdemePlani>();
    public DbSet<LogKaydi> Loglar => Set<LogKaydi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Hesaplama>()
            .HasOne<Urun>()
            .WithMany()
            .HasForeignKey(h => h.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OdemePlani>()
            .HasOne<Hesaplama>()
            .WithMany()
            .HasForeignKey(o => o.HesaplamaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}