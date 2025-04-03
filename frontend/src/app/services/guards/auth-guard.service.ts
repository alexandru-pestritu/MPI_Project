import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Route guard that prevents access to routes for unauthenticated users.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  /**
   * Creates an instance of AuthGuard.
   * @param authService The authentication service used to check login status.
   * @param router Angular Router used for redirecting unauthenticated users.
   */
  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Determines whether the route can be activated based on authentication status.
   * @param route The activated route snapshot.
   * @param state The router state snapshot.
   * @returns True if the user is logged in; otherwise, false and navigates to login.
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }
    return true;
  }
}
