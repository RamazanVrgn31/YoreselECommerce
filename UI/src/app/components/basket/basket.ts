import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, computed, OnInit, signal } from '@angular/core';
import { BasketService } from '../../services/Basket/basket';
import { BasketItem } from '../../models/basket';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-basket',
  standalone: true,
  imports: [CommonModule,RouterModule],
  templateUrl: './basket.html',
  styleUrl: './basket.scss',
})
export class Basket implements OnInit {
  basketItems= signal<BasketItem[]>([]);
  totalAmount = computed(() => {
      return this.basketItems().reduce((sum, item) => sum + (item.price * item.quantity), 0);
  });

  constructor(private basketService: BasketService,private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.getBasket();
  }

  getBasket() {
    // Servis DataResult<BasketDto> dönüyor
    this.basketService.getBasket().subscribe({
      next: (response) => {
        // KUTUYU AÇIYORUZ: res.data bir BasketDto'dur.
        // Bize içindeki 'items' listesi lazım.
        if(response.data && response.data.items) {
          this.basketItems.set(response.data.items);
        }
        else{
          this.basketItems.set([]);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.log("Sepet Yüklenirken Hata:", err);
        this.cdr.detectChanges();
      }
    });
  }

  clearBasket() {
    if(confirm("Sepeti tamamen boşaltmak istediğinize emin misiniz?")) {
      this.basketService.delete().subscribe({
        next: (res) => {
          this.getBasket(); // Listeyi yenile (Boş gelecek)
        },
        error: (err) => {
          console.log("Hata:", err);
        }
      });
    }
  }
}
