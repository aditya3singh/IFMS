import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

export interface RealtimeFuelPrice {
  state: string;
  district: string;
  prices: {
    petrol: number;
    diesel: number;
    cng: number | null;
  };
  currency: string;
  fetchedAt: string;
  source: string;
  note: string;
}

export interface StationRealtimePrice {
  stationId: string;
  stationName: string;
  location: {
    state: string;
    city: string;
  };
  prices: {
    petrol: number;
    diesel: number;
    cng: number | null;
  };
  currency: string;
  fetchedAt: string;
  source: string;
  note: string;
}

@Injectable({
  providedIn: 'root'
})
export class FuelPriceService {
  private readonly apiUrl = 'http://localhost:5010/gateway/stations';

  constructor(private http: HttpClient) {}

  /**
   * Get real-time fuel prices for a specific location
   */
  getRealtimeFuelPrice(state: string, district: string, fuelType: string = 'Petrol'): Observable<RealtimeFuelPrice | null> {
    const params = { state, district, fuelType };
    return this.http.get<RealtimeFuelPrice>(`${this.apiUrl}/realtime-price`, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching realtime fuel price:', error);
          return of(null);
        })
      );
  }

  /**
   * Get real-time fuel prices for a station
   */
  getStationRealtimePrice(stationId: string): Observable<StationRealtimePrice | null> {
    return this.http.get<StationRealtimePrice>(`${this.apiUrl}/${stationId}/realtime-price`)
      .pipe(
        catchError(error => {
          console.error('Error fetching station realtime price:', error);
          return of(null);
        })
      );
  }

  /**
   * Get list of available states
   */
  getAvailableStates(): Observable<string[]> {
    return this.http.get<{ states: string[]; count: number }>(`${this.apiUrl}/available-states`)
      .pipe(
        map(response => response.states),
        catchError(error => {
          console.error('Error fetching available states:', error);
          return of([]);
        })
      );
  }

  /**
   * Get list of districts in a state
   */
  getDistricts(state: string): Observable<string[]> {
    return this.http.get<{ state: string; districts: string[]; count: number }>(`${this.apiUrl}/districts/${state}`)
      .pipe(
        map(response => response.districts),
        catchError(error => {
          console.error('Error fetching districts:', error);
          return of([]);
        })
      );
  }

  /**
   * Get all fuel prices for a state
   */
  getStatePrices(state: string): Observable<any[]> {
    return this.http.get<any>(`${this.apiUrl}/state-prices/${state}`)
      .pipe(
        map(response => response.districts || []),
        catchError(error => {
          console.error('Error fetching state prices:', error);
          return of([]);
        })
      );
  }

  /**
   * Format price for display
   */
  formatPrice(price: number | null | undefined): string {
    if (price == null) return 'N/A';
    return `₹${price.toFixed(2)}`;
  }

  /**
   * Get price by fuel type
   */
  getPriceByType(prices: { petrol: number; diesel: number; cng: number | null }, fuelType: string): number | null {
    const type = fuelType.toLowerCase();
    if (type === 'petrol') return prices.petrol;
    if (type === 'diesel') return prices.diesel;
    if (type === 'cng') return prices.cng;
    return null;
  }
}
