import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/**
 * A wrapper around Angular's HttpClient to simplify HTTP requests
 * with a base API URL and optional headers.
 */
@Injectable({
  providedIn: 'root'
})
export class HttpService {

  /**
   * The base URL for API endpoints.
   */
  private apiUrl = 'http://localhost:8080/api';

  /**
   * Creates an instance of HttpService.
   * @param http Angular's HttpClient used for sending HTTP requests.
   */
  constructor(private http: HttpClient) {}

  /**
   * Sends an HTTP GET request to the specified API endpoint.
   * @template T The expected response type.
   * @param endpoint Relative API endpoint (e.g., 'user/profile').
   * @param headers Optional HTTP headers to include in the request.
   * @returns An Observable of type `T`.
   */
  get<T>(endpoint: string, headers?: HttpHeaders): Observable<T> {
    return this.http.get<T>(`${this.apiUrl}/${endpoint}`, { headers });
  }

  /**
   * Sends an HTTP POST request to the specified API endpoint with a request body.
   * @template T The expected response type.
   * @param endpoint Relative API endpoint.
   * @param body The payload to send in the request body.
   * @param headers Optional HTTP headers.
   * @returns An Observable of type `T`.
   */
  post<T>(endpoint: string, body: any, headers?: HttpHeaders): Observable<T> {
    return this.http.post<T>(`${this.apiUrl}/${endpoint}`, body, { headers });
  }



  /**
 * Sends an HTTP POST request with `FormData` to the specified API endpoint.
 * @template T The expected response type.
 * @param endpoint Relative API endpoint.
 * @param formData The `FormData` payload to send in the request body.
 * @returns An Observable of type `T`.
 */
  postFormData<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.http.post<T>(`${this.apiUrl}/${endpoint}`, formData, {
      responseType: 'json',
    });
  }

  /**
   * Sends an HTTP PUT request to the specified API endpoint with a request body.
   * @template T The expected response type.
   * @param endpoint Relative API endpoint.
   * @param body The payload to update.
   * @param headers Optional HTTP headers.
   * @returns An Observable of type `T`.
   */
  put<T>(endpoint: string, body: any, headers?: HttpHeaders): Observable<T> {
    return this.http.put<T>(`${this.apiUrl}/${endpoint}`, body, { headers });
  }

  /**
   * Sends an HTTP DELETE request to the specified API endpoint.
   * @template T The expected response type.
   * @param endpoint Relative API endpoint.
   * @param headers Optional HTTP headers.
   * @returns An Observable of type `T`.
   */
  delete<T>(endpoint: string, headers?: HttpHeaders): Observable<T> {
    return this.http.delete<T>(`${this.apiUrl}/${endpoint}`, { headers });
  }

  
}
