import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { HttpService } from '../http/http.service';
import { LocalStorageService } from '../local-storage/local-storage.service';
import { jwtDecode } from "jwt-decode";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private endpoint = 'auth';

  constructor(private httpService: HttpService,
    private localStorageService: LocalStorageService) { }

    login(email: string, password: string): Observable<void> {
      const body = { email, password };

      return this.httpService.post<any>(`${this.endpoint}/login`, body).pipe(
        tap(response => {
          this.localStorageService.setItem('token', response.token);
        }),
        catchError(this.handleError)
      );
    }

    register(username: string,email:string, password: string,confirmpassword:string,role:number): Observable<void> {
      const body = { username,email, password,confirmpassword,role };
      return this.httpService.post<any>(`${this.endpoint}/register`, body);

    }

    forgot_password(email:string): Observable<void> {
      const body = { email };
      return this.httpService.post<any>(`${this.endpoint}/forgot-password`, body);
    }

    reset_password(token:string,password:string,confirmPassword:string): Observable<void> {
      const body = { token,password,confirmPassword };
      return this.httpService.post<any>(`${this.endpoint}/reset-password`, body);
    }

    logout(): void {
      this.localStorageService.removeItem('token');
    }

    private hasValidToken(): boolean {
      const token = this.localStorageService.getItem("token");
      if (!token) {
        return false;
      }
  
      try {
        const { exp } = jwtDecode<{ exp: number }>(token);
        return exp > Date.now() / 1000;
      } catch (e) {
        return false;
      }
    }
    
    isLoggedIn(): boolean {
      return this.localStorageService.getItem('token') !== null && this.hasValidToken();
    }

  private handleError(error: any): Observable<never> {
    return throwError(error);
  }
}
