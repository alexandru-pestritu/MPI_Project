import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { HttpService } from '../http/http.service';
import { LocalStorageService } from '../local-storage/local-storage.service';
import { jwtDecode } from 'jwt-decode';

/**
 * Service responsible for user authentication, registration, and token management.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private endpoint = 'auth';

  /**
   * Creates an instance of AuthService.
   * @param httpService The HTTP service for communicating with the backend API.
   * @param localStorageService The service for accessing browser local storage.
   */
  constructor(
    private httpService: HttpService,
    private localStorageService: LocalStorageService
  ) {}

  /**
   * Sends a login request and stores the access token on success.
   * @param email User's email.
   * @param password User's password.
   * @returns An observable that completes when the login is successful.
   */
  login(email: string, password: string): Observable<void> {
    const body = { email, password };
    return this.httpService.post<any>(`${this.endpoint}/login`, body).pipe(
      tap(response => {
        this.localStorageService.setItem('token', response.token);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Registers a new user with the provided details.
   * @param username Chosen username.
   * @param email User's email.
   * @param password User's password.
   * @param confirmpassword Confirmation of the password.
   * @param role Role of the user as a numeric identifier.
   * @returns An observable that completes when the registration is successful.
   */
  register(username: string, email: string, password: string, confirmpassword: string, role: number): Observable<void> {
    const body = { username, email, password, confirmpassword, role };
    return this.httpService.post<any>(`${this.endpoint}/register`, body);
  }

  /**
   * Sends a forgot password request.
   * @param email User's email.
   * @returns An observable that completes when the request is processed.
   */
  forgot_password(email: string): Observable<void> {
    const body = { email };
    return this.httpService.post<any>(`${this.endpoint}/forgot-password`, body);
  }

  /**
   * Sends a reset password request with a token and new credentials.
   * @param token Reset token.
   * @param password New password.
   * @param confirmPassword Confirmation of the new password.
   * @returns An observable that completes when the password is reset.
   */
  reset_password(token: string, password: string, confirmPassword: string): Observable<void> {
    const body = { token, password, confirmPassword };
    return this.httpService.post<any>(`${this.endpoint}/reset-password`, body);
  }

  /**
   * Logs the user out by removing the stored token.
   */
  logout(): void {
    this.localStorageService.removeItem('token');
  }

  /**
   * Checks whether the stored JWT token is present and not expired.
   * @returns True if the token exists and is valid, otherwise false.
   */
  isLoggedIn(): boolean {
    return this.localStorageService.getItem('token') !== null && this.hasValidToken();
  }

  /**
   * Extracts the user ID from the stored JWT token.
   * @returns The user ID as a number, or null if not available or invalid.
   */
  getUserIdFromToken(): number | null {
    const token = this.localStorageService.getItem("token");
    if (!token) return null;

    try {
      const { UserId } = jwtDecode<{ UserId: string }>(token);
      return Number.parseInt(UserId);
    } catch (e) {
      return null;
    }
  }

  /**
   * Extracts the user's role from the stored JWT token.
   * @returns The user's role as a string, or null if not available or invalid.
   */
  getRoleFromToken(): string | null {
    const token = this.localStorageService.getItem("token");
    if (!token) return null;

    try {
      const { Role } = jwtDecode<{ Role: string }>(token);
      return Role;
    } catch (e) {
      return null;
    }
  }

  /**
   * Validates whether the JWT token exists and is not expired.
   * @returns True if the token is valid, otherwise false.
   */
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

  /**
   * Handles HTTP errors.
   * @param error The error object.
   * @returns An observable that throws the error.
   */
  private handleError(error: any): Observable<never> {
    return throwError(error);
  }
}
