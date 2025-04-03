import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth/auth.service';

/**
 * Component responsible for rendering the main navigation menu and user options.
 */
@Component({
  selector: 'app-menu',
  standalone: false,
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent {

  /**
   * Indicates whether the user is currently logged in.
   */
  isLoggedIn = false;

  /**
   * Navigation menu items based on the user's role.
   */
  menuItems: MenuItem[] = [];

  /**
   * User dropdown menu items (e.g., profile, logout).
   */
  userItems: MenuItem[] = [];

  /**
   * Initializes the component and sets up menus if the user is authenticated.
   * @param router Angular Router for navigation.
   * @param authService Service for handling authentication logic.
   * @param confirmationService PrimeNG service for confirmation dialogs.
   */
  constructor(
    private router: Router,
    private authService: AuthService,
    private confirmationService: ConfirmationService
  ) {}

  /**
   * Lifecycle hook that runs after component initialization.
   * Checks authentication and initializes menu items accordingly.
   */
  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    if (this.isLoggedIn) {
      this.initMenu();
      this.initUserMenu();
    }
  }

  /**
   * Initializes the main menu items based on the user's role.
   */
  initMenu(): void {
    if (this.authService.getRoleFromToken() === 'Teacher') {
      this.menuItems = [
        { label: 'Courses', icon: 'pi pi-book', routerLink: ['/courses'] }
      ];
    } else {
      this.menuItems = [
        { label: 'Courses', icon: 'pi pi-book', routerLink: ['/courses'] },
        { label: 'Grades', icon: 'pi pi-chart-line', routerLink: ['/grades'] }
      ];
    }
  }

  /**
   * Initializes the user dropdown menu with profile and logout options.
   */
  initUserMenu(): void {
    this.userItems = [
      { label: 'Profile', icon: 'pi pi-user', routerLink: ['/profile'] },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.confirmLogout()
      }
    ];
  }

  /**
   * Displays a confirmation dialog before logging the user out.
   * If accepted, logs out and redirects to the login page.
   */
  confirmLogout(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to logout?',
      header: 'Confirm Logout',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.authService.logout();
        this.router.navigate(['/login']);
      }
    });
  }
}
