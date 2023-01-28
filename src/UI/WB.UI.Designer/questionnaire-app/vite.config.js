import path from 'path'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue2'
import Components from 'unplugin-vue-components/vite'
import { VuetifyResolver } from 'unplugin-vue-components/resolvers'


export default defineConfig({
    plugins: [
      vue(),
      Components({
        resolvers: [VuetifyResolver()],
      }),
    ],
    resolve: {
        alias: {
            // eslint-disable-next-line no-undef
            '@': path.resolve(__dirname, './src'),
        },
    },
})
