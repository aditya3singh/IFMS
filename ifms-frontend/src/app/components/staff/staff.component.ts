import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { StaffService, StaffMember } from '../../services/staff.service';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, NotificationBellComponent],
  templateUrl: './staff.component.html'
})
export class StaffComponent implements OnInit {
  staffList: StaffMember[] = [];
  showAddModal = false;
  showEditModal = false;
  selectedStaff: StaffMember | null = null;
  searchQuery = '';
  filterStatus = 'All';
  successMsg = '';
  errorMsg = '';

  stationId: string | null = null;
  stationName = '';
  isLoadingStation = true;
  stationLoadError = '';

  newStaff: Omit<StaffMember, 'id'> = {
    name: '', role: 'Pump Operator', shift: 'Morning (6AM-2PM)',
    phone: '', email: '', status: 'Active',
    joinDate: new Date().toISOString().split('T')[0]
  };

  readonly roles = ['Pump Operator', 'Cashier', 'Supervisor', 'Security Guard', 'Maintenance'];
  readonly shifts = ['Morning (6AM-2PM)', 'Afternoon (2PM-10PM)', 'Night (10PM-6AM)'];
  readonly statuses: StaffMember['status'][] = ['Active', 'Off Duty', 'On Leave'];

  staffPage = 1; staffPageSize = 8;
  get pagedStaff() {
    return this.filteredStaff.slice(
      (this.staffPage - 1) * this.staffPageSize,
      this.staffPage * this.staffPageSize
    );
  }

  private readonly stationUrl = environment.stationApiUrl;

  constructor(
    private router: Router,
    private http: HttpClient,
    private authService: AuthService,
    private staffService: StaffService
  ) {}

  ngOnInit() {
    this.resolveStationThenLoad();
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
  }

  private resolveStationThenLoad() {
    this.isLoadingStation = true;
    this.staffList = [];

    this.http.get<any[]>(`${this.stationUrl}/mine`, { headers: this.getHeaders() }).subscribe({
      next: (stations) => {
        this.isLoadingStation = false;
        if (stations && stations.length > 0) {
          const s = stations[0];
          this.stationId = s.id;
          this.stationName = s.name ?? '';
          // Seed default staff by city if this station has no data yet
          this.staffService.seedIfEmpty(s.id, s.city ?? '');
        } else {
          this.stationId = null;
          this.stationLoadError = 'No station assigned to your account yet.';
        }
        this.reload();
      },
      error: () => {
        this.isLoadingStation = false;
        this.stationId = null;
        this.stationLoadError = 'Could not reach station service.';
        this.reload();
      }
    });
  }

  reload() {
    this.staffList = this.stationId
      ? this.staffService.getStaff(this.stationId)
      : [];
  }

  get filteredStaff(): StaffMember[] {
    return this.staffList.filter(s => {
      const matchSearch = !this.searchQuery ||
        s.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        s.role.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchStatus = this.filterStatus === 'All' || s.status === this.filterStatus;
      return matchSearch && matchStatus;
    });
  }

  get activeCount() { return this.staffList.filter(s => s.status === 'Active').length; }
  get offDutyCount() { return this.staffList.filter(s => s.status === 'Off Duty').length; }
  get onLeaveCount() { return this.staffList.filter(s => s.status === 'On Leave').length; }

  openAddModal() {
    this.newStaff = {
      name: '', role: 'Pump Operator', shift: 'Morning (6AM-2PM)',
      phone: '', email: '', status: 'Active',
      joinDate: new Date().toISOString().split('T')[0]
    };
    this.showAddModal = true;
    this.errorMsg = '';
  }

  addStaff() {
    if (!this.stationId) { this.errorMsg = 'No station assigned.'; return; }
    if (!this.newStaff.name.trim() || !this.newStaff.phone.trim()) {
      this.errorMsg = 'Name and phone are required.';
      return;
    }
    this.staffService.addStaff(this.stationId, this.newStaff);
    this.reload();
    this.showAddModal = false;
    this.successMsg = `${this.newStaff.name} added successfully.`;
    setTimeout(() => this.successMsg = '', 3000);
  }

  openEditModal(staff: StaffMember) {
    this.selectedStaff = { ...staff };
    this.showEditModal = true;
    this.errorMsg = '';
  }

  saveEdit() {
    if (!this.selectedStaff || !this.stationId) return;
    this.staffService.updateStaff(this.stationId, this.selectedStaff);
    this.reload();
    this.showEditModal = false;
    this.successMsg = `${this.selectedStaff.name} updated.`;
    setTimeout(() => this.successMsg = '', 3000);
  }

  deleteStaff(id: number) {
    if (!this.stationId) return;
    const member = this.staffList.find(s => s.id === id);
    if (!member) return;
    if (confirm(`Remove ${member.name} from staff?`)) {
      this.staffService.removeStaff(this.stationId, id);
      this.reload();
      this.successMsg = `${member.name} removed.`;
      setTimeout(() => this.successMsg = '', 3000);
    }
  }

  updateStatus(id: number, status: StaffMember['status']) {
    if (!this.stationId) return;
    this.staffService.updateStatus(this.stationId, id, status);
    this.reload();
  }

  getStatusCls(status: string): string {
    if (status === 'Active') return 'bg-green-100 text-green-800';
    if (status === 'Off Duty') return 'bg-gray-100 text-gray-700';
    return 'bg-yellow-100 text-yellow-800';
  }

  goBack() { void this.router.navigate(['/dashboard']); }
  navigate(path: string) { void this.router.navigate([path]); }
}
