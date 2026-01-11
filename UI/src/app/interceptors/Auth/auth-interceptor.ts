import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // 1. Tarayıcı hafızasından Token'ı al
  const token = localStorage.getItem('token');

  // 2. Eğer Token varsa, isteğin başlığına (Header) ekle
  if (token) {
    const newReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(newReq);
  }
  // 3. Token yoksa isteği olduğu gibi gönder (Login vb. için)
  return next(req);
};
