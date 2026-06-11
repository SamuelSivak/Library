import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book, BookCreateDto, Review } from '../models/book.model';
import { Author } from '../models/author.model';
import { Genre } from '../models/genre.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly http = inject(HttpClient);
  readonly apiUrl = environment.apiUrl;

  getBooks(search?: string, genre?: string): Observable<Book[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (genre) params = params.set('genre', genre);
    return this.http.get<Book[]>(`${this.apiUrl}/api/Book`, { params });
  }

  getGenres(includeEmpty: boolean = false): Observable<Genre[]> {
    const params = new HttpParams().set('includeEmpty', String(includeEmpty));
    return this.http.get<Genre[]>(`${this.apiUrl}/api/Genre`, { params });
  }

  createGenre(genre: { name: string }): Observable<Genre> {
    return this.http.post<Genre>(`${this.apiUrl}/api/Genre`, genre);
  }

  createReview(review: { text: string; rating: number; bookId: number }): Observable<Review> {
    return this.http.post<Review>(`${this.apiUrl}/api/Review`, review);
  }

  deleteReview(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/Review/${id}`);
  }

  getBook(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.apiUrl}/api/Book/${id}`);
  }

  getAuthors(): Observable<Author[]> {
    return this.http.get<Author[]>(`${this.apiUrl}/api/Author`);
  }

  createBook(book: BookCreateDto): Observable<Book> {
    return this.http.post<Book>(`${this.apiUrl}/api/Book`, book);
  }

  updateBook(id: number, book: BookCreateDto): Observable<Book> {
    return this.http.put<Book>(`${this.apiUrl}/api/Book/${id}`, book);
  }

  deleteBook(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/Book/${id}`);
  }

  uploadCover(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.apiUrl}/api/blobs/upload`, formData);
  }
}
