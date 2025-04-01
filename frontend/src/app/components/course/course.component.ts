import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../services/notification/notification.service';
import { CourseService } from '../../services/course/course.service';
import { UserService } from '../../services/user/user.service';

import { Course } from '../../models/course';
import { UserProfile } from '../../models/user-profile';
import { CourseStudentLink } from '../../models/course-student-link';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-course',
  standalone: false,
  templateUrl: './course.component.html',
  styleUrls: ['./course.component.css']
})
export class CourseComponent implements OnInit {
  courseId!: number;
  course: Course | null = null;

  teacherName: string = '';
  courseStudents: UserProfile[] = [];


  role:string | null = "";

  
  addStudentDialogVisible: boolean = false;
  availableStudents: UserProfile[] = [];
  selectedStudentIds: number[] = [];

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private courseService: CourseService,
    private userService: UserService,
    private notificationService: NotificationService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    
    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));
    this.role = this.authService.getRoleFromToken();
    this.loadCourse();
  }

  
  loadCourse(): void {
    this.courseService.getCourseById(this.courseId).subscribe({
      next: (courseData) => {
        this.course = courseData;
        
        if (this.course) {
          this.userService.getProfileById(this.course.teacherId).subscribe({
            next: (teacherProfile) => {
              this.teacherName = `${teacherProfile.firstName} ${teacherProfile.lastName}`;
            },
            error: (err) => {
              console.error('Error fetching teacher profile', err);
              this.teacherName = 'Unknown';
            }
          });
        }
       
        this.loadStudentsInCourse();
      },
      error: (err) => {
        console.error('Error loading course', err);
        this.notificationService.showError('Error', 'Failed to load course.');
      }
    });
  }

  loadStudentsInCourse(): void {
    if (!this.course) return;
    this.courseService.getStudentsInCourse(this.course.id).subscribe({
      next: (students) => {
        this.courseStudents = students;
      },
      error: (err) => {
        console.error('Error fetching course students', err);
        this.notificationService.showError('Error', 'Failed to load students.');
      }
    });
  }

  
  openAddStudentDialog(): void {
    this.userService.getAllStudents().subscribe({
      next: (allStudents) => {
        const existingIds = this.courseStudents.map(st => st.id);
        this.availableStudents = allStudents
          .filter(st => !existingIds.includes(st.id))
          .map(st => ({
            ...st,
            fullName: `${st.firstName} ${st.lastName}`
          }));
  
        this.selectedStudentIds = [];
        this.addStudentDialogVisible = true;
      },
      error: (err) => {
        console.error('Error fetching all students', err);
        this.notificationService.showError('Error', 'Failed to load available students.');
      }
    });
  }

  closeAddStudentDialog(): void {
    this.addStudentDialogVisible = false;
    this.selectedStudentIds = [];
  }

  onAddStudents(): void {
    if (!this.course || !this.selectedStudentIds.length) {
      return;
    }

    const link: CourseStudentLink = {
      courseId: this.course.id,
      studentIds: this.selectedStudentIds
    };

    this.courseService.addStudentToCourse(link).subscribe({
      next: () => {
        this.notificationService.showSuccess('Success', 'Student(s) added to the course.');
        this.closeAddStudentDialog();
        this.loadStudentsInCourse();
      },
      error: (err) => {
        console.error('Error adding students', err);
        this.notificationService.showError('Error', 'Failed to add student(s) to the course.');
      }
    });
  }

 
  onRemoveStudent(student: UserProfile): void {
    if (!this.course) return;

    this.confirmationService.confirm({
      message: `Remove ${student.firstName} ${student.lastName} from this course?`,
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const link: CourseStudentLink = {
          courseId: this.course!.id,
          studentIds: [student.id]  
        };

        this.courseService.removeStudentFromCourse(link).subscribe({
          next: () => {
            this.notificationService.showSuccess(
              'Success',
              `Removed ${student.firstName} ${student.lastName} from the course.`
            );
            this.loadStudentsInCourse();
          },
          error: (err) => {
            console.error('Error removing student', err);
            this.notificationService.showError(
              'Error',
              `Failed to remove ${student.firstName} ${student.lastName}.`
            );
          }
        });
      }
    });
  }
}
