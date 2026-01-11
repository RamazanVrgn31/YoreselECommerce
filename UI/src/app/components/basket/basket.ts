import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { BasketService } from '../../services/Basket/basket';
import { BasketItem } from '../../models/basket';

@Component({
  selector: 'app-basket',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './basket.html',
  styleUrl: './basket.scss',
})
export class Basket implements OnInit {
  basketItems: BasketItem[] = [];
  totalAmount: number = 0;

  constructor(private basketService: BasketService) {}

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
          this.basketItems = response.data.items;
        }
        this.calculateTotal();
      },
      error: (err) => {
        console.log("Sepet Yüklenirken Hata:", err);
      }
    });
  }

  calculateTotal() {
    this.totalAmount = 0;
    this.basketItems.forEach(item => {
      // Backend'den gelen veriye göre: item.price * item.quantity
      this.totalAmount += item.price * item.quantity;
    });
  }
}
