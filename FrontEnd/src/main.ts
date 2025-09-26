import { createApp } from 'vue'
import 'typeface-inter'

import './style.css'
import App from './App.vue'
import router from './router'

// 导入 Element Plus 以及默认主题
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

const app = createApp(App)
// 全局注册 Element Plus
app.use(ElementPlus)
app.use(router)
app.mount('#app')