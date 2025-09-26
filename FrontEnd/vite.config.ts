import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': '/src', 
    }
  },
  server: {
    proxy: {
      // 转发所有 /api 请求
      '/api': {
        target: 'http://121.41.121.172',
        changeOrigin: true,
        secure: false,                  // 忽略自签名 HTTPS 证书
        rewrite: (path) => path,        // 不重写路径
        // 移除 headers 配置，避免影响 multipart/form-data 上传
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('proxy error', err);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('Sending Request:', req.method, req.url);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('Received Response from:', req.url, proxyRes.statusCode);
          });
        }
      },
      // 转发静态文件访问 /uploads
      '/uploads': {
        target: 'http://121.41.121.172',
        changeOrigin: true,
        secure: false,
      },
      // 代理所有 /audio 请求到后端
      '/audio': {
        target: 'http://121.41.121.172',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})