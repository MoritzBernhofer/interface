import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoginMode = signal(true);
  isLoading = signal(false);
  error = signal<string | null>(null);

  loginEmail = '';
  loginPassword = '';

  registerName = '';
  registerEmail = '';
  registerPassword = '';
  registerConfirmPassword = '';

  toggleMode(): void {
    this.isLoginMode.update(v => !v);
    this.error.set(null);
  }

  onLogin(): void {
    if (!this.loginEmail || !this.loginPassword) {
      this.error.set('Please fill in all fields');
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.authService.login({ email: this.loginEmail, password: this.loginPassword }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error.set(err.status === 401 ? 'Invalid email or password' : 'Login failed');
        this.isLoading.set(false);
      }
    });
  }

  onRegister(): void {
    if (!this.registerName || !this.registerEmail || !this.registerPassword) {
      this.error.set('Please fill in all fields');
      return;
    }

    if (this.registerPassword !== this.registerConfirmPassword) {
      this.error.set('Passwords do not match');
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.authService.register({
      name: this.registerName,
      email: this.registerEmail,
      password: this.registerPassword
    }).subscribe({
      next: () => {
        this.authService.login({
          email: this.registerEmail,
          password: this.registerPassword
        }).subscribe({
          next: () => this.router.navigate(['/dashboard']),
          error: () => {
            this.isLoginMode.set(true);
            this.loginEmail = this.registerEmail;
            this.error.set('Registration successful. Please log in.');
            this.isLoading.set(false);
          }
        });
      },
      error: () => {
        this.error.set('Registration failed. Email may already be in use.');
        this.isLoading.set(false);
      }
    });
  }
}
