import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';

type BookingRow = {
  bookingId: string;
  customerId: string;
  stationId: string;
  fuelType: string;
  quantityLiters: number;
  totalPaid: number;
  tokenCode: string;
  tokenStatus: string;
  paymentId: string;
  bookedAt: string;
  expiresAt: string;
  usedAt?: string | null;
};

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly apiUrl = environment.bookingApiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  private normalizeBookingRow(raw: Record<string, unknown>): BookingRow {
    const read = (camel: string, pascal: string) => raw[camel] ?? raw[pascal];
    return {
      bookingId: String(read('bookingId', 'BookingId') ?? ''),
      customerId: String(read('customerId', 'CustomerId') ?? ''),
      stationId: String(read('stationId', 'StationId') ?? ''),
      fuelType: String(read('fuelType', 'FuelType') ?? ''),
      quantityLiters: Number(read('quantityLiters', 'QuantityLiters') ?? 0),
      totalPaid: Number(read('totalPaid', 'TotalPaid') ?? 0),
      tokenCode: String(read('tokenCode', 'TokenCode') ?? ''),
      tokenStatus: String(read('tokenStatus', 'TokenStatus') ?? ''),
      paymentId: String(read('paymentId', 'PaymentId') ?? ''),
      bookedAt: String(read('bookedAt', 'BookedAt') ?? ''),
      expiresAt: String(read('expiresAt', 'ExpiresAt') ?? ''),
      usedAt: (read('usedAt', 'UsedAt') as string | null | undefined) ?? null
    };
  }

  createBooking(data: Record<string, unknown>): Observable<{ tokenCode: string }> {
    return this.http.post<{ tokenCode: string }>(`${this.apiUrl}/create`, data, { headers: this.getHeaders() });
  }

  verifyKyc(body: {
    documentType: string;
    pan?: string;
    aadhaar?: string;
    fullName?: string;
  }): Observable<{
    verified: boolean;
    kycSessionId: string;
    referenceId: string;
  }> {
    return this.http.post<{
      verified: boolean;
      kycSessionId: string;
      referenceId: string;
    }>(`${this.apiUrl}/verify-kyc`, body, { headers: this.getHeaders() });
  }

  validateToken(tokenCode: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/validate`, { tokenCode }, { headers: this.getHeaders() });
  }

  confirmBooking(tokenCode: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/confirm`, { tokenCode }, { headers: this.getHeaders() });
  }

  getMyBookings(stationId?: string | null): Observable<BookingRow[]> {
    const url =
      stationId && stationId.trim()
        ? `${this.apiUrl}/my-bookings?stationId=${encodeURIComponent(stationId.trim())}`
        : `${this.apiUrl}/my-bookings`;
    return this.http.get<unknown[]>(url, { headers: this.getHeaders() }).pipe(
      map((rows) =>
        (rows ?? []).map((r) => this.normalizeBookingRow((r ?? {}) as Record<string, unknown>))
      )
    );
  }

  /** Dealer only: bookings for one assigned station. */
  getStationBookings(stationId: string): Observable<BookingRow[]> {
    return this.http.get<unknown[]>(
      `${this.apiUrl}/station/${encodeURIComponent(stationId)}/bookings`,
      { headers: this.getHeaders() }
    ).pipe(
      map((rows) =>
        (rows ?? []).map((r) => this.normalizeBookingRow((r ?? {}) as Record<string, unknown>))
      )
    );
  }

  cancelBooking(tokenCode: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/cancel`, { tokenCode }, { headers: this.getHeaders() });
  }
}
