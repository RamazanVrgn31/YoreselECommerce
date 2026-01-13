import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AddBasketItemDto, BasketDto, BasketItem } from '../../models/basket';
import { DataResult } from '../../models/product';

@Injectable({
  providedIn: 'root',
})
export  class BasketService {
  // Portu kontrol et (5001)
  private apiUrl = "https://localhost:5001/api/Basket";
  // ⭐ 3. SEPET SAYISI YAYINCISI (Başlangıç değeri 0)
  // Navbar bu değişkeni dinleyecek (subscribe olacak)
  public basketCount$ = new BehaviorSubject<number>(0);
  constructor(private http: HttpClient) {}
  
  add(basketItem: AddBasketItemDto): Observable<DataResult<any>> {
    return this.http.post<DataResult<any>>(this.apiUrl + "/additem", basketItem);
  }
  getBasket(): Observable<DataResult<BasketDto>> {

   return this.http.get<DataResult<BasketDto>>(this.apiUrl + "/get").pipe(
      tap(res => {
        // Backend'den cevap geldiğinde araya giriyoruz (tap)
        if (res.success && res.data && res.data.items) {
          // Tüm ürünlerin adetlerini (quantity) topla
          const totalCount = res.data.items.reduce((sum, item) => sum + item.quantity, 0);
          this.basketCount$.next(totalCount); // Yeni sayıyı yayına ver
        } else {
          this.basketCount$.next(0); // Sepet boşsa veya hata varsa 0 yap
        }
      })
    );
  }
  // Sepet sayısını manuel güncellemek için yardımcı metot
  updateCount() {
    this.getBasket().subscribe(); // Bu çağrı yukarıdaki 'tap' sayesinde sayıyı günceller
  }
  delete(): Observable<DataResult<any>> {
    return this.http.delete<DataResult<any>>(this.apiUrl + "/delete");
  }
}

