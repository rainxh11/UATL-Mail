import { createVuePlugin } from 'vite-plugin-vue2';
import Components from 'unplugin-vue-components/vite';
import {  VuetifyResolver }  from 'unplugin-vue-components/resolvers'
import path from 'path';

/**
 * @type {import('vite').UserConfig}
 */
module.exports = {
  resolve: {
    vue: 'vue/dist/vue.js',
    alias: [
      {
        find: '@/',
        replacement: `${path.resolve(__dirname, './src')}/`,
      },
      {
        find: 'src/',
        replacement: `${path.resolve(__dirname, './src')}/`,
      },
    ],
  },
  plugins: [
    createVuePlugin(),
    Components ({
      resolvers: [
        VuetifyResolver(),
      ],
	  dts: true,
	  include: [/\.vue$/, /\.vue\?vue/, /\.md$/],
    }),
  ],
  server: {
    host: '0.0.0.0',
    port: 8080,
  },
  productionSourceMap: false,

  // https://cli.vuejs.org/config/#css-extract
  css: {
    extract: { ignoreOrder: true },
    loaderOptions: {
      sass: {
        additionalData: '@import \'~@/assets/scss/vuetify/variables\''
      },
      scss: {
        additionalData: '@import \'~@/assets/scss/vuetify/variables\';'
      }
    }
  },
};