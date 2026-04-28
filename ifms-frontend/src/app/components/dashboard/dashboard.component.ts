import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SalesService } from '../../services/sales.service';
import { StationService } from '../../services/station.service';
import { BookingService } from '../../services/booking.service';
import { AuthService } from '../../services/auth.service';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';

const DEMO_STOCK_SERIES = [4120, 4055, 4188, 4010, 4234, 4166, 4295];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe, PaginationComponent, NotificationBellComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  overview: { totalTransactions?: number; totalRevenue?: number; petrolSold?: number; dieselSold?: number } | null = null;
  overviewLoading = false;
  overviewError = false;
  fullName: string | null = null;
  role: string | null = null;

  dealerStations: { id: string; name: string; city: string; state: string }[] = [];
  dealerStationsLoading = false;
  selectedStationId: string | null = null;
  stationBookings: any[] = [];
  stationBookingsLoading = false;
  stationBookingsError = '';
  customerBookings: any[] = [];
  customerBookingsLoading = false;
  customerBookingsError = '';

  readonly stockGraphPoints: { x: number; y: number }[];
  readonly stockGraphLast: number;
  readonly stockGraphFirst: number;
  readonly stockGraphPolyline: string;
  readonly stockGraphFillPolygon: string;

  readonly newsItems = [
    { title: 'MoPNG advisory', text: 'Verify pump calibration seals during monthly inspections across all retail outlets.' },
    { title: 'Safety bulletin', text: 'Static discharge: ask customers to switch off engines and mobile devices before dispensing.' },
    { title: 'IFMS bookings', text: 'Prepaid tokens remain valid for 24 hours; dealers must validate before dispensing fuel.' },
    { title: 'EV corridor', text: 'New fast-charging bays reported operational on NH-48 — update station maps in admin.' }
  ];
  activeNewsIndex = 0;
  private newsIntervalId: ReturnType<typeof setInterval> | null = null;

  // Pagination — dealer station bookings
  bookingPage = 1; bookingPageSize = 8;
  get pagedBookings() { return this.stationBookings.slice((this.bookingPage-1)*this.bookingPageSize, this.bookingPage*this.bookingPageSize); }

  // Pagination — customer bookings
  custBookingPage = 1; custBookingPageSize = 5;
  get pagedCustBookings() { return this.customerBookings.slice((this.custBookingPage-1)*this.custBookingPageSize, this.custBookingPage*this.custBookingPageSize); }

  readonly helpFaqs = [
    { q: 'How do I book fuel?', a: 'Open Bookings, pick a station and fuel type, complete PAN or Aadhaar verification, pay, then show your token or QR at the pump.' },
    { q: 'I am a dealer — how do I validate a token?', a: 'Use Bookings → enter the customer token code. Confirm dispensing only after you have verified identity and the vehicle.' },
    { q: 'Why was my KYC rejected?', a: 'Check PAN format (5 letters, 4 digits, 1 letter) or Aadhaar checksum.' }
  ];

  // Fuel price display (static demo)
  readonly petrolPrice = 103.44;
  readonly dieselPrice = 94.82;
  readonly cngPrice = 82.50;

  // Dealer KPIs (derived from overview)
  get salesToday(): string {
    return this.overview?.totalRevenue ? `₹${(this.overview.totalRevenue / 100).toFixed(2)}` : '₹0.00';
  }

  constructor(
    private salesService: SalesService,
    private stationService: StationService,
    private bookingService: BookingService,
    private authService: AuthService,
    private router: Router
  ) {
    const pts = DashboardComponent.buildStockGraphPoints(DEMO_STOCK_SERIES);
    this.stockGraphPoints = pts;
    this.stockGraphPolyline = pts.map((p) => `${p.x},${p.y}`).join(' ');
    this.stockGraphLast = DEMO_STOCK_SERIES[DEMO_STOCK_SERIES.length - 1]!;
    this.stockGraphFirst = DEMO_STOCK_SERIES[0]!;
    const h = 36;
    if (pts.length === 0) {
      this.stockGraphFillPolygon = '';
    } else {
      const first = pts[0]!;
      const last = pts[pts.length - 1]!;
      this.stockGraphFillPolygon = `${first.x},${h} ${pts.map((p) => `${p.x},${p.y}`).join(' ')} ${last.x},${h}`;
    }
  }

  private static buildStockGraphPoints(values: number[]): { x: number; y: number }[] {
    if (values.length === 0) return [];
    const min = Math.min(...values);
    const max = Math.max(...values);
    const span = max - min || 1;
    const w = 100; const h = 36; const pad = 4;
    return values.map((v, i) => {
      const x = values.length === 1 ? w / 2 : (i / (values.length - 1)) * (w - pad * 2) + pad;
      const t = (v - min) / span;
      const y = pad + (1 - t) * (h - pad * 2);
      return { x, y };
    });
  }

  ngOnInit() {
    this.fullName = typeof localStorage !== 'undefined' ? localStorage.getItem('fullName') : null;
    this.role = typeof localStorage !== 'undefined' ? localStorage.getItem('role') : null;

    this.newsIntervalId = setInterval(() => {
      this.activeNewsIndex = (this.activeNewsIndex + 1) % this.newsItems.length;
    }, 4500);

    if (this.role === 'Admin' || this.role === 'Dealer') {
      this.overviewLoading = true;
      this.salesService.getOverview().subscribe({
        next: (data) => { this.overview = data; this.overviewLoading = false; },
        error: () => { this.overviewError = true; this.overviewLoading = false; }
      });
    }

    if (this.role === 'Dealer') {
      this.loadDealerStations();
    }
    if (this.role === 'Customer') {
      this.loadCustomerBookings();
    }
  }

  loadCustomerBookings() {
    this.customerBookingsLoading = true;
    this.customerBookingsError = '';
    this.bookingService.getMyBookings().subscribe({
      next: (rows) => {
        this.customerBookings = rows ?? [];
        this.customerBookingsLoading = false;
      },
      error: () => {
        this.customerBookingsLoading = false;
        this.customerBookingsError = 'Could not load your bookings.';
        this.customerBookings = [];
      }
    });
  }

  loadDealerStations() {
    this.dealerStationsLoading = true;
    this.stationBookingsError = '';
    this.stationService.getMyStations().subscribe({
      next: (list) => {
        this.dealerStations = (list ?? []).map((s: { id?: string; name?: string; city?: string; state?: string }) => ({
          id: String(s?.id ?? ''), name: String(s?.name ?? ''), city: String(s?.city ?? ''), state: String(s?.state ?? '')
        }));
        this.dealerStationsLoading = false;
        if (this.dealerStations.length > 0) {
          this.selectedStationId = this.dealerStations[0]!.id;
          this.loadStationBookings();
        }
      },
      error: () => {
        this.dealerStationsLoading = false;
        this.stationBookingsError = 'Could not load your assigned stations.';
      }
    });
  }

  onDealerStationChange() { this.loadStationBookings(); }

  loadStationBookings() {
    if (!this.selectedStationId) { this.stationBookings = []; return; }
    this.stationBookingsLoading = true;
    this.stationBookingsError = '';
    this.bookingService.getStationBookings(this.selectedStationId).subscribe({
      next: (rows) => { this.stationBookings = rows ?? []; this.stationBookingsLoading = false; },
      error: () => { this.stationBookingsLoading = false; this.stationBookingsError = 'Could not load bookings.'; this.stationBookings = []; }
    });
  }

  ngOnDestroy() {
    if (this.newsIntervalId !== null) { clearInterval(this.newsIntervalId); this.newsIntervalId = null; }
  }

  navigate(path: string) { void this.router.navigate([path]); }

  navigateTo(path: string, queryParams?: Record<string, string>) {
    void this.router.navigate([path], queryParams ? { queryParams } : {});
  }

  showProfileMenu = false;
  toggleProfileMenu() { this.showProfileMenu = !this.showProfileMenu; }

  logout() { this.authService.logout(); void this.router.navigate(['/login']); }

  showOpsNav(): boolean { return this.role === 'Dealer' || this.role === 'Admin'; }

  getStatusClass(status: string): string {
    const s = (status || '').toUpperCase();
    if (s === 'USED' || s === 'COMPLETED') return 'bg-tertiary text-on-tertiary-container';
    if (s === 'PENDING' || s === 'PROCESSING') return 'bg-secondary text-on-secondary-container';
    if (s === 'EXPIRED' || s === 'CANCELLED') return 'bg-error text-on-error';
    return 'bg-surface-container-high text-on-surface-variant';
  }
}
