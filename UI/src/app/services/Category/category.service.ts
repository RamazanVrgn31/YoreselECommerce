import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DataResult } from '../../models/product';
import { Category } from '../../models/category';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private apiUrl = "https://localhost:5001/api/Categories";

  constructor(private http: HttpClient) {}

  getCategories(): Observable<DataResult<Category[]>> {
    return this.http.get<DataResult<Category[]>>(this.apiUrl + "/getall");
  }
}
