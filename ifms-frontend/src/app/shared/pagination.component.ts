import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="totalPages > 1" class="flex items-center justify-between px-2 py-3">
      <p class="text-xs text-on-surface-variant font-medium">
        Showing <span class="font-bold text-on-surface">{{ startItem }}–{{ endItem }}</span> of <span class="font-bold text-on-surface">{{ totalItems }}</span>
      </p>
      <div class="flex items-center gap-1">
        <button (click)="go(1)" [disabled]="currentPage === 1"
          class="p-1.5 rounded-lg hover:bg-surface-container-high transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
          <span class="material-symbols-outlined text-sm">first_page</span>
        </button>
        <button (click)="go(currentPage - 1)" [disabled]="currentPage === 1"
          class="p-1.5 rounded-lg hover:bg-surface-container-high transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
          <span class="material-symbols-outlined text-sm">chevron_left</span>
        </button>
        <ng-container *ngFor="let p of visiblePages">
          <span *ngIf="p === -1" class="px-1 text-outline text-sm">…</span>
          <button *ngIf="p !== -1" (click)="go(p)"
            [class]="p === currentPage ? 'bg-primary text-white' : 'hover:bg-surface-container-high text-on-surface'"
            class="w-8 h-8 rounded-lg text-xs font-bold transition-colors">
            {{ p }}
          </button>
        </ng-container>
        <button (click)="go(currentPage + 1)" [disabled]="currentPage === totalPages"
          class="p-1.5 rounded-lg hover:bg-surface-container-high transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
          <span class="material-symbols-outlined text-sm">chevron_right</span>
        </button>
        <button (click)="go(totalPages)" [disabled]="currentPage === totalPages"
          class="p-1.5 rounded-lg hover:bg-surface-container-high transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
          <span class="material-symbols-outlined text-sm">last_page</span>
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent implements OnChanges {
  @Input() totalItems = 0;
  @Input() pageSize = 10;
  @Input() currentPage = 1;
  @Output() pageChange = new EventEmitter<number>();

  totalPages = 1;
  visiblePages: number[] = [];
  startItem = 1;
  endItem = 1;

  ngOnChanges() {
    this.totalPages = Math.max(1, Math.ceil(this.totalItems / this.pageSize));
    this.startItem = (this.currentPage - 1) * this.pageSize + 1;
    this.endItem = Math.min(this.currentPage * this.pageSize, this.totalItems);
    this.buildVisiblePages();
  }

  go(page: number) {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.pageChange.emit(page);
  }

  buildVisiblePages() {
    const pages: number[] = [];
    const total = this.totalPages;
    const cur = this.currentPage;
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (cur > 3) pages.push(-1);
      for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
      if (cur < total - 2) pages.push(-1);
      pages.push(total);
    }
    this.visiblePages = pages;
  }
}
