/**
 * Staff seed utility.
 *
 * Seeds default staff data into localStorage using the ACTUAL station IDs
 * fetched from the API at runtime — so admin and dealer always share the
 * same key: `ifms_staff_<realStationId>`.
 *
 * Called once from main.ts before Angular bootstraps (for the fixed-ID
 * stations), and also called from StaffComponent after the dealer's station
 * ID is resolved from the API.
 */

export interface SeedStaff {
  id: number;
  name: string;
  role: string;
  shift: string;
  phone: string;
  email: string;
  status: 'Active' | 'Off Duty' | 'On Leave';
  joinDate: string;
}

/** Staff pool keyed by city name (case-insensitive match). */
const CITY_STAFF: Record<string, SeedStaff[]> = {
  mumbai: [
    { id: 1001, name: 'Rajesh Patil',    role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9820011001', email: 'rajesh.patil@wefp.in',  status: 'Active',   joinDate: '2024-01-10' },
    { id: 1002, name: 'Sunita Desai',    role: 'Cashier',        shift: 'Morning (6AM-2PM)',    phone: '9820011002', email: 'sunita.desai@wefp.in',  status: 'Active',   joinDate: '2024-02-15' },
    { id: 1003, name: 'Anil Shinde',     role: 'Pump Operator',  shift: 'Afternoon (2PM-10PM)', phone: '9820011003', email: 'anil.shinde@wefp.in',   status: 'Active',   joinDate: '2024-03-01' },
    { id: 1004, name: 'Meena Jadhav',    role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9820011004', email: 'meena.jadhav@wefp.in',  status: 'Off Duty', joinDate: '2024-04-20' },
    { id: 1005, name: 'Vikram Sawant',   role: 'Security Guard', shift: 'Night (10PM-6AM)',     phone: '9820011005', email: 'vikram.sawant@wefp.in', status: 'Active',   joinDate: '2024-05-05' },
  ],
  bengaluru: [
    { id: 2001, name: 'Kiran Reddy',     role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9845022001', email: 'kiran.reddy@scp.in',    status: 'Active',   joinDate: '2024-01-20' },
    { id: 2002, name: 'Priya Nair',      role: 'Cashier',        shift: 'Afternoon (2PM-10PM)', phone: '9845022002', email: 'priya.nair@scp.in',     status: 'Active',   joinDate: '2024-02-10' },
    { id: 2003, name: 'Suresh Kumar',    role: 'Pump Operator',  shift: 'Morning (6AM-2PM)',    phone: '9845022003', email: 'suresh.kumar@scp.in',   status: 'Active',   joinDate: '2024-03-15' },
    { id: 2004, name: 'Deepa Rao',       role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9845022004', email: 'deepa.rao@scp.in',      status: 'On Leave', joinDate: '2024-04-01' },
    { id: 2005, name: 'Manjunath S',     role: 'Maintenance',    shift: 'Morning (6AM-2PM)',    phone: '9845022005', email: 'manjunath.s@scp.in',    status: 'Active',   joinDate: '2024-06-01' },
  ],
  'new delhi': [
    { id: 3001, name: 'Amit Sharma',     role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9811033001', email: 'amit.sharma@nce.in',    status: 'Active',   joinDate: '2024-01-05' },
    { id: 3002, name: 'Pooja Gupta',     role: 'Cashier',        shift: 'Morning (6AM-2PM)',    phone: '9811033002', email: 'pooja.gupta@nce.in',    status: 'Active',   joinDate: '2024-02-20' },
    { id: 3003, name: 'Rahul Verma',     role: 'Pump Operator',  shift: 'Afternoon (2PM-10PM)', phone: '9811033003', email: 'rahul.verma@nce.in',    status: 'Active',   joinDate: '2024-03-10' },
    { id: 3004, name: 'Neha Singh',      role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9811033004', email: 'neha.singh@nce.in',     status: 'Active',   joinDate: '2024-04-15' },
    { id: 3005, name: 'Deepak Yadav',    role: 'Security Guard', shift: 'Night (10PM-6AM)',     phone: '9811033005', email: 'deepak.yadav@nce.in',   status: 'Off Duty', joinDate: '2024-05-20' },
    { id: 3006, name: 'Kavita Mishra',   role: 'Maintenance',    shift: 'Afternoon (2PM-10PM)', phone: '9811033006', email: 'kavita.mishra@nce.in',  status: 'Active',   joinDate: '2024-06-10' },
  ],
  hyderabad: [
    { id: 4001, name: 'Venkat Rao',      role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9848044001', email: 'venkat.rao@hcf.in',     status: 'Active',   joinDate: '2024-01-15' },
    { id: 4002, name: 'Lakshmi Devi',    role: 'Cashier',        shift: 'Afternoon (2PM-10PM)', phone: '9848044002', email: 'lakshmi.devi@hcf.in',   status: 'Active',   joinDate: '2024-02-25' },
    { id: 4003, name: 'Srinivas Murthy', role: 'Pump Operator',  shift: 'Morning (6AM-2PM)',    phone: '9848044003', email: 'srinivas.m@hcf.in',     status: 'Active',   joinDate: '2024-03-20' },
    { id: 4004, name: 'Anitha Kumari',   role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9848044004', email: 'anitha.k@hcf.in',       status: 'On Leave', joinDate: '2024-04-10' },
    { id: 4005, name: 'Ravi Teja',       role: 'Security Guard', shift: 'Night (10PM-6AM)',     phone: '9848044005', email: 'ravi.teja@hcf.in',      status: 'Active',   joinDate: '2024-05-15' },
  ],
  ahmedabad: [
    { id: 5001, name: 'Hardik Patel',    role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9825055001', email: 'hardik.patel@sro.in',   status: 'Active',   joinDate: '2024-01-25' },
    { id: 5002, name: 'Hetal Shah',      role: 'Cashier',        shift: 'Morning (6AM-2PM)',    phone: '9825055002', email: 'hetal.shah@sro.in',     status: 'Active',   joinDate: '2024-02-05' },
    { id: 5003, name: 'Bhavesh Modi',    role: 'Pump Operator',  shift: 'Afternoon (2PM-10PM)', phone: '9825055003', email: 'bhavesh.modi@sro.in',   status: 'Active',   joinDate: '2024-03-25' },
    { id: 5004, name: 'Ritu Joshi',      role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9825055004', email: 'ritu.joshi@sro.in',     status: 'Off Duty', joinDate: '2024-04-30' },
    { id: 5005, name: 'Nilesh Trivedi',  role: 'Security Guard', shift: 'Night (10PM-6AM)',     phone: '9825055005', email: 'nilesh.trivedi@sro.in', status: 'Active',   joinDate: '2024-05-10' },
  ],
  pune: [
    { id: 6001, name: 'Aditya Kulkarni', role: 'Supervisor',     shift: 'Morning (6AM-2PM)',    phone: '9823066001', email: 'aditya.k@dep.in',       status: 'Active',   joinDate: '2024-01-12' },
    { id: 6002, name: 'Sneha Joshi',     role: 'Cashier',        shift: 'Afternoon (2PM-10PM)', phone: '9823066002', email: 'sneha.j@dep.in',        status: 'Active',   joinDate: '2024-02-18' },
    { id: 6003, name: 'Rohit Pawar',     role: 'Pump Operator',  shift: 'Morning (6AM-2PM)',    phone: '9823066003', email: 'rohit.p@dep.in',        status: 'Active',   joinDate: '2024-03-05' },
    { id: 6004, name: 'Prachi Deshpande',role: 'Pump Operator',  shift: 'Night (10PM-6AM)',     phone: '9823066004', email: 'prachi.d@dep.in',       status: 'On Leave', joinDate: '2024-04-22' },
    { id: 6005, name: 'Sagar Bhosale',   role: 'Security Guard', shift: 'Night (10PM-6AM)',     phone: '9823066005', email: 'sagar.b@dep.in',        status: 'Active',   joinDate: '2024-05-08' },
  ],
};

/** Get seed staff for a city (case-insensitive). Returns empty array if city unknown. */
export function getStaffForCity(city: string): SeedStaff[] {
  return CITY_STAFF[city.toLowerCase().trim()] ?? [];
}

/**
 * Seed staff for a single station by its real ID and city.
 * Only writes if no data exists yet for that station key.
 * Returns true if data was seeded, false if already existed.
 */
export function seedStationStaff(stationId: string, city: string): boolean {
  const key = `ifms_staff_${stationId}`;
  if (localStorage.getItem(key)) return false; // already has data — don't overwrite
  const staff = getStaffForCity(city);
  if (staff.length > 0) {
    localStorage.setItem(key, JSON.stringify(staff));
    return true;
  }
  return false;
}

/**
 * Called from main.ts for the 5 known fixed-ID seed stations.
 * Also safe to call multiple times — skips stations that already have data.
 */
export function seedStaffData(): void {
  const KNOWN_STATIONS = [
    { id: '11111111-1111-1111-1111-111111111111', city: 'Mumbai' },
    { id: '22222222-2222-2222-2222-222222222222', city: 'Bengaluru' },
    { id: '33333333-3333-3333-3333-333333333333', city: 'New Delhi' },
    { id: '44444444-4444-4444-4444-444444444444', city: 'Hyderabad' },
    { id: '55555555-5555-5555-5555-555555555555', city: 'Ahmedabad' },
  ];
  for (const s of KNOWN_STATIONS) {
    seedStationStaff(s.id, s.city);
  }
}
