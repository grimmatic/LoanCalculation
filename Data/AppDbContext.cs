using Microsoft.EntityFrameworkCore;
using LoanCalculation.Models.Entities;

namespace LoanCalculation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Banka> Bankalar => Set<Banka>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<BankaUrunu> BankaUrunleri => Set<BankaUrunu>();
    public DbSet<Hesaplama> Hesaplamalar => Set<Hesaplama>();
    public DbSet<OdemePlani> OdemePlanlari => Set<OdemePlani>();
    public DbSet<LogKaydi> Loglar => Set<LogKaydi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<BankaUrunu>()
            .HasOne(bu => bu.Banka)
            .WithMany(b => b.BankaUrunleri)
            .HasForeignKey(bu => bu.BankaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BankaUrunu>()
            .HasOne(bu => bu.Urun)
            .WithMany()
            .HasForeignKey(bu => bu.UrunId)
            .OnDelete(DeleteBehavior.Restrict);

        // Her banka için aynı üründen sadece bir tane olabilir
        modelBuilder.Entity<BankaUrunu>()
            .HasIndex(bu => new { bu.BankaId, bu.UrunId })
            .IsUnique();

        modelBuilder.Entity<Hesaplama>()
            .HasOne(h => h.BankaUrunu)
            .WithMany(bu => bu.Hesaplamalar)
            .HasForeignKey(h => h.BankaUrunId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OdemePlani>()
            .HasOne<Hesaplama>()
            .WithMany()
            .HasForeignKey(o => o.HesaplamaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}