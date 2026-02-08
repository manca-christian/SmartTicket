/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        'space-black': '#0b1220',
        'space-ink': '#0f172a',
        'space-glow': '#38bdf8',
      },
      boxShadow: {
        glow: '0 0 25px rgba(56, 189, 248, 0.35)',
      },
    },
  },
  plugins: [],
}
