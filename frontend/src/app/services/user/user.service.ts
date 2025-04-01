import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { UserProfile } from '../../models/user-profile';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private endpoint = 'user';

  constructor(private httpService : HttpService) { }

  getProfile(): Observable<UserProfile> {
    return this.httpService.get<UserProfile>(`${this.endpoint}/profile`);
  }

  updateProfile(profile: UserProfile): Observable<UserProfile> {
    return this.httpService.put<UserProfile>(`${this.endpoint}/profile`, profile);
  }

  getAllStudents():Observable<UserProfile[]>{
    return this.httpService.get<UserProfile[]>(`${this.endpoint}/get-all-students`);
  }
}
