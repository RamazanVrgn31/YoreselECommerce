import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { OrderDto } from '../../models/order';
import { OrderService } from '../../services/Order/order.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-orders.html',
  styleUrl: './my-orders.scss',
})
export class MyOrders implements OnInit {
  
  orders= signal<OrderDto[]>([]);
  dataLoaded= signal<boolean>(false);

  constructor(private orderService: OrderService) { }
  
  ngOnInit(): void {
    this.getMyOrders();
  }
  getMyOrders(){
    this.orderService.getMyOrders().subscribe({
      next: (response) => {
        if(response.success){
          this.orders.set(response.data);
          this.dataLoaded.set(true);
        }
      },
      error: (err) => {
        console.error("Siparişler alınırken hata oluştu:", err);
      }
    });
  }

}
