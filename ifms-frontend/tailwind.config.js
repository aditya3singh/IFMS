/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "outline": "#737780",
        "surface": "#f9f9fd",
        "tertiary-container": "#003b33",
        "primary-fixed": "#d5e3ff",
        "on-tertiary": "#ffffff",
        "primary": "#001e40",
        "surface-container-low": "#f3f3f7",
        "surface-bright": "#f9f9fd",
        "surface-container-lowest": "#ffffff",
        "on-surface-variant": "#43474f",
        "on-tertiary-fixed-variant": "#005047",
        "surface-dim": "#d9dadd",
        "inverse-surface": "#2e3133",
        "surface-tint": "#1d5fa8",
        "on-tertiary-container": "#46ac9c",
        "primary-container": "#003365",
        "primary-fixed-dim": "#a6c8ff",
        "background": "#f9f9fd",
        "on-background": "#191c1e",
        "outline-variant": "#c3c6d1",
        "on-secondary-fixed-variant": "#7a3000",
        "on-error-container": "#93000a",
        "secondary": "#a04100",
        "secondary-container": "#fc7728",
        "on-primary": "#ffffff",
        "on-primary-container": "#669deb",
        "inverse-on-surface": "#f0f0f4",
        "on-tertiary-fixed": "#00201b",
        "on-secondary-container": "#5d2300",
        "secondary-fixed": "#ffdbcb",
        "secondary-fixed-dim": "#ffb693",
        "surface-container-highest": "#e2e2e6",
        "surface-variant": "#e2e2e6",
        "on-secondary": "#ffffff",
        "on-primary-fixed-variant": "#004787",
        "surface-container": "#edeef1",
        "on-primary-fixed": "#001c3b",
        "inverse-primary": "#a6c8ff",
        "surface-container-high": "#e7e8eb",
        "on-surface": "#191c1e",
        "tertiary": "#00231e",
        "on-error": "#ffffff",
        "tertiary-fixed-dim": "#74d8c6",
        "tertiary-fixed": "#91f4e2",
        "on-secondary-fixed": "#341000",
        "error": "#ba1a1a",
        "error-container": "#ffdad6"
      },
      borderRadius: {
        "DEFAULT": "0.125rem",
        "lg": "0.25rem",
        "xl": "0.5rem",
        "full": "0.75rem"
      },
      fontFamily: {
        "headline": ["Manrope", "sans-serif"],
        "body": ["Inter", "sans-serif"],
        "label": ["Inter", "sans-serif"]
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}
