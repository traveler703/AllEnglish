<template>
    <div id="app">
        <!-- 判断是否在需要隐藏导航栏的页面 -->
        <NavBar v-if="!isAuthPage" />

        <!-- 主体内容区域，采用 Flex 布局居中 -->
        <main>
            <router-view />
        </main>

        <!-- 页脚区域 -->
        <footer>
            <p>&copy; 2025 AllEn Team</p>
        </footer>

        <!-- 广告组件（不在登录页面显示） -->
        <Advertisement v-if="!isAuthPage" />
    </div>
</template>

<script lang="ts">
    import { defineComponent, computed } from 'vue'
    import { useRoute } from 'vue-router'
    import NavBar from './components/NavBar.vue'
    import Advertisement from './components/Advertisement.vue'

    export default defineComponent({
        name: 'App',
        components: {
            NavBar,
            Advertisement
        },
        setup() {
            const route = useRoute()

            // 根据当前路由路径判断是否是需要隐藏导航栏的页面
            const isAuthPage = computed(() => {
                return ['/', '/register', '/forgetpassword'].includes(route.path)
            })

            return {
                isAuthPage
            }
        }
    })
</script>

<style>
    /* 整体背景使用淡粉色 */
    #app {
        display: flex;
        flex-direction: column;
        padding: 0px;
        min-width: 100vw;
        min-height: 100vh;
        background: #ffe6e6;
    }

    /* 主体区域使用浅蓝色背景 */
    main {
        flex: 1;
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 0;
        background-color: #e6f7ff;
    }

    /* 页脚背景与整体风格保持一致 */
    footer {
        text-align: center;
        padding: 1rem;
        background-color: #ffe6e6;
    }
</style>
