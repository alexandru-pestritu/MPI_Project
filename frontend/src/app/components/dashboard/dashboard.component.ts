import { Component, OnInit } from '@angular/core';
import { CourseService } from '../../services/course/course.service';
import { NotificationService } from '../../services/notification/notification.service';
import { ConfirmationService } from 'primeng/api';
import { Router } from '@angular/router';
import { Course } from '../../models/course';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {

  courses: Course[] = [];

  teacherId:number | null = -1;
  role:string | null = "";

  courseDialogVisible: boolean = false;
  editingCourse: Course = {} as Course;
  submitted: boolean = false;
  loading: boolean = false;

  constructor(
    private authService: AuthService,
    private courseService: CourseService,
    private notificationService: NotificationService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCourses();
    this.teacherId=this.authService.getUserIdFromToken();
    this.role = this.authService.getRoleFromToken();

  }

  loadCourses(): void {
    this.courseService.getAllCourses().subscribe({
      next: (courses) => {
        this.courses = courses;
      },
      error: (err) => {
        console.error('Error fetching courses', err);
        this.notificationService.showError('Error', 'Failed to load courses.');
      }
    });
  }

  onSelectCourse(course: Course): void {
    
    this.router.navigate([`/course/${course.id}`]);
  }

  onAddNewCourse(): void {
    this.editingCourse = {} as Course;
    this.courseDialogVisible = true;
    this.submitted = false;
    this.loading = false;
  }

  onEditCourse(course: Course): void {
    this.editingCourse = { ...course }; 
    this.courseDialogVisible = true;
    this.submitted = false;
    this.loading = false;
  }

  hideDialog(): void {
    this.courseDialogVisible = false;
    this.editingCourse = {} as Course;
    this.submitted = false;
    this.loading = false;
  }

  saveCourse(): void {
    this.submitted = true;

   
    if (!this.editingCourse.name || !this.editingCourse.description) {
      return;
    }

    this.loading = true;
    this.editingCourse.teacherId = this.teacherId!;
   
    if (this.editingCourse.id) {
      this.courseService.editCourse(this.editingCourse).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Course updated successfully.');
          this.courseDialogVisible = false;
          this.loadCourses();
          this.loading = false;
        },
        error: (err) => {
          console.error('Error updating course', err);
          this.notificationService.showError('Error', 'Failed to update course.');
          this.loading = false;
        }
      });
    } else {
      this.courseService.addCourse(this.editingCourse).subscribe({
        next: (createdCourse) => {
          this.notificationService.showSuccess('Success', 'Course created successfully.');
          this.courseDialogVisible = false;
          this.loadCourses();
          this.loading = false;
        },
        error: (err) => {
          console.error('Error creating course', err);
          this.notificationService.showError('Error', 'Failed to create course.');
          this.loading = false;
        }
      });
    }
  }

  onDeleteCourse(): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${this.editingCourse.name}"?`,
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading = true;
        this.courseService.deleteCourse(this.editingCourse.id).subscribe({
          next: () => {
            this.notificationService.showSuccess(
              'Success',
              `Course "${this.editingCourse.name}" deleted successfully.`
            );
            this.courseDialogVisible = false;
            this.loadCourses();
            this.loading = false;
          },
          error: (err) => {
            console.error('Error deleting course', err);
            this.notificationService.showError('Error', `Failed to delete course "${this.editingCourse.name}".`);
            this.loading = false;
          }
        });
      }
    });
  }
}

