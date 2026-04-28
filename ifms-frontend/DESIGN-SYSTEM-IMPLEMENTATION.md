# IFMS Design System Implementation Guide

## Overview
This document provides a complete implementation guide for the Bharat Kinetic / FuelOps Industrial design system across all IFMS frontend components.

## Design System Foundation

### Already Configured ✅
- Tailwind CSS with Material Design 3 color tokens
- Google Fonts: Inter (body), Manrope (headlines)
- Material Symbols Outlined icons
- Custom utility classes for font families

### Color Palette (Material Design 3)
All colors are already configured in `tailwind.config.js`:
- Primary: `#001e40` (Deep Navy)
- Secondary: `#a04100` (Industrial Orange)
- Tertiary: `#00231e` (Dark Teal)
- Surface variations, containers, and semantic colors

## Component Implementation Status

### 1. Authentication Pages

#### Login Component (`/login`)
**Status**: ✅ HTML Created, TypeScript Updated
**File**: `src/app/components/login/login.html`
**Features**:
- Split-screen layout (branding left, form right)
- Reactive forms with validation
- Role-based navigation after login
- Material icons integration
- Responsive mobile view

#### Register Component (`/register`)
**Status**: ⚠️ Needs Update
**Current**: Material Design components
**Target**: Custom Tailwind design
**Files to Update**:
- `src/app/components/register/register.component.html`
- `src/app/components/register/register.component.ts`

**Implementation Steps**:
1. Replace Material components with Tailwind classes
2. Add FormBuilder reactive forms
3. Implement role selection dropdown
4. Add terms & conditions checkbox
5. Update TypeScript with proper validation

### 2. Customer Dashboard (`/customer-dashboard`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Side navigation bar (desktop)
- Top navigation bar (mobile/desktop)
- Bottom navigation bar (mobile only)
- Fuel price cards (Petrol, Diesel, CNG)
- Recent bookings table
- Nearby stations map widget
- "Refuel Now" CTA card

**Required Components**:
```
customer-dashboard/
├── customer-dashboard.html
├── customer-dashboard.ts
├── customer-dashboard.css
└── components/
    ├── side-nav/
    ├── top-nav/
    ├── bottom-nav/
    ├── fuel-price-card/
    ├── bookings-table/
    └── map-widget/
```

### 3. Booking Flow

#### Book Fuel Page (`/booking/new`)
**Status**: 🔴 Not Implemented
**Design Features**:
- 3-step wizard (Station → Fuel Type → Quantity)
- Station search with favorites
- Fuel type selector (radio cards)
- Quantity input with quick amounts
- Live price calculation
- Payment method selection
- Order summary sidebar

#### Booking Confirmation (`/booking/confirmation`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Success animation/icon
- QR code display
- Token ID (large, prominent)
- Booking details card
- Download receipt button
- Support contact info

### 4. Booking History (`/booking/history`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Search and filter controls
- Statistics cards (Total Liters, Active Tokens, Total Spent)
- Data table with status badges
- Pagination
- "Refuel Now" CTA banner

### 5. Dealer Dashboard (`/dashboard`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Command center layout
- KPI cards (Sales Today, Pending Bookings, Operational Pumps)
- Live inventory levels with progress bars
- Recent activity log table
- Stock alert widget
- Quick contacts sidebar

### 6. Token Validation Portal (`/dealer/validate`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Manual token input field
- QR scanner viewport (camera integration)
- Customer transaction details display
- Fuel volume progress indicator
- "Confirm & Refuel" button
- Today's validation log

### 7. Inventory Management (`/inventory`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Stock overview cards with visual levels
- Fuel type cards (Petrol, Diesel, CNG)
- Detailed inventory table
- Recent deliveries section
- Stock alert/forecast widget
- "Add Stock" modal

### 8. Admin Dashboard (`/admin`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Global KPI grid (Revenue, Fuel Sales, Active Stations, Fraud Alerts)
- 30-day revenue trend chart
- Fuel distribution pie chart
- Critical alerts feed
- Compliance score widgets

### 9. Station Management (`/admin/stations`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Search and filter controls
- Station data table
- Dealer assignment actions
- Status indicators
- Pagination
- Quick stats bento grid

### 10. Fraud Monitor (`/admin/fraud`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Threat velocity chart
- Audit assistant card
- Flagged transactions table
- Risk level badges
- Compliance metrics

### 11. Analytics & Reporting (`/admin/analytics`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Date range and filter controls
- Revenue trends line chart
- Payment method distribution
- Regional sales bar chart
- Transaction audit logs table
- Export functionality

### 12. Station Finder Map (`/stations`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Full-screen map view
- Station list sidebar
- Search and filters
- Map markers with tooltips
- Station detail cards
- Live fuel coverage legend

### 13. User Profile & Settings (`/profile`)
**Status**: 🔴 Not Implemented
**Design Features**:
- Account information card
- Profile photo upload
- Security status widget
- 2FA configuration
- Notification preferences matrix
- Active device sessions table

## Shared Components to Create

### Navigation Components

#### 1. SideNavBar Component
**Usage**: Desktop navigation for all authenticated pages
**Features**:
- Logo/branding
- User profile section
- Navigation links with icons
- Active state styling
- Settings/logout at bottom

**File Structure**:
```
shared/side-nav/
├── side-nav.component.html
├── side-nav.component.ts
├── side-nav.component.css
```

**Props**:
- `userRole`: 'customer' | 'dealer' | 'admin'
- `activeRoute`: string
- `userName`: string
- `userId`: string

#### 2. TopNavBar Component
**Usage**: Top bar for all pages
**Features**:
- Brand name
- Horizontal navigation (desktop)
- Search bar
- Notifications icon
- User avatar
- Action buttons

#### 3. BottomNavBar Component
**Usage**: Mobile navigation only
**Features**:
- 4-5 icon buttons
- Active state indicator
- Labels
- Floating action button (optional)

### UI Components

#### 1. FuelPriceCard Component
**Props**:
- `fuelType`: string
- `price`: number
- `unit`: string
- `trend`: 'up' | 'down' | 'stable'
- `trendValue`: number

#### 2. StatusBadge Component
**Props**:
- `status`: 'completed' | 'pending' | 'cancelled' | 'expired'
- `size`: 'sm' | 'md' | 'lg'

#### 3. KPICard Component
**Props**:
- `title`: string
- `value`: string | number
- `icon`: string
- `trend`: string (optional)
- `color`: 'primary' | 'secondary' | 'tertiary' | 'error'

## Implementation Priority

### Phase 1: Core Authentication & Navigation (Week 1)
1. ✅ Login page (DONE)
2. Update Register page
3. Create SideNavBar component
4. Create TopNavBar component
5. Create BottomNavBar component

### Phase 2: Customer Experience (Week 2)
1. Customer Dashboard
2. Book Fuel page
3. Booking Confirmation
4. Booking History
5. Station Finder Map

### Phase 3: Dealer Operations (Week 3)
1. Dealer Dashboard
2. Token Validation Portal
3. Inventory Management

### Phase 4: Admin & Analytics (Week 4)
1. Admin Dashboard
2. Station Management
3. Fraud Monitor
4. Analytics & Reporting
5. User Profile & Settings

## Technical Implementation Notes

### Routing Configuration
Update `app.routes.ts`:
```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: Login, canActivate: [GuestGuard] },
  { path: 'register', component: Register, canActivate: [GuestGuard] },
  { 
    path: 'customer-dashboard', 
    component: CustomerDashboard, 
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Customer'] }
  },
  // ... more routes
];
```

### State Management
Consider using Angular signals or RxJS for:
- User authentication state
- Current user profile
- Booking cart/state
- Real-time notifications

### API Integration
All components should use existing services:
- `AuthService` - authentication
- `BookingService` - fuel bookings
- `StationService` - station data
- `InventoryService` - inventory management
- `SalesService` - sales data

### Responsive Design
All components must support:
- Desktop (1920px+)
- Tablet (768px - 1919px)
- Mobile (320px - 767px)

Use Tailwind breakpoints:
- `sm:` - 640px
- `md:` - 768px
- `lg:` - 1024px
- `xl:` - 1280px

### Accessibility
- Proper ARIA labels
- Keyboard navigation
- Focus states
- Screen reader support
- Color contrast compliance

## Design Tokens Reference

### Typography
```css
/* Headlines */
font-family: 'Manrope', sans-serif;
font-weight: 700-800;

/* Body */
font-family: 'Inter', sans-serif;
font-weight: 400-600;

/* Labels */
font-family: 'Inter', sans-serif;
font-weight: 500-700;
text-transform: uppercase;
letter-spacing: 0.08em;
```

### Spacing Scale
- xs: 0.25rem (4px)
- sm: 0.5rem (8px)
- md: 1rem (16px)
- lg: 1.5rem (24px)
- xl: 2rem (32px)
- 2xl: 3rem (48px)

### Border Radius
- DEFAULT: 0.125rem (2px)
- lg: 0.25rem (4px)
- xl: 0.5rem (8px)
- full: 0.75rem (12px)

### Shadows
```css
/* Elevated cards */
box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04), 
            0 8px 24px rgba(15, 23, 42, 0.06);

/* Floating elements */
box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 
            0 2px 4px -1px rgba(0, 0, 0, 0.06);
```

## Next Steps

1. Review this implementation guide
2. Prioritize components based on business needs
3. Create component scaffolding using Angular CLI
4. Implement components following the design system
5. Test responsive behavior
6. Integrate with backend APIs
7. Conduct accessibility audit
8. Performance optimization

## Resources

- Design Files: See HTML templates in this document
- Tailwind Config: `tailwind.config.js`
- Global Styles: `src/styles.css`
- Material Icons: https://fonts.google.com/icons
- Color Palette: Material Design 3 tokens

---

**Last Updated**: January 2025
**Version**: 1.0.0
**Maintainer**: IFMS Development Team
