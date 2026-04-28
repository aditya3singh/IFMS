import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { friendlyIdentityApiError } from '../../utils/auth-http.util';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  infoMessage = '';
  isLoading = false;

  otpIdentifier = '';
  otpCode = '';
  otpSent = false;
  otpLoading = false;

  activeTab: 'password' | 'otp' = 'password';
  showPassword = false;

  constructor(
    public authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    if (this.authService.isLoggedIn()) {
      void this.router.navigate(['/dashboard']);
      return;
    }
    this.route.queryParams.subscribe((params) => {
      if (params['reason'] === 'session') {
        this.infoMessage = 'Your session expired. Please sign in again.';
      }
    });
  }

  onLogin() {
    this.clearMessages();
    this.isLoading = true;
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.isLoading = false;
        void this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Invalid email or password.');
      }
    });
  }

  requestOtp() {
    this.clearMessages();
    if (!this.otpIdentifier.trim()) {
      this.errorMessage = 'Enter your registered email or mobile number.';
      return;
    }
    this.otpLoading = true;
    this.authService.requestLoginOtp(this.otpIdentifier.trim()).subscribe({
      next: () => {
        this.otpLoading = false;
        this.otpSent = true;
        this.infoMessage = 'A 6-digit code was sent to your email / SMS.';
      },
      error: (err) => {
        this.otpLoading = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Could not send code.');
      }
    });
  }

  verifyOtp() {
    this.clearMessages();
    if (!this.otpIdentifier.trim() || !this.otpCode.trim()) {
      this.errorMessage = 'Enter the identifier and the 6-digit code.';
      return;
    }
    this.otpLoading = true;
    this.authService.verifyLoginOtp(this.otpIdentifier.trim(), this.otpCode.trim()).subscribe({
      next: () => {
        this.otpLoading = false;
        void this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.otpLoading = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Invalid or expired code.');
      }
    });
  }

  private clearMessages() {
    this.errorMessage = '';
    this.infoMessage = '';
  }
}
