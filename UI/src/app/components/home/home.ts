import { AddBasketItemDto } from './../../models/basket';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../services/Product/product';
import { Product } from '../../models/product';
import { BasketService } from '../../services/Basket/basket';



@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  products: Product[] = [];
  dataLoaded: boolean = false;

  constructor(
    private productService: ProductService,
    private basketService: BasketService
  ) {}
  ngOnInit(): void {
    this.getProducts();
  }

  getProducts() {
    this.productService.getProducts().subscribe({
      next: (response) => {
        this.products = response.data;
        this.dataLoaded = true;
        // console.log("Ürünler Geldi:", this.products);
      },
      error: (err) => {
        console.log("Hata:", err);
      }
    });
  }
  getproductImageUrl(url: string): string {
    if (!url) {
      return 'https://placehold.co/600x400?text=Resim+Yok';
    }
    // Eğer resim zaten tam bir link ise (http...) dokunma
    if(url.startsWith('http')) {
      return url;
    }

    // A. Ters slashları (\) düz slash (/) yap
    let cleanUrl = url.replace(/\\/g, "/");

    // B. Eğer tam Windows yolu geliyorsa (D:/Projects/.../wwwroot/Uploads...)
    // "wwwroot/" kelimesinden sonrasını al.
    if (cleanUrl.includes("wwwroot/")) {
      cleanUrl = cleanUrl.split("wwwroot/")[1];
    }
    
    // C. Başında gereksiz '/' varsa onu da sil (çift slash olmasın diye)
    if (cleanUrl.startsWith("/")) {
      cleanUrl = cleanUrl.substring(1);
    }

    // D. KRİTİK NOKTA: Eğer yolda "Uploads" klasörü yazmıyorsa, biz ekleyelim.
    // (Veritabanında sadece '888299ec...jpg' yazıyorsa burası çalışır)
    if (!cleanUrl.includes("Uploads")) {
       cleanUrl = "Uploads/Images/" + cleanUrl;
    }

    // Sonuç: https://localhost:5001/Uploads/Images/resim.jpg
    return "https://localhost:5001/" + cleanUrl;
  }

  // 2. Sepete Ekleme Metodu
  addToCart(product: Product) {
    // Backend'e gidecek veri
    // Backend'in AddItemToBasket metodu sadece ProductId ve Quantity bekler.
    // Bu yüzden buraya ': BasketItem' tipi yazmıyoruz, sade bir obje gönderiyoruz.
    let basketItem: AddBasketItemDto = {
      productId: product.id,
      quantity: 1 // Şimdilik sabit 1 adet

    };

    // Servis 'BasketItem' (dolu model) bekliyorsa, servisteki 'add' metodunun
    // parametre tipini de 'any' veya yeni bir 'AddBasketItemDto' olarak değiştirebiliriz.

    this.basketService.add(basketItem).subscribe({
      next: (response) => {
        alert(response.message);
        console.log("Sepete Ekleme Başarılı:", response);
      },
      error: (err) => {
        // Token yoksa 401 hatası döner
        console.log("Sepete Ekleme Hatası:", err);
        if(err.status == 401) {
            alert("Sepete eklemek için lütfen giriş yapınız!");
        }
      }

    });
  }

}
