import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { BookService } from '../../core/services/book.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';
import { Genre } from '../../core/models';

@Component({
  selector: 'app-genres',
  standalone: true,
  imports: [CommonModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './genres.html',
  styleUrl: './genres.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GenresPage implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly loc = inject(LocalizationService);
  private readonly injector = inject(Injector);

  genres = signal<Genre[]>([]);
  loading = signal(false);

  ngOnInit(): void {
    toObservable(this.loc.currentLang, { injector: this.injector }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadGenres();
    });
  }

  private loadGenres(): void {
    this.loading.set(true);
    this.bookService.getGenres().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        this.genres.set(data);
        this.loading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  selectGenre(name: string): void {
    this.router.navigate(['/'], { queryParams: { genre: name } });
  }
}
