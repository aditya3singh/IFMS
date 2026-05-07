import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import 'zone.js';

/**
 * Clear any stale staff data stored under old keys (e.g. `ifms_staff`,
 * `ifms_staff_unassigned`, or the old fixed-ID seed keys) so the new
 * StaffService keys (lowercase station IDs) are the only source of truth.
 * This runs once; after the first load the correct keys will be populated
 * by StaffService.seedIfEmpty() when each portal opens a station.
 */
function clearStaleStaffKeys(): void {
  const staleKeys: string[] = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (!key) continue;
    if (key === 'ifms_staff' || key === 'ifms_staff_unassigned') {
      staleKeys.push(key);
    }
  }
  staleKeys.forEach(k => localStorage.removeItem(k));
}

clearStaleStaffKeys();

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
