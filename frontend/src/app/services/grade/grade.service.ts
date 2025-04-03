import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { Observable } from 'rxjs';
import { Grade } from '../../models/grade';

/**
 * Service for handling grade-related operations such as retrieving, adding, editing, and deleting grades.
 */
@Injectable({
  providedIn: 'root'
})
export class GradeService {
  private endpoint = 'grade';

  /**
   * Creates an instance of GradeService.
   * @param httpService The service used to send HTTP requests.
   */
  constructor(private httpService: HttpService) {}

  /**
   * Retrieves all grades for a specific course.
   * @param courseId The ID of the course.
   * @returns An observable containing a list of grades.
   */
  getGrades(courseId: number): Observable<Grade[]> {
    return this.httpService.get<Grade[]>(`${this.endpoint}/get-grades/${courseId}`);
  }

  /**
   * Adds a list of grades.
   * @param grades An array of grade objects to add.
   * @returns An observable containing the added grades.
   */
  addGrades(grades: Grade[]): Observable<Grade[]> {
    return this.httpService.post<Grade[]>(`${this.endpoint}/add-grades`, grades);
  }

  /**
   * Updates a single grade.
   * @param grade The grade object with updated values.
   * @returns An observable that completes when the update is successful.
   */
  editGrade(grade: Grade): Observable<any> {
    return this.httpService.put<Grade>(`${this.endpoint}/edit-grade`, grade);
  }

  /**
   * Deletes a grade by its ID.
   * @param gradeId The ID of the grade to delete.
   * @returns An observable that completes when the grade is deleted.
   */
  deleteGrade(gradeId: number): Observable<any> {
    return this.httpService.delete<any>(`${this.endpoint}/delete-grade/${gradeId}`);
  }

  /**
   * Retrieves grades for the currently logged-in student.
   * @returns An observable containing a list of the student's grades.
   */
  getStudentGrades(): Observable<Grade[]> {
    return this.httpService.get<Grade[]>(`${this.endpoint}/get-grades-by-student`);
  }
}
