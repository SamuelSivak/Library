import { Component, inject, OnInit, signal, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { BookService } from '../../core/services/book.service';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';
import { Book } from '../../core/models';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './books.html',
  styleUrl: './books.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BooksPage implements OnInit {
  private readonly bookService = inject(BookService);
  readonly authService = inject(AuthService);
  readonly loc = inject(LocalizationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);

  books = signal<Book[]>([]);
  loading = signal(false);
  imageErrors = signal<Record<number, boolean>>({});

  currentPage = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = signal(0);
  sortBy = signal<string>('rating');

  selectedBook = signal<Book | null>(null);
  newReviewText = signal('');
  newReviewRating = signal(5);
  reviewError = signal('');
  submittingReview = signal(false);

  ngOnInit(): void {
    toObservable(this.loc.currentLang, { injector: this.injector }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadBooks();
    });
  }

  loadBooks(): void {
    this.loading.set(true);
    this.bookService.getBooksPaged(
      this.currentPage(),
      this.pageSize(),
      undefined,
      undefined,
      this.sortBy()
    ).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: response => {
        const total = Number(response.headers.get('X-Total-Count') || 0);
        this.totalCount.set(total);
        this.totalPages.set(Math.ceil(total / this.pageSize()));

        const mapped = (response.body || []).map(b => ({
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

  setPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) {
      return;
    }
    this.currentPage.set(page);
    this.loadBooks();
    // Scroll to top of catalog smoothly
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onSortChange(newSort: string): void {
    this.sortBy.set(newSort);
    this.currentPage.set(1);
    this.loadBooks();
  }

  getPages(): (number | string)[] {
    const current = this.currentPage();
    const total = this.totalPages();
    const pages: (number | string)[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) {
        pages.push('...');
      }

      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (current < total - 2) {
        pages.push('...');
      }
      pages.push(total);
    }
    return pages;
  }

  onImageError(bookId: number): void {
    this.imageErrors.update(errors => ({ ...errors, [bookId]: true }));
  }

  openBookDetails(book: Book): void {
    this.newReviewText.set('');
    this.newReviewRating.set(5);
    this.reviewError.set('');
    this.selectedBook.set(book);
    this.bookService.getBook(book.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: b => {
        const mapped = {
          ...b,
          imageUrl: b.imageUrl ? (b.imageUrl.startsWith('http') ? b.imageUrl : `${this.bookService.apiUrl}${b.imageUrl}`) : undefined
        };
        this.selectedBook.set(mapped);
        this.cdr.markForCheck();
      }
    });
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
        this.reviewError.set(err.error?.message || err.error || this.loc.t('details.submit_failed', 'Nepodarilo sa odoslať recenziu'));
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
        alert(err.error?.message || err.error || this.loc.t('details.delete_failed', 'Nepodarilo sa vymazať recenziu'));
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
}
