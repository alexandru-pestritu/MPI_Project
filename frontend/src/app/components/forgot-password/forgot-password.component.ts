import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth/auth.service';
import { NotificationService } from '../../services/notification/notification.service';

/**
 * Component responsible for handling the forgot password flow.
 */
@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
  standalone: false
})
export class ForgotPasswordComponent {

  /**
   * Reactive form group for the forgot password form.
   */
  forgotPasswordForm: FormGroup;

  /**
   * Initializes the forgot password form.
   * @param fb FormBuilder for constructing the form.
   * @param authService Service for handling password reset requests.
   * @param notificationService Service for displaying success or error notifications.
   */
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  /**
   * Submits the forgot password form.
   * Sends a password reset request and shows a notification based on the result.
   */
  onSubmit(): void {
    if (this.forgotPasswordForm.valid) {
      const { email } = this.forgotPasswordForm.value;
      this.authService.forgot_password(email).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Password reset email sent successfully.');
        },
        error: () => {
          this.notificationService.showError('Error', 'Failed to send password reset email.');
        }
      });
    }
  }
}
