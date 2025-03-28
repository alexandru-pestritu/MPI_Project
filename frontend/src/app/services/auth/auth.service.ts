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
    

  private handleError(error: any): Observable<never> {
    return throwError(error);
  }
}
