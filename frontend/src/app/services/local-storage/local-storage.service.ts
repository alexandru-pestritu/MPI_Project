import { Injectable } from '@angular/core';

/**
 * A service that wraps browser localStorage operations with safety checks for server-side rendering.
 */
@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

  /**
   * Indicates whether the code is running in a browser environment.
   */
  private isBrowser: boolean;

  /**
   * Initializes the service and determines the execution environment.
   */
  constructor() {
    this.isBrowser = typeof window !== 'undefined';
  }

  /**
   * Retrieves a value from localStorage.
   * @param key The key of the item to retrieve.
   * @returns The stored value, or null if not found or not in browser context.
   */
  getItem(key: string): string | null {
    if (this.isBrowser) {
      return localStorage.getItem(key);
    }
    return null;
  }

  /**
   * Stores a key-value pair in localStorage.
   * @param key The key under which the value is stored.
   * @param value The value to store.
   */
  setItem(key: string, value: string): void {
    if (this.isBrowser) {
      localStorage.setItem(key, value);
    }
  }

  /**
   * Removes an item from localStorage by key.
   * @param key The key of the item to remove.
   */
  removeItem(key: string): void {
    if (this.isBrowser) {
      localStorage.removeItem(key);
    }
  }

  /**
   * Clears all items from localStorage.
   */
  clear(): void {
    if (this.isBrowser) {
      localStorage.clear();
    }
  }
}
