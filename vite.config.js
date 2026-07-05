import { defineConfig } from "vite";

export default defineConfig({
  base: "./", // relative asset paths so GitHub Pages (subpath) works
  server: {
    port: 5175,
    host: true, // expose on the home network for the iPhone
  },
  build: {
    outDir: "dist",
  },
  clearScreen: false,
});
