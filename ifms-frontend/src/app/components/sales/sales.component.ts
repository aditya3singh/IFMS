import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SalesService } from '../../services/sales.service';
import { StationService } from '../../services/station.service';
import { FuelPriceService } from '../../services/fuel-price.service';
import { PaginationComponent } from '../../shared/pagination.component';
import { NotificationBellComponent } from '../../shared/notification-bell.component';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, NotificationBellComponent],
  templateUrl: './sales.component.html',
  styleUrls: ['./sales.component.css']
})
export class SalesComponent implements OnInit {
  role: string | null = null;
  transactions: any[] = [];
  stationMonitorRows: any[] = [];
  incomingLoading = false;
  incomingError = '';
  displayedColumns = ['customerName','fuelType','quantity','pricePerLitre','totalAmount','paymentMethod','status','transactionDate'];
  newTransaction = { stationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', fuelType: 'Petrol', quantity: 0, pricePerLitre: 0, paymentMethod: 'UPI', customerName: '' };
  isLoading = false;
  currentStation: any = null;
  priceLoading = false;
  priceSource = '';
  priceLastUpdated = '';

  // Pagination — transactions
  txPage = 1; txPageSize = 10;
  get txPaged() { return this.transactions.slice((this.txPage-1)*this.txPageSize, this.txPage*this.txPageSize); }

  // Pagination — station monitor
  monitorPage = 1; monitorPageSize = 10;
  get monitorPaged() { return this.stationMonitorRows.slice((this.monitorPage-1)*this.monitorPageSize, this.monitorPage*this.monitorPageSize); }

  constructor(
    private salesService: SalesService,
    private stationService: StationService,
    private fuelPriceService: FuelPriceService,
    private notifService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.role = typeof localStorage !== 'undefined' ? localStorage.getItem('role') : null;
    if (this.role === 'Dealer') {
      this.stationService.getMyStations().subscribe({
        next: (stations) => {
          if (stations?.length && stations[0]?.id) {
            this.newTransaction.stationId = stations[0].id;
            this.currentStation = stations[0];
            // Fetch real-time price after getting station
            this.fetchRealtimePrice();
          }
          this.loadTransactions();
        },
        error: () => this.loadTransactions()
      });
    } else if (this.role === 'Admin') {
      this.loadStationMonitor();
    } else {
      this.loadTransactions();
    }
  }

  loadStationMonitor() {
    this.incomingLoading = true;
    this.incomingError = '';
    this.salesService.getStationMonitor().subscribe({
      next: (data) => {
        this.stationMonitorRows = data?.rows ?? [];
        this.incomingLoading = false;
      },
      error: () => {
        this.incomingError = 'Could not load centralized station monitoring data.';
        this.stationMonitorRows = [];
        this.incomingLoading = false;
      }
    });
  }

  loadTransactions() {
    this.salesService.getAllTransactions().subscribe({
      next: (data) => (this.transactions = data),
      error: (err) => console.error(err)
    });
  }

  recordSale() {
    if (this.role !== 'Dealer') return;
    this.isLoading = true;
    this.salesService.createTransaction(this.newTransaction).subscribe({
      next: (transaction) => {
        this.loadTransactions();
        this.isLoading = false;
        // Push notification
        const total = transaction.totalAmount || (this.newTransaction.quantity * this.newTransaction.pricePerLitre);
        this.notifService.pushLocal({
          type: 'success',
          title: 'Sale Recorded',
          message: `₹${total.toFixed(0)} sale for ${this.newTransaction.customerName || 'customer'} — ${this.newTransaction.fuelType} ${this.newTransaction.quantity}L`,
          icon: 'point_of_sale'
        });
        // Generate and download receipt
        this.generateReceipt(transaction);
        
        // Reset form
        this.newTransaction.customerName = '';
        this.newTransaction.quantity = 0;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  generateReceipt(transaction: any) {
    const receiptHtml = `
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Fuel Receipt</title>
  <style>
    body { font-family: 'Courier New', monospace; max-width: 400px; margin: 20px auto; padding: 20px; }
    .receipt { border: 2px solid #000; padding: 20px; }
    .header { text-align: center; border-bottom: 2px dashed #000; padding-bottom: 10px; margin-bottom: 15px; }
    .header h1 { margin: 0; font-size: 24px; }
    .header p { margin: 5px 0; font-size: 12px; }
    .row { display: flex; justify-content: space-between; margin: 8px 0; }
    .row.total { border-top: 2px solid #000; padding-top: 10px; margin-top: 15px; font-weight: bold; font-size: 18px; }
    .label { font-weight: bold; }
    .footer { text-align: center; margin-top: 20px; padding-top: 15px; border-top: 2px dashed #000; font-size: 11px; }
    @media print {
      body { margin: 0; }
      .no-print { display: none; }
    }
  </style>
</head>
<body>
  <div class="receipt">
    <div class="header">
      <h1>⛽ IFMS FUEL RECEIPT</h1>
      <p>Indian Fuel Management System</p>
      <p>Receipt #: ${transaction.id || 'N/A'}</p>
      <p>Date: ${new Date().toLocaleString('en-IN')}</p>
    </div>
    
    <div class="row">
      <span class="label">Customer:</span>
      <span>${transaction.customerName || this.newTransaction.customerName}</span>
    </div>
    
    <div class="row">
      <span class="label">Fuel Type:</span>
      <span>${transaction.fuelType || this.newTransaction.fuelType}</span>
    </div>
    
    <div class="row">
      <span class="label">Quantity:</span>
      <span>${transaction.quantity || this.newTransaction.quantity} Litres</span>
    </div>
    
    <div class="row">
      <span class="label">Price per Litre:</span>
      <span>₹${(transaction.pricePerLitre || this.newTransaction.pricePerLitre).toFixed(2)}</span>
    </div>
    
    <div class="row">
      <span class="label">Payment Method:</span>
      <span>${transaction.paymentMethod || this.newTransaction.paymentMethod}</span>
    </div>
    
    <div class="row total">
      <span class="label">TOTAL AMOUNT:</span>
      <span>₹${(transaction.totalAmount || (this.newTransaction.quantity * this.newTransaction.pricePerLitre)).toFixed(2)}</span>
    </div>
    
    <div class="footer">
      <p>Thank you for your business!</p>
      <p>Drive Safe • Save Fuel • Go Green</p>
      <p style="margin-top: 10px; font-size: 10px;">
        This is a computer-generated receipt.<br>
        For queries: support@ifms.com | 1800-XXX-XXXX
      </p>
    </div>
  </div>
  
  <div class="no-print" style="text-align: center; margin-top: 20px;">
    <button onclick="window.print()" style="padding: 10px 20px; font-size: 16px; cursor: pointer;">
      🖨️ Print Receipt
    </button>
    <button onclick="window.close()" style="padding: 10px 20px; font-size: 16px; cursor: pointer; margin-left: 10px;">
      ✖️ Close
    </button>
  </div>
</body>
</html>
    `;

    // Open receipt in new window
    const receiptWindow = window.open('', '_blank', 'width=500,height=700');
    if (receiptWindow) {
      receiptWindow.document.write(receiptHtml);
      receiptWindow.document.close();
      
      // Auto-print after 500ms
      setTimeout(() => {
        receiptWindow.print();
      }, 500);
    }
  }

  goBack() {
    void this.router.navigate(['/dashboard']);
  }

  printReceipt(transaction: any) {
    this.generateReceipt(transaction);
  }

  fetchRealtimePrice() {
    if (!this.newTransaction.stationId) return;
    
    this.priceLoading = true;
    this.fuelPriceService.getStationRealtimePrice(this.newTransaction.stationId).subscribe({
      next: (priceData) => {
        if (priceData && priceData.prices) {
          const price = this.fuelPriceService.getPriceByType(priceData.prices, this.newTransaction.fuelType);
          if (price !== null) {
            this.newTransaction.pricePerLitre = price;
            this.priceSource = priceData.source || 'Indian Fuel Price API';
            this.priceLastUpdated = new Date(priceData.fetchedAt).toLocaleString('en-IN');
          } else {
            this.setFallbackPrice();
          }
        } else {
          this.setFallbackPrice();
        }
        this.priceLoading = false;
      },
      error: () => {
        this.setFallbackPrice();
        this.priceLoading = false;
      }
    });
  }

  setFallbackPrice() {
    // Fallback prices if API fails
    const fallbackPrices: { [key: string]: number } = {
      'Petrol': 106.31,
      'Diesel': 94.27,
      'CNG': 75.00
    };
    this.newTransaction.pricePerLitre = fallbackPrices[this.newTransaction.fuelType] || 100;
    this.priceSource = 'Fallback Price';
    this.priceLastUpdated = new Date().toLocaleString('en-IN');
  }

  onFuelTypeChange() {
    // Fetch new price when fuel type changes
    this.fetchRealtimePrice();
  }
}
