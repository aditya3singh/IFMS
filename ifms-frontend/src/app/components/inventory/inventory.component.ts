import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { StationService } from '../../services/station.service';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, NotificationBellComponent],
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.css']
})
export class InventoryComponent implements OnInit {
  role: string | null = null;
  stocks: Record<string, unknown>[] = [];
  newStock = {
    fuelType: 'Petrol',
    quantity: 0,
    pricePerLitre: 0,
    stationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6'
  };
  isLoading = false;
  addStockSuccess = '';
  addStockError = '';
  activeFuelTab: 'Petrol' | 'Diesel' | 'CNG' = 'Petrol';
  readonly Math = Math;

  // Pagination
  stockPage = 1; stockPageSize = 10;
  get pagedStocks() { return this.stocks.slice((this.stockPage-1)*this.stockPageSize, this.stockPage*this.stockPageSize); }

  // Schedule Delivery modal
  showScheduleModal = false;
  scheduleForm = { fuelType: 'Petrol', quantity: 0, scheduledDate: '', supplierName: '', notes: '' };
  scheduleSuccess = '';
  scheduleError = '';
  scheduledDeliveries: any[] = [];

  // Forecast modal
  showForecastModal = false;

  // Transaction History
  transactions: any[] = [];
  showTransactionsModal = false;
  isTransactionsLoading = false;
  transactionDays = 30;
  transactionPage = 1;
  transactionPageSize = 10;
  get pagedTransactions() {
    return this.filteredTransactions.slice(
      (this.transactionPage - 1) * this.transactionPageSize,
      this.transactionPage * this.transactionPageSize
    );
  }
  transactionTypeFilter = 'All';
  transactionFuelFilter = 'All';
  get filteredTransactions() {
    return this.transactions.filter(t => {
      const typeMatch = this.transactionTypeFilter === 'All' || t.transactionType === this.transactionTypeFilter;
      const fuelMatch = this.transactionFuelFilter === 'All' || t.fuelType === this.transactionFuelFilter;
      return typeMatch && fuelMatch;
    });
  }

  constructor(
    private inventoryService: InventoryService,
    private stationService: StationService,
    private notifService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.role = typeof localStorage !== 'undefined' ? localStorage.getItem('role') : null;
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.scheduleForm.scheduledDate = tomorrow.toISOString().split('T')[0];

    if (this.role === 'Dealer') {
      this.stationService.getMyStations().subscribe({
        next: (stations) => {
          if (stations?.length && stations[0]?.id) {
            this.newStock.stationId = stations[0].id;
          }
          this.loadStocks();
        },
        error: () => this.loadStocks()
      });
    } else {
      this.loadStocks();
    }
    this.loadScheduledDeliveries();
  }

  loadStocks() {
    this.inventoryService.getAllStock().subscribe({
      next: (data) => {
        const rows = (data ?? []) as Record<string, unknown>[];
        this.stocks = rows.slice().sort((a, b) => this.parseLastUpdatedMs(b) - this.parseLastUpdatedMs(a));
        // Check for low stock and notify
        ['Petrol', 'Diesel', 'CNG'].forEach(fuel => {
          const pct = this.getFillPct(fuel);
          if (pct > 0 && pct <= 15) {
            this.notifService.pushLocal({
              type: 'warning',
              title: `Low ${fuel} Stock`,
              message: `${fuel} is at ${pct}% capacity. Schedule a tanker delivery soon.`,
              icon: 'warning'
            });
          }
        });
      },
      error: (err) => console.error(err)
    });
  }

  addStock() {
    if (this.role !== 'Dealer') return;
    this.isLoading = true;
    this.addStockSuccess = '';
    this.addStockError = '';
    this.inventoryService.createStock(this.newStock).subscribe({
      next: () => {
        this.loadStocks();
        this.isLoading = false;
        this.addStockSuccess = `${this.newStock.fuelType} stock added successfully.`;
        // Check for low stock after adding
        this.notifService.pushLocal({
          type: 'success',
          title: 'Stock Updated',
          message: `${this.newStock.fuelType} stock of ${this.newStock.quantity}L added at ₹${this.newStock.pricePerLitre}/L`,
          icon: 'inventory_2'
        });
        this.newStock = { ...this.newStock, quantity: 0, pricePerLitre: 0 };
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        this.addStockError = 'Failed to add stock. Check that the Inventory API is running.';
      }
    });
  }

  scrollToDeliveryForm() {
    const el = document.getElementById('tanker-delivery-form');
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }

  // Schedule Delivery
  openScheduleModal() { this.showScheduleModal = true; this.scheduleSuccess = ''; this.scheduleError = ''; }
  closeScheduleModal() { this.showScheduleModal = false; }

  saveScheduledDelivery() {
    if (!this.scheduleForm.quantity || !this.scheduleForm.scheduledDate) {
      this.scheduleError = 'Please fill in quantity and scheduled date.';
      return;
    }
    const delivery = {
      id: Date.now(),
      ...this.scheduleForm,
      stationId: this.newStock.stationId,
      createdAt: new Date().toISOString(),
      status: 'Scheduled'
    };
    this.scheduledDeliveries.unshift(delivery);
    localStorage.setItem('scheduledDeliveries', JSON.stringify(this.scheduledDeliveries));
    this.scheduleSuccess = `Delivery of ${delivery.quantity}L ${delivery.fuelType} scheduled for ${delivery.scheduledDate}`;
    this.scheduleError = '';
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.scheduleForm = { fuelType: 'Petrol', quantity: 0, scheduledDate: tomorrow.toISOString().split('T')[0], supplierName: '', notes: '' };
    setTimeout(() => { this.showScheduleModal = false; this.scheduleSuccess = ''; }, 2000);
  }

  loadScheduledDeliveries() {
    try {
      const stored = localStorage.getItem('scheduledDeliveries');
      this.scheduledDeliveries = stored ? JSON.parse(stored) : [];
    } catch { this.scheduledDeliveries = []; }
  }

  cancelScheduledDelivery(id: number) {
    this.scheduledDeliveries = this.scheduledDeliveries.filter((d: any) => d.id !== id);
    localStorage.setItem('scheduledDeliveries', JSON.stringify(this.scheduledDeliveries));
  }

  // Forecast
  openForecastModal() { this.showForecastModal = true; }
  closeForecastModal() { this.showForecastModal = false; }

  // Transaction History
  openTransactionsModal() {
    this.showTransactionsModal = true;
    this.loadTransactions();
  }

  closeTransactionsModal() {
    this.showTransactionsModal = false;
  }

  loadTransactions() {
    if (!this.newStock.stationId) return;
    this.isTransactionsLoading = true;
    this.inventoryService.getStationTransactions(this.newStock.stationId, 200).subscribe({
      next: (data) => {
        this.transactions = data || [];
        this.isTransactionsLoading = false;
      },
      error: () => {
        this.isTransactionsLoading = false;
      }
    });
  }

  getTransactionTypeBadge(type: string): string {
    switch (type) {
      case 'Addition': return 'bg-tertiary-container text-on-tertiary-container';
      case 'Deduction': return 'bg-error-container text-on-error-container';
      case 'Delivery': return 'bg-primary-container text-on-primary-container';
      case 'Adjustment': return 'bg-secondary-container text-on-secondary-container';
      default: return 'bg-surface-variant text-on-surface-variant';
    }
  }

  getTransactionIcon(type: string): string {
    switch (type) {
      case 'Addition': return 'add_circle';
      case 'Deduction': return 'remove_circle';
      case 'Delivery': return 'local_shipping';
      case 'Adjustment': return 'tune';
      default: return 'swap_horiz';
    }
  }

  formatQuantityChange(change: number): string {
    return change > 0 ? `+${change.toFixed(0)}L` : `${change.toFixed(0)}L`;
  }

  formatDateTime(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleString('en-IN', { 
      day: 'numeric', 
      month: 'short', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getDaysRemaining(fuelType: string): number {
    const s = this.getStock(fuelType);
    if (!s) return 0;
    const qty = Number(s['quantity'] ?? 0);
    const dailyConsumption = fuelType === 'CNG' ? 200 : 500;
    return Math.floor(qty / dailyConsumption);
  }

  getForecastStatus(days: number): { label: string; cls: string } {
    if (days <= 0) return { label: 'Out of Stock', cls: 'text-error' };
    if (days <= 3) return { label: 'Critical', cls: 'text-error' };
    if (days <= 7) return { label: 'Order Soon', cls: 'text-secondary' };
    return { label: 'Sufficient', cls: 'text-on-tertiary-container' };
  }

  getReorderDate(fuelType: string): string {
    const days = this.getDaysRemaining(fuelType);
    const reorderDays = Math.max(0, days - 2);
    const d = new Date();
    d.setDate(d.getDate() + reorderDays);
    return d.toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  goBack() {
    void this.router.navigate(['/dashboard']);
  }

  navigate(path: string) {
    void this.router.navigate([path]);
  }

  private parseLastUpdatedMs(row: Record<string, unknown>): number {
    const v = row['lastUpdated'];
    if (typeof v === 'string' || v instanceof Date) {
      const t = new Date(v as any).getTime();
      return Number.isFinite(t) ? t : 0;
    }
    return 0;
  }

  getStock(fuelType: string): Record<string, unknown> | undefined {
    const ft = fuelType.toLowerCase();
    let best: Record<string, unknown> | undefined;
    let bestMs = -1;
    for (const s of this.stocks) {
      const sFt = String(s['fuelType'] ?? '').toLowerCase();
      if (sFt !== ft) continue;
      const ms = this.parseLastUpdatedMs(s);
      if (!best || ms > bestMs) { best = s; bestMs = ms; }
    }
    return best;
  }

  getFillPct(fuelType: string): number {
    const s = this.getStock(fuelType);
    if (!s) return 0;
    const qty = Number(s['quantity'] ?? 0);
    return Math.min(100, Math.round((qty / 20000) * 100));
  }

  getFillColor(pct: number): string {
    if (pct <= 15) return 'bg-error';
    if (pct <= 40) return 'bg-secondary';
    return 'bg-on-tertiary-container';
  }

  getStatusBadge(fuelType: string): { label: string; cls: string } {
    const pct = this.getFillPct(fuelType);
    if (pct === 0) return { label: 'No Stock', cls: 'bg-error-container text-on-error-container' };
    if (pct <= 15) return { label: 'Critical', cls: 'bg-error-container text-on-error-container' };
    if (pct <= 40) return { label: 'Low Stock', cls: 'bg-secondary-fixed text-on-secondary-fixed-variant' };
    return { label: 'Optimal', cls: 'bg-tertiary-fixed/30 text-on-tertiary-fixed-variant' };
  }
}
