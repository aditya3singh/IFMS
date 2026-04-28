import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

export interface RegisterRoleTile {
  roleId: 'Customer' | 'Dealer';
  headline: string;
  description: string;
  icon: string;
  accentClass: string;
}

@Component({
  selector: 'app-register-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './register-landing.component.html',
  styleUrls: ['./register-landing.component.css']
})
export class RegisterLandingComponent {
  readonly tiles: RegisterRoleTile[] = [
    {
      roleId: 'Customer',
      headline: 'Sign up as Customer',
      description: 'Book fuel online, pay securely, and use your token at the pump.',
      icon: 'local_gas_station',
      accentClass: 'accent-customer'
    },
    {
      roleId: 'Dealer',
      headline: 'Sign up as Dealer',
      description: 'Manage inventory and sales only for stations your administrator assigns to you.',
      icon: 'storefront',
      accentClass: 'accent-dealer'
    }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    if (this.authService.isLoggedIn()) {
      void this.router.navigate(['/dashboard']);
    }
  }

  continueAs(role: RegisterRoleTile['roleId']) {
    void this.router.navigate(['/register', 'signup'], { queryParams: { role } });
  }
}
