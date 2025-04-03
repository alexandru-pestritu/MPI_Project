import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../services/user/user.service';
import { NotificationService } from '../../services/notification/notification.service';
import { UserProfile } from '../../models/user-profile';
import { Router } from '@angular/router';

/**
 * Component responsible for displaying and updating the user's profile.
 */
@Component({
  selector: 'app-user-profile',
  standalone: false,
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent {

  /**
   * Reactive form group for user profile data.
   */
  userProfileForm!: FormGroup;

  /**
   * Initializes the component and loads the user's profile.
   * @param fb FormBuilder for creating the reactive form.
   * @param userService Service for retrieving and updating user profile data.
   * @param notificationService Service for displaying success and error notifications.
   * @param router Angular Router for navigation after update.
   */
  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  /**
   * Lifecycle hook that initializes the form and populates it with the current user's profile data.
   */
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
      error: () => {
        this.notificationService.showError('Error', 'Unable to load profile.');
      }
    });
  }

  /**
   * Submits the user profile form to update the profile information.
   * Shows success or error notifications based on the response.
   */
  onSubmit(): void {
    if (this.userProfileForm.valid) {
      this.userService.updateProfile(this.userProfileForm.value).subscribe({
        next: (updatedProfile: UserProfile) => {
          this.notificationService.showSuccess('Profile updated', 'Your profile has been updated successfully!');
          this.router.navigate(['/dashboard']);
        },
        error: () => {
          this.notificationService.showError('Error', 'Failed to update profile. Please try again.');
        }
      });
    }
  }
}
