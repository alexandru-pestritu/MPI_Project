import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { Course } from '../../models/course';
import { CourseStudentLink } from '../../models/course-student-link';
import { Observable } from 'rxjs';
import { UserProfile } from '../../models/user-profile';

@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private endpoint = "course"
  constructor(private httpService: HttpService) { }

    getAllCourses():Observable<Course[]>{
      return this.httpService.get<Course[]>(`${this.endpoint}/get-courses`);
    }

    addCourse(course:Course):Observable<Course>{
      return this.httpService.post<Course>(`${this.endpoint}/add-course`, course);
    }

    editCourse(course:Course):Observable<any>{
      return this.httpService.put<Course>(`${this.endpoint}/edit-course`, course);
    }



    deleteCourse(courseId:number):Observable<any>{
      return this.httpService.delete<any>(`${this.endpoint}/delete-course/${courseId}`);
    }

    addStudentToCourse(courseStudentLink:CourseStudentLink):Observable<any>{
      return this.httpService.post<CourseStudentLink>(`${this.endpoint}/add-student-to-course`, courseStudentLink);
    }
    removeStudentFromCourse(courseStudentLink:CourseStudentLink):Observable<any>{
      return this.httpService.post<CourseStudentLink>(`${this.endpoint}/remove-student-from-course`, courseStudentLink);
    }

    getStudentsInCourse(courseId:number):Observable<UserProfile[]>{
      return this.httpService.get<any>(`${this.endpoint}/get-students-in-course/${courseId}`);
    }

}
