import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Auth } from '../../services/Auth/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {// Dosya adın 'Login' ise böyle kalabilir, ama standart 'LoginComponent'tir.
  // Buraya form değişkenlerini ve metodlarını yazacağız.
  //Örneğin:
  loginObj = {
    email: '',
    password: ''
  };
  constructor(
    private authService: Auth, // Auth servisini kullanmak için
    private router: Router // Sayfa yönlendirmesi için
  ) { }

  onLogin() {
    // Giriş işlemi burada yapılacak.
    this.authService.Login(this.loginObj).subscribe({
      next: (res: any) => {
        // 1. Başarılı ise Token'ı kaydet
        console.log("Giriş Başarılı:", res);
        if (res.success) {
          localStorage.setItem('token', res.data.token);
          alert('Giriş başarılı! Ana sayfaya yönlendiriliyorsunuz.');
          // this.router.navigate(['/dashboard']); // İleride açacağız
        }
      },
      error: (err) => {
        // 2. Hata varsa kullanıcıya bildir
        console.error("Giriş Hatası:", err);
        alert("Giriş başarısız!" + (err.error?.message || "Sunucu hatası."));
      }
    });

  }
}
