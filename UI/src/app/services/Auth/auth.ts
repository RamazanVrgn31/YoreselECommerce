import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  // Backend API adresin (Swagger'da gördüğün adres)
  private apiUrl = "https://localhost:5001/api/Auth";
  // NOT: Port numarasını (5001) kendi projendekiyle kontrol et (launchSettings.json)

  constructor(private http: HttpClient) {}

  Login(loginModel : any) {
    return this.http.post(this.apiUrl + "/login", loginModel);
  }
  Logout() {
    localStorage.removeItem("token");
    // İsteğe bağlı: Sayfayı yenile veya login'e at
    location.href = "/login";
  }

  // Kullanıcı giriş yapmış mı kontrolü (Basitçe)
  isAuthenticated(): boolean {
    return !!localStorage.getItem("token");
  }

}
