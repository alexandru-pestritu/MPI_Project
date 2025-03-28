import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification/notification.service';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  roles = [
    { label: 'Student', value: 0 },
    { label: 'Teacher', value: 1 }
  ];

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

  onSubmit(): void {
    if (this.registerForm.valid) {
      const { username, email, password, confirmPassword, role } = this.registerForm.value;

      this.authService.register(username, email, password, confirmPassword, role.value).subscribe({
        next: () => {
          this.notificationService.showSuccess('Success', 'Registration successful!');
          this.router.navigate(['/login']);
        },
        error: (error) => {
          this.notificationService.showError('Error', 'Registration failed. Please try again.');
        }
      });
    }
  }
}
