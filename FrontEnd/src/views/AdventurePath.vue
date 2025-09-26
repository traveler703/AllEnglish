<template>
  <div class="adventure-learning">
    <!-- 页面头部 -->
    <header class="page-header">
      <h1>🗺️ 英语闯关之旅</h1>
      <p>通过完成各种英语挑战来提升您的语言技能，解锁更多关卡获得奖励</p>
      <div class="progress-stats">
        <el-tag type="success" size="large" effect="light">
          <el-icon class="stats-icon"><Trophy /></el-icon>
          当前进度: {{ completedLevels }}/{{ totalLevels }}
        </el-tag>
        <div class="progress-bar-wrapper">
          <el-progress 
            :percentage="totalLevels > 0 ? Math.round((completedLevels / totalLevels) * 100) : 0"
            :stroke-width="8"
            status="success"
            striped
            striped-flow
          />
        </div>
      </div>
    </header>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-container">
      <el-skeleton rows="3" animated />
      <el-skeleton rows="3" animated />
      <el-skeleton rows="3" animated />
    </div>

    <!-- 关卡列表 -->
    <div v-else class="levels-container">

      <!-- 关卡网格 -->
      <el-row :gutter="20" class="levels-grid">
        <el-col 
          v-for="level in filteredLevels" 
          :key="level.id"
          :xs="24" 
          :sm="12" 
          :md="8" 
          :lg="6"
        >
          <el-card 
            class="level-card"
            :class="[
              level.difficulty,
              { 
                'unlocked': level.unlocked, 
                'completed': level.completed,
                'current': isCurrentLevel(level)
              }
            ]"
            shadow="hover"
            @click="handleLevelClick(level)"
          >
            <div class="level-content">
              <!-- 关卡图标和编号 -->
              <div class="level-header">
                <div class="level-icon">{{ level.icon }}</div>
                <div class="level-number">{{ level.id }}</div>
              </div>
              
              <!-- 关卡信息 -->
              <div class="level-info">
                <h3 class="level-title">{{ level.title }}</h3>
                <p class="level-description">{{ level.description }}</p>
                
                <!-- 难度标签 -->
                <el-tag 
                  :type="getDifficultyTagType(level.difficulty)"
                  size="small"
                  class="difficulty-tag"
                >
                  {{ getDifficultyText(level.difficulty) }}
                </el-tag>
              </div>

              <!-- 状态覆盖层 -->
              <div v-if="!level.unlocked" class="status-overlay locked">
                <el-icon class="status-icon"><Lock /></el-icon>
                <span>未解锁</span>
              </div>
              
              <div v-else-if="level.completed" class="status-overlay completed">
                <el-icon class="status-icon"><SuccessFilled /></el-icon>
                <span>已完成</span>
              </div>
              
              <div v-else-if="isCurrentLevel(level)" class="status-overlay current">
                <el-icon class="status-icon"><VideoPlay /></el-icon>
                <span>开始挑战</span>
              </div>

              <!-- 宝箱按钮 -->
              <el-button
                v-if="level.hasChest"
                class="treasure-button"
                :class="{ 'opened': isChestOpenedLocal(level.id) }"
                :type="isChestOpenedLocal(level.id) ? 'info' : 'warning'"
                :icon="isChestOpenedLocal(level.id) ? 'FolderOpened' : 'Present'"
                circle
                size="small"
                @click.stop="openChest(level.id)"
                :disabled="!level.completed || isChestOpenedLocal(level.id)"
              >
              </el-button>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 空状态 -->
      <el-empty v-if="filteredLevels.length === 0 && !loading" description="暂无匹配的关卡" />
    </div>

    <!-- 宝箱奖励弹窗 -->
    <el-dialog
      v-model="showChestModal"
      title="🎉 恭喜获得奖励！"
      width="400px"
      align-center
      destroy-on-close
    >
      <div class="rewards-container">
        <el-space direction="vertical" size="large" fill>
          <div v-for="reward in currentChestRewards" :key="reward" class="reward-item">
            <el-tag type="success" size="large" effect="dark">{{ reward }}</el-tag>
          </div>
        </el-space>
      </div>
      <template #footer>
        <el-button type="primary" size="large" @click="closeChestModal">
          <el-icon class="el-icon--left"><Check /></el-icon>
          领取奖励
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { 
  Search, 
  Trophy, 
  Lock, 
  SuccessFilled, 
  VideoPlay, 
  Check,
  Present,
  FolderOpened
} from '@element-plus/icons-vue'
import { 
  getAllAdventures,
  getAllTreasures,
  getUserAdventureProgress,
  getUserOpenedTreasures,
  completeAdventure,
  openTreasure
} from '../api/adventure'
import { setRefreshCallback } from '../utils/localProgress'

const router = useRouter()

// 响应式数据
const adventures = ref([])
const treasures = ref([])
const userProgress = ref([])
const userTreasures = ref([])
const showChestModal = ref(false)
const currentChestRewards = ref([])
const loading = ref(true)
const userId = ref(0)

// 筛选和搜索
const difficultyFilter = ref('')
const searchTerm = ref('')

// 获取用户ID
const getUserId = () => {
  try {
    const userData = localStorage.getItem('user')
    if (userData) {
      const parsedData = JSON.parse(userData)
      return parsedData.Id || parsedData.id || 0
    }
  } catch (error) {
    console.error('获取用户ID失败:', error)
  }
  return 0
}

// 计算属性
const totalLevels = computed(() => adventures.value.length)
const completedLevels = computed(() => {
  return userProgress.value.filter(progress => progress.status === 'completed').length
})

// 转换后端数据为前端使用的格式
const levels = computed(() => {
  console.log('🎯 计算属性 levels 被触发')
  console.log('📊 adventures.value:', adventures.value.length, '个关卡')
  console.log('📊 userProgress.value:', userProgress.value.length, '个进度记录')
  
  const result = adventures.value.map(adventure => {
    const progress = userProgress.value.find(p => p.adventureId === adventure.id)
    const routeParams = adventure.routeParams ? JSON.parse(adventure.routeParams) : {}
    
    const levelData = {
      id: adventure.levelNumber,
      type: adventure.type,
      title: adventure.title,
      description: adventure.description,
      target: adventure.targetValue,
      route: adventure.routePath,
      difficulty: adventure.difficulty,
      icon: adventure.icon,
      unlocked: progress ? progress.status !== 'locked' : adventure.levelNumber === 1,
      completed: progress ? progress.status === 'completed' : false,
      hasChest: treasures.value.some(t => t.levelNumber === adventure.levelNumber),
      ...routeParams
    }
    
    // 详细日志每个关卡的状态计算
    if (adventure.levelNumber <= 3) { // 只打印前3个关卡避免日志过多
      console.log(`🎮 关卡 ${adventure.levelNumber}:`, {
        adventureId: adventure.id,
        progress: progress,
        unlocked: levelData.unlocked,
        completed: levelData.completed
      })
    }
    
    return levelData
  }).sort((a, b) => a.id - b.id)
  
  console.log('🎯 计算属性 levels 计算完成，共', result.length, '个关卡')
  return result
})

// 筛选后的关卡列表
const filteredLevels = computed(() => {
  let filtered = levels.value

  // 难度筛选
  if (difficultyFilter.value) {
    filtered = filtered.filter(level => level.difficulty === difficultyFilter.value)
  }

  // 关键词搜索
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    filtered = filtered.filter(level => 
      level.title.toLowerCase().includes(term) || 
      level.description.toLowerCase().includes(term)
    )
  }

  return filtered
})

// 获取难度标签类型
const getDifficultyTagType = (difficulty) => {
  const types = {
    'easy': 'success',
    'medium': 'warning', 
    'hard': 'danger',
    'expert': ''
  }
  return types[difficulty] || 'info'
}

// 获取难度文本
const getDifficultyText = (difficulty) => {
  const texts = {
    'easy': '简单',
    'medium': '中等',
    'hard': '困难', 
    'expert': '专家'
  }
  return texts[difficulty] || '未知'
}

// 判断是否为当前关卡
const isCurrentLevel = (level) => {
  const progress = userProgress.value.find(p => p.adventureId === adventures.value.find(a => a.levelNumber === level.id)?.id)
  return progress ? progress.status === 'unlocked' : level.id === 1
}

// 判断宝箱是否已开启
const isChestOpenedLocal = (levelId) => {
  const treasure = treasures.value.find(t => t.levelNumber === levelId)
  if (!treasure) return false
  return userTreasures.value.some(ut => ut.treasureId === treasure.id)
}

// 处理关卡点击
const handleLevelClick = async (level) => {
  const adventure = adventures.value.find(a => a.levelNumber === level.id)
  if (!adventure) return
  
  const progress = userProgress.value.find(p => p.adventureId === adventure.id)
  const status = progress ? progress.status : (level.id === 1 ? 'unlocked' : 'locked')
  
  if (status === 'locked') {
    ElMessage.warning('请先完成前面的关卡！')
    return
  }
  
  if (status === 'completed') {
    ElMessage.info('该关卡已完成！')
    return
  }
  
  try {
    // 跳转到对应页面
    const routeParams = adventure.routeParams ? JSON.parse(adventure.routeParams) : {}
    await router.push({
      path: adventure.routePath,
      query: { 
        levelId: level.id,
        target: adventure.targetValue,
        type: adventure.type,
        ...routeParams
      }
    })
  } catch (error) {
    console.error('路由跳转失败:', error)
    ElMessage.error('跳转失败，请重试')
  }
}

// 开启宝箱
const openChest = async (levelId) => {
  const treasure = treasures.value.find(t => t.levelNumber === levelId)
  if (!treasure) return
  
  // 检查宝箱是否已开启
  if (userTreasures.value.some(ut => ut.treasureId === treasure.id)) {
    ElMessage.info('宝箱已经开启过了！')
    return
  }
  
  // 检查对应关卡是否完成
  const adventure = adventures.value.find(a => a.levelNumber === levelId)
  if (!adventure) return
  
  const progress = userProgress.value.find(p => p.adventureId === adventure.id)
  if (!progress || progress.status !== 'completed') {
    ElMessage.warning('请先完成该关卡！')
    return
  }
  
  try {
    // 调用后端API开启宝箱
    const result = await openTreasure(userId.value, treasure.id)
    if (result.success) {
      // 更新本地状态
      userTreasures.value.push({
        id: Date.now(),
        userId: userId.value,
        treasureId: treasure.id,
        openedAt: new Date().toISOString(),
        rewardsReceived: JSON.stringify(result.rewards)
      })
      
      // 显示奖励弹窗
      const rewards = treasure.rewards ? JSON.parse(treasure.rewards) : {}
      currentChestRewards.value = Array.isArray(rewards) ? rewards : Object.values(rewards)
      showChestModal.value = true
      
      ElMessage.success('宝箱开启成功！')
    }
  } catch (error) {
    console.error('开启宝箱失败:', error)
    ElMessage.error('开启宝箱失败，请重试')
  }
}

// 关闭宝箱弹窗
const closeChestModal = () => {
  showChestModal.value = false
  currentChestRewards.value = []
}

// 完成关卡的方法（供其他页面调用）
const completeLevelLocal = async (levelId, data = {}) => {
  try {
    const adventure = adventures.value.find(a => a.levelNumber === levelId)
    if (!adventure) {
      console.error('找不到对应的关卡:', levelId)
      return false
    }
    
    // 调用后端API完成关卡
    const result = await completeAdventure(userId.value, adventure.id, data)
    if (result.success) {
      // 重新加载用户进度
      await loadUserProgress()
      
      ElMessage.success(`恭喜完成关卡：${adventure.title}！`)
      return true
    }
  } catch (error) {
    console.error('完成关卡失败:', error)
    ElMessage.error('完成关卡失败，请重试')
  }
  return false
}

// 加载所有数据
const loadAllData = async () => {
  console.error('🚨🚨🚨 LOADALLDATA 开始执行 🚨🚨🚨')
  
  try {
    loading.value = true
    
    console.error('📦 开始并行加载基础数据...')
    // 并行加载基础数据
    const [adventuresData, treasuresData] = await Promise.all([
      getAllAdventures(),
      getAllTreasures()
    ])
    
    console.error('✅ 基础数据加载完成')
    console.error('- 关卡数据:', adventuresData.length, '个')
    console.error('- 宝箱数据:', treasuresData.length, '个')
    
    adventures.value = adventuresData
    treasures.value = treasuresData
    
    console.error('🔍 检查用户ID状态:', {
      userId: userId.value,
      type: typeof userId.value,
      isValid: userId.value > 0
    })
    
    // 如果用户已登录，加载用户相关数据
    console.error('✅ 用户ID有效，即将调用loadUserProgress')
    await loadUserProgress()
    console.error('✅ loadUserProgress调用完毕')
  } catch (error) {
    console.error('❌❌❌ LOADALLDATA 发生错误 ❌❌❌')
    console.error('错误详情:', error)
    console.error('错误消息:', error.message)
    console.error('错误堆栈:', error.stack)
    ElMessage.error('加载数据失败，请刷新页面重试')
  } finally {
    loading.value = false
    console.error('📚 LOADALLDATA 执行完毕，loading设为false')
  }
}

// 加载用户进度
const loadUserProgress = async () => {
  console.error('🚀🚀🚀 LOADUSERPROGRESS 开始执行 🚀🚀🚀')
  console.error('📊 当前用户ID:', userId.value)
  console.error('📊 用户ID类型:', typeof userId.value)
  console.error('📊 用户ID是否有效:', userId.value > 0)
  
  try {
    console.error('📡 准备调用后端API获取用户进度数据...')
    console.error('📡 即将并行调用两个API:')
    console.error('- getUserAdventureProgress(userId:', userId.value, ')')
    console.error('- getUserOpenedTreasures(userId:', userId.value, ')')
    
    const [progressData, treasureData] = await Promise.all([
      getUserAdventureProgress(userId.value),
      getUserOpenedTreasures(userId.value)
    ])
    
    console.error('✅ API调用完成，分析返回数据:')
    console.error('📋 后端返回的进度数据:')
    console.error('- 数据类型:', typeof progressData)
    console.error('- 数据长度:', Array.isArray(progressData) ? progressData.length : 'Not Array')
    console.error('- 原始数据:', JSON.stringify(progressData, null, 2))
    
    console.error('📋 后端返回的宝箱数据:')
    console.error('- 数据类型:', typeof treasureData)
    console.error('- 数据长度:', Array.isArray(treasureData) ? treasureData.length : 'Not Array')
    console.error('- 原始数据:', JSON.stringify(treasureData, null, 2))
    
    console.error('📋 更新前的前端进度数据:', JSON.stringify(userProgress.value, null, 2))
    console.error('📋 更新前的前端宝箱数据:', JSON.stringify(userTreasures.value, null, 2))
    
    // 强制更新数据
    console.error('📄 开始强制更新数据...')
    userProgress.value = [...progressData]
    userTreasures.value = [...treasureData]
    
    console.error('✅ 数据更新完成')
    console.error('📋 更新后的前端进度数据:', JSON.stringify(userProgress.value, null, 2))
    console.error('📋 更新后的前端宝箱数据:', JSON.stringify(userTreasures.value, null, 2))
    
    // 延迟一点时间确保响应式更新完成
    setTimeout(() => {
      console.error('🔍 验证响应式更新结果:')
      console.error('- 当前adventures数据:', adventures.value.length, '个关卡')
      console.error('- 当前userProgress数据:', userProgress.value.length, '个进度记录')
      console.error('- 计算属性levels结果:', levels.value.map(l => ({
        id: l.id, 
        title: l.title,
        unlocked: l.unlocked, 
        completed: l.completed
      })))
      console.error('🎯 详细关卡状态分析:')
      levels.value.forEach(level => {
        console.error(`关卡${level.id}: ${level.title} - 解锁:${level.unlocked} 完成:${level.completed}`)
      })
    }, 100)
    
  } catch (error) {
    console.error('❌❌❌ LOADUSERPROGRESS 发生错误 ❌❌❌')
    console.error('错误类型:', error.constructor?.name)
    console.error('错误消息:', error.message)
    console.error('错误响应:', error.response?.data)
    console.error('错误状态码:', error.response?.status)
    console.error('完整错误对象:', error)
    console.error('错误堆栈:', error.stack)
    
    // 设置默认进度
    console.error('🔧 设置默认进度数据...')
    userProgress.value = adventures.value.length > 0 ? [{
      id: 0,
      userId: userId.value,
      adventureId: adventures.value[0].id,
      status: 'unlocked',
      attempts: 0
    }] : []
    userTreasures.value = []
    
    console.error('📋 默认进度设置完成:', JSON.stringify(userProgress.value, null, 2))
  }
  
  console.error('📚 LOADUSERPROGRESS 执行完毕')
}

// 暴露方法供外部调用
defineExpose({
  completeLevel: completeLevelLocal,
  refreshProgress: loadUserProgress,
  forceRefresh: () => {
    console.log('手动触发数据刷新')
    return loadUserProgress()
  }
})

// 组件挂载时初始化
onMounted(async () => {
  console.log('🚀 AdventurePath 组件开始挂载')
  
  // 获取用户ID
  userId.value = getUserId()
  console.log('👤 获取用户ID:', userId.value)
  
  // 注册刷新回调函数
  setRefreshCallback(loadUserProgress)
  console.log('🔗 已注册数据刷新回调函数')
  
  // 加载所有数据
  console.log('📦 开始加载所有数据...')
  await loadAllData()
  console.log('✅ 所有数据加载完成')
  
  // 最终验证
  setTimeout(() => {
    console.log('🔍 最终数据状态验证:')
    console.log('- 用户ID:', userId.value)
    console.log('- 关卡数据:', adventures.value.length, '个')
    console.log('- 用户进度:', userProgress.value.length, '个')
    console.log('- 计算后的关卡状态:', levels.value.slice(0, 5).map(l => ({
      id: l.id,
      title: l.title, 
      unlocked: l.unlocked,
      completed: l.completed
    })))
  }, 500)
})
</script>

<style scoped>
.adventure-learning {
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #409EFF;
  font-size: 28px;
  margin-bottom: 10px;
}

.page-header p {
  color: #606266;
  font-size: 16px;
  margin-bottom: 20px;
}

.progress-stats {
  margin-top: 15px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
}

.progress-bar-wrapper {
  width: 100%;
  max-width: 400px;
}

.filter-card {
  margin-bottom: 20px;
  border-radius: 8px;
}

.search-input {
  width: 250px;
}

.loading-container {
  padding: 20px;
}

.levels-container {
  width: 100%;
}

.levels-grid {
  margin-top: 20px;
}

.level-card {
  cursor: pointer;
  transition: all 0.3s ease;
  border-radius: 12px;
  position: relative;
  overflow: hidden;
  min-height: 200px;
}

.level-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15);
}

.level-card.easy {
  border-left: 4px solid #67C23A;
}

.level-card.medium {
  border-left: 4px solid #E6A23C;
}

.level-card.hard {
  border-left: 4px solid #F56C6C;
}

.level-card.expert {
  border-left: 4px solid #9C27B0;
}

.level-card.unlocked {
  background: linear-gradient(135deg, #f6ffed 0%, #f0f9ff 100%);
}

.level-card.completed {
  background: linear-gradient(135deg, #fff2e8 0%, #fff7ed 100%);
}

.level-card.current {
  background: linear-gradient(135deg, #e6f7ff 0%, #f0f9ff 100%);
  border-color: #409EFF;
  animation: pulse 2s ease-in-out infinite alternate;
}

@keyframes pulse {
  from { 
    box-shadow: 0 4px 12px rgba(64, 158, 255, 0.2);
  }
  to { 
    box-shadow: 0 8px 25px rgba(64, 158, 255, 0.4);
  }
}

.level-content {
  padding: 20px;
  height: 100%;
  display: flex;
  flex-direction: column;
  position: relative;
}

.level-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 15px;
}

.level-icon {
  font-size: 32px;
  filter: drop-shadow(2px 2px 4px rgba(0,0,0,0.1));
}

.level-number {
  background: linear-gradient(135deg, #409EFF, #667eea);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 16px;
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.3);
}

.level-info {
  flex: 1;
  text-align: left;
}

.level-title {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}

.level-description {
  color: #606266;
  font-size: 14px;
  line-height: 1.4;
  margin-bottom: 12px;
}

.difficulty-tag {
  margin-top: auto;
}

.status-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
  backdrop-filter: blur(2px);
  border-radius: 8px;
}

.status-overlay.locked {
  background: rgba(0, 0, 0, 0.6);
}

.status-overlay.completed {
  background: rgba(103, 194, 58, 0.9);
}

.status-overlay.current {
  background: rgba(64, 158, 255, 0.9);
}

.status-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.treasure-button {
  position: absolute;
  top: 15px;
  right: 15px;
  z-index: 10;
}

.treasure-button.opened {
  opacity: 0.6;
}

.rewards-container {
  text-align: center;
  padding: 20px 0;
}

.reward-item {
  display: flex;
  justify-content: center;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .page-header h1 {
    font-size: 24px;
  }
  
  .levels-grid {
    margin-top: 15px;
  }
  
  .level-card {
    min-height: 180px;
  }
  
  .level-content {
    padding: 15px;
  }
  
  .level-icon {
    font-size: 24px;
  }
  
  .level-title {
    font-size: 16px;
  }
  
  .level-description {
    font-size: 13px;
  }
  
  .search-input {
    width: 200px;
  }
}

@media (max-width: 480px) {
  .page-header h1 {
    font-size: 20px;
  }
  
  .level-card {
    min-height: 160px;
  }
  
  .level-content {
    padding: 12px;
  }
  
  .level-icon {
    font-size: 20px;
  }
  
  .level-number {
    width: 28px;
    height: 28px;
    font-size: 14px;
  }
  
  .level-title {
    font-size: 14px;
  }
  
  .level-description {
    font-size: 12px;
  }
  
  .search-input {
    width: 150px;
  }
}
</style>