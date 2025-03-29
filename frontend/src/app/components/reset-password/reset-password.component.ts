import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { NotificationService } from '../../services/notification/notification.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
  standalone: false
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  token: string = '';

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

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') || '';
  }

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
