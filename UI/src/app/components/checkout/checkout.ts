import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { OrderCreateDto } from '../../models/order';
import { OrderService } from '../../services/Order/order.service';
import { BasketService } from '../../services/Basket/basket';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout {

  orderModel: OrderCreateDto = {
    address: '',
    PaymentDto: {
      cardHolderName: '',
      cardNumber: '',
      expireMonth: '',
      expireYear: '',
      cvv: '',
      price: 0 //Fiyat backend'den gelecek
    }
  
  }
  expiryInput: string = ''; // '12/28' formatındaki geçici veri

  constructor(
    private orderService: OrderService,
    private basketService : BasketService,
    private router: Router
  ) { }

  completeOrder(){
    // 1. Tarih Formatlama (12/28 -> Month: 12, Year: 2028)
    if (this.expiryInput.includes('/')) {
      const parts = this.expiryInput.split('/');
      this.orderModel.PaymentDto.expireMonth = parts[0].trim();
      this.orderModel.PaymentDto.expireYear = '20' + parts[1].trim();// 2028 formatına çeviriyoruz
    }
    else {
      //Test için direkt atama
      this.orderModel.PaymentDto.expireMonth = '12';
      this.orderModel.PaymentDto.expireYear = '2030';
    }
    console.log("Sipariş Gönderiliyor:", this.orderModel);

    // 2. Servisi çağır ve siparişi oluştur
    this.orderService.createOrder(this.orderModel).subscribe({
      next: (response) => {
        if (response.success) {
          //Sepet Sayısını sıfırla
          this.basketService.updateCount();

          //Ana sayfaya yönlendir
          this.router.navigate(['/home']);
        }
        
      },
      error: (err) => {
        console.error("Sipariş Oluşturma Hatası:", err);
        alert("Sipariş oluşturulamadı!" + (err.error?.message || "Bilinmeyen hata."));
      }
    });
      
  }
}
