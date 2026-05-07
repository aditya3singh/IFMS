import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SalesService {
  private apiUrl = 'http://localhost:5010/gateway/sales';
  private adminUrl = 'http://localhost:5010/gateway/admin';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAllTransactions(): Observable<any[]> {
    return this.http.get<any>(this.apiUrl, { headers: this.getHeaders() })
      .pipe(
        map((response: any) => {
          // Handle paginated response from API
          if (response && response.data && Array.isArray(response.data)) {
            return response.data;
          }
          // Fallback for non-paginated response
          return Array.isArray(response) ? response : [];
        })
      );
  }

  createTransaction(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.getHeaders() });
  }

  getOverview(): Observable<any> {
    return this.http.get(`${this.adminUrl}/overview`, { headers: this.getHeaders() });
  }

  /** Dealer-scoped summary: total transactions + revenue for their assigned stations. */
  getDealerSummary(): Observable<any> {
    return this.http.get(`${this.apiUrl}/summary`, { headers: this.getHeaders() });
  }

  getDailyReport(date: string): Observable<any> {
    return this.http.get(`${this.adminUrl}/daily-report?date=${date}`, 
      { headers: this.getHeaders() });
  }

  getStationMonitor(): Observable<{ totalStations: number; rows: any[] }> {
    return this.http.get<{ totalStations: number; rows: any[] }>(
      `${this.adminUrl}/station-monitor`,
      { headers: this.getHeaders() }
    );
  }
}