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

interface Urun {
  id: number;
  ad: string;
  faizOrani: number;
  minTutar: number;
  maxTutar: number;
  minVade: number;
  maxVade: number;
  aktif: boolean;
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
  
  bankalar: Banka[] = [];
  urunler: Urun[] = [];
  selectedBankaId: number | null = null;
  selectedUrunId: number | null = null;
  selectedUrun: Urun | null = null;
  krediTutari: number | null = null;
  krediVadesi: number | null = null;
  hesaplamaResult: any = null;

  // Başvuru formu
  basvuru = {
    email: '',
    adSoyad: '',
    bankaId: null as number | null,
    urunId: null as number | null,
    krediTutari: null as number | null,
    krediVadesi: null as number | null
  };
  basvuruResult: any = null;

  constructor(private http: HttpClient) { 
    this.activeTab = 'hesaplama';
  }

  ngOnInit(): void {
    this.loadBankalar();
    this.loadUrunler();
  }

  setActiveTab(tab: 'hesaplama' | 'basvuru'): void {
    this.activeTab = tab;
  }

  loadBankalar(): void {
    this.http.get<Banka[]>(`${this.baseUrl}/bankalar`).subscribe({
      next: (bankalar: Banka[]) => {
        this.bankalar = bankalar;
        console.log('Bankalar yüklendi:', bankalar);
      },
      error: (error: any) => {
        console.error('Bankalar yüklenemedi:', error);
        // Test verileri
        this.bankalar = [
          { id: 1, ad: 'Test Bankası', kod: 'TEST', aktif: true }
        ];
      }
    });
  }

  loadUrunler(): void {
    this.http.get<Urun[]>(`${this.baseUrl}/urunler`).subscribe({
      next: (urunler: Urun[]) => {
        this.urunler = urunler;
        console.log('Ürünler yüklendi:', urunler);
      },
      error: (error: any) => {
        console.error('Ürünler yüklenemedi:', error);
        // Test verileri
        this.urunler = [
          { 
            id: 1, 
            ad: 'Genel Amaçlı Kredi', 
            faizOrani: 15.50, 
            minTutar: 10000, 
            maxTutar: 500000,
            minVade: 12,
            maxVade: 60,
            aktif: true 
          }
        ];
      }
    });
  }

  onUrunChange(): void {
    this.selectedUrun = this.urunler.find(u => u.id == this.selectedUrunId) || null;
    console.log('Seçilen ürün:', this.selectedUrun);
  }

  onBasvuruUrunChange(): void {
    // Başvuru formunda ürün değiştiğinde
  }

  canCalculate(): boolean {
    return !!(this.selectedUrunId && this.krediTutari && this.krediVadesi);
  }

  hesapla(): void {
    if (!this.selectedUrun || !this.krediTutari || !this.krediVadesi) return;

    const istek = {
      tutar: this.krediTutari,
      vade: this.krediVadesi,
      faizOrani: this.selectedUrun.faizOrani
    };

    console.log('Hesaplama isteği:', istek);

    this.http.post(`${this.baseUrl}/kredi-hesapla`, istek).subscribe({
      next: (result: any) => {
        this.hesaplamaResult = result;
        console.log('Hesaplama sonucu:', result);
      },
      error: (error: any) => {
        console.error('Hesaplama hatası:', error);
        alert('Hesaplama sırasında bir hata oluştu: ' + (error.message || 'Bilinmeyen hata'));
      }
    });
  }

  basvuruYap(): void {
    if (this.hesaplamaResult) {
      // Hesaplama sonucunu başvuru formuna aktar
      this.basvuru.bankaId = this.selectedBankaId;
      this.basvuru.urunId = this.selectedUrunId;
      this.basvuru.krediTutari = this.krediTutari;
      this.basvuru.krediVadesi = this.krediVadesi;
      this.setActiveTab('basvuru');
    }
  }

  canSubmitBasvuru(): boolean {
    return !!(this.basvuru.email && this.basvuru.adSoyad && this.basvuru.bankaId && 
              this.basvuru.urunId && this.basvuru.krediTutari && this.basvuru.krediVadesi);
  }

  basvuruyuGonder(): void {
    if (!this.canSubmitBasvuru()) return;

    const istek = {
      email: this.basvuru.email,
      adSoyad: this.basvuru.adSoyad,
      bankaId: this.basvuru.bankaId,
      urunId: this.basvuru.urunId,
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
          bankaId: null,
          urunId: null,
          krediTutari: null,
          krediVadesi: null
        };
      },
      error: (error: any) => {
        console.error('Başvuru hatası:', error);
        alert('Başvuru sırasında bir hata oluştu: ' + (error.message || 'Bilinmeyen hata'));
      }
    });
  }
}