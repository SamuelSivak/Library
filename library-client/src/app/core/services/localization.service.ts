import { Injectable, inject, signal, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LocalizationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/localization`;

  readonly currentLang = signal<string>(localStorage.getItem('lang') || 'SK');
  readonly dictionary = signal<Record<string, string>>({});

  constructor() {
    effect(() => {
      const lang = this.currentLang();
      this.http.get<Record<string, string>>(`${this.apiUrl}?lang=${lang}`).subscribe({
        next: data => this.dictionary.set(data),
        error: () => {}
      });
    });
  }

  setLanguage(lang: string): void {
    localStorage.setItem('lang', lang);
    this.currentLang.set(lang);
  }

  t(key: string, defaultValue = ''): string {
    return this.dictionary()[key] || defaultValue || key;
  }
}
