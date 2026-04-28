import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { friendlyIdentityApiError } from '../../utils/auth-http.util';

const VALID_ROLES = ['Customer', 'Dealer'] as const;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  fullName = '';
  email = '';
  phoneNumber = '';
  password = '';
  role = '';
  errorMessage = '';
  successMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    if (this.authService.isLoggedIn()) {
      void this.router.navigate(['/dashboard']);
    }
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const r = (params['role'] || '').trim();
      if (!r || !VALID_ROLES.includes(r as (typeof VALID_ROLES)[number])) {
        void this.router.navigate(['/register']);
        return;
      }
      this.role = r;
    });
  }

  onRegister() {
    this.errorMessage = '';
    this.successMessage = '';
    if (!this.role) {
      void this.router.navigate(['/register']);
      return;
    }

    this.isLoading = true;
    this.authService.register(this.fullName, this.email, this.password, this.role, this.phoneNumber || null).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Account created. Redirecting to sign in…';
        setTimeout(() => void this.router.navigate(['/login']), 1400);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Registration failed. Try again.');
      }
    });
  }

}
