export const environment = {
  authApiUrl: 'http://localhost:5010/gateway/auth',
  /** Must match IFMS.Booking.API launchSettings (default http profile). */
  bookingApiUrl: 'http://localhost:5010/gateway/booking',
  /** Host only — paths are /api/Stations and /api/fuel-price (see Station.API). */
  stationServiceHost: 'http://localhost:5010/gateway',
  /** Stations CRUD + list */
  stationApiUrl: 'http://localhost:5010/gateway/stations'
};
