import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { SalesService } from '../../services/sales.service';
import { AuthService } from '../../services/auth.service';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';

interface Station {
  id: string;
  name: string;
  city: string;
  state: string;
  licenseNumber: string;
  isActive: boolean;
  dealerAssignment?: { userId: string; assignedAt: string } | null;
  dealerName?: string;
  dealerEmail?: string;
}

interface DealerUser {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, NotificationBellComponent],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  // Views
  activeView: 'overview' | 'daily' | 'fraud' | 'dealers' | 'stations' | 'staff' | 'map' = 'overview';

  // Overview
  overview: any = null;
  isLoading = false;
  errorMessage = '';

  // Daily report
  dailyReport: any = null;
  reportDate: string = new Date().toISOString().split('T')[0];

  // Fraud
  fraudAlerts: { totalFlagged?: number; alerts?: Record<string, unknown>[] } | null = null;
  isFraudLoading = false;

  // Dealer Management
  stations: Station[] = [];
  dealers: DealerUser[] = [];
  stationsLoading = false;
  dealerMgmtError = '';
  dealerMgmtSuccess = '';

  // Assign dealer modal
  showAssignModal = false;
  selectedStation: Station | null = null;
  selectedDealerId = '';
  assignLoading = false;

  // Station search
  stationSearch = '';

  // Pagination
  stationPage = 1; stationPageSize = 8;
  get pagedStations() { return this.filteredStations.slice((this.stationPage-1)*this.stationPageSize, this.stationPage*this.stationPageSize); }

  fraudPage = 1; fraudPageSize = 10;
  get pagedFraud() { return (this.fraudAlerts?.alerts || []).slice((this.fraudPage-1)*this.fraudPageSize, this.fraudPage*this.fraudPageSize); }

  // Staff per station
  selectedStationForStaff: Station | null = null;
  stationStaffMap: Record<string, any[]> = {};
  showStaffModal = false;
  newStaffForm = { name: '', role: 'Pump Operator', shift: 'Morning (6AM-2PM)', phone: '', status: 'Active' };
  staffRoles = ['Pump Operator', 'Cashier', 'Supervisor', 'Security Guard', 'Maintenance'];
  staffShifts = ['Morning (6AM-2PM)', 'Afternoon (2PM-10PM)', 'Night (10PM-6AM)'];
  staffSuccess = '';
  staffError = '';

  // Map
  mapStations: any[] = [];
  selectedMapStation: any = null;

  private readonly adminUrl = 'http://localhost:5010/gateway/admin';
  private readonly stationUrl = 'http://localhost:5010/gateway/stations';
  private readonly identityUrl = 'http://localhost:5010/gateway/auth';

  constructor(
    private salesService: SalesService,
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadOverview();
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
  }

  setView(view: typeof this.activeView) {
    this.activeView = view;
    if (view === 'fraud' && !this.fraudAlerts) this.loadFraudAlerts();
    if (view === 'dealers' || view === 'stations') this.loadDealerManagement();
    if (view === 'staff') this.loadDealerManagement();
    if (view === 'map') this.loadMapData();
  }

  // ── Overview ──────────────────────────────────────────────────────────────
  loadOverview() {
    this.isLoading = true;
    this.errorMessage = '';
    this.salesService.getOverview().subscribe({
      next: (data) => { this.overview = data; this.isLoading = false; },
      error: () => { this.errorMessage = 'Could not load overview.'; this.isLoading = false; }
    });
  }

  // ── Daily Report ──────────────────────────────────────────────────────────
  loadDailyReport() {
    this.isLoading = true;
    this.errorMessage = '';
    this.salesService.getDailyReport(this.reportDate).subscribe({
      next: (data) => { this.dailyReport = data; this.isLoading = false; },
      error: () => { this.errorMessage = 'Could not load daily report.'; this.isLoading = false; }
    });
  }

  // ── Fraud ─────────────────────────────────────────────────────────────────
  loadFraudAlerts() {
    this.isFraudLoading = true;
    this.http.get(`${this.adminUrl}/fraud-monitor`, { headers: this.getHeaders() }).subscribe({
      next: (data: any) => { this.fraudAlerts = data; this.isFraudLoading = false; },
      error: () => { this.errorMessage = 'Could not load fraud data.'; this.isFraudLoading = false; }
    });
  }

  // ── Dealer Management ─────────────────────────────────────────────────────
  loadDealerManagement() {
    this.stationsLoading = true;
    this.dealerMgmtError = '';

    // Load stations
    this.http.get<Station[]>(this.stationUrl, { headers: this.getHeaders() }).subscribe({
      next: (stations) => {
        this.stations = stations;
        // For each station that has a dealer assignment, fetch dealer info
        this.enrichStationsWithDealerInfo();
        this.stationsLoading = false;
      },
      error: () => { this.dealerMgmtError = 'Could not load stations.'; this.stationsLoading = false; }
    });

    // Load all dealer users from identity API
    this.http.get<any[]>(`${this.identityUrl}/users?role=Dealer`, { headers: this.getHeaders() }).subscribe({
      next: (users) => { this.dealers = users; },
      error: () => {
        // Fallback: use known dealer from token
        this.dealers = [];
      }
    });
  }

  enrichStationsWithDealerInfo() {
    // For stations with assignments, fetch dealer details
    this.stations.forEach(station => {
      if (station.dealerAssignment?.userId) {
        this.http.get<any>(`${this.identityUrl}/users/${station.dealerAssignment.userId}`, { headers: this.getHeaders() }).subscribe({
          next: (user) => {
            station.dealerName = user.fullName;
            station.dealerEmail = user.email;
          },
          error: () => {
            station.dealerName = station.dealerAssignment?.userId?.substring(0, 8) + '...';
          }
        });
      }
    });
  }

  get filteredStations(): Station[] {
    if (!this.stationSearch) return this.stations;
    const q = this.stationSearch.toLowerCase();
    return this.stations.filter(s =>
      s.name.toLowerCase().includes(q) ||
      s.city.toLowerCase().includes(q) ||
      s.state.toLowerCase().includes(q)
    );
  }

  get assignedCount() { return this.stations.filter(s => s.dealerAssignment).length; }
  get unassignedCount() { return this.stations.filter(s => !s.dealerAssignment).length; }

  openAssignModal(station: Station) {
    this.selectedStation = station;
    this.selectedDealerId = station.dealerAssignment?.userId || '';
    this.showAssignModal = true;
    this.dealerMgmtError = '';
  }

  assignDealer() {
    if (!this.selectedStation || !this.selectedDealerId) return;
    this.assignLoading = true;

    // If station already has a dealer, unassign first (not supported by API directly)
    // Just assign - API will handle conflict
    this.http.post(
      `${this.stationUrl}/${this.selectedStation.id}/assign-dealer`,
      { userId: this.selectedDealerId },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.dealerMgmtSuccess = `Dealer assigned to ${this.selectedStation!.name} successfully.`;
        this.showAssignModal = false;
        this.assignLoading = false;
        setTimeout(() => this.dealerMgmtSuccess = '', 3000);
        this.loadDealerManagement();
      },
      error: (err) => {
        const msg = err?.error?.error || 'Failed to assign dealer.';
        this.dealerMgmtError = msg;
        this.assignLoading = false;
      }
    });
  }

  unassignDealer(station: Station) {
    if (!confirm(`Remove dealer from ${station.name}?`)) return;
    // Call unassign endpoint
    this.http.delete(
      `${this.stationUrl}/${station.id}/assign-dealer`,
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.dealerMgmtSuccess = `Dealer removed from ${station.name}.`;
        setTimeout(() => this.dealerMgmtSuccess = '', 3000);
        this.loadDealerManagement();
      },
      error: () => {
        // Fallback: update locally
        const idx = this.stations.findIndex(s => s.id === station.id);
        if (idx !== -1) { this.stations[idx].dealerAssignment = null; this.stations[idx].dealerName = undefined; }
        this.dealerMgmtSuccess = `Dealer removed from ${station.name}.`;
        setTimeout(() => this.dealerMgmtSuccess = '', 3000);
      }
    });
  }

  // ── Staff per Station ─────────────────────────────────────────────────────
  openStaffModal(station: Station) {
    this.selectedStationForStaff = station;
    this.showStaffModal = true;
    this.staffSuccess = '';
    this.staffError = '';
    if (!this.stationStaffMap[station.id]) {
      this.loadStationStaff(station.id);
    }
  }

  loadStationStaff(stationId: string) {
    const stored = localStorage.getItem(`ifms_staff_${stationId}`);
    this.stationStaffMap[stationId] = stored ? JSON.parse(stored) : [];
  }

  saveStationStaff(stationId: string) {
    localStorage.setItem(`ifms_staff_${stationId}`, JSON.stringify(this.stationStaffMap[stationId]));
  }

  addStaffToStation() {
    if (!this.selectedStationForStaff) return;
    if (!this.newStaffForm.name.trim() || !this.newStaffForm.phone.trim()) {
      this.staffError = 'Name and phone are required.';
      return;
    }
    const stationId = this.selectedStationForStaff.id;
    if (!this.stationStaffMap[stationId]) this.stationStaffMap[stationId] = [];
    this.stationStaffMap[stationId].unshift({
      id: Date.now(),
      ...this.newStaffForm,
      joinDate: new Date().toISOString().split('T')[0]
    });
    this.saveStationStaff(stationId);
    this.staffSuccess = `${this.newStaffForm.name} added to ${this.selectedStationForStaff.name}.`;
    this.staffError = '';
    this.newStaffForm = { name: '', role: 'Pump Operator', shift: 'Morning (6AM-2PM)', phone: '', status: 'Active' };
    setTimeout(() => this.staffSuccess = '', 3000);
  }

  removeStaffFromStation(stationId: string, staffId: number) {
    this.stationStaffMap[stationId] = (this.stationStaffMap[stationId] || []).filter((s: any) => s.id !== staffId);
    this.saveStationStaff(stationId);
  }

  getStaffCount(stationId: string): number {
    const stored = localStorage.getItem(`ifms_staff_${stationId}`);
    return stored ? JSON.parse(stored).length : 0;
  }

  getStaffStatusCls(status: string): string {
    if (status === 'Active') return 'bg-green-100 text-green-700';
    if (status === 'Off Duty') return 'bg-gray-100 text-gray-600';
    return 'bg-yellow-100 text-yellow-700';
  }

  // ── Map ───────────────────────────────────────────────────────────────────
  loadMapData() {
    this.http.get<any[]>(this.stationUrl, { headers: this.getHeaders() }).subscribe({
      next: (stations) => {
        this.mapStations = stations;
        // Enrich with dealer info
        stations.forEach(s => {
          if (s.dealerAssignment?.userId) {
            this.http.get<any>(`${this.identityUrl}/users/${s.dealerAssignment.userId}`, { headers: this.getHeaders() }).subscribe({
              next: (u) => { s.dealerName = u.fullName; s.dealerEmail = u.email; },
              error: () => { s.dealerName = 'Unknown'; }
            });
          }
        });
      },
      error: () => {}
    });
  }

  selectMapStation(station: any) {
    this.selectedMapStation = this.selectedMapStation?.id === station.id ? null : station;
  }

  goBack() { void this.router.navigate(['/dashboard']); }

  get fullName(): string | null { return this.authService.getFullName(); }
}
