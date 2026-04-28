import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';

interface FeedbackEntry {
  id: number;
  section: string;
  rating: number;
  comment: string;
  submittedBy: string;
  role: string;
  date: string;
  status: 'New' | 'Reviewed' | 'Resolved';
}

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, NotificationBellComponent],
  templateUrl: './feedback.component.html'
})
export class FeedbackComponent implements OnInit {
  role: string | null = null;
  fullName: string | null = null;

  // Submit form
  feedbackForm = { section: 'Inventory Management', rating: 0, comment: '' };
  hoverRating = 0;
  submitSuccess = '';
  submitError = '';

  // All feedback list (admin/dealer view)
  feedbackList: FeedbackEntry[] = [];
  filterSection = 'All';
  filterStatus = 'All';

  // Active tab
  activeTab: 'submit' | 'view' = 'submit';

  readonly sections = [
    'Inventory Management',
    'Sales & Transactions',
    'Token / Booking System',
    'Staff Management',
    'Tanker Delivery',
    'Dashboard & Reports',
    'Customer Service',
    'App Performance',
    'Other'
  ];

  readonly ratingLabels = ['', 'Very Poor', 'Poor', 'Average', 'Good', 'Excellent'];

  // Pagination
  fbPage = 1; fbPageSize = 5;
  get pagedFeedback() { return this.filteredFeedback.slice((this.fbPage-1)*this.fbPageSize, this.fbPage*this.fbPageSize); }

  constructor(private router: Router) {}

  ngOnInit() {
    this.role = localStorage.getItem('role');
    this.fullName = localStorage.getItem('fullName') || 'User';
    this.loadFeedback();
  }

  loadFeedback() {
    const stored = localStorage.getItem('ifms_feedback');
    if (stored) {
      this.feedbackList = JSON.parse(stored);
    } else {
      // Seed some demo feedback
      this.feedbackList = [
        { id: 1, section: 'Inventory Management', rating: 4, comment: 'Stock tracking works well but forecast could be more accurate.', submittedBy: 'Dealer User', role: 'Dealer', date: '2026-04-10', status: 'Reviewed' },
        { id: 2, section: 'Token / Booking System', rating: 5, comment: 'Token validation is fast and reliable. Great feature!', submittedBy: 'John Doe', role: 'Customer', date: '2026-04-11', status: 'New' },
        { id: 3, section: 'Sales & Transactions', rating: 3, comment: 'Receipt generation is good but needs email option.', submittedBy: 'Dealer User', role: 'Dealer', date: '2026-04-12', status: 'New' },
        { id: 4, section: 'Dashboard & Reports', rating: 5, comment: 'Admin dashboard is very comprehensive. Love the fraud monitor.', submittedBy: 'Admin User', role: 'Admin', date: '2026-04-13', status: 'Resolved' },
      ];
      this.saveFeedback();
    }
  }

  saveFeedback() {
    localStorage.setItem('ifms_feedback', JSON.stringify(this.feedbackList));
  }

  setRating(r: number) { this.feedbackForm.rating = r; }
  setHover(r: number) { this.hoverRating = r; }
  clearHover() { this.hoverRating = 0; }

  getStarClass(star: number): string {
    const active = this.hoverRating || this.feedbackForm.rating;
    return star <= active ? 'text-yellow-400' : 'text-gray-300';
  }

  submitFeedback() {
    if (!this.feedbackForm.rating) { this.submitError = 'Please select a rating.'; return; }
    if (!this.feedbackForm.comment.trim()) { this.submitError = 'Please write a comment.'; return; }

    const entry: FeedbackEntry = {
      id: Date.now(),
      section: this.feedbackForm.section,
      rating: this.feedbackForm.rating,
      comment: this.feedbackForm.comment.trim(),
      submittedBy: this.fullName || 'User',
      role: this.role || 'User',
      date: new Date().toISOString().split('T')[0],
      status: 'New'
    };
    this.feedbackList.unshift(entry);
    this.saveFeedback();
    this.submitSuccess = 'Thank you! Your feedback has been submitted.';
    this.submitError = '';
    this.feedbackForm = { section: 'Inventory Management', rating: 0, comment: '' };
    setTimeout(() => { this.submitSuccess = ''; this.activeTab = 'view'; }, 2000);
  }

  get filteredFeedback(): FeedbackEntry[] {
    return this.feedbackList.filter(f => {
      const matchSection = this.filterSection === 'All' || f.section === this.filterSection;
      const matchStatus = this.filterStatus === 'All' || f.status === this.filterStatus;
      return matchSection && matchStatus;
    });
  }

  get avgRating(): string {
    if (!this.feedbackList.length) return '0.0';
    return (this.feedbackList.reduce((s, f) => s + f.rating, 0) / this.feedbackList.length).toFixed(1);
  }

  get newCount() { return this.feedbackList.filter(f => f.status === 'New').length; }

  updateStatus(id: number, status: FeedbackEntry['status']) {
    const idx = this.feedbackList.findIndex(f => f.id === id);
    if (idx !== -1) { this.feedbackList[idx].status = status; this.saveFeedback(); }
  }

  deleteFeedback(id: number) {
    this.feedbackList = this.feedbackList.filter(f => f.id !== id);
    this.saveFeedback();
  }

  getStatusCls(status: string): string {
    if (status === 'New') return 'bg-blue-100 text-blue-700';
    if (status === 'Reviewed') return 'bg-yellow-100 text-yellow-700';
    return 'bg-green-100 text-green-700';
  }

  getRatingColor(r: number): string {
    if (r >= 4) return 'text-green-600';
    if (r >= 3) return 'text-yellow-600';
    return 'text-red-500';
  }

  stars(n: number): number[] { return Array.from({ length: n }, (_, i) => i + 1); }

  goBack() { void this.router.navigate(['/dashboard']); }
  navigate(path: string) { void this.router.navigate([path]); }
}
