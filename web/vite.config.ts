// FILE: vite.config.js

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import { quasar, transformAssetUrls } from "@quasar/vite-plugin";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue({
      template: { transformAssetUrls },
    }),

    quasar({
      autoImportComponentCase: "pascal",
      sassVariables: "src/quasar-variables.sass",
    }),
  ],
  server: {
    https: false,
    cors: {
      origin: "*",
      methods: "GET,POST,PUT,PATCH,DELETE,OPTIONS",
    }
  },
});
