import { ref, computed, type Ref } from 'vue'

interface UserStats {
  wordsLearned: number
  articlesRead: number
  listeningMinutes: number
  speakingMinutes: number
  totalExperience: number
}

interface UserProgress {
  currentLevel: number
  completedLevels: number[]
  openedChests: number[]
  userStats: UserStats
}

interface ActivityData {
  wordsLearned?: number
  articlesRead?: number
  listeningMinutes?: number
  speakingMinutes?: number
  experience?: number
}

interface ProgressData {
  levelId: number
  completedAt: string
  activityData: ActivityData
}

// 全局状态管理
const userProgress: Ref<UserProgress> = ref({
  currentLevel: 1,
  completedLevels: [],
  openedChests: [],
  userStats: {
    wordsLearned: 0,
    articlesRead: 0,
    listeningMinutes: 0,
    speakingMinutes: 0,
    totalExperience: 0
  }
})

export function useAdventureProgress() {
  // 完成关卡
  const completeLevel = async (levelId: number, activityData: ActivityData = {}): Promise<boolean> => {
    try {
      // 更新本地状态
      if (!userProgress.value.completedLevels.includes(levelId)) {
        userProgress.value.completedLevels.push(levelId)
      }
      
      // 更新用户统计数据
      updateUserStats(activityData)
      
      // 同步到后端
      await syncProgressToServer({
        levelId,
        completedAt: new Date().toISOString(),
        activityData
      })
      
      return true
    } catch (error) {
      console.error('完成关卡失败:', error)
      return false
    }
  }
  
  // 更新用户统计
  const updateUserStats = (data: ActivityData): void => {
    const stats = userProgress.value.userStats
    
    if (data.wordsLearned) stats.wordsLearned += data.wordsLearned
    if (data.articlesRead) stats.articlesRead += data.articlesRead  
    if (data.listeningMinutes) stats.listeningMinutes += data.listeningMinutes
    if (data.speakingMinutes) stats.speakingMinutes += data.speakingMinutes
    if (data.experience) stats.totalExperience += data.experience
  }
  
  // 开启宝箱
  const openChest = async (chestId: number): Promise<boolean> => {
    try {
      if (!userProgress.value.openedChests.includes(chestId)) {
        userProgress.value.openedChests.push(chestId)
      }
      
      await syncChestToServer(chestId)
      return true
    } catch (error) {
      console.error('开启宝箱失败:', error)
      return false
    }
  }
  
  // 同步进度到服务器
  const syncProgressToServer = async (progressData: ProgressData): Promise<any> => {
    // TODO: 替换为实际的API端点
    const response = await fetch('/api/adventure/progress', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify(progressData)
    })
    
    if (!response.ok) {
      throw new Error('同步进度失败')
    }
    
    return response.json()
  }
  
  // 同步宝箱状态到服务器
  const syncChestToServer = async (chestId: number): Promise<any> => {
    const response = await fetch('/api/adventure/chest', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify({
        chestId,
        openedAt: new Date().toISOString()
      })
    })
    
    if (!response.ok) {
      throw new Error('同步宝箱状态失败')
    }
    
    return response.json()
  }
  
  // 从服务器加载用户进度
  const loadUserProgress = async (): Promise<void> => {
    try {
      const response = await fetch('/api/adventure/progress', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })
      
      if (response.ok) {
        const data = await response.json()
        userProgress.value = { ...userProgress.value, ...data }
      }
    } catch (error) {
      console.error('加载用户进度失败:', error)
    }
  }
  
  // 计算属性
  const completedCount = computed(() => userProgress.value.completedLevels.length)
  const currentLevel = computed(() => {
    const maxCompleted = Math.max(...userProgress.value.completedLevels, 0)
    return Math.min(maxCompleted + 1, 15) // 最多15关
  })
  
  return {
    userProgress,
    completeLevel,
    openChest,
    loadUserProgress,
    updateUserStats,
    completedCount,
    currentLevel
  }
}
