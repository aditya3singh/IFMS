import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StationService {
  private readonly apiUrl = environment.stationApiUrl;
  private readonly fuelQuoteUrl = `${environment.stationApiUrl}/fuel-price-quote`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return token
      ? new HttpHeaders({ 'Authorization': `Bearer ${token}` })
      : new HttpHeaders();
  }

  /** Public endpoint — no auth required, but token sent if available */
  getAllStations(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  /** Public endpoint — no auth required */
  getStationById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  /** Admin: all stations. Dealer: only assigned stations. Requires Bearer token. */
  getMyStations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/mine`, { headers: this.getHeaders() });
  }

  /** Area-based retail quote (state + city + fuel type). Public fallback. */
  getFuelPriceQuote(state: string, city: string, fuelType: string): Observable<{
    pricePerUnit: number;
    unitLabel: string;
    areaSummary: string;
    asOfUtc: string;
  }> {
    const params = new HttpParams()
      .set('state', state ?? '')
      .set('city', city ?? '')
      .set('fuelType', fuelType ?? '');
    return this.http.get<{
      pricePerUnit: number;
      unitLabel: string;
      areaSummary: string;
      asOfUtc: string;
    }>(this.fuelQuoteUrl, { params });
  }

  /** Quote from server using the station record (correct city/state for that outlet). */
  getFuelPriceQuoteForStation(stationId: string, fuelType: string): Observable<{
    pricePerUnit: number;
    unitLabel: string;
    areaSummary: string;
    asOfUtc: string;
  }> {
    const params = new HttpParams().set('fuelType', fuelType ?? '');
    return this.http.get<{
      pricePerUnit: number;
      unitLabel: string;
      areaSummary: string;
      asOfUtc: string;
    }>(`${this.apiUrl}/${encodeURIComponent(stationId)}/fuel-price-quote`, { params });
  }

  /** Admin only — requires auth */
  createStation(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.getHeaders() });
  }

  /** Admin only — requires auth */
  updateStation(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data, { headers: this.getHeaders() });
  }

  /** Admin only — requires auth */
  deleteStation(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  /** Admin only — requires auth */
  assignDealer(stationId: string, userId: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/${stationId}/assign-dealer`,
      { userId },
      { headers: this.getHeaders() }
    );
  }
}
