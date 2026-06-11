import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { BookService } from '../../core/services/book.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';
import { Author } from '../../core/models';

@Component({
  selector: 'app-authors',
  standalone: true,
  imports: [CommonModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './authors.html',
  styleUrl: './authors.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthorsPage implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly loc = inject(LocalizationService);
  private readonly injector = inject(Injector);

  authors = signal<Author[]>([]);
  loading = signal(false);

  ngOnInit(): void {
    toObservable(this.loc.currentLang, { injector: this.injector }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadAuthors();
    });
  }

  private loadAuthors(): void {
    this.loading.set(true);
    this.bookService.getAuthors().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        this.authors.set(data);
        this.loading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  getInitials(author: Author): string {
    const first = author.name ? author.name.charAt(0).toUpperCase() : '';
    const last = author.surname ? author.surname.charAt(0).toUpperCase() : '';
    return first + last || '?';
  }

  getAvatarGradient(author: Author): string {
    const gradients = [
      'linear-gradient(135deg, #ff7e5f, #feb47b)',
      'linear-gradient(135deg, #11998e, #38ef7d)',
      'linear-gradient(135deg, #8a2387, #e94057, #f27121)',
      'linear-gradient(135deg, #654ea3, #eaafc8)',
      'linear-gradient(135deg, #00c6ff, #0072ff)',
      'linear-gradient(135deg, #f12711, #f5af19)',
      'linear-gradient(135deg, #da4453, #89216b)',
      'linear-gradient(135deg, #3a7bd5, #3a6073)',
      'linear-gradient(135deg, #7F00FF, #E100FF)',
      'linear-gradient(135deg, #1d976c, #93f9b9)',
      'linear-gradient(135deg, #eb3349, #f45c43)',
      'linear-gradient(135deg, #4568dc, #b06ab3)',
    ];
    const index = author.id % gradients.length;
    return gradients[index];
  }

  exploreAuthor(fullName: string): void {
    this.router.navigate(['/'], { queryParams: { search: fullName } });
  }
}
