/**
 * Shared test setup for Vitest + jsdom environment.
 * Ensures localStorage is available as a proper in-memory mock.
 */

const store: Record<string, string> = {};

const localStorageMock = {
  getItem: (key: string): string | null => store[key] ?? null,
  setItem: (key: string, value: string): void => { store[key] = value; },
  removeItem: (key: string): void => { delete store[key]; },
  clear: (): void => { Object.keys(store).forEach(k => delete store[k]); },
  get length(): number { return Object.keys(store).length; },
  key: (index: number): string | null => Object.keys(store)[index] ?? null,
};

Object.defineProperty(window, 'localStorage', { value: localStorageMock, writable: true });
