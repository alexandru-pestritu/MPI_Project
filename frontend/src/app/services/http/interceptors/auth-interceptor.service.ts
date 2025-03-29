import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest
} from '@angular/common/http';

import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { LocalStorageService } from '../../local-storage/local-storage.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService, private localStorageService: LocalStorageService, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const excludedUrls = [
      '/api/auth/login',
      '/api/auth/register',
      '/api/auth/forgot-password',
      '/api/auth/reset-password',
    ];

    const shouldSkip = excludedUrls.some(url => req.url.includes(url));

    if (shouldSkip) {
      return next.handle(req);
    }

    const token = this.localStorageService.getItem('token');

    if (!token) {
      this.authService.logout();
      this.router.navigate(['/login']);
      throw new Error('No access token found. Logging out.');
    }

    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    return next.handle(authReq);
  }
}

