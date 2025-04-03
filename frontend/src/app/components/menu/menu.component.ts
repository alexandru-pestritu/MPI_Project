import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-menu',
  standalone: false,
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent {
  isLoggedIn = false;
  menuItems: MenuItem[] = [];
  userItems: MenuItem[] = [];

  constructor(
    private router: Router,
    private authService: AuthService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
      if (this.isLoggedIn) {
        this.initMenu();
        this.initUserMenu();
      }
  }

  initMenu() {
    if(this.authService.getRoleFromToken() === 'Teacher') {
    this.menuItems = [
      { label: 'Courses', icon: 'pi pi-book', routerLink: ['/courses'] },
    ];
  }
    else
    {
      this.menuItems = [
      { label: 'Courses', icon: 'pi pi-book', routerLink: ['/courses'] },
      { label: 'Grades', icon: 'pi pi-chart-line', routerLink: ['/grades'] }
    ];
  }
  }

  initUserMenu() {
    this.userItems = [
      { label: 'Profile', icon: 'pi pi-user', routerLink: ['/profile'] },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.confirmLogout()
      }
    ];
  }

  confirmLogout() {
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
