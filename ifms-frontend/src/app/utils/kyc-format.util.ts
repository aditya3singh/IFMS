/** PAN: 5 letters + 4 digits + 1 letter (person/entity type in 4th char). */
const PAN_REGEX = /^[A-Z]{5}[0-9]{4}[A-Z]$/;

/** Aadhaar: 12 digits, first digit 2–9 (basic pattern; full check uses Verhoeff). */
const AADHAAR_BASIC = /^[2-9][0-9]{11}$/;

const D: number[][] = [
  [0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
  [1, 2, 3, 4, 0, 6, 7, 8, 9, 5],
  [2, 3, 4, 0, 1, 7, 8, 9, 5, 6],
  [3, 4, 0, 1, 2, 8, 9, 5, 6, 7],
  [4, 0, 1, 2, 3, 9, 5, 6, 7, 8],
  [5, 9, 8, 7, 6, 0, 4, 3, 2, 1],
  [6, 5, 9, 8, 7, 1, 0, 4, 3, 2],
  [7, 6, 5, 9, 8, 2, 1, 0, 4, 3],
  [8, 7, 6, 5, 9, 3, 2, 1, 0, 4],
  [9, 8, 7, 6, 5, 4, 3, 2, 1, 0]
];

const P: number[][] = [
  [0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
  [1, 5, 7, 6, 2, 8, 3, 0, 9, 4],
  [5, 8, 0, 3, 7, 9, 6, 1, 4, 2],
  [8, 9, 1, 6, 0, 4, 3, 5, 2, 7],
  [9, 4, 5, 7, 2, 0, 8, 6, 3, 1],
  [4, 2, 8, 6, 5, 7, 3, 9, 0, 1],
  [2, 7, 9, 3, 8, 0, 6, 4, 1, 5],
  [7, 0, 4, 1, 5, 2, 3, 9, 8, 6]
];

/** Uppercase A–Z and digits only (strips spaces, hyphens, dots — common entry mistakes). */
export function normalizePan(pan: string): string {
  return pan
    .trim()
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '');
}

export function cleanAadhaarDigits(input: string): string {
  return input.replace(/\D/g, '');
}

export function isValidPanFormat(pan: string): boolean {
  const n = normalizePan(pan);
  return n.length === 10 && PAN_REGEX.test(n);
}

function verhoeffValid12(twelve: string): boolean {
  if (twelve.length !== 12 || !/^\d{12}$/.test(twelve)) return false;
  let c = 0;
  for (let i = 0; i < 12; i++) {
    const digit = parseInt(twelve[twelve.length - 1 - i], 10);
    c = D[c][P[i % 8][digit]];
  }
  return c === 0;
}

export function isValidAadhaarFormat(aadhaar: string): boolean {
  const d = cleanAadhaarDigits(aadhaar);
  if (d.length !== 12 || !AADHAAR_BASIC.test(d)) return false;
  return verhoeffValid12(d);
}
