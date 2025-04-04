import { Component } from '@angular/core';
import { GradeService } from '../../services/grade/grade.service';
import { NotificationService } from '../../services/notification/notification.service';
import { Grade } from '../../models/grade';

/**
 * Component responsible for displaying the grades of the currently logged-in student.
 */
@Component({
  selector: 'app-grades',
  standalone: false,
  templateUrl: './grades.component.html',
  styleUrl: './grades.component.css'
})
export class GradesComponent {

  /**
   * Array of grades to be displayed.
   */
  grades: Grade[] = [];

  /**
   * The average grade retrived from the server.
   */
  averageGrade: number = 0;

  /**
   * Initializes the component with necessary services.
   * @param gradeService Service for retrieving student grades.
   * @param notificationService Service for displaying error notifications.
   */
  constructor(
    private gradeService: GradeService,
    private notificationService: NotificationService
  ) {}

  /**
   * Lifecycle hook that triggers grade loading on component initialization.
   */
  ngOnInit(): void {
    this.loadStudentGrades();
    this.getAverageGrade();
  }

  /**
   * Loads the grades for the currently logged-in student.
   * Updates the `grades` array or displays an error notification on failure.
   */
  loadStudentGrades(): void {
    this.gradeService.getStudentGrades().subscribe({
      next: (fetchedGrades) => {
        this.grades = fetchedGrades;
      },
      error: (error) => {
        console.error('Error loading student grades', error);
        this.notificationService.showError('Error', 'Failed to load grades.');
      }
    });
  }


  /**
   * Retrieves the average grade for the currently logged-in student.
   * @returns The average grade.
   */
  getAverageGrade() : number {
    this.gradeService.getAverageGrade().subscribe({
      next: (response) => {
        this.averageGrade = response.averageGrade;
      }
    });
    return this.averageGrade;
  }
}
