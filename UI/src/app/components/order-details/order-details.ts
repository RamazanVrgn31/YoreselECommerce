import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { OrderDetailDto } from '../../models/order';
import { OrderService } from '../../services/Order/order.service';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './order-details.html',
  styleUrl: './order-details.scss',
})
export class OrderDetails implements OnInit {

  details = signal<OrderDetailDto[]>([]);
  dataLoaded = signal<boolean>(false);
  orderId: number = 0;
  
  // Bağımlılıkları inject edelim
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);

  ngOnInit(): void {
    // URL'den sipariş ID'sini al
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.orderId) {
      this.getOrderDetails();
    }
  }
  getOrderDetails() {
    this.orderService.getOrderDetails(this.orderId).subscribe({
      next: (response) => {
        if (response.success) {
          this.details.set(response.data);
          this.dataLoaded.set(true);
        }
      },
      error: (err) => {
        console.error("Sipariş detayları alınırken hata oluştu:", err);
      }
    });
  }
}
