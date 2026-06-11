export interface Review {
  id: number;
  text: string;
  rating: number;
  createdAt: string;
  bookId: number;
  reviewerId: number;
  reviewerName?: string;
}

export interface Book {
  id: number;
  title: string;
  description?: string;
  isbn?: string;
  pageCount: number;
  imageUrl?: string;
  published: string;
  authorName?: string;
  authorId?: number;
  genres?: string[];
  genreIds?: number[];
  reviews?: Review[];
  averageRating?: number;
}

export interface BookFormData {
  id: number;
  title: string;
  description: string;
  isbn: string;
  pageCount: number;
  imageUrl: string;
  published: string;
  authorId: number;
  genreIds: number[];
}

export interface BookCreateDto {
  title: string;
  description: string;
  isbn: string | null;
  pageCount: number;
  imageUrl: string | null;
  published: string;
  authorId: number;
  genreIds: number[];
}
