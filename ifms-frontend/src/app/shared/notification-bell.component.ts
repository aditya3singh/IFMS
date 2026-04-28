import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, AppNotification } from '../services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative">
      <!-- Bell button -->
      <button (click)="toggleDropdown($event)"
        class="relative p-2 hover:bg-surface-container rounded-full transition-colors">
        <span class="material-symbols-outlined text-on-surface-variant">notifications</span>
        <span *ngIf="unreadCount > 0"
          class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] bg-error text-white text-[10px] font-black rounded-full flex items-center justify-center px-1">
          {{ unreadCount > 99 ? '99+' : unreadCount }}
        </span>
      </button>

      <!-- Dropdown -->
      <div *ngIf="isOpen"
        class="absolute right-0 top-12 w-96 bg-white rounded-2xl shadow-2xl border border-outline-variant/10 z-[200] overflow-hidden">

        <!-- Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-outline-variant/10">
          <div class="flex items-center gap-2">
            <h3 class="font-black text-primary font-headline">Notifications</h3>
            <span *ngIf="unreadCount > 0" class="text-[10px] font-bold bg-error text-white px-2 py-0.5 rounded-full">
              {{ unreadCount }} new
            </span>
          </div>
          <button *ngIf="unreadCount > 0" (click)="markAllRead()"
            class="text-xs font-bold text-primary hover:underline">
            Mark all read
          </button>
        </div>

        <!-- Notification list -->
        <div class="overflow-y-auto" style="max-height: 400px;">
          <div *ngIf="notifications.length === 0" class="px-5 py-10 text-center text-on-surface-variant">
            <span class="material-symbols-outlined text-3xl text-outline block mb-2">notifications_none</span>
            <p class="text-sm">No notifications yet</p>
          </div>

          <div *ngFor="let n of notifications"
            (click)="markRead(n)"
            [class]="n.isRead ? 'bg-white' : 'bg-surface-container-low/50'"
            class="flex items-start gap-3 px-5 py-4 border-b border-outline-variant/5 cursor-pointer hover:bg-surface-container-low transition-colors">

            <!-- Icon -->
            <div [class]="getIconBg(n.type)" class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0 mt-0.5">
              <span class="material-symbols-outlined text-sm" style="font-variation-settings:'FILL' 1;">{{ n.icon }}</span>
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-2">
                <p class="text-sm font-bold text-on-surface leading-tight">{{ n.title }}</p>
                <span class="text-[10px] text-outline shrink-0 mt-0.5">{{ getTimeAgo(n.createdAt) }}</span>
              </div>
              <p class="text-xs text-on-surface-variant mt-0.5 leading-relaxed line-clamp-2">{{ n.message }}</p>
            </div>

            <!-- Unread dot -->
            <div *ngIf="!n.isRead" class="w-2 h-2 rounded-full bg-primary shrink-0 mt-2"></div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-5 py-3 border-t border-outline-variant/10 text-center">
          <button (click)="refresh()" class="text-xs font-bold text-primary hover:underline flex items-center gap-1 mx-auto">
            <span class="material-symbols-outlined text-sm">refresh</span>Refresh
          </button>
        </div>
      </div>
    </div>
  `
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications: AppNotification[] = [];
  unreadCount = 0;
  isOpen = false;
  private subs: Subscription[] = [];

  constructor(private notifService: NotificationService) {}

  ngOnInit() {
    this.subs.push(
      this.notifService.notifications$.subscribe(n => this.notifications = n),
      this.notifService.unreadCount$.subscribe(c => this.unreadCount = c)
    );
    this.notifService.startPolling();
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    this.notifService.stopPolling();
  }

  toggleDropdown(e: Event) {
    e.stopPropagation();
    this.isOpen = !this.isOpen;
  }

  @HostListener('document:click')
  closeDropdown() { this.isOpen = false; }

  markRead(n: AppNotification) {
    if (!n.isRead) this.notifService.markRead(n.id);
  }

  markAllRead() { this.notifService.markAllRead(); }

  refresh() { this.notifService.fetchNotifications(); }

  getIconBg(type: string): string {
    switch (type) {
      case 'success': return 'bg-green-100 text-green-600';
      case 'warning': return 'bg-yellow-100 text-yellow-600';
      case 'error': return 'bg-red-100 text-red-600';
      default: return 'bg-blue-100 text-blue-600';
    }
  }

  getTimeAgo(dateStr: string): string {
    return this.notifService.getTimeAgo(dateStr);
  }
}
