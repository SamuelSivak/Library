import { Component, Input, inject, HostListener, ElementRef, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';

interface Lang {
  code: string;
  flag: string;
  label: string;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  @Input() activeLink: 'books' | 'authors' | 'genres' | 'home' | '' = '';

  readonly authService = inject(AuthService);
  readonly loc = inject(LocalizationService);
  private readonly router = inject(Router);
  private readonly elRef = inject(ElementRef);

  readonly langs: Lang[] = [
    { code: 'SK', flag: '🇸🇰', label: 'Slovenčina' },
    { code: 'GR', flag: '🇬🇷', label: 'Ελληνικά' },
    { code: 'EN', flag: '🇬🇧', label: 'English' },
  ];

  get activeLang(): string { return this.loc.currentLang(); }
  setLang(code: string): void { this.loc.setLanguage(code); }

  userMenuOpen = false;
  toggleUserMenu(): void { this.userMenuOpen = !this.userMenuOpen; }
  closeUserMenu(): void { this.userMenuOpen = false; }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.userMenuOpen && !this.elRef.nativeElement.contains(event.target)) {
      this.userMenuOpen = false;
    }
  }

  logout(): void {
    this.authService.logout();
    this.userMenuOpen = false;
    this.router.navigate(['/']);
  }

  goToAuth(): void {
    this.closeUserMenu();
    this.router.navigate(['/auth']);
  }

  getInitial(): string {
    return this.authService.currentUser()?.username?.charAt(0)?.toUpperCase() ?? '?';
  }
}
