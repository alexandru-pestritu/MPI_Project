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

/**
 * An HTTP interceptor that attaches the JWT access token to outgoing requests,
 * and handles missing tokens by logging out and redirecting the user.
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  /**
   * Creates an instance of AuthInterceptor.
   * @param authService The authentication service used for handling logout.
   * @param localStorageService Service used to retrieve tokens from local storage.
   * @param router Angular's router for navigation on auth failure.
   */
  constructor(
    private authService: AuthService,
    private localStorageService: LocalStorageService,
    private router: Router
  ) {}

  /**
   * Intercepts outgoing HTTP requests to attach an Authorization header.
   * Skips requests to authentication endpoints.
   *
   * @param req The outgoing HTTP request.
   * @param next The next handler in the request pipeline.
   * @returns An observable of the HTTP event.
   */
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
