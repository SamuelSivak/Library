import { Component, OnInit, inject, DestroyRef, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BookService } from '../../../core/services/book.service';

interface BackdropCover {
  url: string;
  title: string;
}

const DEFAULT_COVERS: BackdropCover[] = [
  { url: 'https://covers.openlibrary.org/b/isbn/9780451524935-M.jpg', title: '1984' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780451526342-M.jpg', title: 'Animal Farm' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780684801223-M.jpg', title: 'The Old Man and the Sea' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780684801469-M.jpg', title: 'A Farewell to Arms' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780199232765-M.jpg', title: 'War and Peace' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780140449174-M.jpg', title: 'Anna Karenina' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780451419439-M.jpg', title: 'Les Misérables' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780140443530-M.jpg', title: 'The Hunchback of Notre-Dame' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780553213690-M.jpg', title: 'The Metamorphosis' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780805210408-M.jpg', title: 'The Trial' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780618640157-M.jpg', title: 'The Lord of the Rings' },
  { url: 'https://covers.openlibrary.org/b/isbn/9780618968633-M.jpg', title: 'The Hobbit' },
];

function buildRows(covers: BackdropCover[]): { row1: BackdropCover[]; row2: BackdropCover[]; row3: BackdropCover[] } {
  const doubled = [...covers, ...covers];
  const shifted4 = [...covers.slice(4), ...covers.slice(0, 4)];
  const shifted8 = [...covers.slice(8), ...covers.slice(0, 8)];
  return {
    row1: doubled,
    row2: [...shifted4, ...shifted4],
    row3: [...shifted8, ...shifted8]
  };
}

@Component({
  selector: 'app-book-backdrop',
  standalone: true,
  templateUrl: './backdrop.html',
  styleUrl: './backdrop.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BookBackdropComponent implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  private defaultRows = buildRows(DEFAULT_COVERS);
  row1 = this.defaultRows.row1;
  row2 = this.defaultRows.row2;
  row3 = this.defaultRows.row3;

  ngOnInit(): void {
    this.bookService.getBooks().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: books => {
        const validCovers = books
          .filter(b => b.imageUrl)
          .map(b => ({
            url: b.imageUrl!.startsWith('http') ? b.imageUrl! : `${this.bookService.apiUrl}${b.imageUrl}`,
            title: b.title
          }));

        if (validCovers.length > 0) {
          const rows = buildRows(validCovers);
          this.row1 = rows.row1;
          this.row2 = rows.row2;
          this.row3 = rows.row3;
          this.cdr.markForCheck();
        }
      }
    });
  }
}
