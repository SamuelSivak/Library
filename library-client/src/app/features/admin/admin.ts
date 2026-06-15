import { Component, inject, OnInit, signal, computed, HostListener, ElementRef, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { BookService } from '../../core/services/book.service';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { BookBackdropComponent } from '../../shared/components/backdrop/backdrop';
import { Book, BookFormData, BookCreateDto } from '../../core/models';
import { Author, Genre } from '../../core/models';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, BookBackdropComponent],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminPage implements OnInit {
  private readonly bookService = inject(BookService);
  readonly authService = inject(AuthService);
  readonly loc = inject(LocalizationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);
  private readonly elRef = inject(ElementRef);

  books = signal<Book[]>([]);
  authors = signal<Author[]>([]);
  genres = signal<Genre[]>([]);
  countries = signal<{ id: number; name: string }[]>([]);
  imageErrors = signal<Record<number, boolean>>({});
  loading = signal(false);
  formError = signal('');
  pendingDeleteId = signal<number | null>(null);
  pendingDeleteTitle = signal('');
  searchQuery = signal('');
  private readonly searchSubject = new Subject<string>();

  isModalOpen = signal(false);
  modalTitle = signal('');
  isEditing = signal(false);
  uploading = signal(false);
  uploadError = signal('');

  formModel: BookFormData = this.getEmptyForm();
  newGenreName = '';

  authorSearchQuery = signal('');
  showAuthorDropdown = signal(false);

  isAddingAuthor = signal(false);
  newAuthorName = '';
  newAuthorSurname = '';
  newAuthorCountryId = 0;

  filteredAuthors = computed(() => {
    const query = this.authorSearchQuery().toLowerCase().trim();
    if (!query) {
      return this.authors();
    }
    return this.authors().filter(a => 
      `${a.name} ${a.surname}`.toLowerCase().includes(query)
    );
  });

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (this.showAuthorDropdown() && !target.closest('.custom-select-container')) {
      this.showAuthorDropdown.set(false);
    }
  }

  getSelectedAuthorName(): string {
    const author = this.authors().find(a => a.id === this.formModel.authorId);
    return author ? `${author.name} ${author.surname}` : '';
  }

  selectAuthor(author: Author): void {
    this.formModel.authorId = author.id;
    this.showAuthorDropdown.set(false);
    this.authorSearchQuery.set('');
  }

  startAddingAuthor(): void {
    this.isAddingAuthor.set(true);
    this.showAuthorDropdown.set(false);
    const query = this.authorSearchQuery().trim();
    if (query) {
      const parts = query.split(/\s+/);
      if (parts.length > 1) {
        this.newAuthorName = parts[0];
        this.newAuthorSurname = parts.slice(1).join(' ');
      } else {
        this.newAuthorName = query;
        this.newAuthorSurname = '';
      }
    } else {
      this.newAuthorName = '';
      this.newAuthorSurname = '';
    }
    this.newAuthorCountryId = 0;
  }

  addNewAuthor(): void {
    const name = this.newAuthorName.trim();
    const surname = this.newAuthorSurname.trim();
    const countryId = this.newAuthorCountryId;
    if (!name || !surname || countryId === 0) return;

    this.formError.set('');
    this.bookService.createAuthor({ name, surname, countryId }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (newAuthor) => {
        this.authors.update(list => [...list, newAuthor]);
        this.formModel.authorId = newAuthor.id;
        this.isAddingAuthor.set(false);
        this.newAuthorName = '';
        this.newAuthorSurname = '';
        this.newAuthorCountryId = 0;
        this.authorSearchQuery.set('');
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.formError.set(err.error?.message || err.error || 'Failed to create author');
        this.cdr.markForCheck();
      }
    });
  }

  onSearchQueryChange(query: string): void {
    this.searchQuery.set(query);
    this.searchSubject.next(query);
  }

  onImageError(bookId: number): void {
    this.imageErrors.update(errors => ({ ...errors, [bookId]: true }));
  }

  ngOnInit(): void {
    if (!this.authService.isAdmin()) {
      this.router.navigate(['/']);
      return;
    }
    toObservable(this.loc.currentLang, { injector: this.injector }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadData(this.searchQuery());
    });

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.loadData(query);
    });
  }

  loadData(search?: string): void {
    this.loading.set(true);

    this.bookService.getBooks(search).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
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

    this.bookService.getAuthors().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        this.authors.set(data);
        this.cdr.markForCheck();
      }
    });

    this.bookService.getGenres(true).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        this.genres.set(data);
        this.cdr.markForCheck();
      }
    });

    this.bookService.getCountries().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        this.countries.set(data);
        this.cdr.markForCheck();
      }
    });
  }

  openAddModal(): void {
    this.isEditing.set(false);
    this.modalTitle.set(this.loc.t('admin.addTitle', 'Add New Book'));
    this.formError.set('');
    this.uploadError.set('');
    this.formModel = this.getEmptyForm();
    this.authorSearchQuery.set('');
    this.showAuthorDropdown.set(false);
    this.isAddingAuthor.set(false);
    this.newAuthorName = '';
    this.newAuthorSurname = '';
    this.newAuthorCountryId = 0;
    this.isModalOpen.set(true);
  }

  openEditModal(book: Book): void {
    this.isEditing.set(true);
    this.modalTitle.set(this.loc.t('admin.editTitle', 'Edit Book'));
    this.formError.set('');
    this.uploadError.set('');

    const currentGenreIds = this.genres()
      .filter(g => book.genres?.includes(g.name))
      .map(g => g.id);

    this.formModel = {
      id: book.id,
      title: book.title,
      description: book.description || '',
      isbn: book.isbn || '',
      pageCount: book.pageCount,
      imageUrl: book.imageUrl || '',
      published: book.published ? new Date(book.published).toISOString().substring(0, 10) : '',
      authorId: book.authorId || (this.authors().length > 0 ? this.authors()[0].id : 0),
      genreIds: currentGenreIds
    };
    this.authorSearchQuery.set('');
    this.showAuthorDropdown.set(false);
    this.isAddingAuthor.set(false);
    this.newAuthorName = '';
    this.newAuthorSurname = '';
    this.newAuthorCountryId = 0;
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.formError.set('');
  }

  toggleGenre(genreId: number): void {
    const idx = this.formModel.genreIds.indexOf(genreId);
    if (idx > -1) {
      this.formModel.genreIds.splice(idx, 1);
    } else {
      this.formModel.genreIds.push(genreId);
    }
  }

  addNewGenre(): void {
    const name = this.newGenreName.trim();
    if (!name) return;

    this.formError.set('');
    this.bookService.createGenre({ name }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (newGenre) => {
        this.genres.update(list => [...list, newGenre]);
        if (!this.formModel.genreIds.includes(newGenre.id)) {
          this.formModel.genreIds.push(newGenre.id);
        }
        this.newGenreName = '';
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.formError.set(err.error?.message || err.error || 'Failed to create genre');
        this.cdr.markForCheck();
      }
    });
  }

  isGenreSelected(genreId: number): boolean {
    return this.formModel.genreIds.includes(genreId);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploading.set(true);
    this.uploadError.set('');
    this.bookService.uploadCover(file).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: res => {
        this.formModel.imageUrl = res.url;
        this.uploading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.uploading.set(false);
        this.uploadError.set(this.loc.t('admin.uploadFailed', 'Upload failed. Make sure Docker MinIO is running.'));
        this.cdr.markForCheck();
      }
    });
  }

  onSubmit(): void {
    if (!this.formModel.title || this.formModel.authorId === 0) {
      this.formError.set(this.loc.t('admin.validationError', 'Title and author are required fields.'));
      return;
    }

    this.formError.set('');
    const payload: BookCreateDto = {
      title: this.formModel.title,
      description: this.formModel.description,
      isbn: this.formModel.isbn || null,
      pageCount: this.formModel.pageCount,
      imageUrl: this.formModel.imageUrl || null,
      published: new Date(this.formModel.published).toISOString(),
      authorId: this.formModel.authorId,
      genreIds: this.formModel.genreIds
    };

    const request$ = this.isEditing()
      ? this.bookService.updateBook(this.formModel.id, payload)
      : this.bookService.createBook(payload);

    request$.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.closeModal();
        this.loadData(this.searchQuery());
      },
      error: (err) => {
        this.formError.set(err.error?.message || err.error || 'Operation failed');
        this.cdr.markForCheck();
      }
    });
  }

  requestDelete(id: number, title: string): void {
    this.pendingDeleteId.set(id);
    this.pendingDeleteTitle.set(title);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  confirmDelete(): void {
    const id = this.pendingDeleteId();
    if (id === null) return;

    this.bookService.deleteBook(id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.pendingDeleteId.set(null);
        this.loadData(this.searchQuery());
      },
      error: (err) => {
        this.formError.set(err.error?.message || 'Delete failed');
        this.pendingDeleteId.set(null);
        this.cdr.markForCheck();
      }
    });
  }

  private getEmptyForm(): BookFormData {
    return {
      id: 0,
      title: '',
      description: '',
      isbn: '',
      pageCount: 100,
      imageUrl: '',
      published: new Date().toISOString().substring(0, 10),
      authorId: this.authors?.()?.length > 0 ? this.authors()[0].id : 0,
      genreIds: []
    };
  }
}
