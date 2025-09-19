import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Banka {
  id: number;
  ad: string;
  kod?: string;
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
})
export class AppComponent implements OnInit {
  private readonly baseUrl = 'http://localhost:5188/api';
  
  activeTab: 'hesaplama' | 'basvuru' = 'hesaplama';
  
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
    bankaUrunId: null as number | null,
    krediTutari: null as number | null,
    krediVadesi: null as number | null
  };
  basvuruResult: any = null;
  
  // Başvuru için seçilen banka ve ürünler
  basvuruBankaId: number | null = null;
  basvuruBankaUrunleri: BankaUrunu[] = [];

  constructor(private http: HttpClient) { 
    this.activeTab = 'hesaplama';
  }

  ngOnInit(): void {
    this.loadBankalar();
    this.loadAllBankaUrunleri();
  }

  setActiveTab(tab: 'hesaplama' | 'basvuru'): void {
    this.activeTab = tab;
    this.basvuruResult = null;
  }

  loadBankalar(): void {
    this.http.get<Banka[]>(`${this.baseUrl}/bankalar`).subscribe({
      next: (bankalar: Banka[]) => {
        this.bankalar = bankalar;
        console.log('Bankalar yüklendi:', bankalar);
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
    console.log('Banka ürünleri yükleniyor, bankaId:', bankaId);
    this.http.get<BankaUrunu[]>(`${this.baseUrl}/banka-urunleri/banka/${bankaId}`).subscribe({
      next: (urunler: BankaUrunu[]) => {
        this.bankaUrunleri = urunler;
        console.log(`Banka ${bankaId} ürünleri yüklendi:`, urunler);
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
    console.log('Banka değişti, selectedBankaId:', this.selectedBankaId);
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
    console.log('Seçilen banka ürünü:', this.selectedBankaUrunu);
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
    
    if (this.hesaplamaModu === 'urun') {
      if (!this.selectedBankaUrunu) return;
      faizOrani = this.selectedBankaUrunu.faizOrani;
    } else {
      if (!this.manuelFaizOrani) return;
      faizOrani = this.manuelFaizOrani;
    }

    const istek = {
      tutar: this.krediTutari,
      vade: this.krediVadesi,
      faizOrani: faizOrani
    };

    console.log('Hesaplama isteği:', istek);

    this.http.post(`${this.baseUrl}/kredi-hesapla`, istek).subscribe({
      next: (result: any) => {
        this.hesaplamaResult = result;
        console.log('Hesaplama sonucu:', result);
      },
      error: (error: any) => {
        console.error('Hesaplama hatası:', error);
        alert('Hesaplama sırasında bir hata oluştu: ' + (error.error?.message || error.message || 'Bilinmeyen hata'));
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
              this.basvuru.krediVadesi);
  }

  basvuruyuGonder(): void {
    if (!this.canSubmitBasvuru()) return;

    const istek = {
      email: this.basvuru.email,
      adSoyad: this.basvuru.adSoyad,
      bankaUrunId: this.basvuru.bankaUrunId,
      krediTutari: this.basvuru.krediTutari,
      krediVadesi: this.basvuru.krediVadesi
    };

    console.log('Başvuru isteği:', istek);

    this.http.post(`${this.baseUrl}/kredi-basvuru`, istek).subscribe({
      next: (result: any) => {
        this.basvuruResult = result;
        console.log('Başvuru sonucu:', result);
        // Formu temizle
        this.basvuru = {
          email: '',
          adSoyad: '',
          bankaUrunId: null,
          krediTutari: null,
          krediVadesi: null
        };
        this.basvuruBankaId = null;
        this.basvuruBankaUrunleri = [];
      },
      error: (error: any) => {
        console.error('Başvuru hatası:', error);
        alert('Başvuru sırasında bir hata oluştu: ' + (error.error?.message || error.message || 'Bilinmeyen hata'));
      }
    });
  }

  getSelectedBasvuruUrun(): BankaUrunu | null {
    if (!this.basvuru.bankaUrunId) return null;
    return this.allBankaUrunleri.find(u => u.id == this.basvuru.bankaUrunId) || 
           this.basvuruBankaUrunleri.find(u => u.id == this.basvuru.bankaUrunId) || null;
  }
}