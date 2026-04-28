import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = 'http://localhost:5010/gateway/inventory';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAllStock(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl, { headers: this.getHeaders() });
  }

  getStockByStation(stationId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/station/${encodeURIComponent(stationId)}`, { headers: this.getHeaders() });
  }

  createStock(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.getHeaders() });
  }

  updateStock(id: string, newQuantity: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-stock`, 
      { id, newQuantity }, 
      { headers: this.getHeaders() });
  }
}