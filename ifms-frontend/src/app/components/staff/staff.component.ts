import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';

interface StaffMember {
  id: number;
  name: string;
  role: string;
  shift: string;
  phone: string;
  email: string;
  status: 'Active' | 'Off Duty' | 'On Leave';
  joinDate: string;
}

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

  newStaff: Omit<StaffMember, 'id'> = {
    name: '', role: 'Pump Operator', shift: 'Morning',
    phone: '', email: '', status: 'Active', joinDate: new Date().toISOString().split('T')[0]
  };

  readonly roles = ['Pump Operator', 'Cashier', 'Supervisor', 'Security Guard', 'Maintenance'];
  readonly shifts = ['Morning (6AM-2PM)', 'Afternoon (2PM-10PM)', 'Night (10PM-6AM)'];
  readonly statuses: StaffMember['status'][] = ['Active', 'Off Duty', 'On Leave'];

  // Pagination
  staffPage = 1; staffPageSize = 8;
  get pagedStaff() { return this.filteredStaff.slice((this.staffPage-1)*this.staffPageSize, this.staffPage*this.staffPageSize); }

  constructor(private router: Router) {}

  ngOnInit() {
    this.loadStaff();
  }

  loadStaff() {
    const stored = localStorage.getItem('ifms_staff');
    if (stored) {
      this.staffList = JSON.parse(stored);
    } else {
      // Default seed data
      this.staffList = [
        { id: 1, name: 'Ramesh Kumar', role: 'Pump Operator', shift: 'Morning (6AM-2PM)', phone: '9876543210', email: 'ramesh@station.com', status: 'Active', joinDate: '2024-01-15' },
        { id: 2, name: 'Suresh Patel', role: 'Cashier', shift: 'Afternoon (2PM-10PM)', phone: '9876543211', email: 'suresh@station.com', status: 'Active', joinDate: '2024-03-01' },
        { id: 3, name: 'Anita Singh', role: 'Supervisor', shift: 'Morning (6AM-2PM)', phone: '9876543212', email: 'anita@station.com', status: 'Active', joinDate: '2023-11-10' },
        { id: 4, name: 'Vijay Sharma', role: 'Security Guard', shift: 'Night (10PM-6AM)', phone: '9876543213', email: 'vijay@station.com', status: 'Off Duty', joinDate: '2024-02-20' },
        { id: 5, name: 'Priya Nair', role: 'Pump Operator', shift: 'Afternoon (2PM-10PM)', phone: '9876543214', email: 'priya@station.com', status: 'On Leave', joinDate: '2024-05-05' },
      ];
      this.saveStaff();
    }
  }

  saveStaff() {
    localStorage.setItem('ifms_staff', JSON.stringify(this.staffList));
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
    this.newStaff = { name: '', role: 'Pump Operator', shift: 'Morning (6AM-2PM)', phone: '', email: '', status: 'Active', joinDate: new Date().toISOString().split('T')[0] };
    this.showAddModal = true;
    this.errorMsg = '';
  }

  addStaff() {
    if (!this.newStaff.name.trim() || !this.newStaff.phone.trim()) {
      this.errorMsg = 'Name and phone are required.';
      return;
    }
    const member: StaffMember = { id: Date.now(), ...this.newStaff };
    this.staffList.unshift(member);
    this.saveStaff();
    this.showAddModal = false;
    this.successMsg = `${member.name} added successfully.`;
    setTimeout(() => this.successMsg = '', 3000);
  }

  openEditModal(staff: StaffMember) {
    this.selectedStaff = { ...staff };
    this.showEditModal = true;
    this.errorMsg = '';
  }

  saveEdit() {
    if (!this.selectedStaff) return;
    const idx = this.staffList.findIndex(s => s.id === this.selectedStaff!.id);
    if (idx !== -1) {
      this.staffList[idx] = { ...this.selectedStaff };
      this.saveStaff();
      this.showEditModal = false;
      this.successMsg = `${this.selectedStaff.name} updated.`;
      setTimeout(() => this.successMsg = '', 3000);
    }
  }

  deleteStaff(id: number) {
    const member = this.staffList.find(s => s.id === id);
    if (!member) return;
    if (confirm(`Remove ${member.name} from staff?`)) {
      this.staffList = this.staffList.filter(s => s.id !== id);
      this.saveStaff();
      this.successMsg = `${member.name} removed.`;
      setTimeout(() => this.successMsg = '', 3000);
    }
  }

  updateStatus(id: number, status: StaffMember['status']) {
    const idx = this.staffList.findIndex(s => s.id === id);
    if (idx !== -1) {
      this.staffList[idx].status = status;
      this.saveStaff();
    }
  }

  getStatusCls(status: string): string {
    if (status === 'Active') return 'bg-green-100 text-green-800';
    if (status === 'Off Duty') return 'bg-gray-100 text-gray-700';
    return 'bg-yellow-100 text-yellow-800';
  }

  goBack() { void this.router.navigate(['/dashboard']); }
  navigate(path: string) { void this.router.navigate([path]); }
}
