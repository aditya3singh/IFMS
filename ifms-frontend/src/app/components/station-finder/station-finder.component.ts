import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StationService } from '../../services/station.service';
import { PaginationComponent } from '../../shared/pagination.component';

@Component({
  selector: 'app-station-finder',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './station-finder.component.html',
  styleUrls: ['./station-finder.component.css']
})
export class StationFinderComponent implements OnInit {
  stations: Array<{ id: string; name: string; city: string; state: string }> = [];
  loading = false;
  error = '';
  searchQuery = '';

  // Pagination
  stationPage = 1; stationPageSize = 6;
  get filteredStations() {
    if (!this.searchQuery) return this.stations;
    const q = this.searchQuery.toLowerCase();
    return this.stations.filter(s => s.name.toLowerCase().includes(q) || s.city.toLowerCase().includes(q) || s.state.toLowerCase().includes(q));
  }
  get pagedStations() { return this.filteredStations.slice((this.stationPage-1)*this.stationPageSize, this.stationPage*this.stationPageSize); }

  constructor(private stationService: StationService, private router: Router) {}

  ngOnInit(): void {
    this.loading = true;
    this.stationService.getAllStations().subscribe({
      next: (rows) => {
        this.stations = (rows ?? []).map((s: any) => ({
          id: String(s.id ?? s.Id ?? ''),
          name: String(s.name ?? s.Name ?? 'Unknown Station'),
          city: String(s.city ?? s.City ?? ''),
          state: String(s.state ?? s.State ?? '')
        }));
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load stations right now.';
        this.loading = false;
      }
    });
  }

  goBack() {
    void this.router.navigate(['/dashboard']);
  }

  bookAtStation(stationId: string) {
    void this.router.navigate(['/booking'], { queryParams: { stationId } });
  }
}
