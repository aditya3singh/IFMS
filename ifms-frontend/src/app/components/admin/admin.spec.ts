import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminComponent } from './admin.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

describe('AdminComponent', () => {
  let component: AdminComponent;
  let fixture: ComponentFixture<AdminComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminComponent, HttpClientTestingModule, RouterTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with overview tab', () => {
    expect(component.activeTab).toBe('overview');
  });

  it('should switch tabs', () => {
    component.setTab('report');
    expect(component.activeTab).toBe('report');
    component.setTab('fraud');
    expect(component.activeTab).toBe('fraud');
  });

  it('should have correct initial date', () => {
    const today = new Date().toISOString().split('T')[0];
    expect(component.reportDate).toBe(today);
  });

  it('should start with no fraud alerts', () => {
    expect(component.fraudAlerts).toBeNull();
  });

  it('should start with no overview data', () => {
    expect(component.overview).toBeNull();
  });
});
