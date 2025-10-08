using Microsoft.EntityFrameworkCore;
using LoanCalculation.Models.Entities;

namespace LoanCalculation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Banka> Bankalar => Set<Banka>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<BankaUrunu> BankaUrunleri => Set<BankaUrunu>();
    public DbSet<Basvuru> Basvurular => Set<Basvuru>();
    public DbSet<OdemePlani> OdemePlanlari => Set<OdemePlani>();
    public DbSet<LogKaydi> Loglar => Set<LogKaydi>();
    public DbSet<Musteri> Musteriler => Set<Musteri>();
    public DbSet<MusteriBanka> MusteriBankalar => Set<MusteriBanka>();

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

        modelBuilder.Entity<Basvuru>()
            .HasOne(h => h.BankaUrunu)
            .WithMany(bu => bu.Basvurular)
            .HasForeignKey(h => h.BankaUrunId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OdemePlani>()
            .HasOne<Basvuru>()
            .WithMany()
            .HasForeignKey(o => o.BasvuruId)
            .OnDelete(DeleteBehavior.Cascade);

        // Müşteri-Banka ilişkileri
        modelBuilder.Entity<MusteriBanka>()
            .HasOne(mb => mb.Musteri)
            .WithMany(m => m.MusteriBankalar)
            .HasForeignKey(mb => mb.MusteriId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MusteriBanka>()
            .HasOne(mb => mb.Banka)
            .WithMany()
            .HasForeignKey(mb => mb.BankaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Her müşteri için aynı bankaya sadece bir kez üye olabilir
        modelBuilder.Entity<MusteriBanka>()
            .HasIndex(mb => new { mb.MusteriId, mb.BankaId })
            .IsUnique();

        // Müşteri-Başvuru ilişkisi
        modelBuilder.Entity<Basvuru>()
            .HasOne(h => h.Musteri)
            .WithMany(m => m.Basvurular)
            .HasForeignKey(h => h.MusteriId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Email benzersiz olmalı
        modelBuilder.Entity<Musteri>()
            .HasIndex(m => m.Email)
            .IsUnique();
    }
}