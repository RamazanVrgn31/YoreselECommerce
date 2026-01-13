import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/Auth/auth';
import { BasketService } from '../../services/Basket/basket';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule,CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {

  constructor(public authService: AuthService,
    public basketService: BasketService
  ) {}
ngOnInit() {
    if (this.authService.isAuthenticated()) {
        this.basketService.updateCount();
    }
}
  logout() {
    this.authService.Logout();
  }
}
