import axios from 'axios'

/**
 * 严格匹配图片中的API规范
 * @param {number} id - 学习计划ID
 * @returns {Promise<{
 *   success: boolean,
 *   message: string,
 *   data: {
 *     id: number,
 *     title: string,
 *     userId: string,
 *     planDate: string,
 *     duration: number,
 *     isPublic: number,
 *     planType: string,
 *     status: string,
 *     wordCount: number,
 *     wordIds: string,
 *     articleCount: number,
 *     oralTime: number
 *   }
 * }>}
 */
export const getStudyPlanById = async (id) => {
  try {
    const response = await axios.get(`/api/StudyPlan/get-by-id/${id}`, {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      timeout: 5000
    })

    // 严格验证响应数据结构
    if (!response.data || typeof response.data !== 'object') {
      throw new Error('Invalid response structure')
    }

    if (!response.data.success) {
      throw new Error(response.data.message || 'Request failed')
    }

    if (!response.data.data || typeof response.data.data !== 'object') {
      throw new Error('Missing plan data')
    }

    return response.data

  } catch (error) {
    console.error(`[API] 获取学习计划失败 ID:${id}`, {
      error: error.message,
      status: error.response?.status,
      config: error.config
    })
    throw error
  }
}

export const getRandomStudyPlans = async (limit = 3) => {
  try {
    const response = await axios.get(`https://localhost:7071/api/StudyPlan/random?limit=${limit}`)
    return response.data
  } catch (error) {
    throw error
  }
}