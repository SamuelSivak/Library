import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then(m => m.LandingPage)
  },
  {
    path: 'books',
    loadComponent: () => import('./features/books/books').then(m => m.BooksPage)
  },
  {
    path: 'authors',
    loadComponent: () => import('./features/authors/authors').then(m => m.AuthorsPage)
  },
  {
    path: 'genres',
    loadComponent: () => import('./features/genres/genres').then(m => m.GenresPage)
  },
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth').then(m => m.AuthPage)
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/admin').then(m => m.AdminPage),
    canActivate: [adminGuard]
  },
  { path: '**', redirectTo: '' }
];
