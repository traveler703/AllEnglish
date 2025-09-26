<template>
    <div v-if="shouldShowAds && !isClosed" class="advertisement-container">
        <!-- 左侧广告 -->
        <div class="ad-left">
            <div class="ad-close-btn" @click="closeAd('left')">×</div>
            <div class="ad-content">
                <div class="ad-icon">🎯</div>
                <h3>购买课程</h3>
                <p>购买专业课程，助你快速进步</p>
                <div class="ad-cta" @click="handleAdClick('left')">立即购买</div>
                <div class="ad-badge">热门</div>
            </div>
        </div>
        
        <!-- 右侧广告 -->
        <div class="ad-right">
            <div class="ad-close-btn" @click="closeAd('right')">×</div>
            <div class="ad-content">
                <div class="ad-icon">🌟</div>
                <h3>升级用户</h3>
                <p>连续签到七天，成为VIP用户，免看广告，尊享折扣~</p>
                <div class="ad-cta" @click="handleAdClick('right')">进入中心</div>
                <div class="ad-badge">推荐</div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { defineComponent, computed, onMounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'

export default defineComponent({
    name: 'Advertisement',
    setup() {
        const router = useRouter()
        const route = useRoute()
        const userRole = ref<string>('')
        const isClosed = ref(false)
        
        // 在组件挂载时获取用户角色
        onMounted(() => {
            updateUserRole()
        })
        
        // 监听路由变化，重新显示广告
        watch(() => route.path, () => {
            isClosed.value = false
        })
        
        // 监听localStorage变化
        const updateUserRole = () => {
            userRole.value = localStorage.getItem('role') || ''
        }
        
        // 监听storage事件（当其他标签页修改localStorage时）
        const handleStorageChange = (e: StorageEvent) => {
            if (e.key === 'role') {
                updateUserRole()
            }
        }
        
        onMounted(() => {
            window.addEventListener('storage', handleStorageChange)
            // 定期检查角色变化（作为备用方案）
            const interval = setInterval(updateUserRole, 5000)
            
            return () => {
                window.removeEventListener('storage', handleStorageChange)
                clearInterval(interval)
            }
        })
        
        // 计算是否应该显示广告
        const shouldShowAds = computed(() => {
            // 管理员和VIP会员不显示广告
            return userRole.value !== 'admin' && userRole.value !== 'vip'
        })
        
        // 关闭广告
        const closeAd = (side: 'left' | 'right') => {
            isClosed.value = true
        }
        
        // 处理广告点击
        const handleAdClick = (side: 'left' | 'right') => {
            if (side === 'left') {
                // 跳转到课程购买页面
                router.push('/learningresources')
            } else {
                // 跳转到用户中心
                router.push('/user_center')
            }
        }
        
        return {
            shouldShowAds,
            isClosed,
            userRole,
            handleAdClick,
            closeAd
        }
    }
})
</script>

<style scoped>
.advertisement-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    pointer-events: none;
    z-index: 1000;
}

.ad-left, .ad-right {
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    width: 150px;
    height: 300px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: 3px solid #4a90e2;
    border-radius: 15px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: auto;
    transition: all 0.3s ease;
    overflow: hidden;
}

.ad-left {
    left: 20px;
}

.ad-right {
    right: 20px;
}

.ad-left:hover, .ad-right:hover {
    transform: translateY(-50%) scale(1.05);
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.4);
}

.ad-close-btn {
    position: absolute;
    top: 8px;
    right: 8px;
    width: 20px;
    height: 20px;
    background: rgba(255, 255, 255, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.3);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.3s ease;
    z-index: 10;
}

.ad-close-btn:hover {
    background: rgba(255, 255, 255, 0.3);
    border-color: rgba(255, 255, 255, 0.5);
    transform: scale(1.1);
}

.ad-content {
    text-align: center;
    color: white;
    padding: 15px;
    position: relative;
    width: 100%;
}

.ad-icon {
    font-size: 24px;
    margin-bottom: 10px;
    animation: bounce 2s infinite;
}

@keyframes bounce {
    0%, 20%, 50%, 80%, 100% {
        transform: translateY(0);
    }
    40% {
        transform: translateY(-10px);
    }
    60% {
        transform: translateY(-5px);
    }
}

.ad-content h3 {
    margin: 0 0 10px 0;
    font-size: 16px;
    font-weight: 600;
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.ad-content p {
    margin: 0 0 15px 0;
    font-size: 12px;
    line-height: 1.4;
    opacity: 0.9;
}

.ad-cta {
    background: rgba(255, 255, 255, 0.2);
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-radius: 25px;
    padding: 6px 16px;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    backdrop-filter: blur(10px);
    margin-bottom: 10px;
}

.ad-cta:hover {
    background: rgba(255, 255, 255, 0.3);
    border-color: rgba(255, 255, 255, 0.5);
    transform: translateY(-2px);
}

.ad-badge {
    position: absolute;
    top: 10px;
    right: 10px;
    background: linear-gradient(45deg, #ff6b6b, #ff8e53);
    color: white;
    padding: 3px 8px;
    border-radius: 15px;
    font-size: 10px;
    font-weight: 600;
    animation: pulse 2s infinite;
}

@keyframes pulse {
    0% {
        transform: scale(1);
    }
    50% {
        transform: scale(1.1);
    }
    100% {
        transform: scale(1);
    }
}

/* 响应式设计 */
@media (max-width: 1400px) {
    .ad-left, .ad-right {
        width: 160px;
        height: 320px;
    }
    
    .ad-content h3 {
        font-size: 16px;
    }
    
    .ad-content p {
        font-size: 12px;
    }
    
    .ad-icon {
        font-size: 28px;
    }
}

@media (max-width: 1200px) {
    .ad-left, .ad-right {
        width: 140px;
        height: 280px;
    }
    
    .ad-content {
        padding: 15px;
    }
    
    .ad-content h3 {
        font-size: 14px;
        margin-bottom: 10px;
    }
    
    .ad-content p {
        font-size: 11px;
        margin-bottom: 15px;
    }
    
    .ad-cta {
        padding: 6px 16px;
        font-size: 12px;
    }
    
    .ad-icon {
        font-size: 24px;
        margin-bottom: 10px;
    }
}

@media (max-width: 1000px) {
    .advertisement-container {
        display: none;
    }
}
</style> 