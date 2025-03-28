import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { HttpService } from '../http/http.service';
import { LocalStorageService } from '../local-storage/local-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private endpoint = 'auth';

  constructor(private httpService: HttpService,
    private localStorageService: LocalStorageService) { }

    login(username: string, password: string): Observable<void> {
      const body = { username, password };
    
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
    
    

  private handleError(error: any): Observable<never> {
    return throwError(error);
  }
}
