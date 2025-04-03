import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification/notification.service';

/**
 * Component responsible for handling user login.
 */
@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  /**
   * The reactive form group for login inputs.
   */
  loginForm: FormGroup;

  /**
   * Initializes the login component and redirects if the user is already logged in.
   * @param fb FormBuilder service for building the login form.
   * @param authService Service for handling authentication.
   * @param router Angular Router used for navigation.
   * @param notificationService Service for showing toast notifications.
   */
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  /**
   * Handles the login form submission.
   * If credentials are valid, navigates to the dashboard.
   * Otherwise, displays an error notification.
   */
  onSubmit(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      this.authService.login(email, password).subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.notificationService.showError('Error', 'Invalid credentials. Please try again.');
        }
      });
    }
  }
}
