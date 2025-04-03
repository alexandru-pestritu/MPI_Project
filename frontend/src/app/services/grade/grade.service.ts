import { Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { Observable } from 'rxjs';
import { Grade } from '../../models/grade';

@Injectable({
  providedIn: 'root'
})
export class GradeService {
  private endpoint ="grade"; 
  constructor(private httpService:HttpService) { }


  getGrades(courseId:number):Observable<Grade[]>{
        return this.httpService.get<Grade[]>(`${this.endpoint}/get-grades/${courseId}`);
      }

  addGrades(grades:Grade[]):Observable<Grade[]>{
        return this.httpService.post<Grade[]>(`${this.endpoint}/add-grades`,grades);
      }

  editGrade(grade:Grade):Observable<any>{
        return this.httpService.put<Grade>(`${this.endpoint}/edit-grade`,grade);
      }

  deleteGrade(gradeId:number):Observable<any>{
        return this.httpService.delete<any>(`${this.endpoint}/delete-grade/${gradeId}`);
      }

  getStudentGrades():Observable<Grade[]>{
        return this.httpService.get<Grade[]>(`${this.endpoint}/get-grades-by-student`);
      }
}
