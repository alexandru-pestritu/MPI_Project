import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../services/user/user.service';
import { NotificationService } from '../../services/notification/notification.service';
import { UserProfile } from '../../models/user-profile';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: false,
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent {
  userProfileForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userProfileForm = this.fb.group({
      id: [null],
      userId: [null],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      bio: ['']
    });

    this.userService.getProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userProfileForm.patchValue(profile);
      },
      error: (error) => {
        this.notificationService.showError('Error', 'Unable to load profile.');
      }
    });
  }

  onSubmit(): void {
    if (this.userProfileForm.valid) {
      this.userService.updateProfile(this.userProfileForm.value).subscribe({
        next: (updatedProfile: UserProfile) => {
          this.notificationService.showSuccess('Profile updated', 'Your profile has been updated successfully!');
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.notificationService.showError('Error', 'Failed to update profile. Please try again.');
        }
      });
    }
  }

}
