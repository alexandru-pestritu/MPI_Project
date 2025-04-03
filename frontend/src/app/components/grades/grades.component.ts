import { Component } from '@angular/core';
import { GradeService } from '../../services/grade/grade.service';
import { NotificationService } from '../../services/notification/notification.service';
import { Grade } from '../../models/grade';

@Component({
  selector: 'app-grades',
  standalone: false,
  templateUrl: './grades.component.html',
  styleUrl: './grades.component.css'
})
export class GradesComponent {
  grades: Grade[] = [];

  constructor(
    private gradeService: GradeService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadStudentGrades();
  }

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
}
