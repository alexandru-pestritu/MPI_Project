import { Component, OnInit } from '@angular/core';
import { CourseService } from '../../services/course/course.service';
import { NotificationService } from '../../services/notification/notification.service';
import { ConfirmationService } from 'primeng/api';
import { Router } from '@angular/router';
import { Course } from '../../models/course';
import { AuthService } from '../../services/auth/auth.service';

/**
 * Dashboard component responsible for displaying, managing, and navigating between courses.
 */
@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {

  /**
   * List of courses associated with the user.
   */
  courses: Course[] = [];

  /**
   * ID of the currently logged-in teacher.
   */
  teacherId: number | null = -1;

  /**
   * Role of the currently logged-in user.
   */
  role: string | null = "";

  /**
   * Visibility state of the course form dialog.
   */
  courseDialogVisible: boolean = false;

  /**
   * Course being edited or created.
   */
  editingCourse: Course = {} as Course;

  /**
   * Indicates whether the form has been submitted.
   */
  submitted: boolean = false;

  /**
   * Indicates whether a course-related operation is in progress.
   */
  loading: boolean = false;

  /**
   * Creates an instance of DashboardComponent and injects required services.
   * @param authService Service for authentication and user info.
   * @param courseService Service for managing course data.
   * @param notificationService Service for showing success or error messages.
   * @param confirmationService Service for showing confirmation dialogs.
   * @param router Angular Router for navigation.
   */
  constructor(
    private authService: AuthService,
    private courseService: CourseService,
    private notificationService: NotificationService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {}

  /**
   * Lifecycle hook that initializes user info and loads available courses.
   */
  ngOnInit(): void {
    this.loadCourses();
    this.teacherId = this.authService.getUserIdFromToken();
    this.role = this.authService.getRoleFromToken();
  }

  /**
   * Fetches the list of courses available to the user.
   */
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

  /**
   * Navigates to the course details page.
   * @param course The course to open.
   */
  onSelectCourse(course: Course): void {
    this.router.navigate([`/course/${course.id}`]);
  }

  /**
   * Opens the dialog to add a new course.
   */
  onAddNewCourse(): void {
    this.editingCourse = {} as Course;
    this.courseDialogVisible = true;
    this.submitted = false;
    this.loading = false;
  }

  /**
   * Opens the dialog to edit an existing course.
   * @param course The course to edit.
   */
  onEditCourse(course: Course): void {
    this.editingCourse = { ...course };
    this.courseDialogVisible = true;
    this.submitted = false;
    this.loading = false;
  }

  /**
   * Hides the course dialog and resets the form state.
   */
  hideDialog(): void {
    this.courseDialogVisible = false;
    this.editingCourse = {} as Course;
    this.submitted = false;
    this.loading = false;
  }

  /**
   * Saves the current course (create or update).
   * Shows notifications based on the result.
   */
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
        next: () => {
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

  /**
   * Confirms and deletes the selected course.
   */
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
