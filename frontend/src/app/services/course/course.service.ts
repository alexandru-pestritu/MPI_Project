import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { Course } from '../../models/course';
import { CourseStudentLink } from '../../models/course-student-link';
import { Observable } from 'rxjs';
import { UserProfile } from '../../models/user-profile';

/**
 * Service for handling course-related operations, including management and enrollment.
 */
@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private endpoint = 'course';

  /**
   * Creates an instance of CourseService.
   * @param httpService The service used to perform HTTP requests.
   */
  constructor(private httpService: HttpService) {}

  /**
   * Retrieves all courses available to the current user.
   * @returns An observable containing a list of courses.
   */
  getAllCourses(): Observable<Course[]> {
    return this.httpService.get<Course[]>(`${this.endpoint}/get-courses`);
  }

  /**
   * Adds a new course.
   * @param course The course object to add.
   * @returns An observable containing the newly created course.
   */
  addCourse(course: Course): Observable<Course> {
    return this.httpService.post<Course>(`${this.endpoint}/add-course`, course);
  }

  /**
   * Updates an existing course.
   * @param course The updated course data.
   * @returns An observable that completes when the update is successful.
   */
  editCourse(course: Course): Observable<any> {
    return this.httpService.put<Course>(`${this.endpoint}/edit-course`, course);
  }

  /**
   * Retrieves a course by its ID.
   * @param courseId The ID of the course to retrieve.
   * @returns An observable containing the course.
   */
  getCourseById(courseId: number): Observable<Course> {
    return this.httpService.get<Course>(`${this.endpoint}/get-course-by-id/${courseId}`);
  }

  /**
   * Deletes a course by its ID.
   * @param courseId The ID of the course to delete.
   * @returns An observable that completes when the course is deleted.
   */
  deleteCourse(courseId: number): Observable<any> {
    return this.httpService.delete<any>(`${this.endpoint}/delete-course/${courseId}`);
  }

  /**
   * Adds one or more students to a course.
   * @param courseStudentLink The link object containing course and student IDs.
   * @returns An observable that completes when students are added.
   */
  addStudentToCourse(courseStudentLink: CourseStudentLink): Observable<any> {
    return this.httpService.post<CourseStudentLink>(`${this.endpoint}/add-student-to-course`, courseStudentLink);
  }

  /**
   * Removes one or more students from a course.
   * @param courseStudentLink The link object containing course and student IDs.
   * @returns An observable that completes when students are removed.
   */
  removeStudentFromCourse(courseStudentLink: CourseStudentLink): Observable<any> {
    return this.httpService.post<CourseStudentLink>(`${this.endpoint}/remove-student-from-course`, courseStudentLink);
  }

  /**
   * Retrieves the list of students enrolled in a specific course.
   * @param courseId The ID of the course.
   * @returns An observable containing an array of student profiles.
   */
  getStudentsInCourse(courseId: number): Observable<UserProfile[]> {
    return this.httpService.get<UserProfile[]>(`${this.endpoint}/get-students-in-course/${courseId}`);
  }
}
