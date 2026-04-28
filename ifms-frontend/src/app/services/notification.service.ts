import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, interval, Subscription } from 'rxjs';
import { AuthService } from './auth.service';

export interface AppNotification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  icon: string;
  targetRole: string;
  createdAt: string;
  isRead: boolean;
}

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private readonly apiUrl = 'http://localhost:5010/gateway/notifications';

  private _notifications = new BehaviorSubject<AppNotification[]>([]);
  private _unreadCount = new BehaviorSubject<number>(0);
  private pollSub: Subscription | null = null;

  notifications$ = this._notifications.asObservable();
  unreadCount$ = this._unreadCount.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
  }

  /** Start polling every 30 seconds */
  startPolling() {
    this.fetchNotifications();
    this.pollSub = interval(30000).subscribe(() => this.fetchNotifications());
  }

  stopPolling() {
    this.pollSub?.unsubscribe();
    this.pollSub = null;
  }

  fetchNotifications() {
    if (!this.authService.getToken()) return;
    this.http.get<{ notifications: AppNotification[]; unreadCount: number }>(
      `${this.apiUrl}?limit=30`, { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this._notifications.next(data.notifications);
        this._unreadCount.next(data.unreadCount);
      },
      error: () => {}
    });
  }

  markRead(id: string) {
    this.http.put(`${this.apiUrl}/${id}/read`, {}, { headers: this.getHeaders() }).subscribe({
      next: () => {
        const updated = this._notifications.value.map(n =>
          n.id === id ? { ...n, isRead: true } : n
        );
        this._notifications.next(updated);
        this._unreadCount.next(Math.max(0, this._unreadCount.value - 1));
      },
      error: () => {}
    });
  }

  markAllRead() {
    this.http.put(`${this.apiUrl}/read-all`, {}, { headers: this.getHeaders() }).subscribe({
      next: () => {
        const updated = this._notifications.value.map(n => ({ ...n, isRead: true }));
        this._notifications.next(updated);
        this._unreadCount.next(0);
      },
      error: () => {}
    });
  }

  /** Push a local notification immediately (without API call) */
  pushLocal(notification: Omit<AppNotification, 'id' | 'createdAt' | 'isRead' | 'targetRole'>) {
    const n: AppNotification = {
      id: Date.now().toString(),
      ...notification,
      targetRole: this.authService.getRole() || 'All',
      createdAt: new Date().toISOString(),
      isRead: false
    };
    this._notifications.next([n, ...this._notifications.value]);
    this._unreadCount.next(this._unreadCount.value + 1);
  }

  getTypeColor(type: string): string {
    switch (type) {
      case 'success': return 'text-green-600 bg-green-50';
      case 'warning': return 'text-yellow-600 bg-yellow-50';
      case 'error': return 'text-red-600 bg-red-50';
      default: return 'text-blue-600 bg-blue-50';
    }
  }

  getTimeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
  }

  ngOnDestroy() { this.stopPolling(); }
}
