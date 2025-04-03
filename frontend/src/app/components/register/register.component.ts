import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification/notification.service';
import { AuthService } from '../../services/auth/auth.service';

/**
 * Component responsible for handling user registration.
 */
@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  /**
   * Reactive form group for user registration.
   */
  registerForm: FormGroup;

  /**
   * Available user roles for selection.
   */
  roles = [
    { label: 'Student', value: 0 },
    { label: 'Teacher', value: 1 }
  ];

  /**
   * Initializes the register component and builds the registration form.
   * @param fb FormBuilder service to create the form.
   * @param authService Service for handling registration logic.
   * @param router Angular Router used for navigation after successful registration.
   * @param notificationService Service for displaying notifications.
   */
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required],
      role: [null, Validators.required]
    });
  }

  /**
   * Submits the registration form.
   * On success, shows a success message and navigates to the login page.
   * On failure, shows an error notification.
   */
  onSubmit(): void {
    if (this.registerForm.valid) {
      const { username, email, password, confirmPassword, role } = this.registerForm.value;

      this.authService.register(username, email, password, confirmPassword, role.value).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Registration successful!');
          this.router.navigate(['/login']);
        },
        error: () => {
          this.notificationService.showError('Error', 'Registration failed. Please try again.');
        }
      });
    }
  }
}
