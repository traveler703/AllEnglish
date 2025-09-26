<template>
    <div class="navbar-container">
        <!-- 左侧 Home 图标 -->
        <div class="navbar-left" @click="goHome">
            <img src="/ALogo.png" alt="Home" class="home-icon" />
        </div>

        <el-menu class="navbar-center"
                 mode="horizontal"
                 background-color="#e6f7ff"
                 text-color="#333"
                 active-text-color="#ff66b3"
                 :default-active="activeIndex"
                 @select="onSelect">
            <el-menu-item index="/word_learning">Word</el-menu-item>
            <el-menu-item index="/read_learning">Reading</el-menu-item>
            <el-menu-item index="/oral_practice">AI oral</el-menu-item>
            <el-menu-item index="/listening_practice">Listening</el-menu-item>
            <el-menu-item index="/adventure">Adventure</el-menu-item>
            <el-menu-item index="/crossword">Game</el-menu-item>
            <el-menu-item index="/study-plans">Study plan</el-menu-item>
            <el-menu-item index="/learningresources">Resources</el-menu-item>
            <el-menu-item index="/leaderboard">Ranking</el-menu-item>
            <el-menu-item index="/user_center">Account</el-menu-item>
        </el-menu>

        <!-- 右侧退出按钮 -->
        <div class="navbar-right">
            <el-button
                class="logout-btn"
                type="danger"
                size="small"
                round
                @click="logout"
            >
                Log out
            </el-button>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, onMounted, onBeforeUnmount } from 'vue'
    import { useRoute, useRouter } from 'vue-router'
    import axios from 'axios'
    import { ElMessage } from 'element-plus'
    import { Trophy } from '@element-plus/icons-vue'

    export default defineComponent({
        name: 'NavBar',
        components: {
            Trophy
        },
        setup() {
            const router = useRouter();
            const route = useRoute();

            // 用于高亮当前路由
            const activeIndex = ref(route.path);

            // 监听路由变化，更新选中状态
            onMounted(() => {
                activeIndex.value = route.path;
            });

            const onSelect = (index: string) => {
                if (index !== route.path) {
                    router.push(index);
                    activeIndex.value = index;
                }
            };
            
            const goHome = () => {
                router.push("/home");
                activeIndex.value = "/home";
            };

            
            const logout = async () => {
                try {
                    const user = JSON.parse(localStorage.getItem('user') || '{}');
                    const email = user?.Email;

                    if (!email) {
                        ElMessage.error("用户邮箱不存在，无法退出");
                        return;
                    }

                    // 调用后端接口，退出登录
                    const res = await axios.post('/api/User/logout', null, {
                        params: { email }
                    });

                    if (res.status === 200) {
                        ElMessage.success("退出成功");
                    }

                } catch (err) {
                    console.error(err);
                    ElMessage.error("退出时出现错误，请稍后再试");
                } finally {
                    // 清除本地存储中的用户信息和认证信息
                    localStorage.removeItem('user');
                    localStorage.removeItem('isAuthenticated');
                    localStorage.removeItem('role');
                    localStorage.removeItem('token');  // 也清除 token

                    // 跳转到登录页面
                    router.push('/');
                }
            };

            // 在页面关闭时调用后端注销
            const sendLogoutRequest = async () => {
                try {
                    const user = JSON.parse(localStorage.getItem('user') || '{}');
                    const email = user?.Email;

                    if (!email) return;

                    // 调用后端注销 API
                    const res = await axios.post('/api/User/logout', null, {
                        params: { email }
                    });

                    if (res.status === 200) {
                        console.log('Logout request sent successfully');
                    }
                } catch (err) {
                    console.error('Error during logout request:', err);
                }
            };

            // 在组件加载时注册事件，页面关闭时发送注销请求
            onMounted(() => {
                window.addEventListener('beforeunload', sendLogoutRequest);
            });

            // 清理事件监听器
            onBeforeUnmount(() => {
                window.removeEventListener('beforeunload', sendLogoutRequest);
            });

            return {
                onSelect,
                activeIndex,
                logout,
                goHome
            };
        }
    });
</script>

<style scoped>
.navbar-container {
  display: flex;
  align-items: center;
  background-color: #e6f7ff;
  padding: 0 20px;
  border: 0;
  font-weight: 600;
}

.navbar-left {
  display: flex;
  align-items: center;
}

.navbar-center {
  flex: 1;
  display: flex;
  justify-content: center;
  --el-menu-item-font-size: 16px;
}

.navbar-right {
  display: flex;
  align-items: center;
}
    
.home-icon {
  width: 36px;
  height: 36px;
  cursor: pointer;
}

.logout-btn {
  position: absolute;
  right: 80px;
  top: 10px;
  z-index: 10;
  font-size: 16px;
  height: 36px;
  padding: 0 20px;
  border-radius: 10px; /* 圆角 */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15); /* 阴影 */
  transition: all 0.3s ease; /* 平滑过渡 */
}

/* 悬停时阴影更明显 */
.logout-btn:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  transform: translateY(-1px); /* 微微上移 */
}

/* Vue 3 scoped 深度选择器写法 */
:deep(.el-menu--horizontal) {
  border-bottom: none !important;
}


</style>