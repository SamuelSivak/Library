import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject, combineLatest, Subscription } from 'rxjs';
import { BookService } from '../../core/services/book.service';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';
import { Book, Genre } from '../../core/models';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingPage implements OnInit {
  private readonly bookService = inject(BookService);
  readonly authService = inject(AuthService);
  readonly loc = inject(LocalizationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);

  books = signal<Book[]>([]);
  genres = signal<Genre[]>([]);
  loading = signal(false);
  searchQuery = signal('');
  selectedGenre = signal('');
  selectedBook = signal<Book | null>(null);
  imageErrors = signal<Record<number, boolean>>({});

  newReviewText = signal('');
  newReviewRating = signal(5);
  submittingReview = signal(false);
  reviewError = signal('');

  onImageError(bookId: number): void {
    this.imageErrors.update(errors => ({ ...errors, [bookId]: true }));
  }

  private readonly searchSubject = new Subject<string>();
  private booksSubscription?: Subscription;

  ngOnInit(): void {
    combineLatest([
      toObservable(this.loc.currentLang, { injector: this.injector }),
      this.route.queryParams
    ]).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(([lang, params]) => {
      this.bookService.getGenres().pipe(
        takeUntilDestroyed(this.destroyRef)
      ).subscribe(data => {
        this.genres.set(data);
        this.cdr.markForCheck();
      });
      
      const genreParam = params['genre'] || '';
      this.selectedGenre.set(genreParam);
      const searchParam = params['search'] || '';
      this.searchQuery.set(searchParam);
      
      this.loadBooks();
      
      const currentBook = this.selectedBook();
      if (currentBook) {
        this.reloadBookDetails(currentBook.id);
      }
    });

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.loadBooks();
    });
  }

  onSearchQueryChange(query: string): void {
    this.searchQuery.set(query);
    this.searchSubject.next(query);
  }

  clearGenreFilter(): void {
    this.router.navigate(['/'], { queryParams: { genre: null }, queryParamsHandling: 'merge' });
  }

  onGenreChange(genre: string): void {
    this.selectedGenre.set(genre);
    this.loadBooks();
  }

  openBookDetails(book: Book): void {
    this.newReviewText.set('');
    this.newReviewRating.set(5);
    this.reviewError.set('');
    this.selectedBook.set(book);
  }

  closeBookDetails(): void {
    this.selectedBook.set(null);
  }

  selectRating(rating: number): void {
    this.newReviewRating.set(rating);
  }

  submitReview(): void {
    const text = this.newReviewText().trim();
    const rating = this.newReviewRating();
    const book = this.selectedBook();
    if (!book) return;

    if (!text) {
      this.reviewError.set(this.loc.t('details.validation_empty_review', 'Text recenzie je povinný.'));
      return;
    }

    this.submittingReview.set(true);
    this.reviewError.set('');

    this.bookService.createReview({ text, rating, bookId: book.id }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.newReviewText.set('');
        this.newReviewRating.set(5);
        this.submittingReview.set(false);
        this.reloadBookDetails(book.id);
      },
      error: (err) => {
        this.submittingReview.set(false);
        this.reviewError.set(err.error?.message || err.error || 'Nepodarilo sa odoslať recenziu');
        this.cdr.markForCheck();
      }
    });
  }

  deleteReview(id: number): void {
    const book = this.selectedBook();
    if (!book) return;

    this.bookService.deleteReview(id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.reloadBookDetails(book.id);
      },
      error: (err) => {
        alert(err.error?.message || err.error || 'Nepodarilo sa vymazať recenziu');
      }
    });
  }

  reloadBookDetails(bookId: number): void {
    this.bookService.getBook(bookId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: b => {
        const mapped = {
          ...b,
          imageUrl: b.imageUrl ? (b.imageUrl.startsWith('http') ? b.imageUrl : `${this.bookService.apiUrl}${b.imageUrl}`) : undefined
        };
        this.selectedBook.set(mapped);
        this.loadBooks();
        this.cdr.markForCheck();
      }
    });
  }

  private loadBooks(): void {
    this.loading.set(true);
    if (this.booksSubscription) {
      this.booksSubscription.unsubscribe();
    }
    this.booksSubscription = this.bookService.getBooks(this.searchQuery(), this.selectedGenre()).subscribe({
      next: data => {
        const mapped = data.map(b => ({
          ...b,
          imageUrl: b.imageUrl ? (b.imageUrl.startsWith('http') ? b.imageUrl : `${this.bookService.apiUrl}${b.imageUrl}`) : undefined
        }));
        this.books.set(mapped);
        this.loading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    });
  }
}
