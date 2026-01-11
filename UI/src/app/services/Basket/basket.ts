import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AddBasketItemDto, BasketDto, BasketItem } from '../../models/basket';
import { DataResult } from '../../models/product';

@Injectable({
  providedIn: 'root',
})
export  class BasketService {
  // Portu kontrol et (5001)
  private apiUrl = "https://localhost:5001/api/basket";

  constructor(private http: HttpClient) {}
  
  add(basketItem: AddBasketItemDto): Observable<DataResult<any>> {
    return this.http.post<DataResult<any>>(this.apiUrl + "/additem", basketItem);
  }
  getBasket(): Observable<DataResult<BasketDto>> {
    return this.http.get<DataResult<BasketDto>>(this.apiUrl + "/get");
  }
}

