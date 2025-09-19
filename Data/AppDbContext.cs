using Microsoft.EntityFrameworkCore;
using LoanCalculation.Models.Entities;

namespace LoanCalculation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Banka> Bankalar => Set<Banka>();
    public DbSet<UrunTipi> UrunTipleri => Set<UrunTipi>();
    public DbSet<BankaUrunu> BankaUrunleri => Set<BankaUrunu>();
    public DbSet<Hesaplama> Hesaplamalar => Set<Hesaplama>();
    public DbSet<OdemePlani> OdemePlanlari => Set<OdemePlani>();
    public DbSet<LogKaydi> Loglar => Set<LogKaydi>();
    
    public DbSet<Urun> Urunler => Set<Urun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<BankaUrunu>()
            .HasOne(bu => bu.Banka)
            .WithMany(b => b.BankaUrunleri)
            .HasForeignKey(bu => bu.BankaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BankaUrunu>()
            .HasOne(bu => bu.UrunTipi)
            .WithMany(ut => ut.BankaUrunleri)
            .HasForeignKey(bu => bu.UrunTipiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Her banka için aynı ürün tipinden sadece bir tane olabilir
        modelBuilder.Entity<BankaUrunu>()
            .HasIndex(bu => new { bu.BankaId, bu.UrunTipiId })
            .IsUnique();

        modelBuilder.Entity<Hesaplama>()
            .HasOne(h => h.BankaUrunu)
            .WithMany(bu => bu.Hesaplamalar)
            .HasForeignKey(h => h.BankaUrunId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OdemePlani>()
            .HasOne<Hesaplama>()
            .WithMany()
            .HasForeignKey(o => o.HesaplamaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Banka>().HasData(
            new Banka { Id = 1, Ad = "Ziraat Bankası", Kod = "ZB", Aktif = true },
            new Banka { Id = 2, Ad = "Garanti BBVA", Kod = "GB", Aktif = true },
            new Banka { Id = 3, Ad = "İş Bankası", Kod = "IB", Aktif = true },
            new Banka { Id = 4, Ad = "Yapı Kredi", Kod = "YK", Aktif = true },
            new Banka { Id = 5, Ad = "Akbank", Kod = "AK", Aktif = true }
        );

        modelBuilder.Entity<UrunTipi>().HasData(
            new UrunTipi { Id = 1, Ad = "İhtiyaç Kredisi", Kod = "IHT", Aciklama = "Genel amaçlı nakit ihtiyaçları için", Aktif = true },
            new UrunTipi { Id = 2, Ad = "Konut Kredisi", Kod = "KNT", Aciklama = "Ev alımı için uzun vadeli kredi", Aktif = true },
            new UrunTipi { Id = 3, Ad = "Taşıt Kredisi", Kod = "TST", Aciklama = "Araç alımı için özel kredi", Aktif = true }
        );

        modelBuilder.Entity<BankaUrunu>().HasData(
            // Ziraat Bankası Ürünleri
            new BankaUrunu { 
                Id = 1, BankaId = 1, UrunTipiId = 1, 
                FaizOrani = 3.29m, MinTutar = 10000, MaxTutar = 500000,
                MinVade = 12, MaxVade = 60, Aktif = true 
            },
            new BankaUrunu { 
                Id = 2, BankaId = 1, UrunTipiId = 2, 
                FaizOrani = 2.89m, MinTutar = 50000, MaxTutar = 5000000,
                MinVade = 60, MaxVade = 240, Aktif = true 
            },
            new BankaUrunu { 
                Id = 3, BankaId = 1, UrunTipiId = 3, 
                FaizOrani = 3.09m, MinTutar = 20000, MaxTutar = 1000000,
                MinVade = 12, MaxVade = 48, Aktif = true 
            },
            
            // Garanti BBVA Ürünleri
            new BankaUrunu { 
                Id = 4, BankaId = 2, UrunTipiId = 1, 
                FaizOrani = 3.49m, MinTutar = 10000, MaxTutar = 750000,
                MinVade = 12, MaxVade = 72, KampanyaAdi = "Yeni Yıl Kampanyası", Aktif = true 
            },
            new BankaUrunu { 
                Id = 5, BankaId = 2, UrunTipiId = 2, 
                FaizOrani = 2.99m, MinTutar = 75000, MaxTutar = 7500000,
                MinVade = 60, MaxVade = 240, Aktif = true 
            },
            
            // İş Bankası Ürünleri
            new BankaUrunu { 
                Id = 6, BankaId = 3, UrunTipiId = 1, 
                FaizOrani = 3.39m, MinTutar = 5000, MaxTutar = 600000,
                MinVade = 6, MaxVade = 60, Aktif = true 
            },
            new BankaUrunu { 
                Id = 7, BankaId = 3, UrunTipiId = 3, 
                FaizOrani = 3.19m, MinTutar = 25000, MaxTutar = 1250000,
                MinVade = 12, MaxVade = 48, KampanyaAdi = "Sıfır Araç Kampanyası", Aktif = true 
            },
            
            // Yapı Kredi Ürünleri
            new BankaUrunu { 
                Id = 8, BankaId = 4, UrunTipiId = 1, 
                FaizOrani = 3.44m, MinTutar = 10000, MaxTutar = 700000,
                MinVade = 12, MaxVade = 60, Aktif = true 
            },
            new BankaUrunu { 
                Id = 9, BankaId = 4, UrunTipiId = 2, 
                FaizOrani = 2.95m, MinTutar = 60000, MaxTutar = 6000000,
                MinVade = 60, MaxVade = 240, Aktif = true 
            },
            
            // Akbank Ürünleri
            new BankaUrunu { 
                Id = 10, BankaId = 5, UrunTipiId = 1, 
                FaizOrani = 3.35m, MinTutar = 10000, MaxTutar = 800000,
                MinVade = 12, MaxVade = 72, KampanyaAdi = "Hızlı Onay Kampanyası", Aktif = true 
            },
            new BankaUrunu { 
                Id = 11, BankaId = 5, UrunTipiId = 2, 
                FaizOrani = 2.85m, MinTutar = 100000, MaxTutar = 10000000,
                MinVade = 60, MaxVade = 240, Aktif = true 
            },
            new BankaUrunu { 
                Id = 12, BankaId = 5, UrunTipiId = 3, 
                FaizOrani = 3.15m, MinTutar = 30000, MaxTutar = 1500000,
                MinVade = 12, MaxVade = 48, Aktif = true 
            }
        );
    }
}