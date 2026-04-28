import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { StationService } from './station.service';

describe('StationService', () => {
  let service: StationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [StationService]
    });
    service = TestBed.inject(StationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getAllStations', () => {
    const mockStations = [{ id: '1', name: 'HP Bandra' }];
    service.getAllStations().subscribe(res => {
      expect(res.length).toBe(1);
      expect(res[0].name).toBe('HP Bandra');
    });
    const req = httpMock.expectOne('http://localhost:5006/api/Stations');
    expect(req.request.method).toBe('GET');
    req.flush(mockStations);
  });

  it('should call getStationById', () => {
    service.getStationById('abc-123').subscribe();
    const req = httpMock.expectOne('http://localhost:5006/api/Stations/abc-123');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call createStation', () => {
    service.createStation({ name: 'Test' }).subscribe();
    const req = httpMock.expectOne('http://localhost:5006/api/Stations');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call updateStation', () => {
    service.updateStation('abc-123', { name: 'Updated' }).subscribe();
    const req = httpMock.expectOne('http://localhost:5006/api/Stations/abc-123');
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });
});
