/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        nitro: {
          bg: '#0B0F1A',
          card: '#121829',
          cardBorder: '#1F293D',
          cyan: '#00E5FF',
          magenta: '#FF2E92',
          yellow: '#FFE600',
          green: '#00FF66',
          purple: '#A855F7',
          gray: '#94A3B8'
        }
      },
      boxShadow: {
        'glow-cyan': '0 0 15px rgba(0, 229, 255, 0.4), 0 0 30px rgba(0, 229, 255, 0.15)',
        'glow-magenta': '0 0 15px rgba(255, 46, 146, 0.4), 0 0 30px rgba(255, 46, 146, 0.15)',
        'glow-yellow': '0 0 15px rgba(255, 230, 0, 0.4), 0 0 30px rgba(255, 230, 0, 0.15)',
        'glow-green': '0 0 15px rgba(0, 255, 102, 0.4), 0 0 30px rgba(0, 255, 102, 0.15)'
      },
      fontFamily: {
        heading: ['Orbitron', 'sans-serif'],
        sans: ['Inter', 'sans-serif']
      }
    },
  },
  plugins: [],
}
