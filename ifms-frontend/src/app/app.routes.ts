import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { RegisterLandingComponent } from './components/register/register-landing.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { InventoryComponent } from './components/inventory/inventory.component';
import { SalesComponent } from './components/sales/sales.component';
import { AdminComponent } from './components/admin/admin.component';
import { BookingComponent } from './components/booking/booking.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { StationFinderComponent } from './components/station-finder/station-finder.component';
import { LandingComponent } from './components/landing/landing.component';
import { StaffComponent } from './components/staff/staff.component';
import { FeedbackComponent } from './components/feedback/feedback.component';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';
import { createRoleGuard } from './guards/role.guard';

const dealerOrAdmin = createRoleGuard(['Dealer', 'Admin']);
const adminOnly = createRoleGuard(['Admin']);

export const routes: Routes = [
  { path: '', component: LandingComponent, canActivate: [guestGuard] },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterLandingComponent, canActivate: [guestGuard] },
  { path: 'register/signup', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'inventory', component: InventoryComponent, canActivate: [authGuard, dealerOrAdmin] },
  { path: 'sales', component: SalesComponent, canActivate: [authGuard, dealerOrAdmin] },
  { path: 'admin', component: AdminComponent, canActivate: [authGuard, adminOnly] },
  { path: 'booking', component: BookingComponent, canActivate: [authGuard] },
  { path: 'station-finder', component: StationFinderComponent, canActivate: [authGuard] },
  { path: 'staff', component: StaffComponent, canActivate: [authGuard, dealerOrAdmin] },
  { path: 'feedback', component: FeedbackComponent, canActivate: [authGuard] },
];
