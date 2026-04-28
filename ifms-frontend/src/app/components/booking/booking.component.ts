import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BookingService } from '../../services/booking.service';
import { StationService } from '../../services/station.service';
import { AuthService } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ComplaintService } from '../../services/complaint.service';
import { cleanAadhaarDigits, isValidAadhaarFormat, isValidPanFormat, normalizePan } from '../../utils/kyc-format.util';
import * as QRCode from 'qrcode';
import { PaginationComponent } from '../../shared/pagination.component';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  // Expose Math for template quick-fill buttons
  readonly Math = Math;
  private readonly authApiUrl = 'http://localhost:5010/gateway/auth';

  // User info
  role = typeof localStorage !== 'undefined' && localStorage.getItem ? localStorage.getItem('role') : null;

  /** Controls which section is visible: booking form, history panel, support, or settings */
  activeView: 'booking' | 'history' | 'support' | 'settings' = 'booking';

  // ── Support / Complaints ──────────────────────────────────────────────────
  supportTab: 'raise' | 'track' | 'faq' = 'raise';

  complaintForm = {
    category: 'FuelQuality',
    subject: '',
    description: '',
    referenceId: ''
  };
  complaintSubmitting = false;
  complaintSuccess = '';
  complaintError = '';

  myComplaints: any[] = [];
  complaintsLoading = false;
  complaintsError = '';

  readonly complaintCategories = [
    { value: 'FuelQuality',     label: 'Fuel Quality Issue',     icon: 'science',       desc: 'Adulteration, wrong grade, odour' },
    { value: 'QuantityDispute', label: 'Quantity Dispute',       icon: 'scale',         desc: 'Received less than booked' },
    { value: 'PaymentIssue',    label: 'Payment / Refund Issue', icon: 'payments',      desc: 'Amount debited, booking not created' },
    { value: 'BookingFailed',   label: 'Booking Failed',         icon: 'error_outline', desc: 'Token not received after payment' },
    { value: 'Other',           label: 'Other',                  icon: 'help_outline',  desc: 'Any other issue' }
  ];

  readonly faqs = [
    {
      q: 'How do I book fuel?',
      a: 'Go to Station Finder, select a station and click "Book". Choose fuel type and quantity, complete KYC verification, then proceed to payment. Your token will be sent via SMS and email.'
    },
    {
      q: 'How do I use my token code at the pump?',
      a: 'Visit the station and show your token code to the dealer, or let them scan the QR code. The dealer will confirm and dispense your fuel.'
    },
    {
      q: 'How long is a token valid?',
      a: 'Each token is valid for 24 hours from the time of booking. Unused tokens expire automatically after that.'
    },
    {
      q: 'How do I cancel a booking?',
      a: 'Go to Booking History, find the booking with PENDING status, and click the "Cancel" button. Only PENDING bookings can be cancelled.'
    },
    {
      q: 'Payment was deducted but I did not receive a booking?',
      a: 'Raise a complaint under the "Payment / Refund Issue" category. Include your transaction ID or payment reference. We will resolve it within 24–48 hours.'
    },
    {
      q: 'What payment methods are accepted?',
      a: 'UPI (GPay, PhonePe, Paytm), Credit/Debit Cards (Visa, Mastercard, RuPay), and Net Banking — all major Indian banks.'
    },
    {
      q: 'When will I receive my refund?',
      a: 'Refunds for cancelled bookings are processed within 5–7 business days to the original payment method.'
    },
    {
      q: 'How do I check my complaint status?',
      a: 'Go to Support → "Track Status" tab. All your complaints and their current status are listed there.'
    }
  ];

  openFaqIndex: number | null = null;

  // ── Settings ──────────────────────────────────────────────────────────────
  settingsTab: 'profile' | 'password' | 'notifications' | 'fuel' = 'profile';

  // Profile form
  profileForm = { fullName: '', phoneNumber: '' };
  profileLoading = false;
  profileSaving = false;
  profileSuccess = '';
  profileError = '';

  // Password form
  passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
  passwordSaving = false;
  passwordSuccess = '';
  passwordError = '';
  showCurrentPwd = false;
  showNewPwd = false;

  // Notification preferences (localStorage)
  notifPrefs = {
    bookingConfirmSms:   true,
    bookingConfirmEmail: true,
    fuelDispensedSms:    true,
    paymentReceiptEmail: true,
    promotionalAlerts:   false
  };

  // Fuel preferences (localStorage)
  fuelPrefs = {
    defaultFuelType:    'Petrol',
    defaultPaymentMethod: 'UPI'
  };

  // Customer booking form
  stations: any[] = [];
  stationsLoading = false;
  stationsError = '';
  selectedStationId = '';
  selectedStationName = '';
  selectedStationNumber = 1;
  fuelType = 'Petrol';
  quantity = 0;
  pricePerLitre = 0;
  /** Display label from station pricing API: litre, kg, or kWh */
  priceUnitLabel = 'litre';
  fuelPriceAreaNote = '';
  fuelPriceLoading = false;
  fuelPriceError = '';
  totalAmount = 0;
  isBooking = false;
  bookingResult: any = null;
  myBookings: any[] = [];
  myBookingsLoading = false;
  myBookingsError = '';

  // Pagination — booking history
  myBookingPage = 1; myBookingPageSize = 8;
  get pagedMyBookings() { return this.myBookings.slice((this.myBookingPage-1)*this.myBookingPageSize, this.myBookingPage*this.myBookingPageSize); }
  get pendingBookingsCount() { return this.myBookings.filter(b => b.tokenStatus === 'PENDING').length; }
  get usedBookingsCount()    { return this.myBookings.filter(b => b.tokenStatus === 'USED').length; }
  get totalSpentAmount()     { return this.myBookings.filter(b => b.tokenStatus === 'USED').reduce((s, b) => s + (b.totalPaid ?? 0), 0); }

  // Razorpay mock flow
  showPaymentModal = false;
  paymentStep: 'select' | 'processing' | 'done' = 'select';
  paymentMethod = 'UPI';

  // KYC: customer chooses PAN or Aadhaar (one document) before payment
  kycDocumentType: 'Pan' | 'Aadhaar' = 'Pan';
  panNumber = '';
  aadhaarNumber = '';
  kycSessionId: string | null = null;
  kycVerified = false;
  kycVerifying = false;
  kycError = '';

  // Contact details for notification delivery
  contactPhone = '';
  contactEmail = '';
  contactName  = '';

  // Success popup
  showSuccessPopup = false;
  successPopupData: { tokenCode: string; stationName: string; fuelType: string; quantity: number; totalPaid: number; expiresAt: string } | null = null;

  // Dealer validation
  tokenInput = '';
  validationResult: any = null;
  isValidating = false;
  confirmMessage = '';

  errorMessage = '';
  successMessage = '';
  /** Dealer-only UI state for token validation chips */
  dealerTokenState: 'idle' | 'verified' | 'invalid' | 'expired' | 'used' | 'cancelled' | 'confirmed' = 'idle';
  /** No longer used for inventory errors — inventory is handled server-side */
  dealerInventoryNote = '';

  @ViewChild('qrCanvas', { static: false }) qrCanvas!: ElementRef;

  constructor(
    private bookingService: BookingService,
    private stationService: StationService,
    private authService: AuthService,
    private inventoryService: InventoryService,
    private complaintService: ComplaintService,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Admin should not access booking/token portal - redirect to admin dashboard
    if (this.role === 'Admin') {
      void this.router.navigate(['/admin']);
      return;
    }
    if (this.role === 'Customer') {
      // Pre-fill contact details from stored profile
      this.contactEmail = localStorage.getItem('email') ?? '';
      this.contactName  = this.authService.getFullName() ?? '';
      this.contactPhone = localStorage.getItem('phone') ?? '';

      // Apply saved fuel preferences
      const savedFuelPrefs = localStorage.getItem('ifms_fuel_prefs');
      if (savedFuelPrefs) {
        const fp = JSON.parse(savedFuelPrefs);
        if (fp.defaultFuelType) this.fuelType = fp.defaultFuelType;
        this.fuelPrefs = { ...this.fuelPrefs, ...fp };
      }
      const savedNotifPrefs = localStorage.getItem('ifms_notif_prefs');
      if (savedNotifPrefs) this.notifPrefs = { ...this.notifPrefs, ...JSON.parse(savedNotifPrefs) };
      this.loadStations();
      this.loadMyBookings();

      // Read query params: ?view=history or ?view=support or ?stationId=xxx
      this.route.queryParams.subscribe(params => {
        if (params['view'] === 'history') {
          this.activeView = 'history';
        } else if (params['view'] === 'support') {
          this.openSupport('raise');
        } else if (params['view'] === 'settings') {
          this.openSettings('profile');
        }
        if (params['stationId']) {
          // Pre-select station once stations are loaded
          const preselect = params['stationId'] as string;
          this.activeView = 'booking';
          // Wait for stations to load then select
          const trySelect = () => {
            const found = this.stations.find(s => s.id === preselect);
            if (found) {
              this.selectedStationId = found.id;
              this.onStationChange();
            } else if (this.stationsLoading) {
              setTimeout(trySelect, 100);
            }
          };
          setTimeout(trySelect, 100);
        }
      });
    }
  }

  loadStations() {
    this.stationsLoading = true;
    this.stationsError = '';
    this.stationService.getAllStations().subscribe({
      next: (data) => {
        this.stations = (data ?? []).map((raw: Record<string, unknown>) => this.normalizeStation(raw));
        this.stationsLoading = false;
        if (data.length === 0) {
          this.stationsError = 'No stations are currently available.';
        }
      },
      error: (err) => {
        console.error('Failed to load stations', err);
        this.stationsLoading = false;
        if (err.status === 0) {
          this.stationsError = 'Cannot connect to Station service. Please ensure the backend is running.';
        } else if (err.status === 401) {
          this.stationsError = 'Session expired. Please log in again.';
        } else {
          this.stationsError = `Failed to load stations (${err.status || 'Network Error'}). Please retry.`;
        }
      }
    });
  }

  retryLoadStations() {
    this.loadStations();
  }

  /** Support both camelCase and PascalCase API payloads. */
  private normalizeStation(raw: Record<string, unknown>): {
    id: string;
    name: string;
    city: string;
    state: string;
  } {
    return {
      id: String(raw['id'] ?? raw['Id'] ?? ''),
      name: String(raw['name'] ?? raw['Name'] ?? ''),
      city: String(raw['city'] ?? raw['City'] ?? ''),
      state: String(raw['state'] ?? raw['State'] ?? '')
    };
  }

  onStationChange() {
    const found = this.stations.find((s) => s.id === this.selectedStationId);
    this.selectedStationName = found ? `${found.name} — ${found.city}` : '';
    const idx = this.stations.findIndex((s) => s.id === this.selectedStationId);
    this.selectedStationNumber = idx >= 0 ? Math.min(99, idx + 1) : 1;
    this.refreshFuelPriceQuote();
  }

  onFuelTypeChange() {
    this.refreshFuelPriceQuote();
  }

  /** Fetches area-based unit price when station and fuel type are known. */
  refreshFuelPriceQuote() {
    this.fuelPriceError = '';
    if (!this.selectedStationId || !this.stations.length) {
      return;
    }
    const s = this.stations.find((x) => x.id === this.selectedStationId);
    if (!s) return;

    this.fuelPriceLoading = true;
    // Quote by city/state from the same list row (matches regional pricing); avoids 404 when DB id differs from list source.
    this.stationService.getFuelPriceQuote(s.state ?? '', s.city ?? '', this.fuelType).subscribe({
      next: (q) => {
        this.pricePerLitre = q.pricePerUnit;
        this.priceUnitLabel = q.unitLabel || 'litre';
        this.fuelPriceAreaNote = q.areaSummary || '';
        this.fuelPriceLoading = false;
        this.fuelPriceError = '';
        this.calculateTotal();
      },
      error: (err) => {
        console.error('Fuel price quote failed', err);
        this.fuelPriceLoading = false;
        this.fuelPriceError =
          err.status === 0
            ? 'Could not load area price (station service unreachable). Enter price manually.'
            : 'Could not load area price. Enter price manually.';
      }
    });
  }

  onKycFieldsChange() {
    this.kycVerified = false;
    this.kycSessionId = null;
    this.kycError = '';
  }

  onKycDocumentTypeChange() {
    this.onKycFieldsChange();
  }

  canSubmitKyc(): boolean {
    if (this.kycDocumentType === 'Pan') {
      return !!this.panNumber?.trim();
    }
    return !!this.aadhaarNumber?.trim();
  }

  verifyIdentity() {
    this.kycError = '';
    this.successMessage = '';
    if (this.kycDocumentType === 'Pan') {
      if (!isValidPanFormat(this.panNumber)) {
        this.kycError = 'Enter a valid PAN (5 letters, 4 digits, 1 letter — e.g. ABCDE1234F).';
        return;
      }
    } else {
      if (!isValidAadhaarFormat(this.aadhaarNumber)) {
        this.kycError =
          'Enter a valid 12-digit Aadhaar (first digit 2–9, Verhoeff checksum must pass). Spaces or dashes are OK.';
        return;
      }
    }

    const fullName = this.authService.getFullName() ?? undefined;
    this.kycVerifying = true;
    const body =
      this.kycDocumentType === 'Pan'
        ? {
            documentType: 'Pan',
            pan: normalizePan(this.panNumber),
            fullName: fullName || undefined
          }
        : {
            documentType: 'Aadhaar',
            aadhaar: cleanAadhaarDigits(this.aadhaarNumber),
            fullName: fullName || undefined
          };
    this.bookingService.verifyKyc(body)
      .subscribe({
        next: (r) => {
          this.kycVerifying = false;
          if (r.verified && r.kycSessionId) {
            this.kycSessionId = r.kycSessionId;
            this.kycVerified = true;
            this.successMessage = 'Identity verified. You can continue to payment.';
          }
        },
        error: (err: { error?: { error?: string; flags?: string[] } }) => {
          this.kycVerifying = false;
          this.kycVerified = false;
          this.kycSessionId = null;
          const msg = err.error?.error ?? 'Verification failed.';
          const flags = err.error?.flags;
          this.kycError = flags?.length ? `${msg} (${flags.join(', ')})` : msg;
        }
      });
  }

  loadMyBookings() {
    this.myBookingsLoading = true;
    this.myBookingsError = '';
    this.bookingService.getMyBookings().subscribe({
      next: (data) => {
        this.myBookings = data;
        this.myBookingsLoading = false;
      },
      error: (err) => {
        console.error('Failed to load bookings', err);
        this.myBookingsLoading = false;
        this.myBookingsError =
          err.status === 0
            ? 'Cannot load history (booking service unreachable).'
            : 'Could not load your booking history. Try refresh.';
      }
    });
  }

  calculateTotal() {
    this.totalAmount = this.quantity * this.pricePerLitre;
  }

  // Razorpay mock flow
  initiatePayment() {
    if (!this.selectedStationId || this.quantity <= 0 || this.pricePerLitre <= 0) {
      this.errorMessage = 'Please fill all fields before proceeding to payment.';
      return;
    }
    if (!this.kycVerified || !this.kycSessionId) {
      this.errorMessage = 'Complete identity verification (PAN or Aadhaar) before payment.';
      return;
    }
    this.errorMessage = '';
    this.successMessage = '';
    this.showPaymentModal = true;
    this.paymentStep = 'select';
  }

  processPayment() {
    this.paymentStep = 'processing';

    // Simulate Razorpay processing delay
    setTimeout(() => {
      this.paymentStep = 'done';
      // Auto-create booking after payment
      this.createBooking();
    }, 2000);
  }

  closePaymentModal() {
    this.showPaymentModal = false;
    this.paymentStep = 'select';
  }

  createBooking() {
    this.errorMessage = '';
    this.successMessage = '';
    this.isBooking = true;

    const customerId = this.authService.getCustomerIdFromToken();
    if (!customerId) {
      this.errorMessage = 'Could not read your account. Please sign in again.';
      this.isBooking = false;
      this.showPaymentModal = false;
      return;
    }

    if (!this.kycSessionId) {
      this.errorMessage = 'Verification session missing. Verify identity again.';
      this.isBooking = false;
      this.showPaymentModal = false;
      return;
    }

    const bookingData = {
      customerId,
      stationId: this.selectedStationId,
      stationNumber: this.selectedStationNumber,
      fuelType: this.fuelType,
      quantityLiters: this.quantity,
      pricePerLitre: this.pricePerLitre,
      paymentId: `RZPAY-${this.paymentMethod}-${Date.now()}`,
      kycSessionId: this.kycSessionId,
      // Contact details for SMS + Email notification
      customerPhone: this.contactPhone.trim(),
      customerEmail: this.contactEmail.trim(),
      customerName:  this.contactName.trim(),
      stationName:   this.selectedStationName
    };

    this.bookingService.createBooking(bookingData).subscribe({
      next: (result: any) => {
        this.bookingResult = result;
        this.successMessage = `Booking created! Your token: ${result.tokenCode}`;
        this.isBooking = false;
        this.showPaymentModal = false;
        this.kycSessionId = null;
        this.kycVerified = false;
        this.loadMyBookings();

        // Show success popup
        this.successPopupData = {
          tokenCode:   result.tokenCode,
          stationName: this.selectedStationName,
          fuelType:    result.fuelType ?? this.fuelType,
          quantity:    result.quantityLiters ?? this.quantity,
          totalPaid:   result.totalPaid ?? this.totalAmount,
          expiresAt:   result.expiresAt ?? ''
        };
        this.showSuccessPopup = true;
        // Auto-dismiss after 8 seconds
        setTimeout(() => { this.showSuccessPopup = false; }, 8000);

        // Generate QR code after view updates
        setTimeout(() => this.generateQrCode(result.tokenCode), 100);
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Booking failed. Please try again.';
        this.isBooking = false;
        this.showPaymentModal = false;
      }
    });
  }

  async generateQrCode(tokenCode: string) {
    const canvas = document.getElementById('qr-code-canvas') as HTMLCanvasElement;
    if (canvas) {
      try {
        await QRCode.toCanvas(canvas, tokenCode, {
        width: 200,
        margin: 2,
        color: { dark: '#1a1a2e', light: '#ffffff' }
      });
      } catch (err) {
        console.error('QR generation failed', err);
      }
    }
  }

  cancelBooking(tokenCode: string) {
    if (!confirm('Are you sure you want to cancel this booking?')) return;

    this.bookingService.cancelBooking(tokenCode).subscribe({
      next: () => {
        this.successMessage = `Booking ${tokenCode} cancelled successfully.`;
        this.loadMyBookings();
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Cancellation failed.';
      }
    });
  }

  downloadReceipt() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.bookingResult?.tokenCode) {
      this.errorMessage = 'Receipt is not available yet. Please complete payment first.';
      return;
    }

    const tokenCode = String(this.bookingResult.tokenCode);
    const paid = this.bookingResult.totalPaid ?? this.totalAmount ?? 0;
    const fuelType = this.bookingResult.fuelType ?? this.fuelType ?? '—';
    const qty = this.bookingResult.quantityLiters ?? this.quantity ?? 0;
    const expiresAt = this.bookingResult.expiresAt ? new Date(this.bookingResult.expiresAt) : null;
    const generatedAt = new Date();

    // If QR is rendered, embed it in the receipt for offline proof.
    const qrDataUrl =
      (document.getElementById('qr-code-canvas') as HTMLCanvasElement | null)?.toDataURL?.('image/png') ?? '';

    const escapeHtml = (v: unknown) =>
      String(v ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');

    const receiptHtml = `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>IFMS Receipt - ${escapeHtml(tokenCode)}</title>
  <style>
    :root { color-scheme: light; }
    body { margin: 0; font-family: ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial; background: #f6f7fb; }
    .wrap { max-width: 760px; margin: 24px auto; padding: 0 16px; }
    .card { background: #fff; border: 1px solid #e7e9f2; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(0,0,0,.06); }
    .hdr { padding: 18px 20px; background: linear-gradient(135deg,#0b2a66,#2b57c6); color: #fff; }
    .hdr .brand { font-weight: 800; letter-spacing: .04em; text-transform: uppercase; font-size: 12px; opacity: .9; }
    .hdr .title { font-weight: 900; margin-top: 6px; font-size: 22px; }
    .body { padding: 20px; display: grid; gap: 16px; }
    .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .row { padding: 12px 14px; border: 1px solid #edf0f6; border-radius: 12px; background: #fbfcff; }
    .k { font-size: 11px; text-transform: uppercase; letter-spacing: .08em; color: #5b647a; font-weight: 800; }
    .v { margin-top: 6px; font-size: 14px; font-weight: 750; color: #121826; }
    .token { font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace; font-size: 18px; letter-spacing: .08em; }
    .qr { display: flex; align-items: center; justify-content: center; padding: 18px; border: 1px dashed #d7def0; border-radius: 14px; background: #fff; }
    .qr img { width: 220px; height: 220px; image-rendering: pixelated; }
    .ftr { padding: 14px 20px; border-top: 1px solid #edf0f6; display: flex; justify-content: space-between; gap: 12px; flex-wrap: wrap; color: #5b647a; font-size: 12px; }
    @media print {
      body { background: #fff; }
      .wrap { margin: 0; max-width: none; padding: 0; }
      .card { box-shadow: none; border: 0; border-radius: 0; }
    }
  </style>
</head>
<body>
  <div class="wrap">
    <div class="card">
      <div class="hdr">
        <div class="brand">IFMS • Bharat Kinetic</div>
        <div class="title">Payment Receipt</div>
      </div>
      <div class="body">
        <div class="grid">
          <div class="row"><div class="k">Token</div><div class="v token">${escapeHtml(tokenCode)}</div></div>
          <div class="row"><div class="k">Amount Paid</div><div class="v">₹${escapeHtml(Number(paid).toFixed(2))}</div></div>
          <div class="row"><div class="k">Fuel Type</div><div class="v">${escapeHtml(fuelType)}</div></div>
          <div class="row"><div class="k">Quantity</div><div class="v">${escapeHtml(qty)} ${escapeHtml(fuelType === 'CNG' ? 'KG' : fuelType === 'EV' ? 'kWh' : 'L')}</div></div>
          <div class="row"><div class="k">Generated At</div><div class="v">${escapeHtml(generatedAt.toLocaleString())}</div></div>
          <div class="row"><div class="k">Expires At</div><div class="v">${escapeHtml(expiresAt ? expiresAt.toLocaleString() : '—')}</div></div>
        </div>
        <div class="qr">
          ${qrDataUrl ? `<img alt="QR Code" src="${qrDataUrl}" />` : `<div style="color:#5b647a;font-weight:700;">QR not available</div>`}
        </div>
      </div>
      <div class="ftr">
        <div>Receipt ID: ${escapeHtml(tokenCode)}</div>
        <div>Tip: You can print this page as PDF from your browser.</div>
      </div>
    </div>
  </div>
</body>
</html>`;

    const blob = new Blob([receiptHtml], { type: 'text/html;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `IFMS-Receipt-${tokenCode}.html`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 0);
  }

  // Dealer functions
  validateToken() {
    this.errorMessage = '';
    this.successMessage = '';
    this.dealerInventoryNote = '';
    this.isValidating = true;
    this.validationResult = null;
    this.dealerTokenState = 'idle';

    this.bookingService.validateToken(this.tokenInput).subscribe({
      next: (result) => {
        this.validationResult = result;
        this.isValidating = false;
        const token = String(result?.tokenCode ?? result?.TokenCode ?? this.tokenInput ?? '').trim();
        this.dealerTokenState = 'verified';
        this.successMessage = token
          ? `Token verified: ${token}. You can now confirm dispensing.`
          : 'Token verified. You can now confirm dispensing.';
      },
      error: (err) => {
        const apiMsg = err?.error?.error;
        const status = typeof err?.status === 'number' && err.status !== 0 ? err.status : null;
        const extra =
          status === 403 ? ' (not authorized as Dealer)' :
          status === 0 ? ' (Booking API unreachable)' :
          status ? ` (HTTP ${status})` : '';
        const msg = String(apiMsg || `Token validation failed${extra}.`);
        const m = msg.toLowerCase();
        if (m.includes('expired')) this.dealerTokenState = 'expired';
        else if (m.includes('already used') || m.includes('used')) this.dealerTokenState = 'used';
        else if (m.includes('cancel')) this.dealerTokenState = 'cancelled';
        else this.dealerTokenState = 'invalid';
        this.errorMessage = msg;
        this.isValidating = false;
        this.validationResult = null;
      }
    });
  }

  confirmDispensing() {
    this.errorMessage = '';
    this.dealerInventoryNote = '';
    this.bookingService.confirmBooking(this.tokenInput).subscribe({
      next: () => {
        this.successMessage = `Booking confirmed! Token ${this.tokenInput} marked as USED. Fuel dispensed and inventory updated.`;
        this.dealerTokenState = 'confirmed';
        this.validationResult = null;
        this.tokenInput = '';
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Confirmation failed.';
      }
    });
  }

  // ── Support methods ───────────────────────────────────────────────────────

  openSupport(tab: 'raise' | 'track' | 'faq' = 'raise') {
    this.activeView = 'support';
    this.supportTab = tab;
    if (tab === 'track') this.loadMyComplaints();
  }

  loadMyComplaints() {
    this.complaintsLoading = true;
    this.complaintsError = '';
    this.complaintService.getMine().subscribe({
      next: (data) => { this.myComplaints = data; this.complaintsLoading = false; },
      error: () => { this.complaintsError = 'Could not load your complaints. Please try again.'; this.complaintsLoading = false; }
    });
  }

  submitComplaint() {
    this.complaintError = '';
    this.complaintSuccess = '';
    if (!this.complaintForm.subject.trim()) { this.complaintError = 'Please enter a subject.'; return; }
    if (!this.complaintForm.description.trim()) { this.complaintError = 'Please describe the issue.'; return; }

    this.complaintSubmitting = true;
    this.complaintService.raise({
      category: this.complaintForm.category,
      subject: this.complaintForm.subject.trim(),
      description: this.complaintForm.description.trim(),
      referenceId: this.complaintForm.referenceId.trim() || undefined
    }).subscribe({
      next: (res) => {
        this.complaintSubmitting = false;
        this.complaintSuccess = `Complaint submitted successfully! Ticket ID: ${res.id}. We will respond within 24–48 hours.`;
        this.complaintForm = { category: 'FuelQuality', subject: '', description: '', referenceId: '' };
        setTimeout(() => { this.supportTab = 'track'; this.loadMyComplaints(); }, 2000);
      },
      error: (err) => {
        this.complaintSubmitting = false;
        this.complaintError = err.error?.error || 'Could not submit your complaint. Please try again.';
      }
    });
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Open':       return 'bg-blue-100 text-blue-700';
      case 'InProgress': return 'bg-yellow-100 text-yellow-700';
      case 'Resolved':   return 'bg-green-100 text-green-700';
      case 'Closed':     return 'bg-gray-100 text-gray-600';
      default:           return 'bg-gray-100 text-gray-600';
    }
  }

  getCategoryLabel(value: string): string {
    return this.complaintCategories.find(c => c.value === value)?.label ?? value;
  }

  toggleFaq(i: number) {
    this.openFaqIndex = this.openFaqIndex === i ? null : i;
  }

  // ── Settings methods ──────────────────────────────────────────────────────

  openSettings(tab: 'profile' | 'password' | 'notifications' | 'fuel' = 'profile') {
    this.activeView = 'settings';
    this.settingsTab = tab;
    if (tab === 'profile') this.loadProfile();
    this.loadPrefsFromStorage();
  }

  loadProfile() {
    this.profileLoading = true;
    this.profileError = '';
    const token = this.authService.getToken();
    if (!token) { this.profileLoading = false; return; }

    this.http.get<any>(`${this.authApiUrl}/me`, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: (u) => {
        this.profileForm.fullName    = u.fullName ?? '';
        this.profileForm.phoneNumber = u.phoneNumber ?? '';
        this.profileLoading = false;
      },
      error: () => { this.profileError = 'Could not load profile.'; this.profileLoading = false; }
    });
  }

  saveProfile() {
    this.profileSuccess = '';
    this.profileError = '';
    if (!this.profileForm.fullName.trim()) { this.profileError = 'Full name is required.'; return; }

    this.profileSaving = true;
    const token = this.authService.getToken();
    this.http.put<any>(`${this.authApiUrl}/me`,
      { fullName: this.profileForm.fullName.trim(), phoneNumber: this.profileForm.phoneNumber.trim() || null },
      { headers: { Authorization: `Bearer ${token}` } }
    ).subscribe({
      next: (u) => {
        this.profileSaving = false;
        this.profileSuccess = 'Profile updated successfully.';
        // Update localStorage so the name shows correctly everywhere
        localStorage.setItem('fullName', u.fullName);
        if (u.phoneNumber) localStorage.setItem('phone', u.phoneNumber);
        this.contactName  = u.fullName;
        this.contactPhone = u.phoneNumber ?? this.contactPhone;
        setTimeout(() => { this.profileSuccess = ''; }, 3000);
      },
      error: (err) => {
        this.profileSaving = false;
        this.profileError = err.error?.error || 'Could not save profile. Please try again.';
      }
    });
  }

  changePassword() {
    this.passwordSuccess = '';
    this.passwordError = '';
    if (!this.passwordForm.currentPassword) { this.passwordError = 'Enter your current password.'; return; }
    if (this.passwordForm.newPassword.length < 8) { this.passwordError = 'New password must be at least 8 characters.'; return; }
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) { this.passwordError = 'New passwords do not match.'; return; }

    this.passwordSaving = true;
    const token = this.authService.getToken();
    this.http.put<any>(`${this.authApiUrl}/me/change-password`,
      { currentPassword: this.passwordForm.currentPassword, newPassword: this.passwordForm.newPassword },
      { headers: { Authorization: `Bearer ${token}` } }
    ).subscribe({
      next: () => {
        this.passwordSaving = false;
        this.passwordSuccess = 'Password changed successfully.';
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
        setTimeout(() => { this.passwordSuccess = ''; }, 3000);
      },
      error: (err) => {
        this.passwordSaving = false;
        this.passwordError = err.error?.error || 'Could not change password. Please try again.';
      }
    });
  }

  private loadPrefsFromStorage() {
    const n = localStorage.getItem('ifms_notif_prefs');
    if (n) this.notifPrefs = { ...this.notifPrefs, ...JSON.parse(n) };
    const f = localStorage.getItem('ifms_fuel_prefs');
    if (f) this.fuelPrefs = { ...this.fuelPrefs, ...JSON.parse(f) };
  }

  saveNotifPrefs() {
    localStorage.setItem('ifms_notif_prefs', JSON.stringify(this.notifPrefs));
    this.profileSuccess = 'Notification preferences saved.';
    setTimeout(() => { this.profileSuccess = ''; }, 2500);
  }

  saveFuelPrefs() {
    localStorage.setItem('ifms_fuel_prefs', JSON.stringify(this.fuelPrefs));
    // Apply default fuel type to booking form
    this.fuelType = this.fuelPrefs.defaultFuelType;
    this.profileSuccess = 'Fuel preferences saved.';
    setTimeout(() => { this.profileSuccess = ''; }, 2500);
  }

  navigate(path: string) {
    void this.router.navigate([path]);
  }

  dismissSuccessPopup() {
    this.showSuccessPopup = false;
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}
