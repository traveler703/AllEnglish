// 本地进度管理工具
import { updateUserAdventureStatus, getUserAdventureProgress } from '../api/adventure'

// 用于通知AdventurePath组件刷新数据的回调函数
let refreshCallback: (() => Promise<void>) | null = null

// 设置刷新回调函数
export const setRefreshCallback = (callback: () => Promise<void>) => {
  refreshCallback = callback
}

interface LocalProgress {
  completedLevels: number[]
  openedChests: number[]
  userStats: {
    wordsLearned: number
    articlesRead: number
    listeningMinutes: number
    speakingMinutes: number
    totalExperience: number
  }
  lastUpdated: string
}

const STORAGE_KEY = 'adventure_progress'

// 获取默认进度
const getDefaultProgress = (): LocalProgress => ({
  completedLevels: [1], // 第一关默认解锁
  openedChests: [],
  userStats: {
    wordsLearned: 0,
    articlesRead: 0,
    listeningMinutes: 0,
    speakingMinutes: 0,
    totalExperience: 0
  },
  lastUpdated: new Date().toISOString()
})

// 获取本地进度
export const getLocalProgress = (): LocalProgress => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      return { ...getDefaultProgress(), ...JSON.parse(stored) }
    }
  } catch (error) {
    console.error('读取本地进度失败:', error)
  }
  return getDefaultProgress()
}

// 保存本地进度
export const saveLocalProgress = (progress: Partial<LocalProgress>): void => {
  console.log('=== saveLocalProgress 开始执行 ===')
  console.log('要保存的进度数据:', progress)
  
  try {
    console.log('获取当前进度...')
    const currentProgress = getLocalProgress()
    console.log('当前进度:', currentProgress)
    
    const updatedProgress: LocalProgress = {
      ...currentProgress,
      ...progress,
      lastUpdated: new Date().toISOString()
    }
    console.log('合并后的进度:', updatedProgress)
    
    const jsonString = JSON.stringify(updatedProgress)
    console.log('序列化后的JSON长度:', jsonString.length)
    console.log('序列化后的JSON:', jsonString)
    
    console.log('保存到localStorage...')
    localStorage.setItem(STORAGE_KEY, jsonString)
    console.log('✅ 进度保存成功到 localStorage')
    
    // 验证保存是否成功
    const verification = localStorage.getItem(STORAGE_KEY)
    if (verification) {
      console.log('✅ 验证保存成功，数据长度:', verification.length)
      const parsed = JSON.parse(verification)
      console.log('✅ 验证解析成功，已完成关卡:', parsed.completedLevels)
    } else {
      console.error('❌ 验证失败：无法从localStorage读取数据')
    }
    
  } catch (error) {
    console.error('❌ 保存本地进度失败:', error)
    console.error('错误类型:', error instanceof Error ? error.constructor.name : typeof error)
    console.error('错误消息:', error instanceof Error ? error.message : error)
    if (error instanceof Error && error.stack) {
      console.error('错误堆栈:', error.stack)
    }
  }
}

// 获取用户ID
const getUserId = (): number => {
  try {
    const userData = localStorage.getItem('user')
    if (userData) {
      const parsedData = JSON.parse(userData)
      return parsedData.Id || 114514 // 默认id
    }
  } catch (error) {
    console.error('获取用户ID失败:', error)
  }
  return 114514 // 默认fallback ID
}

// 完成关卡
export const completeLevel = async (levelId: number): Promise<boolean> => {
  console.log(`=== 开始完成关卡 ${levelId} ===`)
  console.log('函数参数:', { levelId, type: typeof levelId })
  
  try {
    console.log('步骤1: 获取本地进度...')
    const progress = getLocalProgress()
    console.log('当前进度:', progress)
    console.log('已完成关卡列表:', progress.completedLevels)
    
    console.log(`步骤2: 检查关卡 ${levelId} 是否已完成...`)
    const isAlreadyCompleted = progress.completedLevels.includes(levelId)
    console.log(`关卡 ${levelId} 是否已完成:`, isAlreadyCompleted)
    
    // 不管是否已完成，都要确保当前关卡在完成列表中
    if (!isAlreadyCompleted) {
      console.log(`步骤3: 关卡 ${levelId} 尚未完成，添加到完成列表`)
      progress.completedLevels.push(levelId)
    } else {
      console.log(`步骤3: 关卡 ${levelId} 已完成，但仍要确保下一关解锁`)
    }
    
    console.log('当前已完成关卡:', progress.completedLevels)
    
    // 步骤4: 调用后端API更新关卡状态
    console.log('步骤4: 调用后端API更新关卡状态...')
    const userId = getUserId()
    console.log('获取到的用户ID:', userId)
    
    try {
      // 将当前关卡设为completed
      console.log(`设置关卡 ${levelId} 为 completed`)
      await updateUserAdventureStatus(userId, levelId, 'completed')
      
      // 将下一关设为unlocked
      const nextLevelId = levelId + 1
      if (nextLevelId <= 15) {
        console.log(`设置关卡 ${nextLevelId} 为 unlocked`)
        await updateUserAdventureStatus(userId, nextLevelId, 'unlocked')
      }
      
      console.log('✅ 后端API调用成功')
      
      // 步骤5: 通知AdventurePath组件刷新数据
      if (refreshCallback) {
        console.log('步骤5: 通知前端组件刷新数据...')
        try {
          await refreshCallback()
          console.log('✅ 前端数据刷新成功')
        } catch (refreshError) {
          console.error('❌ 前端数据刷新失败:', refreshError)
        }
      } else {
        console.warn('⚠️ 未设置刷新回调函数，数据可能不会实时更新')
      }
      
    } catch (apiError) {
      console.error('❌ 后端API调用失败，但继续本地处理:', apiError)
      // 即使API调用失败，也继续本地处理，保证用户体验
    }
    
    console.log('步骤6: 保存进度到localStorage...')
    saveLocalProgress(progress)
    console.log(`✅ 关卡 ${levelId} 处理完成！`)
    return true
  } catch (error) {
    console.error(`❌ 完成关卡 ${levelId} 失败:`, error)
    console.error('错误详情:', {
      levelId,
      error: error instanceof Error ? error.message : error,
      stack: error instanceof Error ? error.stack : undefined
    })
    return false
  }
}

// 开启宝箱
export const openChest = (chestId: number): boolean => {
  try {
    const progress = getLocalProgress()
    
    if (!progress.openedChests.includes(chestId)) {
      progress.openedChests.push(chestId)
      saveLocalProgress(progress)
      return true
    }
    return false
  } catch (error) {
    console.error('开启宝箱失败:', error)
    return false
  }
}

// 更新用户统计
export const updateUserStats = (stats: Partial<LocalProgress['userStats']>): void => {
  try {
    const progress = getLocalProgress()
    
    if (stats.wordsLearned) progress.userStats.wordsLearned += stats.wordsLearned
    if (stats.articlesRead) progress.userStats.articlesRead += stats.articlesRead
    if (stats.listeningMinutes) progress.userStats.listeningMinutes += stats.listeningMinutes
    if (stats.speakingMinutes) progress.userStats.speakingMinutes += stats.speakingMinutes
    if (stats.totalExperience) progress.userStats.totalExperience += stats.totalExperience
    
    saveLocalProgress(progress)
  } catch (error) {
    console.error('更新用户统计失败:', error)
  }
}

// 检查关卡是否解锁
export const isLevelUnlocked = (levelId: number): boolean => {
  const progress = getLocalProgress()
  
  // 第一关总是解锁的
  if (levelId === 1) return true
  
  // 检查前一关是否完成
  return progress.completedLevels.includes(levelId - 1)
}

// 检查关卡是否完成
export const isLevelCompleted = (levelId: number): boolean => {
  const progress = getLocalProgress()
  return progress.completedLevels.includes(levelId)
}

// 检查宝箱是否开启
export const isChestOpened = (chestId: number): boolean => {
  const progress = getLocalProgress()
  return progress.openedChests.includes(chestId)
}

// 获取当前可进行的关卡
export const getCurrentLevel = (): number => {
  const progress = getLocalProgress()
  const maxCompleted = Math.max(...progress.completedLevels, 0)
  return Math.min(maxCompleted + 1, 15)
}

// 重置进度（调试用）
export const resetProgress = (): void => {
  localStorage.removeItem(STORAGE_KEY)
}
