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

/**
 * Component responsible for displaying course details, managing enrolled students,
 * and handling grades (for teachers).
 */
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
  role: string | null = '';

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

  /**
   * Creates an instance of CourseComponent.
   * @param route ActivatedRoute to extract route parameters.
   * @param authService Service for auth and token info.
   * @param courseService Service to manage courses.
   * @param userService Service to manage user profiles.
   * @param gradeService Service to manage grades.
   * @param notificationService Service to show notifications.
   * @param confirmationService Service to handle confirmation dialogs.
   */
  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private courseService: CourseService,
    private userService: UserService,
    private gradeService: GradeService,
    private notificationService: NotificationService,
    private confirmationService: ConfirmationService
  ) {}

  /**
   * Initializes the component and loads course data.
   */
  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));
    this.role = this.authService.getRoleFromToken();
    this.loadCourse();
  }

  /**
   * Loads the course details, teacher info, enrolled students, and grades if applicable.
   */
  loadCourse(): void {
    this.courseService.getCourseById(this.courseId).subscribe({
      next: (courseData) => {
        this.course = courseData;
        if (this.course) {
          this.userService.getProfileById(this.course.teacherId).subscribe({
            next: (teacherProfile) => {
              this.teacherName = `${teacherProfile.firstName} ${teacherProfile.lastName}`;
            },
            error: () => {
              this.teacherName = 'Unknown';
            }
          });

          this.loadStudentsInCourse();
          if (this.role === 'Teacher') {
            this.loadGrades();
          }
        }
      },
      error: () => {
        this.notificationService.showError('Error', 'Failed to load course.');
      }
    });
  }

  /**
   * Loads the list of students enrolled in the course.
   */
  loadStudentsInCourse(): void {
    if (!this.course) return;
    this.courseService.getStudentsInCourse(this.course.id).subscribe({
      next: (students) => {
        this.courseStudents = students;
      },
      error: () => {
        this.notificationService.showError('Error', 'Failed to load students.');
      }
    });
  }

  /**
   * Loads all grades associated with the course.
   */
  loadGrades(): void {
    if (!this.course) return;
    this.gradeService.getGrades(this.course.id).subscribe({
      next: (gradesList) => {
        this.grades = gradesList;
      },
      error: () => {
        this.notificationService.showError('Error', 'Failed to load grades.');
      }
    });
  }

  /**
   * Returns the latest grade value for a given student.
   * @param studentId ID of the student.
   * @returns The most recent grade value or null.
   */
  getLatestGrade(studentId: number): number | null {
    const studentGrades = this.grades
      .filter(g => g.studentId === studentId)
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    return studentGrades.length ? studentGrades[0].value : null;
  }

  /**
   * Opens the dialog to add students to the course.
   */
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
      error: () => {
        this.notificationService.showError('Error', 'Failed to load available students.');
      }
    });
  }

  /**
   * Closes the add student dialog and resets selections.
   */
  closeAddStudentDialog(): void {
    this.addStudentDialogVisible = false;
    this.selectedStudentIds = [];
  }

  /**
   * Adds the selected students to the course.
   */
  onAddStudents(): void {
    if (!this.course || !this.selectedStudentIds.length) return;

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
      error: () => {
        this.notificationService.showError('Error', 'Failed to add student(s) to the course.');
      }
    });
  }

  /**
   * Removes a student from the course after confirmation.
   * @param student The student to remove.
   */
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
            this.notificationService.showSuccess('Success', `${student.firstName} removed.`);
            this.loadStudentsInCourse();
            this.grades = this.grades.filter(g => g.studentId !== student.id);
          },
          error: () => {
            this.notificationService.showError('Error', `Failed to remove ${student.firstName}.`);
          }
        });
      }
    });
  }

  /**
   * Opens dialog for adding the same grade to multiple students.
   */
  openAddMultipleGradesDialog(): void {
    this.selectedStudentsForGrades = [];
    this.gradeValue = null;
    this.gradeDate = new Date();
    this.addMultipleGradesDialogVisible = true;
  }

  /**
   * Closes the add multiple grades dialog and resets fields.
   */
  closeAddMultipleGradesDialog(): void {
    this.addMultipleGradesDialogVisible = false;
    this.selectedStudentsForGrades = [];
    this.gradeValue = null;
    this.gradeDate = new Date();
  }

  /**
   * Submits and saves the same grade for multiple students.
   */
  onAddMultipleGrades(): void {
    if (!this.course || !this.gradeValue || !this.selectedStudentsForGrades.length) return;

    const newGrades: Grade[] = this.selectedStudentsForGrades.map((studentId) => ({
      id: 0,
      courseId: this.course!.id,
      studentId,
      value: this.gradeValue!,
      date: this.gradeDate
    }));

    this.gradeService.addGrades(newGrades).subscribe({
      next: (addedGrades) => {
        this.notificationService.showSuccess('Success', 'Grades added successfully.');
        this.grades = [...this.grades, ...addedGrades];
        this.closeAddMultipleGradesDialog();
      },
      error: () => {
        this.notificationService.showError('Error', 'Failed to add grades.');
      }
    });
  }

  /**
   * Opens the grade management dialog for a specific student.
   * @param student The student whose grades are being managed.
   */
  openManageGradesDialog(student: UserProfile): void {
    this.selectedStudentForGrades = student;
    this.gradesForSelectedStudent = this.grades.filter(g => g.studentId === student.id);
    this.manageGradesDialogVisible = true;
  }

  /**
   * Closes the grade management dialog and clears selections.
   */
  closeManageGradesDialog(): void {
    this.manageGradesDialogVisible = false;
    this.selectedStudentForGrades = null;
    this.gradesForSelectedStudent = [];
  }

  /**
   * Opens dialog to add a single grade.
   */
  openAddSingleGradeDialog(): void {
    this.editingGrade = null;
    this.singleGradeValue = null;
    this.singleGradeDate = new Date();
    this.singleGradeDialogVisible = true;
  }

  /**
   * Opens dialog to edit an existing grade.
   * @param grade The grade to edit.
   */
  openEditGradeDialog(grade: Grade): void {
    this.editingGrade = grade;
    this.singleGradeValue = grade.value;
    this.singleGradeDate = new Date(grade.date);
    this.singleGradeDialogVisible = true;
  }

  /**
   * Closes the single grade dialog and resets values.
   */
  closeSingleGradeDialog(): void {
    this.singleGradeDialogVisible = false;
    this.editingGrade = null;
    this.singleGradeValue = null;
    this.singleGradeDate = new Date();
  }

  /**
   * Saves or updates a single grade.
   */
  onSaveSingleGrade(): void {
    if (!this.selectedStudentForGrades || !this.course || !this.singleGradeValue) return;

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
        error: () => {
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
        error: () => {
          this.notificationService.showError('Error', 'Failed to create grade.');
        }
      });
    }
  }

  /**
   * Deletes a specific grade after confirmation.
   * @param gradeId ID of the grade to delete.
   */
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
          error: () => {
            this.notificationService.showError('Error', 'Failed to delete grade.');
          }
        });
      }
    });
  }
}
