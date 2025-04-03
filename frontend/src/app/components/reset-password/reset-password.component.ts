import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { NotificationService } from '../../services/notification/notification.service';

/**
 * Component responsible for handling the password reset process.
 */
@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
  standalone: false
})
export class ResetPasswordComponent implements OnInit {

  /**
   * Reactive form group for resetting the password.
   */
  resetPasswordForm: FormGroup;

  /**
   * The password reset token retrieved from the route.
   */
  token: string = '';

  /**
   * Initializes the component and builds the reset password form.
   * @param fb FormBuilder for constructing the form.
   * @param route ActivatedRoute for accessing route parameters.
   * @param authService Service for handling password reset requests.
   * @param router Angular Router for navigation.
   * @param notificationService Service for displaying notifications.
   */
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.resetPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  /**
   * Lifecycle hook that runs on component initialization.
   * Retrieves the reset token from the route parameters.
   */
  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') || '';
  }

  /**
   * Submits the reset password form.
   * Validates password match and triggers password reset.
   * Displays appropriate notifications based on success or failure.
   */
  onSubmit(): void {
    if (this.resetPasswordForm.valid) {
      const { password, confirmPassword } = this.resetPasswordForm.value;

      if (password !== confirmPassword) {
        this.notificationService.showError('Error', 'Passwords do not match.');
        return;
      }

      this.authService.reset_password(this.token, password, confirmPassword).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Password reset successfully.');
          this.router.navigate(['/login']);
        },
        error: () => {
          this.notificationService.showError('Error', 'Failed to reset password.');
        }
      });
    }
  }
}
