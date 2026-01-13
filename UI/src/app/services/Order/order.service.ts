import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OrderCreateDto, OrderDetailDto, OrderDto } from '../../models/order';
import { DataResult } from '../../models/product';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private apiUrl = "https://localhost:5001/api/Orders";

  constructor(private http: HttpClient) { }

  //Backend'e sipariş oluşturma isteği gönder
  createOrder(order: OrderCreateDto) {
    return this.http.post<DataResult<any>>(this.apiUrl + "/create", order);
  }

  getMyOrders(): Observable<DataResult<OrderDto[]>> {
    return this.http.get<DataResult<OrderDto[]>>(this.apiUrl + "/getmyorders");
  }

  getOrderDetails(orderId: number): Observable<DataResult<OrderDetailDto[]>> {
    return this.http.get<DataResult<OrderDetailDto[]>>(this.apiUrl + "/getdetails?orderId=" + orderId);
  }
}
