import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DataResult, Product } from '../../models/product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = "https://localhost:5001/api/products";

  constructor(private http: HttpClient) {}

  getProducts(): Observable<DataResult<Product[]>> {
    return this.http.get<DataResult<Product[]>>(this.apiUrl + "/getall");
  }
}
