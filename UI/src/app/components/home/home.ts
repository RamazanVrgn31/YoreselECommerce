import { AddBasketItemDto } from './../../models/basket';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, signal } from '@angular/core';
import { ProductService } from '../../services/Product/product';
import { Product } from '../../models/product';
import { BasketService } from '../../services/Basket/basket';
import { CategoryMenu } from '../category-menu/category-menu';



@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule,CategoryMenu],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  products = signal<Product[]>([]); 
  dataLoaded = signal<boolean>(false);

  constructor(
    private productService: ProductService,
    private basketService: BasketService,
    private cdr: ChangeDetectorRef
  ) {}
  ngOnInit(): void {
    this.getProducts();
  }

  getProducts() {
    this.dataLoaded.set(false); // başlarken false olsun
    this.productService.getProducts().subscribe({
      next: (response) => {
        this.products.set(response.data);
        this.dataLoaded.set(true);
        // console.log("Ürünler Geldi:", this.products);

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.log("Hata:", err);

        // 4. HATA OLSA BİLE SPINNER'I KAPAT
        // Aksi takdirde hata alan kullanıcı sonsuza kadar dönen tekerleği izler.
        this.dataLoaded.set(true); 
        this.cdr.detectChanges();
      }
    });
  }
  
  getproductImageUrl(url: string): string {
      if (!url) return 'https://placehold.co/600x400?text=Resim+Yok';// Eğer resim zaten tam bir link ise (http...) dokunma
      if (url.startsWith('http')) return url;// Eğer resim zaten tam bir link ise (http...) dokunma
      let path = url.replace(/\\/g, "/");// A. Ters slashları (\) düz slash (/) yap

      // B. Eğer tam Windows yolu geliyorsa (D:/Projects/.../wwwroot/Uploads...)
      // "wwwroot/" kelimesinden sonrasını al.
      if (path.includes("wwwroot/")) path = path.split("wwwroot/")[1];
      if (path.startsWith("/")) path = path.substring(1);// C. Başında gereksiz '/' varsa onu da sil (çift slash olmasın diye)
      if (!path.includes("Uploads")) path = "Uploads/Images/" + path;// (Veritabanında sadece '888299ec...jpg' yazıyorsa burası çalışır)
      return "https://localhost:5001/" + path;
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
        console.log("Sepete Ekleme Başarılı:", response);
        this.basketService.updateCount();
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

  // Kategori Menüsünden Tetiklenen Metod
  onCategoryChange(categoryId: number) {
    this.dataLoaded.set(false); // Yükleniyor...

    if (categoryId === 0) {
      // 0 ise hepsini getir (Eski metodun)
      this.getProducts(); 
    } else {
      // ID varsa filtrele (Yeni servis metodun)
      this.productService.getProductsByCategory(categoryId).subscribe({
        next: (res) => {
          this.products.set(res.data); // Signal güncellenir -> Ekran değişir
          this.dataLoaded.set(true);
        },
        error: (err) => {
            console.log(err);
            this.dataLoaded.set(true);
        }
      });
    }
  }

}
