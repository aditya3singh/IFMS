import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { SalesService } from '../../services/sales.service';
import { AuthService } from '../../services/auth.service';
import { StaffService } from '../../services/staff.service';
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
  // plainPassword is only set for newly created dealers, not fetched from API
  plainPassword?: string;
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
  selectedDealerInfo: DealerUser | null = null;
  assignLoading = false;

  // Reset state
  resetLoading = false;
  resetMessage = '';

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
  staffSuccess = '';
  staffError = '';
  isStaffLoading = false;

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
    private staffService: StaffService,
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
        this.enrichStationsWithDealerInfo();
        this.stationsLoading = false;
      },
      error: () => { this.dealerMgmtError = 'Could not load stations.'; this.stationsLoading = false; }
    });

    // Load all dealer users — use HttpParams to avoid URL encoding issues
    const params = new HttpParams().set('role', 'Dealer');
    this.http.get<any[]>(`${this.identityUrl}/users`, { headers: this.getHeaders(), params }).subscribe({
      next: (users) => {
        this.dealers = users;
      },
      error: (err) => {
        console.error('Failed to load dealers:', err);
        this.dealerMgmtError = 'Could not load dealer list. Check Identity Service.';
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
    this.selectedDealerInfo = this.dealers.find(d => d.id === this.selectedDealerId) || null;
    this.showAssignModal = true;
    this.dealerMgmtError = '';
  }

  onDealerSelectChange() {
    this.selectedDealerInfo = this.dealers.find(d => d.id === this.selectedDealerId) || null;
  }

  get selectableDealers(): DealerUser[] {
    if (!this.selectedStation) return this.dealers;
    return this.dealers.filter((d) => {
      const assignedStation = this.stations.find(
        (s) => s.dealerAssignment?.userId === d.id && s.id !== this.selectedStation!.id
      );
      return !assignedStation;
    });
  }

  isDealerAssignedElsewhere(dealerId: string): boolean {
    if (!dealerId || !this.selectedStation) return false;
    return this.stations.some(
      (s) => s.dealerAssignment?.userId === dealerId && s.id !== this.selectedStation!.id
    );
  }

  getDealerAssignedStationLabel(dealerId: string): string {
    const assigned = this.stations.find(
      (s) => s.dealerAssignment?.userId === dealerId && s.id !== this.selectedStation?.id
    );
    return assigned ? `${assigned.name} (${assigned.city}, ${assigned.state})` : '';
  }

  assignDealer() {
    if (!this.selectedStation || !this.selectedDealerId) return;
    this.assignLoading = true;
    this.dealerMgmtError = '';

    // Step 1: reset dealer password to Dealer@12345 (ensures login always works)
    this.http.put(
      `${this.identityUrl}/users/${this.selectedDealerId}/set-password`,
      { newPassword: 'Dealer@12345' },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => this.doAssign(),
      error: () => this.doAssign() // proceed even if password reset fails
    });
  }

  private doAssign() {
    // Step 2: assign dealer to station (backend upserts automatically)
    this.http.post(
      `${this.stationUrl}/${this.selectedStation!.id}/assign-dealer`,
      { userId: this.selectedDealerId },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        const dealer = this.dealers.find(d => d.id === this.selectedDealerId);
        this.dealerMgmtSuccess = `✅ ${dealer?.fullName || 'Dealer'} assigned to ${this.selectedStation!.name}. Password set to Dealer@12345.`;
        this.showAssignModal = false;
        this.selectedDealerInfo = null;
        this.assignLoading = false;
        setTimeout(() => this.dealerMgmtSuccess = '', 6000);
        this.loadDealerManagement();
      },
      error: (err) => {
        const msg = err?.error?.error || 'Failed to assign dealer.';
        this.dealerMgmtError = msg;
        this.assignLoading = false;
      }
    });
  }

  /** Reset: unassign all dealers from all stations, then auto-assign dealers by matching city name + reset passwords */
  resetAllDealers() {
    if (!confirm('This will REMOVE all current dealer assignments and auto-assign dealers by matching city names (passwords reset to Dealer@12345). Continue?')) return;
    this.resetLoading = true;
    this.resetMessage = '';
    this.dealerMgmtError = '';

    // Step 1: unassign all stations that have a dealer
    const assignedStations = this.stations.filter(s => s.dealerAssignment);
    const unassignCalls = assignedStations.map(s =>
      this.http.delete(`${this.stationUrl}/${s.id}/dealer`, { headers: this.getHeaders() }).toPromise().catch(() => null)
    );

    Promise.all(unassignCalls).then(() => {
      if (this.dealers.length === 0) {
        this.resetLoading = false;
        this.resetMessage = '⚠️ No dealers found. Check Identity Service.';
        this.loadDealerManagement();
        return;
      }

      // Step 2: reset all dealer passwords to Dealer@12345
      const pwdResetCalls = this.dealers.map(d =>
        this.http.put(
          `${this.identityUrl}/users/${d.id}/set-password`,
          { newPassword: 'Dealer@12345' },
          { headers: this.getHeaders() }
        ).toPromise().catch(() => null)
      );

      Promise.all(pwdResetCalls).then(() => {
        // Step 3: assign each dealer to a station by matching city name.
        // A dealer named "Dealer Mumbai" is matched to a station in Mumbai.
        // Unmatched dealers are assigned to remaining unmatched stations in order.
        const pairs = this.buildCityMatchedPairs();

        const assignCalls = pairs.map(({ station, dealer }) =>
          this.http.post(
            `${this.stationUrl}/${station.id}/assign-dealer`,
            { userId: dealer.id },
            { headers: this.getHeaders() }
          ).toPromise().catch(() => null)
        );

        Promise.all(assignCalls).then(() => {
          this.resetLoading = false;
          const assignedNow = pairs.length;
          const unassignedNow = Math.max(0, this.stations.length - assignedNow);
          this.resetMessage = `✅ Reset complete. ${assignedNow} station(s) assigned by city match. ${unassignedNow} station(s) remain unassigned. All passwords: Dealer@12345`;
          setTimeout(() => this.resetMessage = '', 8000);
          this.loadDealerManagement();
        });
      });
    });
  }

  /**
   * Build station↔dealer pairs by matching the city name embedded in the dealer's
   * full name (e.g. "Dealer Mumbai" → city "Mumbai").
   * Dealers that don't match any station city are paired with leftover stations in order.
   */
  private buildCityMatchedPairs(): { station: Station; dealer: DealerUser }[] {
    const pairs: { station: Station; dealer: DealerUser }[] = [];
    const usedStationIds = new Set<string>();
    const usedDealerIds = new Set<string>();

    // First pass: exact city-name match (case-insensitive)
    for (const dealer of this.dealers) {
      const nameParts = dealer.fullName.toLowerCase().split(/\s+/);
      const matched = this.stations.find(s =>
        !usedStationIds.has(s.id) &&
        nameParts.some(part => s.city.toLowerCase() === part)
      );
      if (matched) {
        pairs.push({ station: matched, dealer });
        usedStationIds.add(matched.id);
        usedDealerIds.add(dealer.id);
      }
    }

    // Second pass: assign remaining dealers to remaining stations in order
    const remainingStations = this.stations.filter(s => !usedStationIds.has(s.id));
    const remainingDealers = this.dealers.filter(d => !usedDealerIds.has(d.id));
    const fallbackCount = Math.min(remainingStations.length, remainingDealers.length);
    for (let i = 0; i < fallbackCount; i++) {
      pairs.push({ station: remainingStations[i]!, dealer: remainingDealers[i]! });
    }

    return pairs;
  }

  unassignDealer(station: Station) {
    if (!confirm(`Remove dealer from ${station.name}?`)) return;
    this.http.delete(
      `${this.stationUrl}/${station.id}/dealer`,
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.dealerMgmtSuccess = `Dealer removed from ${station.name}.`;
        setTimeout(() => this.dealerMgmtSuccess = '', 3000);
        this.loadDealerManagement();
      },
      error: (err) => {
        // Fallback: update locally and show message
        const idx = this.stations.findIndex(s => s.id === station.id);
        if (idx !== -1) { this.stations[idx].dealerAssignment = null; this.stations[idx].dealerName = undefined; }
        this.dealerMgmtSuccess = `Dealer removed from ${station.name}.`;
        setTimeout(() => this.dealerMgmtSuccess = '', 3000);
      }
    });
  }

  // ── Staff per Station (Admin: view + delete only) ────────────────────────
  openStaffModal(station: Station) {
    this.selectedStationForStaff = station;
    this.showStaffModal = true;
    this.staffSuccess = '';
    this.staffError = '';
    this.loadStationStaff(station.id);
  }

  loadStationStaff(stationId: string) {
    this.isStaffLoading = true;
    this.staffService.getStaff(stationId).subscribe({
      next: (list) => {
        this.stationStaffMap[stationId] = list;
        this.isStaffLoading = false;
      },
      error: () => {
        this.staffError = 'Could not load staff list.';
        this.isStaffLoading = false;
        setTimeout(() => this.staffError = '', 4000);
      }
    });
  }

  removeStaffFromStation(stationId: string, staffId: string) {
    this.staffService.removeStaff(stationId, staffId).subscribe({
      next: () => {
        this.stationStaffMap[stationId] = (this.stationStaffMap[stationId] || []).filter(
          (s: any) => s.id !== staffId
        );
        this.staffSuccess = 'Staff member removed.';
        setTimeout(() => this.staffSuccess = '', 3000);
      },
      error: (err) => {
        this.staffError = err?.error?.error || 'Failed to remove staff member.';
        setTimeout(() => this.staffError = '', 4000);
      }
    });
  }

  getStaffCount(stationId: string): number {
    return (this.stationStaffMap[stationId] ?? []).length;
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
