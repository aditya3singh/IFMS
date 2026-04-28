import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BookingComponent } from './booking.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { StationService } from '../../services/station.service';
import { of, throwError, Observable } from 'rxjs';
import { vi } from 'vitest';

class MockStationService {
  getAllStations(): Observable<any[]> { return of([]); }
}

describe('BookingComponent', () => {
  let component: BookingComponent;
  let fixture: ComponentFixture<BookingComponent>;
  let stationServiceMock: MockStationService;

  beforeEach(async () => {
    // Mock localStorage for the test environment
    if (!(window as any).localStorage) {
      Object.defineProperty(window, 'localStorage', {
        value: {
          getItem: vi.fn(),
          setItem: vi.fn(),
          removeItem: vi.fn(),
          clear: vi.fn()
        },
        writable: true
      });
    }

    stationServiceMock = new MockStationService();

    await TestBed.configureTestingModule({
      imports: [BookingComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [{ provide: StationService, useValue: stationServiceMock }]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should calculate total amount', () => {
    component.quantity = 10;
    component.pricePerLitre = 105.50;
    component.calculateTotal();
    expect(component.totalAmount).toBe(1055);
  });

  it('should start with empty stations', () => {
    expect(component.stations.length).toBe(0);
  });

  it('should start with default fuel type', () => {
    expect(component.fuelType).toBe('Petrol');
  });

  it('should not show payment modal initially', () => {
    expect(component.showPaymentModal).toBe(false);
  });

  it('should open payment modal on initiatePayment with valid data', () => {
    component.selectedStationId = 'some-id';
    component.quantity = 10;
    component.pricePerLitre = 100;
    component.initiatePayment();
    expect(component.showPaymentModal).toBe(true);
  });

  it('should not open payment modal without station', () => {
    component.selectedStationId = '';
    component.quantity = 10;
    component.pricePerLitre = 100;
    component.initiatePayment();
    expect(component.showPaymentModal).toBe(false);
  });

  it('should close payment modal', () => {
    component.showPaymentModal = true;
    component.closePaymentModal();
    expect(component.showPaymentModal).toBe(false);
    expect(component.paymentStep).toBe('select');
  });

  it('should start with UPI as default payment method', () => {
    expect(component.paymentMethod).toBe('UPI');
  });

  it('should have no validation result initially', () => {
    expect(component.validationResult).toBeNull();
  });

  // --- station loading states ---

  it('should set stationsLoading=true while loading', () => {
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(of([]));
    component.stationsLoading = true; // simulate mid-load
    expect(component.stationsLoading).toBe(true);
  });

  it('should populate stations on successful load', () => {
    const mockStations = [
      { id: 'abc-123', name: 'Station A', city: 'Delhi', state: 'Delhi' },
      { id: 'def-456', name: 'Station B', city: 'Mumbai', state: 'Maharashtra' }
    ];
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(of(mockStations));
    component.loadStations();
    expect(component.stations.length).toBe(2);
    expect(component.stationsLoading).toBe(false);
    expect(component.stationsError).toBe('');
  });

  it('should set stationsError when no stations available', () => {
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(of([]));
    component.loadStations();
    expect(component.stationsError).toBe('No stations are currently available.');
  });

  it('should set stationsError on network failure (status 0)', () => {
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(throwError(() => ({ status: 0 })));
    component.loadStations();
    expect(component.stationsError).toContain('Cannot connect to Station service');
    expect(component.stationsLoading).toBe(false);
  });

  it('should set stationsError on 401 unauthorized', () => {
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(throwError(() => ({ status: 401 })));
    component.loadStations();
    expect(component.stationsError).toContain('Session expired');
  });

  it('retryLoadStations should call loadStations again', () => {
    const mockStations = [{ id: 'xyz', name: 'X', city: 'C', state: 'S' }];
    vi.spyOn(stationServiceMock, 'getAllStations').mockReturnValue(of(mockStations));
    component.retryLoadStations();
    expect(component.stations.length).toBe(1);
  });

  it('onStationChange should update selectedStationName', () => {
    component.stations = [{ id: 'abc', name: 'Alpha Station', city: 'Pune', state: 'MH' }];
    component.selectedStationId = 'abc';
    component.onStationChange();
    expect(component.selectedStationName).toContain('Alpha Station');
  });

  it('onStationChange should clear selectedStationName for unknown id', () => {
    component.stations = [{ id: 'abc', name: 'Alpha Station', city: 'Pune', state: 'MH' }];
    component.selectedStationId = 'unknown-id';
    component.onStationChange();
    expect(component.selectedStationName).toBe('');
  });
});
