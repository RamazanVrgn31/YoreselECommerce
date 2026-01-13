import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CategoryService } from '../../services/Category/category.service';
import { Category } from '../../models/category';

@Component({
  selector: 'app-category-menu',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category-menu.html',
  styleUrl: './category-menu.scss',
})
export class CategoryMenu implements OnInit {
  categories = signal<Category[]>([]);
  currentCategory = signal<number>(0); // 0 = Tüm Ürünler

  // Dışarıya (Home Component'e) haber vereceğiz
  @Output() categoryClick = new EventEmitter<number>();

  constructor(private categoryService: CategoryService) { }

  ngOnInit(): void {
    this.getCategories();
  }

  getCategories() {
    this.categoryService.getCategories().subscribe({
      next: (res) => {
        if (res.success) {
          this.categories.set(res.data);
        }
      },
      error: (err) => console.log("Kategori Hatası:", err)
    });
  }

  // Tıklanınca çalışacak metod
  selectCategory(categoryId: number) {
    this.currentCategory.set(categoryId);
    this.categoryClick.emit(categoryId); // Home Component'e ID'yi fırlat
  }

}
