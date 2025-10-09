import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Banka {
  id: number;
  ad: string;
  logoUrl?: string;
  aktif: boolean;
}

interface BankaUrunu {
  id: number;
  bankaId: number;
  bankaAdi: string;
  urunId: number;
  urunAdi: string;
  faizOrani: number;
  minTutar: number;
  maxTutar: number;
  minVade: number;
  maxVade: number;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
})
export class AppComponent implements OnInit {
  private readonly baseUrl = 'http://localhost:5188/api';
  
  activeTab: 'hesaplama' | 'basvuru' | 'auth' | 'banka-uyelik' = 'hesaplama';
  
  // Hesaplama modu: 'urun' veya 'manuel'
  hesaplamaModu: 'urun' | 'manuel' = 'urun';
  
  bankalar: Banka[] = [];
  bankaUrunleri: BankaUrunu[] = [];
  allBankaUrunleri: BankaUrunu[] = []; 
  
  selectedBankaId: number | null = null;
  selectedBankaUrunId: number | null = null;
  selectedBankaUrunu: BankaUrunu | null = null;
  
  krediTutari: number | null = null;
  krediVadesi: number | null = null;
  manuelFaizOrani: number | null = null;
  hesaplamaResult: any = null;

  // Başvuru formu
  basvuru = {
    email: '',
    adSoyad: '',
    tcKimlikNo: '',
    telefon: '',
    bankaUrunId: null as number | null,
    krediTutari: null as number | null,
    krediVadesi: null as number | null,
    gelir: null as number | null
  };
  basvuruResult: any = null;
  
  // Başvuru için seçilen banka ve ürünler
  basvuruBankaId: number | null = null;
  basvuruBankaUrunleri: BankaUrunu[] = [];

  // Authentication
  isLoggedIn: boolean = false;
  currentUser: any = null;
  authMode: 'login' | 'signup' = 'login';
  
  // Auth form data
  authForm = {
    email: '',
    adSoyad: '',
    telefon: '',
    dogumTarihi: '',
    tcKimlikNo: '',
    sifre: '',
    sifreTekrar: ''
  };

  // Banka üyelik
  availableBanks: Banka[] = [];
  myBanks: any[] = [];

  // Geçmiş başvurular
  showPastApplications: boolean = false;
  pastApplications: any[] = [];

  constructor(private http: HttpClient) { 
    this.activeTab = 'hesaplama';
  }

  ngOnInit(): void {
    this.loadBankalar();
    this.loadAllBankaUrunleri();
    this.checkAuthStatus();
  }

  getLogoUrl(url?: string): string {
    if (!url) return '/banks/default.svg';
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return url.startsWith('/') ? url : `/${url}`;
  }

  private logoMap: Record<string, string> = {
    'Akbank T.A.Ş.': '/banks/akbank.svg',
    'DenizBank A.Ş.': '/banks/denizbank.svg',
    'Türkiye Garanti Bankası A.Ş.': '/banks/garanti-bbva.svg',
    'Türkiye Halk Bankası A.Ş.': '/banks/halkbank.svg',
    'ING Bank A.Ş.': '/banks/ing-bank.svg',
    'Türkiye İş Bankası A.Ş.': '/banks/is-bankasi.svg',
    'QNB Finansbank A.Ş.': '/banks/qnb-finansbank.svg',
    'Kuveyt Türk Katılım Bankası': '/banks/kuveyt-turk.svg',
    'Türkiye Vakıflar Bankası T.A.Ş.': '/banks/vakifbank.svg',
    'Yapı ve Kredi Bankası A.Ş.': '/banks/yapi-kredi.svg',
    'Türkiye Cumhuriyeti Ziraat Bankası A.Ş.': '/banks/ziraat-bankasi.svg',
    'Türkiye Vakıflar Bankası T.A.O.': '/banks/vakifbank.svg',
    'Türk Ekonomi Bankası A.Ş.': '/banks/teb.svg',
    'Türkiye Finans Katılım Bankası A.Ş.': '/banks/turkiye-finans.svg'
  };



  resolveBankLogo(bank: Banka): string {
    if (bank?.logoUrl) return this.getLogoUrl(bank.logoUrl);
    if (!bank?.ad) return '/banks/default.svg';
    const mapped = this.logoMap[bank.ad];
    return this.getLogoUrl(mapped || '/banks/default.svg');
  }

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (!img) return;
    if (!img.src.includes('/banks/default.svg')) {
      img.src = '/banks/default.svg';
    }
  }

  setActiveTab(tab: 'hesaplama' | 'basvuru' | 'auth' | 'banka-uyelik'): void {
    this.activeTab = tab;
    this.basvuruResult = null;

    if (tab === 'banka-uyelik' && this.isLoggedIn) {
      this.loadAvailableBanks();
      this.loadMyBanks();
    }
    // Başvuru sekmesine geçildiğinde üye bankaları yenile ve kullanıcı bilgilerini doldur
    if (tab === 'basvuru' && this.isLoggedIn) {
      this.loadMyBanks();
      this.fillBasvuruFormWithUserData();
    }
  }

  fillBasvuruFormWithUserData(): void {
    if (this.currentUser) {
      if (this.currentUser.email) {
        this.basvuru.email = this.currentUser.email;
      }
      if (this.currentUser.adSoyad) {
        this.basvuru.adSoyad = this.currentUser.adSoyad;
      }
      if (this.currentUser.tcKimlikNo) {
        this.basvuru.tcKimlikNo = this.currentUser.tcKimlikNo;
      }
      if (this.currentUser.telefon) {
        this.basvuru.telefon = this.currentUser.telefon;
      }
    } 
  }

  loadBankalar(): void {
    this.http.get<Banka[]>(`${this.baseUrl}/bankalar`).subscribe({
      next: (bankalar: Banka[]) => {
        this.bankalar = bankalar;
      },
      error: (error: any) => {
        console.error('Bankalar yüklenemedi:', error);
      }
    });
  }

  loadAllBankaUrunleri(): void {
    this.http.get<BankaUrunu[]>(`${this.baseUrl}/banka-urunleri`).subscribe({
      next: (urunler: BankaUrunu[]) => {
        this.allBankaUrunleri = urunler;
        console.log('Tüm banka ürünleri yüklendi:', urunler);
      },
      error: (error: any) => {
        console.error('Banka ürünleri yüklenemedi:', error);
      }
    });
  }

  loadBankaUrunleri(bankaId: number): void {
    this.http.get<BankaUrunu[]>(`${this.baseUrl}/banka-urunleri/banka/${bankaId}`).subscribe({
      next: (urunler: BankaUrunu[]) => {
        this.bankaUrunleri = urunler;
        if (urunler.length === 0) {
          console.warn(`Banka ${bankaId} için ürün bulunamadı`);
        }
      },
      error: (error: any) => {
        console.error('Banka ürünleri yüklenemedi:', error);
        console.error('Error details:', error.error);
        alert('Banka ürünleri yüklenirken hata oluştu. Konsolu kontrol edin.');
      }
    });
  }

  onBankaChange(): void {
    if (this.selectedBankaId) {
      this.loadBankaUrunleri(this.selectedBankaId);
      this.selectedBankaUrunId = null;
      this.selectedBankaUrunu = null;
    } else {
      this.bankaUrunleri = [];
      this.selectedBankaUrunId = null;
      this.selectedBankaUrunu = null;
    }
  }

  onBankaUrunChange(): void {
    this.selectedBankaUrunu = this.bankaUrunleri.find(u => u.id == this.selectedBankaUrunId) || null;
  }

  onBasvuruBankaChange(): void {
    if (this.basvuruBankaId) {
      this.basvuruBankaUrunleri = this.allBankaUrunleri.filter(u => u.bankaId == this.basvuruBankaId);
      this.basvuru.bankaUrunId = null;
    } else {
      this.basvuruBankaUrunleri = [];
      this.basvuru.bankaUrunId = null;
    }
  }

  onHesaplamaModuChange(): void {
    if (this.hesaplamaModu === 'manuel') {
      this.selectedBankaId = null;
      this.selectedBankaUrunId = null;
      this.selectedBankaUrunu = null;
      this.bankaUrunleri = [];
    } else {
      this.manuelFaizOrani = null;
    }
    this.hesaplamaResult = null;
  }

  canCalculate(): boolean {
    if (this.hesaplamaModu === 'urun') {
      return !!(this.selectedBankaUrunId && this.krediTutari && this.krediVadesi);
    } else {
      return !!(this.manuelFaizOrani && this.krediTutari && this.krediVadesi);
    }
  }

  hesapla(): void {
    if (!this.krediTutari || !this.krediVadesi) return;

    let faizOrani: number;
    let bankaUrunId: number | null = null;

    if (this.hesaplamaModu === 'urun') {
      if (!this.selectedBankaUrunu) return;
      faizOrani = this.selectedBankaUrunu.faizOrani;
      bankaUrunId = this.selectedBankaUrunId;
    } else {
      if (!this.manuelFaizOrani) return;
      faizOrani = this.manuelFaizOrani;
    }

    const istek: any = {
      tutar: this.krediTutari,
      vade: this.krediVadesi,
      faizOrani: faizOrani
    };

    if (bankaUrunId !== null) {
      istek.bankaUrunId = bankaUrunId;
    }

    console.log('Hesaplama isteği:', istek);

    this.http.post(`${this.baseUrl}/kredi-hesapla`, istek).subscribe({
      next: (result: any) => {
        this.hesaplamaResult = result;
      },
      error: (error: any) => {
        console.error('Hesaplama hatası:', error);
        const errorMessage = error.error?.message || error.message || 'Bilinmeyen hata';
        alert('Hesaplama sırasında bir hata oluştu:\n' + errorMessage);
      }
    });
  }

  basvuruYap(): void {
    if (this.hesaplamaResult && this.hesaplamaModu === 'urun' && this.selectedBankaUrunId) {
      // Hesaplama sonucunu başvuru formuna aktar
      this.basvuru.bankaUrunId = this.selectedBankaUrunId;
      this.basvuru.krediTutari = this.krediTutari;
      this.basvuru.krediVadesi = this.krediVadesi;
      
      // Başvuru sekmesine banka bilgisini aktar
      this.basvuruBankaId = this.selectedBankaId;
      this.basvuruBankaUrunleri = this.bankaUrunleri;
      
      this.setActiveTab('basvuru');
    }
  }

  canSubmitBasvuru(): boolean {
    return !!(this.basvuru.email && this.basvuru.adSoyad && 
              this.basvuru.bankaUrunId && this.basvuru.krediTutari && 
              this.basvuru.krediVadesi && this.basvuru.gelir);
  }

  basvuruyuGonder(): void {
    if (!this.canSubmitBasvuru()) return;

    const istek = {
      email: this.basvuru.email,
      adSoyad: this.basvuru.adSoyad,
      tcKimlikNo: this.basvuru.tcKimlikNo,
      telefon: this.basvuru.telefon,
      bankaUrunId: this.basvuru.bankaUrunId,
      krediTutari: this.basvuru.krediTutari,
      krediVadesi: this.basvuru.krediVadesi,
      gelir: this.basvuru.gelir
    };

    console.log('Başvuru isteği:', istek);

    this.http.post(`${this.baseUrl}/kredi-basvuru`, istek, { withCredentials: true }).subscribe({
      next: (result: any) => {
        this.basvuruResult = result;

        // Geçmiş başvuruları güncelle
        if (this.pastApplications.length > 0 || this.showPastApplications) {
          this.loadPastApplications();
        }

        // Formu temizle (kişisel bilgiler hariç)
        this.basvuru.bankaUrunId = null;
        this.basvuru.krediTutari = null;
        this.basvuru.krediVadesi = null;
        this.basvuru.gelir = null;
        this.basvuruBankaId = null;
        this.basvuruBankaUrunleri = [];
      },
      error: (error: any) => {
        console.error('Başvuru hatası:', error);
        const errorMessage = error.error?.message || error.message || 'Bilinmeyen hata';
        alert('Başvuru sırasında bir hata oluştu:\n' + errorMessage);
      }
    });
  }

  getSelectedBasvuruUrun(): BankaUrunu | null {
    if (!this.basvuru.bankaUrunId) return null;
    return this.allBankaUrunleri.find(u => u.id == this.basvuru.bankaUrunId) || 
           this.basvuruBankaUrunleri.find(u => u.id == this.basvuru.bankaUrunId) || null;
  }

  getMemberBanks(): Banka[] {
    // Öncelik: myBanks listesi yüklüyse onu kullan
    if (this.myBanks && this.myBanks.length > 0) {
      return this.bankalar.filter(banka => 
        this.myBanks.some((memberBank: any) => memberBank.id === banka.id)
      );
    }
    // Geriye dönük: currentUser.bankalar varsa onu kullan
    if (this.currentUser?.bankalar) {
      return this.bankalar.filter(banka => 
        this.currentUser.bankalar.some((memberBank: any) => memberBank.id === banka.id)
      );
    }
    return [];
  }

  // Authentication methods
  checkAuthStatus(): void {
    this.http.get(`${this.baseUrl}/auth/me`, { withCredentials: true }).subscribe({
      next: (user: any) => {
        this.isLoggedIn = true;
        this.currentUser = user;

        if (!this.basvuru.email && user?.email) {
          this.basvuru.email = user.email;
        }
        if (!this.basvuru.adSoyad && user?.adSoyad) {
          this.basvuru.adSoyad = user.adSoyad;
        }
        if (!this.basvuru.tcKimlikNo && user?.tcKimlikNo) {
          this.basvuru.tcKimlikNo = user.tcKimlikNo;
        }
        if (!this.basvuru.telefon && user?.telefon) {
          this.basvuru.telefon = user.telefon;
        }
      },
      error: () => {
        this.isLoggedIn = false;
        this.currentUser = null;
      }
    });
  }

  setAuthMode(mode: 'login' | 'signup'): void {
    this.authMode = mode;
    this.authForm = { email: '', adSoyad: '', telefon: '', dogumTarihi: '', tcKimlikNo: '', sifre: '', sifreTekrar: '' };
  }

  canSubmitAuth(): boolean {
    if (this.authMode === 'login') {
      return !!(this.authForm.email && this.authForm.sifre);
    } else {
      return !!(this.authForm.email && this.authForm.sifre && this.authForm.sifreTekrar && 
                this.authForm.sifre === this.authForm.sifreTekrar);
    }
  }

  submitAuth(): void {
    if (!this.canSubmitAuth()) return;

    const endpoint = this.authMode === 'login' ? 'login' : 'signup';
    const data: any = {
      email: this.authForm.email,
      sifre: this.authForm.sifre
    };

    if (this.authMode === 'signup') {
      data.adSoyad = this.authForm.adSoyad || '';
      data.telefon = this.authForm.telefon || null;
      data.dogumTarihi = this.authForm.dogumTarihi && this.authForm.dogumTarihi.trim() !== '' ? this.authForm.dogumTarihi : null;
      data.tcKimlikNo = this.authForm.tcKimlikNo || null;
    }

    console.log('Gönderilen data:', data);

    this.http.post(`${this.baseUrl}/auth/${endpoint}`, data, { withCredentials: true }).subscribe({
      next: (result: any) => {
        this.isLoggedIn = true;
        this.currentUser = result;
        this.authForm = { email: '', adSoyad: '', telefon: '', dogumTarihi: '', tcKimlikNo: '', sifre: '', sifreTekrar: '' };

        // Başvuru formunu güncelle
        if (!this.basvuru.email && result?.email) {
          this.basvuru.email = result.email;
        }
        if (!this.basvuru.adSoyad && result?.adSoyad) {
          this.basvuru.adSoyad = result.adSoyad;
        }
        if (!this.basvuru.tcKimlikNo && result?.tcKimlikNo) {
          this.basvuru.tcKimlikNo = result.tcKimlikNo;
        }
        if (!this.basvuru.telefon && result?.telefon) {
          this.basvuru.telefon = result.telefon;
        }

        this.setActiveTab('basvuru');
        alert(result.message);
      },
      error: (error: any) => {
        console.error('Auth hatası:', error);
        alert(error.error?.message || 'Bir hata oluştu');
      }
    });
  }

  logout(): void {
    this.http.post(`${this.baseUrl}/auth/logout`, {}, { withCredentials: true }).subscribe({
      next: () => {
        this.isLoggedIn = false;
        this.currentUser = null;
        // Başvuru kişisel bilgilerini temizle
        this.basvuru.email = '';
        this.basvuru.adSoyad = '';
        this.basvuru.tcKimlikNo = '';
        this.basvuru.telefon = '';
        this.setActiveTab('hesaplama');
        alert('Çıkış yapıldı');
      },
      error: (error: any) => {
        console.error('Logout hatası:', error);
      }
    });
  }

  // Banka üyelik methods
  loadAvailableBanks(): void {
    this.http.get(`${this.baseUrl}/banka-uyelik/available-banks`, { withCredentials: true }).subscribe({
      next: (banks: any) => {
        this.availableBanks = banks;
      },
      error: (error: any) => {
        console.error('Mevcut bankalar yüklenemedi:', error);
      }
    });
  }

  loadMyBanks(): void {
    this.http.get(`${this.baseUrl}/banka-uyelik/my-banks`, { withCredentials: true }).subscribe({
      next: (banks: any) => {
        this.myBanks = banks;
      },
      error: (error: any) => {
        console.error('Üye bankalar yüklenemedi:', error);
      }
    });
  }

  joinBank(bankaId: number): void {
    this.http.post(`${this.baseUrl}/banka-uyelik/join/${bankaId}`, {}, { withCredentials: true }).subscribe({
      next: (result: any) => {
        alert(result.message);
        this.loadAvailableBanks();
        this.loadMyBanks();
        // Kullanıcı bilgisini tazele
        this.checkAuthStatus();
      },
      error: (error: any) => {
        console.error('Banka üyelik hatası:', error);
        alert(error.error?.message || 'Bir hata oluştu');
      }
    });
  }

  leaveBank(bankaId: number): void {
    if (confirm('Bu bankadan ayrılmak istediğinizden emin misiniz?')) {
      this.http.delete(`${this.baseUrl}/banka-uyelik/leave/${bankaId}`, { withCredentials: true }).subscribe({
        next: (result: any) => {
          alert(result.message);
          this.loadAvailableBanks();
          this.loadMyBanks();
          // Kullanıcı bilgisini tazele
          this.checkAuthStatus();
        },
        error: (error: any) => {
          console.error('Banka ayrılma hatası:', error);
          alert(error.error?.message || 'Bir hata oluştu');
        }
      });
    }
  }

  // Geçmiş başvurular methods
  togglePastApplications(): void {
    this.showPastApplications = !this.showPastApplications;
    if (this.showPastApplications && this.pastApplications.length === 0) {
      this.loadPastApplications();
    }
  }

  loadPastApplications(): void {
    this.http.get(`${this.baseUrl}/kredi-basvuru/my-applications`, { withCredentials: true }).subscribe({
      next: (applications: any) => {
        this.pastApplications = applications;
      },
      error: (error: any) => {
        console.error('Geçmiş başvurular yüklenemedi:', error);
        alert('Geçmiş başvurular yüklenirken bir hata oluştu');
      }
    });
  }
}