import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { UserProfile } from '../../models/user-profile';
import { Observable } from 'rxjs';

/**
 * Service for performing user-related operations via HTTP.
 */
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private endpoint = 'user';

  /**
   * Creates an instance of UserService.
   * @param httpService The service used to send HTTP requests to the backend.
   */
  constructor(private httpService: HttpService) {}

  /**
   * Retrieves the currently authenticated user's profile.
   * @returns An observable containing the user's profile.
   */
  getProfile(): Observable<UserProfile> {
    return this.httpService.get<UserProfile>(`${this.endpoint}/profile`);
  }

  /**
   * Updates the currently authenticated user's profile.
   * @param profile The updated user profile.
   * @returns An observable containing the updated profile.
   */
  updateProfile(profile: UserProfile): Observable<UserProfile> {
    return this.httpService.put<UserProfile>(`${this.endpoint}/profile`, profile);
  }

  /**
   * Retrieves a list of all student user profiles.
   * @returns An observable containing an array of student profiles.
   */
  getAllStudents(): Observable<UserProfile[]> {
    return this.httpService.get<UserProfile[]>(`${this.endpoint}/get-all-students`);
  }

  /**
   * Retrieves the profile of a specific user by ID.
   * @param userId The ID of the user.
   * @returns An observable containing the user's profile.
   */
  getProfileById(userId: number): Observable<UserProfile> {
    return this.httpService.get<UserProfile>(`${this.endpoint}/get-user-profile/${userId}`);
  }
}
