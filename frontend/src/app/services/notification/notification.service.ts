import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

/**
 * A service for displaying toast notifications using PrimeNG's MessageService.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  /**
   * Creates an instance of NotificationService.
   * @param messageService The PrimeNG MessageService used to display messages.
   */
  constructor(private messageService: MessageService) {}

  /**
   * Displays a success notification.
   * @param summary The title or summary of the message.
   * @param detail The detailed message text.
   */
  showSuccess(summary: string, detail: string): void {
    this.messageService.add({severity: 'success', summary: summary, detail: detail});
  }

  /**
   * Displays an informational notification.
   * @param summary The title or summary of the message.
   * @param detail The detailed message text.
   */
  showInfo(summary: string, detail: string): void {
    this.messageService.add({severity: 'info', summary: summary, detail: detail});
  }

  /**
   * Displays a warning notification.
   * @param summary The title or summary of the message.
   * @param detail The detailed message text.
   */
  showWarn(summary: string, detail: string): void {
    this.messageService.add({severity: 'warn', summary: summary, detail: detail});
  }

  /**
   * Displays an error notification.
   * @param summary The title or summary of the message.
   * @param detail The detailed message text.
   */
  showError(summary: string, detail: string): void {
    this.messageService.add({severity: 'error', summary: summary, detail: detail});
  }

  /**
   * Clears all currently displayed notifications.
   */
  clear(): void {
    this.messageService.clear();
  }
}
