import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../services/notification/notification.service';
import { CourseService } from '../../services/course/course.service';
import { UserService } from '../../services/user/user.service';
import { AuthService } from '../../services/auth/auth.service';
import { GradeService } from '../../services/grade/grade.service';

import { Course } from '../../models/course';
import { UserProfile } from '../../models/user-profile';
import { CourseStudentLink } from '../../models/course-student-link';
import { Grade } from '../../models/grade';

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

  
  role: string | null = "";

  
  addStudentDialogVisible: boolean = false;
  availableStudents: UserProfile[] = [];
  selectedStudentIds: number[] = [];

 
  grades: Grade[] = []; 

  
  addMultipleGradesDialogVisible: boolean = false;
  selectedStudentsForGrades: number[] = [];
  gradeValue: number | null = null;
  gradeDate: Date = new Date();

  
  manageGradesDialogVisible: boolean = false;
  selectedStudentForGrades: UserProfile | null = null;
  gradesForSelectedStudent: Grade[] = [];

  
  singleGradeDialogVisible: boolean = false;
  editingGrade: Grade | null = null;
  singleGradeValue: number | null = null;
  singleGradeDate: Date = new Date();

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private courseService: CourseService,
    private userService: UserService,
    private gradeService: GradeService,
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

         
          this.loadStudentsInCourse();

         
          this.loadGrades();
        }
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

  loadGrades(): void {
    if (!this.course) return;
    this.gradeService.getGrades(this.course.id).subscribe({
      next: (gradesList) => {
        this.grades = gradesList;
      },
      error: (err) => {
        console.error('Error fetching grades', err);
        this.notificationService.showError('Error', 'Failed to load grades.');
      }
    });
  }

 
  getLatestGrade(studentId: number): number | null {
    const studentGrades = this.grades
      .filter(g => g.studentId === studentId)
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    return studentGrades.length ? studentGrades[0].value : null;
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
            
            this.grades = this.grades.filter(g => g.studentId !== student.id);
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

 
 
  openAddMultipleGradesDialog(): void {
    this.selectedStudentsForGrades = [];
    this.gradeValue = null;
    this.gradeDate = new Date();
    this.addMultipleGradesDialogVisible = true;
  }

  closeAddMultipleGradesDialog(): void {
    this.addMultipleGradesDialogVisible = false;
    this.selectedStudentsForGrades = [];
    this.gradeValue = null;
    this.gradeDate = new Date();
  }

  onAddMultipleGrades(): void {
    if (!this.course || !this.gradeValue || !this.selectedStudentsForGrades.length) {
      return;
    }

    const newGrades: Grade[] = this.selectedStudentsForGrades.map((studentId) => ({
      id: 0, 
      courseId: this.course!.id,
      studentId: studentId,
      value: this.gradeValue!,
      date: this.gradeDate
    }));

    this.gradeService.addGrades(newGrades).subscribe({
      next: (addedGrades) => {
        this.notificationService.showSuccess('Success', 'Grades added successfully.');
       
        this.grades = [...this.grades, ...addedGrades];
        this.closeAddMultipleGradesDialog();
      },
      error: (err) => {
        console.error('Error adding multiple grades', err);
        this.notificationService.showError('Error', 'Failed to add grades.');
      }
    });
  }

 


  openManageGradesDialog(student: UserProfile): void {
    this.selectedStudentForGrades = student;
    this.gradesForSelectedStudent = this.grades.filter(g => g.studentId === student.id);
    this.manageGradesDialogVisible = true;
  }

  closeManageGradesDialog(): void {
    this.manageGradesDialogVisible = false;
    this.selectedStudentForGrades = null;
    this.gradesForSelectedStudent = [];
  }


 
  openAddSingleGradeDialog(): void {
    this.editingGrade = null;
    this.singleGradeValue = null;
    this.singleGradeDate = new Date();
    this.singleGradeDialogVisible = true;
  }

  openEditGradeDialog(grade: Grade): void {
    this.editingGrade = grade;
    this.singleGradeValue = grade.value;
    this.singleGradeDate = new Date(grade.date);
    this.singleGradeDialogVisible = true;
  }

  closeSingleGradeDialog(): void {
    this.singleGradeDialogVisible = false;
    this.editingGrade = null;
    this.singleGradeValue = null;
    this.singleGradeDate = new Date();
  }

  onSaveSingleGrade(): void {
    if (!this.selectedStudentForGrades || !this.course || !this.singleGradeValue) {
      return;
    }

   
    if (this.editingGrade) {
      const updatedGrade: Grade = {
        ...this.editingGrade,
        value: this.singleGradeValue,
        date: this.singleGradeDate
      };
      this.gradeService.editGrade(updatedGrade).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Grade updated.');
         
          this.grades = this.grades.map((g) => g.id === updatedGrade.id ? updatedGrade : g);
          this.gradesForSelectedStudent = this.gradesForSelectedStudent.map((g) =>
            g.id === updatedGrade.id ? updatedGrade : g
          );
          this.closeSingleGradeDialog();
        },
        error: (err) => {
          console.error('Error editing grade', err);
          this.notificationService.showError('Error', 'Failed to update grade.');
        }
      });
    } else {
      
      const newGrade: Grade = {
        id: 0,
        courseId: this.course.id,
        studentId: this.selectedStudentForGrades.id,
        value: this.singleGradeValue,
        date: this.singleGradeDate
      };
      this.gradeService.addGrades([newGrade]).subscribe({
        next: ([createdGrade]) => {
          this.notificationService.showSuccess('Success', 'Grade added.');
          
          this.grades.push(createdGrade);
          this.gradesForSelectedStudent.push(createdGrade);
          this.closeSingleGradeDialog();
        },
        error: (err) => {
          console.error('Error adding grade', err);
          this.notificationService.showError('Error', 'Failed to create grade.');
        }
      });
    }
  }

 
  onDeleteGrade(gradeId: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this grade?',
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.gradeService.deleteGrade(gradeId).subscribe({
          next: () => {
            this.notificationService.showSuccess('Success', 'Grade deleted.');
           
            this.grades = this.grades.filter((g) => g.id !== gradeId);
            this.gradesForSelectedStudent = this.gradesForSelectedStudent.filter(
              (g) => g.id !== gradeId
            );
          },
          error: (err) => {
            console.error('Error deleting grade', err);
            this.notificationService.showError('Error', 'Failed to delete grade.');
          }
        });
      }
    });
  }
}
