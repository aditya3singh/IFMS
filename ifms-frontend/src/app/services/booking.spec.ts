import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BookingService } from './booking.service';

describe('BookingService', () => {
  let service: BookingService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookingService]
    });
    service = TestBed.inject(BookingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call createBooking', () => {
    const mockData = { tokenCode: 'IFM-01-12345678' };
    service.createBooking({ stationId: 'abc' }).subscribe(res => {
      expect(res.tokenCode).toBe('IFM-01-12345678');
    });
    const req = httpMock.expectOne('http://localhost:5007/api/booking/create');
    expect(req.request.method).toBe('POST');
    req.flush(mockData);
  });

  it('should call validateToken', () => {
    service.validateToken('IFM-01-12345678').subscribe();
    const req = httpMock.expectOne('http://localhost:5007/api/booking/validate');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call confirmBooking', () => {
    service.confirmBooking('IFM-01-12345678').subscribe();
    const req = httpMock.expectOne('http://localhost:5007/api/booking/confirm');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call cancelBooking', () => {
    service.cancelBooking('IFM-01-12345678').subscribe();
    const req = httpMock.expectOne('http://localhost:5007/api/booking/cancel');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getMyBookings', () => {
    service.getMyBookings().subscribe(res => {
      expect(res.length).toBe(0);
    });
    const req = httpMock.expectOne('http://localhost:5007/api/booking/my-bookings');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
