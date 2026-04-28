# Bharat Kinetic IFMS - Quick Reference Card

## 🚀 Quick Start

```bash
cd IFMS/ifms-frontend
npm install
npm start
```

Navigate to: `http://localhost:4200/dashboard`

## 📦 Generate New Component

```bash
ng g c components/[name] --standalone
```

Example:
```bash
ng g c components/fuel-booking --standalone
```

## 🎨 Design System Colors

### Primary Colors
```css
Primary: #001e40          /* Navy Blue */
Secondary: #a04100        /* Orange */
Tertiary: #00231e         /* Teal */
```

### Surface Colors
```css
Surface: #f9f9fd
Surface Container: #edeef1
Surface Container Low: #f3f3f7
Surface Container Lowest: #ffffff
```

### Tailwind Classes
```html
<div class="bg-primary text-on-primary">Primary</div>
<div class="bg-secondary text-on-secondary">Secondary</div>
<div class="bg-tertiary text-on-tertiary">Tertiary</div>
<div class="bg-surface text-on-surface">Surface</div>
```

## 🔤 Typography

### Fonts
```css
Headline: font-headline (Manrope)
Body: font-body (Inter)
Label: font-label (Inter)
```

### Usage
```html
<h1 class="font-headline text-4xl font-black">Headline</h1>
<p class="font-body text-base">Body text</p>
<label class="font-label text-sm font-semibold">Label</label>
```

## 🎯 Icons

### Material Symbols
```html
<span class="material-symbols-outlined">dashboard</span>
<span class="material-symbols-outlined">local_gas_station</span>
<span class="material-symbols-outlined">notifications</span>
```

### Filled Icons
```html
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1;">
  home
</span>
```

## 📱 Responsive Breakpoints

```css
Mobile: < 768px       (sm:)
Tablet: 768px-1024px  (md:)
Desktop: > 1024px     (lg:, xl:)
```

### Usage
```html
<div class="hidden lg:flex">Desktop only</div>
<div class="lg:hidden">Mobile/Tablet only</div>
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
  Responsive grid
</div>
```

## 🧩 Component Template

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-[name]',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './[name].component.html',
  styleUrls: ['./[name].component.css']
})
export class [Name]Component implements OnInit {
  
  constructor() {}

  ngOnInit(): void {
    // Initialize component
  }
}
```

## 🔌 Service Template

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class [Name]Service {
  private apiUrl = `${environment.apiUrl}/[endpoint]`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  create(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  update(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

## 🛣️ Add Route

In `app.routes.ts`:

```typescript
import { Routes } from '@angular/router';
import { [Name]Component } from './components/[name]/[name].component';

export const routes: Routes = [
  { path: '[path]', component: [Name]Component },
  // Add more routes...
];
```

## 🎨 Common UI Patterns

### Button
```html
<button class="bg-primary text-on-primary px-6 py-3 rounded-lg font-bold 
               hover:opacity-90 active:scale-95 transition-all">
  Click Me
</button>
```

### Card
```html
<div class="bg-surface-container-lowest p-6 rounded-xl shadow-sm 
            border border-outline-variant/10">
  Card content
</div>
```

### Input
```html
<input class="w-full px-4 py-3 bg-surface-container-highest 
              border-none rounded-lg focus:ring-2 focus:ring-primary"
       type="text" placeholder="Enter text">
```

### Badge
```html
<span class="bg-tertiary text-on-tertiary-container px-3 py-1 
             rounded-sm text-xs font-bold uppercase">
  Active
</span>
```

### Table
```html
<table class="w-full text-left border-collapse">
  <thead class="bg-surface-container-low text-on-surface-variant 
                text-xs uppercase tracking-widest">
    <tr>
      <th class="px-8 py-4 font-bold">Header</th>
    </tr>
  </thead>
  <tbody class="text-sm text-on-surface">
    <tr class="hover:bg-surface-container-low/50 transition-colors">
      <td class="px-8 py-6">Data</td>
    </tr>
  </tbody>
</table>
```

## 🔄 Angular Directives

### *ngFor
```html
<div *ngFor="let item of items; trackBy: trackById">
  {{ item.name }}
</div>
```

### *ngIf
```html
<div *ngIf="isVisible">Visible content</div>
<div *ngIf="isVisible; else elseBlock">Content</div>
<ng-template #elseBlock>Else content</ng-template>
```

### [ngClass]
```html
<div [ngClass]="{
  'bg-primary': isPrimary,
  'bg-secondary': isSecondary,
  'text-white': isLight
}">
  Dynamic classes
</div>
```

### [ngStyle]
```html
<div [ngStyle]="{
  'width': width + 'px',
  'background-color': color
}">
  Dynamic styles
</div>
```

## 🔗 Router Navigation

### Template
```html
<a routerLink="/dashboard">Dashboard</a>
<a [routerLink]="['/booking', bookingId]">Booking</a>
<button (click)="navigate()">Go</button>
```

### Component
```typescript
import { Router } from '@angular/router';

constructor(private router: Router) {}

navigate(): void {
  this.router.navigate(['/dashboard']);
}

navigateWithParams(): void {
  this.router.navigate(['/booking', id], {
    queryParams: { filter: 'active' }
  });
}
```

## 📡 HTTP Requests

### GET
```typescript
this.http.get<Type>('/api/endpoint').subscribe({
  next: (data) => console.log(data),
  error: (error) => console.error(error)
});
```

### POST
```typescript
this.http.post<Type>('/api/endpoint', body).subscribe({
  next: (response) => console.log(response),
  error: (error) => console.error(error)
});
```

### With Headers
```typescript
const headers = new HttpHeaders({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${token}`
});

this.http.get('/api/endpoint', { headers }).subscribe(...);
```

## 🎭 Lifecycle Hooks

```typescript
ngOnInit(): void {
  // Component initialized
}

ngOnDestroy(): void {
  // Cleanup subscriptions
}

ngOnChanges(changes: SimpleChanges): void {
  // Input properties changed
}

ngAfterViewInit(): void {
  // View initialized
}
```

## 🧪 Testing

### Run Tests
```bash
npm test
```

### Test Template
```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { [Name]Component } from './[name].component';

describe('[Name]Component', () => {
  let component: [Name]Component;
  let fixture: ComponentFixture<[Name]Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [[Name]Component]
    }).compileComponents();

    fixture = TestBed.createComponent([Name]Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

## 🏗️ Build Commands

```bash
# Development
npm start

# Production build
npm run build

# Watch mode
npm run watch

# Run tests
npm test

# Lint
ng lint

# Format
npx prettier --write "src/**/*.{ts,html,css}"
```

## 🐛 Debug Tips

### Console Logging
```typescript
console.log('Value:', value);
console.table(array);
console.error('Error:', error);
```

### Angular DevTools
Install Chrome extension: Angular DevTools

### Check Component State
```html
<pre>{{ data | json }}</pre>
```

## 📚 Useful Commands

```bash
# Generate service
ng g s services/[name]

# Generate guard
ng g guard guards/[name]

# Generate interceptor
ng g interceptor interceptors/[name]

# Generate interface
ng g interface models/[name]

# Generate pipe
ng g pipe pipes/[name]

# Generate directive
ng g directive directives/[name]
```

## 🔐 Environment Variables

### Development
`src/app/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5010/api'
};
```

### Production
`src/app/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.bharatkinetic.com'
};
```

## 🎯 Common Tasks

### Add New Page
1. Generate component: `ng g c components/[name] --standalone`
2. Add route in `app.routes.ts`
3. Add navigation link in sidebar/navbar
4. Implement component logic
5. Connect to API service

### Add New API Endpoint
1. Create/update service
2. Define interface for response
3. Add method to service
4. Call from component
5. Handle loading/error states

### Style Component
1. Use Tailwind utility classes
2. Add custom CSS in component.css if needed
3. Follow design system tokens
4. Test responsive behavior

## 📞 Quick Links

- [Angular Docs](https://angular.io/docs)
- [Tailwind Docs](https://tailwindcss.com/docs)
- [Material Design 3](https://m3.material.io/)
- [RxJS Docs](https://rxjs.dev/)

---

**Keep this handy while developing! 📌**
