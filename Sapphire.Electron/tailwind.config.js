const colors = require('tailwindcss/colors')

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.{razor,html,cshtml}"],
  theme: {
    extend:{
      colors: {
        transparent: 'transparent',
        current: 'currentColor',
        background: colors.neutral,
        primary: colors.slate,
      },
    }
  },
  plugins: [],
}

