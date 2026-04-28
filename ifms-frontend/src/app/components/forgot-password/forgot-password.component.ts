import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { friendlyIdentityApiError } from '../../utils/auth-http.util';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  identifier = '';
  code = '';
  newPassword = '';
  confirmPassword = '';
  errorMessage = '';
  infoMessage = '';
  busy = false;
  stepIndex = 0;

  constructor(
    public authService: AuthService,
    private router: Router
  ) {
    if (this.authService.isLoggedIn()) {
      void this.router.navigate(['/dashboard']);
    }
  }

  requestCode() {
    this.clear();
    if (!this.identifier.trim()) {
      this.errorMessage = 'Enter your registered email or mobile number.';
      return;
    }
    this.busy = true;
    this.authService.requestPasswordResetOtp(this.identifier.trim()).subscribe({
      next: () => {
        this.busy = false;
        this.stepIndex = 1;
        this.infoMessage =
          'If an account exists, a reset code was sent. Check email/SMS or Identity API logs (mock mode).';
      },
      error: (err) => {
        this.busy = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Could not start reset.');
      }
    });
  }

  submitNewPassword() {
    this.clear();
    if (this.newPassword.length < 8) {
      this.errorMessage = 'Password must be at least 8 characters.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }
    if (!this.code.trim()) {
      this.errorMessage = 'Enter the code we sent.';
      return;
    }
    this.busy = true;
    this.authService.resetPassword(this.identifier.trim(), this.code.trim(), this.newPassword).subscribe({
      next: () => {
        this.busy = false;
        this.stepIndex = 2;
        this.infoMessage = 'Password updated. You can sign in now.';
      },
      error: (err) => {
        this.busy = false;
        this.errorMessage = friendlyIdentityApiError(err, 'Could not reset password.');
      }
    });
  }

  private clear() {
    this.errorMessage = '';
    this.infoMessage = '';
  }
}
