<template>
    <div class="achievements-layout">
        <!-- 返回按钮 -->
        <div class="back-button-container">
            <el-button type="primary" plain @click="goBackToUserCenter" class="back-button">
                ← 返回用户中心
            </el-button>
        </div>
        
        <!-- 页面标题 -->
        <div class="page-header">
            <h1>我的成就</h1>
            <p>记录您的学习里程碑</p>
        </div>

        <!-- 成就分类 -->
        <div class="achievements-container" v-loading="loading" element-loading-text="正在加载成就数据...">
            <AchievementCategory 
                v-for="category in achievementCategories" 
                :key="category.key"
                :category="category"
                :user-stats="userStats"
                @achievement-click="showAchievementDetail"
            />
        </div>

        <!-- 成就详情弹窗 -->
        <el-dialog v-model="dialogVisible" :title="selectedAchievement?.title" width="50%">
            <div v-if="selectedAchievement" class="achievement-detail">
                <div class="detail-icon">
                    <span class="detail-emoji">🎖️</span>
                </div>
                <div class="detail-info">
                    <p class="detail-description">{{ selectedAchievement.description }}</p>
                    <div class="detail-stats">
                        <p><strong>目标:</strong> {{ selectedAchievement.target }}</p>
                        <p><strong>当前进度:</strong> {{ getCurrentValue(selectedAchievement.category) }}</p>
                        <p><strong>状态:</strong> 
                            <el-tag :type="isUnlocked(selectedAchievement) ? 'success' : 'info'">
                                {{ isUnlocked(selectedAchievement) ? '已解锁' : '未解锁' }}
                            </el-tag>
                        </p>
                        <p v-if="isUnlocked(selectedAchievement)"><strong>解锁时间:</strong> {{ formatUnlockDate(selectedAchievement.unlockedAt) }}</p>
                    </div>
                </div>
            </div>
        </el-dialog>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { useRouter } from 'vue-router'
import { Check } from '@element-plus/icons-vue'
import AchievementCategory from '../components/AchievementCategory.vue'
import { getUserCompletedArticlesCount } from '../api/index'
import api from '../utils/axios'

// 响应式数据
const router = useRouter()
const dialogVisible = ref(false)
const selectedAchievement = ref(null)
const loading = ref(true)

// 用户当前数据（从API获取）
const userStats = ref({
    totalArticles: 0,        // 累计阅读文章数 -> articleCount
    totalWords: 0,          // 累计学习单词数 -> wordCount
    totalOralTime: 0,        // 累计口语练习时间（分钟） -> oralTime
    totalListeningTime: 0,   // 累计听力时间（分钟） -> listeningTime
    dailyMaxArticles: 0,      // 一天最大阅读文章数 -> articlePerDay
    dailyMaxWords: 0,        // 一天最大学习单词数 -> wordPerDay
    dailyMaxOralTime: 0,      // 一天最大口语练习时间（分钟） -> oralPerDay
    dailyMaxListeningTime: 0  // 一天最大听力时间（分钟） -> listeningPerDay
})

// 获取用户ID的函数
const getUserId = () => {
  try {
    const userStr = localStorage.getItem('user')
    if (userStr) {
      const user = JSON.parse(userStr)
      return user.Id || '114514' // 默认使用测试ID
    }
    return '114514' // 默认测试ID
  } catch (error) {
    console.error('获取用户ID失败:', error)
    return '114514' // 默认测试ID
  }
}

// 获取单词学习统计数据
const fetchWordLearningStats = async (userId) => {
  try {
    // 获取总学习单词数 (hasLearned: 1)
    const learnedResponse = await fetch(`/api/Word/user/${userId}?hasLearned=1`)
    if (!learnedResponse.ok) throw new Error('获取已学单词失败')
    const learnedData = await learnedResponse.json()
    const totalWordsLearned = Array.isArray(learnedData.wordIds) ? learnedData.wordIds.length : 0
    
    // 从本地存储获取今日学习单词数（这个通常存储在前端）
    const todayLearned = parseInt(localStorage.getItem('todayLearned') || '0')
    
    return {
      totalWords: totalWordsLearned,
      dailyMaxWords: todayLearned
    }
  } catch (error) {
    console.error('获取单词学习统计失败:', error)
    return {
      totalWords: 0,
      dailyMaxWords: 0
    }
  }
}

// 获取阅读文章统计数据
const fetchReadingStats = async (userId) => {
  try {
    const numericUserId = parseInt(userId)
    const result = await getUserCompletedArticlesCount(numericUserId)
    // 注意：这里只能获取总完成数，单日最大需要其他方式获取
    const todayArticles = parseInt(localStorage.getItem('todayReadArticles') || '0')
    
    return {
      totalArticles: result.completedCount || 0,
      dailyMaxArticles: todayArticles
    }
  } catch (error) {
    console.error('获取阅读统计失败:', error)
    return {
      totalArticles: 0,
      dailyMaxArticles: 0
    }
  }
}

// 获取听力练习统计数据
const fetchListeningStats = async () => {
  try {
    const cntRes = await api.get('/api/listening-practice/completed/count')
    const totalListeningTime = cntRes.data.count || 0
    
    // 从本地存储获取今日听力时间（如果有的话）
    const todayListening = parseInt(localStorage.getItem('todayListeningTime') || '0')
    
    return {
      totalListeningTime: totalListeningTime,
      dailyMaxListeningTime: todayListening
    }
  } catch (error) {
    console.error('获取听力统计失败:', error)
    return {
      totalListeningTime: 0,
      dailyMaxListeningTime: 0
    }
  }
}

// 获取口语练习统计数据（目前可能没有专门的API，使用默认值）
const fetchOralStats = async () => {
  try {
    // 从本地存储获取口语练习数据
    const totalOral = parseInt(localStorage.getItem('totalOralTime') || '0')
    const todayOral = parseInt(localStorage.getItem('todayOralTime') || '0')
    
    return {
      totalOralTime: totalOral,
      dailyMaxOralTime: todayOral
    }
  } catch (error) {
    console.error('获取口语统计失败:', error)
    return {
      totalOralTime: 0,
      dailyMaxOralTime: 0
    }
  }
}

// 加载用户学习记录
const loadUserLearningRecord = async () => {
  try {
    loading.value = true
    const userId = getUserId()
    console.log('正在获取用户学习记录，用户ID:', userId)
    
    // 并行获取各个模块的统计数据
    const [wordStats, readingStats, listeningStats, oralStats] = await Promise.all([
      fetchWordLearningStats(userId),
      fetchReadingStats(userId),
      fetchListeningStats(),
      fetchOralStats()
    ])
    
    console.log('单词学习统计:', wordStats)
    console.log('阅读统计:', readingStats)
    console.log('听力统计:', listeningStats)
    console.log('口语统计:', oralStats)
    
    // 合并所有统计数据
    userStats.value = {
      totalArticles: readingStats.totalArticles,
      totalWords: wordStats.totalWords,
      totalOralTime: oralStats.totalOralTime,
      totalListeningTime: listeningStats.totalListeningTime,
      dailyMaxArticles: readingStats.dailyMaxArticles,
      dailyMaxWords: wordStats.dailyMaxWords,
      dailyMaxOralTime: oralStats.dailyMaxOralTime,
      dailyMaxListeningTime: listeningStats.dailyMaxListeningTime
    }
    
    console.log('最终统计数据:', userStats.value)
    ElMessage.success('成就数据加载完成')
  } catch (error) {
    console.error('加载用户学习记录失败:', error)
    ElMessage.error('加载学习记录失败，显示默认数据')
  } finally {
    loading.value = false
  }
}

// 成就分类配置
const achievementCategories = ref([
    {
        key: 'totalArticles',
        title: '累计阅读文章',
        icon: 'Reading',
        achievements: [
            {
                id: 'articles_1',
                category: 'totalArticles',
                title: '阅读新手',
                description: '累计阅读1篇文章',
                target: 1,
                icon: 'Trophy'
            },
            {
                id: 'articles_5',
                category: 'totalArticles',
                title: '阅读爱好者',
                description: '累计阅读5篇文章',
                target: 5,
                icon: 'Trophy'
            },
            {
                id: 'articles_10',
                category: 'totalArticles',
                title: '阅读达人',
                description: '累计阅读10篇文章',
                target: 10,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'totalWords',
        title: '累计学习单词',
        icon: 'Edit',
        achievements: [
            {
                id: 'words_10',
                category: 'totalWords',
                title: '单词初学者',
                description: '累计学习10个单词',
                target: 10,
                icon: 'School'
            },
            {
                id: 'words_50',
                category: 'totalWords',
                title: '单词收集家',
                description: '累计学习50个单词',
                target: 50,
                icon: 'Star'
            },
            {
                id: 'words_100',
                category: 'totalWords',
                title: '词汇大师',
                description: '累计学习100个单词',
                target: 100,
                icon: 'Star'
            }
        ]
    },
    {
        key: 'totalOralTime',
        title: '累计口语练习',
        icon: 'Microphone',
        achievements: [
            {
                id: 'oral_10',
                category: 'totalOralTime',
                title: '口语新手',
                description: '累计口语练习10分钟',
                target: 10,
                icon: 'VideoPlay'
            },
            {
                id: 'oral_30',
                category: 'totalOralTime',
                title: '口语爱好者',
                description: '累计口语练习30分钟',
                target: 30,
                icon: 'ChatDotRound'
            },
            {
                id: 'oral_60',
                category: 'totalOralTime',
                title: '口语专家',
                description: '累计口语练习60分钟',
                target: 60,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'totalListeningTime',
        title: '累计听力练习',
        icon: 'Headset',
        achievements: [
            {
                id: 'listening_10',
                category: 'totalListeningTime',
                title: '听力新手',
                description: '累计听力练习10分钟',
                target: 10,
                icon: 'Headset'
            },
            {
                id: 'listening_30',
                category: 'totalListeningTime',
                title: '听力达人',
                description: '累计听力练习30分钟',
                target: 30,
                icon: 'Service'
            },
            {
                id: 'listening_60',
                category: 'totalListeningTime',
                title: '听力专家',
                description: '累计听力练习60分钟',
                target: 60,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'dailyMaxArticles',
        title: '单日最大阅读',
        icon: 'Calendar',
        achievements: [
            {
                id: 'daily_articles_1',
                category: 'dailyMaxArticles',
                title: '日读起步',
                description: '单日阅读1篇文章',
                target: 1,
                icon: 'Sunny'
            },
            {
                id: 'daily_articles_3',
                category: 'dailyMaxArticles',
                title: '日读勤奋者',
                description: '单日阅读3篇文章',
                target: 3,
                icon: 'Trophy'
            },
            {
                id: 'daily_articles_5',
                category: 'dailyMaxArticles',
                title: '日读专家',
                description: '单日阅读5篇文章',
                target: 5,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'dailyMaxWords',
        title: '单日最大学词',
        icon: 'ChatLineRound',
        achievements: [
            {
                id: 'daily_words_10',
                category: 'dailyMaxWords',
                title: '单词学习者',
                description: '单日学习10个单词',
                target: 10,
                icon: 'Trophy'
            },
            {
                id: 'daily_words_30',
                category: 'dailyMaxWords',
                title: '记忆达人',
                description: '单日学习30个单词',
                target: 30,
                icon: 'Star'
            },
            {
                id: 'daily_words_50',
                category: 'dailyMaxWords',
                title: '学习专家',
                description: '单日学习50个单词',
                target: 50,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'dailyMaxOralTime',
        title: '单日最大口语',
        icon: 'ChatRound',
        achievements: [
            {
                id: 'daily_oral_5',
                category: 'dailyMaxOralTime',
                title: '口语起步',
                description: '单日口语练习5分钟',
                target: 5,
                icon: 'Clock'
            },
            {
                id: 'daily_oral_15',
                category: 'dailyMaxOralTime',
                title: '口语坚持者',
                description: '单日口语练习15分钟',
                target: 15,
                icon: 'Timer'
            },
            {
                id: 'daily_oral_30',
                category: 'dailyMaxOralTime',
                title: '表达能手',
                description: '单日口语练习30分钟',
                target: 30,
                icon: 'Medal'
            }
        ]
    },
    {
        key: 'dailyMaxListeningTime',
        title: '单日最大听力',
        icon: 'Service',
        achievements: [
            {
                id: 'daily_listening_5',
                category: 'dailyMaxListeningTime',
                title: '听力起步',
                description: '单日听力练习5分钟',
                target: 5,
                icon: 'AlarmClock'
            },
            {
                id: 'daily_listening_15',
                category: 'dailyMaxListeningTime',
                title: '专注听众',
                description: '单日听力练习15分钟',
                target: 15,
                icon: 'UserFilled'
            },
            {
                id: 'daily_listening_30',
                category: 'dailyMaxListeningTime',
                title: '听力专家',
                description: '单日听力练习30分钟',
                target: 30,
                icon: 'Medal'
            }
        ]
    }
])

// 计算方法
const getCurrentValue = (category) => {
    return userStats.value[category] || 0
}

const isUnlocked = (achievement) => {
    return getCurrentValue(achievement.category) >= achievement.target
}

const formatUnlockDate = (dateString) => {
    // 如果有具体的解锁日期，显示该日期
    if (dateString) {
        const date = new Date(dateString)
        return date.toLocaleDateString('zh-CN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        })
    }
    // 如果没有具体日期但成就已解锁，显示今天
    return new Date().toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    })
}

const showAchievementDetail = (achievement) => {
    console.log('显示成就详情:', achievement)
    console.log('当前用户数据:', userStats.value)
    console.log('成就类别值:', getCurrentValue(achievement.category))
    console.log('成就目标:', achievement.target)
    console.log('是否解锁:', isUnlocked(achievement))
    
    selectedAchievement.value = achievement
    dialogVisible.value = true
}

const goBackToUserCenter = () => {
    router.push('/user_center')
}

// 组件挂载
onMounted(() => {
    // 加载用户真实学习数据
    loadUserLearningRecord()
})
</script>

<style scoped>
.achievements-layout {
    width: 90%;
    min-height: 90vh;
    margin: 10px auto 30px auto;
    padding: 30px;
    background-color: #e6f7ff;
    border-radius: 20px;
}

.back-button-container {
    margin-bottom: 20px;
    display: flex;
    justify-content: flex-start;
}

.back-button {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    padding: 8px 16px;
    border-radius: 8px;
    transition: all 0.3s;
}

.back-button:hover {
    background-color: #ff66b3;
    border-color: #ff66b3;
    color: white;
}

.page-header {
    text-align: center;
    margin-bottom: 40px;
}

.page-header h1 {
    color: #ff66b3;
    font-size: 32px;
    margin-bottom: 10px;
    font-weight: 600;
}

.page-header p {
    color: #606266;
    font-size: 16px;
    margin: 0;
}

/* 统计概览卡片 */
.stats-overview {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 20px;
    margin-bottom: 40px;
}

.stat-card {
    background: white;
    border-radius: 12px;
    padding: 20px;
    display: flex;
    align-items: center;
    gap: 15px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    transition: all 0.3s;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
}

.stat-icon {
    width: 50px;
    height: 50px;
    border-radius: 12px;
    background: linear-gradient(135deg, #ff66b3, #ff99cc);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 20px;
}

.stat-info h3 {
    margin: 0;
    font-size: 24px;
    font-weight: 600;
    color: #303133;
}

.stat-info p {
    margin: 5px 0 0 0;
    font-size: 14px;
    color: #606266;
}

/* 成就分类区域 */
.achievements-container {
    display: flex;
    flex-direction: column;
    gap: 40px;
}

/* 弹窗样式 */
.achievement-detail {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 20px;
}

.detail-icon {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background: linear-gradient(135deg, #ff66b3, #ff99cc);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 32px;
}

.detail-emoji {
    font-size: 36px;
}

.detail-info {
    text-align: center;
    width: 100%;
}

.detail-description {
    font-size: 16px;
    color: #303133;
    margin-bottom: 20px;
    line-height: 1.6;
}

.detail-stats {
    background: #f8f9fa;
    padding: 20px;
    border-radius: 8px;
    text-align: left;
}

.detail-stats p {
    margin: 8px 0;
    font-size: 14px;
}

/* 响应式设计 */
@media (max-width: 768px) {
    .achievements-layout {
        width: 95%;
        padding: 20px;
    }
    
    .stats-overview {
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 15px;
    }
}

@media (max-width: 480px) {
    .page-header h1 {
        font-size: 24px;
    }
}
</style>