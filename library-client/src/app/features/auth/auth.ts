import { Component, inject, signal, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './auth.html',
  styleUrl: './auth.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthPage {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  isSignUp = signal(false);
  loading = signal(false);
  errorMessage = signal('');
  showPassword = signal(false);
  confirmPassword = signal('');

  formData = {
    username: '',
    email: '',
    password: ''
  };

  toggleMode(): void {
    this.isSignUp.set(!this.isSignUp());
    this.errorMessage.set('');
    this.showPassword.set(false);
    this.confirmPassword.set('');
    this.formData = {
      username: '',
      email: '',
      password: ''
    };
  }

  toggleShowPassword(): void {
    this.showPassword.set(!this.showPassword());
  }

  onSubmit(): void {
    if (!this.formData.username || !this.formData.password) {
      this.errorMessage.set('Username and password are required.');
      return;
    }
    if (this.isSignUp()) {
      if (!this.formData.email) {
        this.errorMessage.set('Email address is required.');
        return;
      }
      if (this.formData.password !== this.confirmPassword()) {
        this.errorMessage.set('Passwords do not match.');
        return;
      }
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const request$ = this.isSignUp()
      ? this.authService.register(this.formData)
      : this.authService.login({ username: this.formData.username, password: this.formData.password });

    request$.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.loading.set(false);
        if (this.authService.isAdmin()) {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/']);
        }
        this.cdr.markForCheck();
      },
      error: err => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || err.error || 'Authentication failed');
        this.cdr.markForCheck();
      }
    });
  }
}
