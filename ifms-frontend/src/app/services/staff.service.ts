import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface StaffMember {
  id: string;          // Guid from backend
  stationId: string;
  name: string;
  role: string;
  shift: string;
  phone: string;
  email: string;
  status: 'Active' | 'Off Duty' | 'On Leave';
  joinDate: string;
  createdAt?: string;
}

export type CreateStaffPayload = Omit<StaffMember, 'id' | 'stationId' | 'createdAt'>;
export type UpdateStaffPayload = Omit<StaffMember, 'id' | 'stationId' | 'createdAt'>;

/**
 * Staff service — all data is persisted in SQL Server via the Station API.
 * Both Admin and Dealer always read/write the same shared database,
 * so changes are immediately visible to all roles.
 *
 * Gateway base: http://localhost:5010/gateway/stations/{stationId}/staff
 */
@Injectable({ providedIn: 'root' })
export class StaffService {

  private readonly gatewayBase = 'http://localhost:5010/gateway/stations';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
  }

  private staffUrl(stationId: string): string {
    return `${this.gatewayBase}/${stationId}/staff`;
  }

  /** GET  /gateway/stations/{stationId}/staff — Admin + Dealer */
  getStaff(stationId: string): Observable<StaffMember[]> {
    return this.http.get<StaffMember[]>(this.staffUrl(stationId), { headers: this.headers });
  }

  /** POST /gateway/stations/{stationId}/staff — Dealer only */
  addStaff(stationId: string, payload: CreateStaffPayload): Observable<StaffMember> {
    return this.http.post<StaffMember>(this.staffUrl(stationId), payload, { headers: this.headers });
  }

  /** PUT  /gateway/stations/{stationId}/staff/{staffId} — Dealer only */
  updateStaff(stationId: string, member: StaffMember): Observable<StaffMember> {
    return this.http.put<StaffMember>(
      `${this.staffUrl(stationId)}/${member.id}`,
      member,
      { headers: this.headers }
    );
  }

  /** PATCH /gateway/stations/{stationId}/staff/{staffId}/status — Dealer only */
  updateStatus(stationId: string, staffId: string, status: StaffMember['status']): Observable<StaffMember> {
    return this.http.patch<StaffMember>(
      `${this.staffUrl(stationId)}/${staffId}/status`,
      { status },
      { headers: this.headers }
    );
  }

  /** DELETE /gateway/stations/{stationId}/staff/{staffId} — Admin + Dealer */
  removeStaff(stationId: string, staffId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.staffUrl(stationId)}/${staffId}`,
      { headers: this.headers }
    );
  }
}
