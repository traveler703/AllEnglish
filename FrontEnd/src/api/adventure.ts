import axios from 'axios'

// Adventure API 相关接口类型定义
export interface Adventure {
  id: number
  levelNumber: number
  title: string
  description: string
  type: 'vocabulary' | 'reading' | 'listening' | 'speaking' | 'comprehensive'
  difficulty: 'beginner' | 'intermediate' | 'advanced'
  targetType: string
  targetValue: number
  routePath: string
  routeParams: string
  icon: string
  rewardExp: number
  rewardCoins: number
  isActive: boolean
}

// 用户学习记录接口类型定义
export interface UserLearningRecord {
  userId: string
  articleCount: number      // 累计阅读文章数
  wordCount: number        // 累计学习单词数
  oralTime: number         // 累计口语练习时间（分钟）
  listeningTime: number    // 累计听力时间（分钟）
  articlePerDay: number    // 单日最大阅读文章数
  wordPerDay: number       // 单日最大学习单词数
  oralPerDay: number       // 单日最大口语练习时间（分钟）
  listeningPerDay: number  // 单日最大听力时间（分钟）
}

export interface AdventureTreasure {
  id: number
  levelNumber: number
  title: string
  description: string
  rewards: string // JSON string
  icon: string
  isActive: boolean
}

export interface UserAdventureProgress {
  id: number
  userId: number
  adventureId: number
  status: 'locked' | 'unlocked' | 'completed'
  score?: number
  completionTime?: number
  attempts: number
  bestScore?: number
  completedAt?: string
  firstAttemptAt?: string
}

export interface UserAdventureTreasure {
  id: number
  userId: number
  treasureId: number
  openedAt: string
  rewardsReceived: string // JSON string
}

// API 接口函数

/**
 * 获取所有关卡信息
 */
export const getAllAdventures = async (): Promise<Adventure[]> => {
  try {
    const response = await axios.get('/api/Adventure')
    return response.data
  } catch (error) {
    console.error('获取关卡信息失败:', error)
    throw error
  }
}

/**
 * 获取所有宝箱信息
 */
export const getAllTreasures = async (): Promise<AdventureTreasure[]> => {
  try {
    const response = await axios.get('/api/AdventureTreasure')
    return response.data
  } catch (error) {
    console.error('获取宝箱信息失败:', error)
    throw error
  }
}

/**
 * 获取用户关卡进度
 */
export const getUserAdventureProgress = async (userId: number): Promise<UserAdventureProgress[]> => {
  try {
    const response = await axios.get(`/api/UserAdventure/${userId}`)
    return response.data
  } catch (error) {
    console.error('获取用户关卡进度失败:', error)
    throw error
  }
}

/**
 * 完成关卡
 */
export const completeAdventure = async (
  userId: number, 
  adventureId: number, 
  data: {
    score?: number
    completionTime?: number
  }
): Promise<{ success: boolean, rewards?: any }> => {
  try {
    const response = await axios.post(`/api/UserAdventure/${userId}/${adventureId}/complete`, data)
    return response.data
  } catch (error) {
    console.error('完成关卡失败:', error)
    throw error
  }
}

/**
 * 获取用户已开启的宝箱
 */
export const getUserOpenedTreasures = async (userId: number): Promise<UserAdventureTreasure[]> => {
  try {
    const response = await axios.get(`/api/UserAdventureTreasure/${userId}`)
    return response.data || []
  } catch (error: any) {
    console.error('获取用户宝箱信息失败:', error)
    // 如果接口不存在或发生错误，返回空数组作为默认值
    if (error.response?.status === 404) {
      console.warn('宝箱接口不存在(404)，返回空数组')
    } else {
      console.warn(`宝箱接口错误(${error.response?.status || 'unknown'})，返回空数组`)
    }
    return []
  }
}

/**
 * 开启宝箱
 */
export const openTreasure = async (
  userId: number, 
  treasureId: number
): Promise<{ success: boolean, rewards: any }> => {
  try {
    const response = await axios.post(`/api/UserAdventureTreasure/${userId}/${treasureId}/open`)
    return response.data
  } catch (error) {
    console.error('开启宝箱失败:', error)
    throw error
  }
}

/**
 * 更新用户关卡状态
 */
export const updateUserAdventureStatus = async (
  userId: number,
  adventureId: number,
  status: 'locked' | 'unlocked' | 'completed'
): Promise<{ success: boolean }> => {
  try {
    console.log(`调用API更新关卡状态: userId=${userId}, adventureId=${adventureId}, status=${status}`)
    const response = await axios.put(
      `/api/UserAdventure/${userId}/${adventureId}`,
      JSON.stringify(status),
      {
        headers: {
          'accept': '*/*',
          'Content-Type': 'application/json'
        }
      }
    )
    console.log('API响应:', response.data)
    return { success: true }
  } catch (error) {
    console.error('更新关卡状态失败:', error)
    throw error
  }
}

/**
 * 获取用户学习记录
 */
export const getUserLearningRecord = async (userId: string): Promise<UserLearningRecord> => {
  try {
    const response = await axios.get(`https://localhost:7071/api/UserLearningRecord/${userId}`)
    if (response.data.success) {
      return response.data.data
    } else {
      throw new Error(response.data.message || '获取用户学习记录失败')
    }
  } catch (error: any) {
    console.error('获取用户学习记录失败:', error)
    // 返回默认值，避免页面崩溃
    return {
      userId: userId,
      articleCount: 0,
      wordCount: 0,
      oralTime: 0,
      listeningTime: 0,
      articlePerDay: 0,
      wordPerDay: 0,
      oralPerDay: 0,
      listeningPerDay: 0
    }
  }
}
