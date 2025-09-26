<template>
  <div class="study-plan-search">
    <!-- 页面标题 -->
    <h2 class="page-title">学习计划查询</h2>
    
    <!-- 搜索表单和自定义计划按钮 -->
    <div class="search-form">
      <el-form :inline="true">
        <el-form-item label="计划ID">
          <el-input
            v-model.number="searchId"
            placeholder="请输入计划ID"
            clearable
            @clear="resetSearch"
          />
        </el-form-item>
        <el-form-item>
          <el-button
            type="primary"
            :disabled="!searchId"
            @click="handleSearch"
            :loading="loading"
          >
            查询
          </el-button>
        </el-form-item>
        <el-form-item>
          <el-button
            type="success"
            @click="handleCreateCustomPlan"
            :icon="Plus"
          >
            自定义学习计划
          </el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <el-icon class="loading-icon"><Loading /></el-icon>
      正在查询中...
    </div>

    <!-- 错误提示 -->
    <div v-if="errorMessage" class="error-message">
      <el-alert :title="errorMessage" type="error" show-icon />
    </div>

    <!-- 计划详情展示 -->
    <div v-if="currentPlan" class="plan-detail">
      <h3 class="plan-title">{{ currentPlan.title }}</h3>
      
      <div class="plan-meta">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="计划ID">{{ currentPlan.id }}</el-descriptions-item>
          <el-descriptions-item label="用户ID">{{ currentPlan.userId }}</el-descriptions-item>
          <el-descriptions-item label="计划日期">{{ formatDate(currentPlan.planDate) }}</el-descriptions-item>
          <el-descriptions-item label="持续时间">{{ currentPlan.duration }}天</el-descriptions-item>
          <el-descriptions-item label="计划类型">
            <el-tag :type="getPlanTypeTag(currentPlan.planType)">
              {{ currentPlan.planType }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="getStatusTag(currentPlan.status)">
              {{ currentPlan.status }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="单词数量">{{ currentPlan.wordCount }}</el-descriptions-item>
          <el-descriptions-item label="文章数量">{{ currentPlan.articleCount }}</el-descriptions-item>
          <el-descriptions-item label="口语练习(分钟)">{{ currentPlan.oralTime }}</el-descriptions-item>
          <el-descriptions-item label="单词ID列表">
            <el-popover placement="top" width="300" trigger="hover">
              <template #reference>
                <el-tag>查看ID列表</el-tag>
              </template>
              <div class="word-ids-list">
                {{ formatWordIds(currentPlan.wordIds) }}
              </div>
            </el-popover>
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-if="!currentPlan && !loading && !errorMessage" class="empty-state">
      <el-empty description="请输入计划ID查询学习计划详情" />
    </div>

    <!-- 推荐学习计划 -->
    <div class="recommend-plan-section">
      <h3 class="recommend-title">推荐学习计划</h3>
      <el-button
        type="primary"
        @click="fetchRandomPlans"
        :loading="recommendLoading"
        :icon="Refresh"
      >
        换一批
      </el-button>
      
      <div v-if="recommendLoading" class="loading-state">
        <el-icon class="loading-icon"><Loading /></el-icon>
        正在加载推荐计划...
      </div>
      
      <div v-if="recommendError" class="error-message">
        <el-alert :title="recommendError" type="error" show-icon />
      </div>
      
      <div v-if="recommendPlans.length > 0" class="recommend-plans">
        <el-row :gutter="20">
          <el-col
            v-for="plan in recommendPlans"
            :key="plan.id"
            :xs="24"
            :sm="12"
            :md="8"
          >
            <el-card class="plan-card" shadow="hover" @click="selectRecommendPlan(plan)">
              <div class="card-header">
                <h4>{{ plan.title }}</h4>
                <el-tag :type="getPlanTypeTag(plan.planType)" size="small">
                  {{ plan.planType }}
                </el-tag>
              </div>
              <div class="card-content">
                <p><span class="label">持续时间:</span> {{ plan.duration }}天</p>
                <p><span class="label">单词数量:</span> {{ plan.wordCount }}</p>
                <p><span class="label">文章数量:</span> {{ plan.articleCount }}</p>
                <p><span class="label">口语练习:</span> {{ plan.oralTime }}分钟</p>
              </div>
              <div class="card-footer">
                <el-tag :type="getStatusTag(plan.status)" size="small">
                  {{ plan.status }}
                </el-tag>
                <span class="plan-date">{{ formatDate(plan.planDate) }}</span>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>
      
      <div v-if="!recommendLoading && recommendPlans.length === 0 && !recommendError" class="empty-state">
        <el-empty description="暂无推荐学习计划" />
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { Loading, Plus, Refresh } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import { getStudyPlanById, getRandomStudyPlans } from '@/api/studyPlan'

export default {
  name: 'StudyPlanSearch',
  components: { Loading },
  setup() {
    const router = useRouter()
    const searchId = ref('')
    const loading = ref(false)
    const errorMessage = ref('')
    const currentPlan = ref(null)
    
    // 新增推荐计划相关状态
    const recommendPlans = ref([])
    const recommendLoading = ref(false)
    const recommendError = ref('')
    const recommendLimit = ref(3) // 默认获取3条推荐计划

    // 处理搜索
    const handleSearch = async () => {
      if (!searchId.value) return
      
      loading.value = true
      errorMessage.value = ''
      currentPlan.value = null

      try {
        const response = await getStudyPlanById(searchId.value)
        if (response.success) {
          currentPlan.value = response.data
        }
      } catch (error) {
        errorMessage.value = `查询失败: ${error.message}`
        console.error('搜索错误:', error)
      } finally {
        loading.value = false
      }
    }

    // 重置搜索
    const resetSearch = () => {
      searchId.value = ''
      currentPlan.value = null
      errorMessage.value = ''
    }

    // 跳转到自定义学习计划页面
    const handleCreateCustomPlan = () => {
      console.log('跳转到自定义学习计划')
      router.push({ name: 'CustomStudyPlan' }).catch(err => {
        console.error('路由跳转失败:', err)
        router.push('/study-plan/custom').catch(error => {
          console.error('备用路由跳转也失败:', error)
        })
      })
    }

    // 格式化日期
    const formatDate = (dateString) => {
      return new Date(dateString).toLocaleString()
    }

    // 格式化单词ID
    const formatWordIds = (wordIds) => {
      return wordIds.split(',').join(', ')
    }

    // 获取计划类型标签样式
    const getPlanTypeTag = (type) => {
      switch (type) {
        case 'MANUAL': return 'success'
        case 'AUTO': return 'info'
        default: return ''
      }
    }

    // 获取状态标签样式
    const getStatusTag = (status) => {
      switch (status) {
        case 'PROCEEDING': return 'warning'
        case 'COMPLETED': return 'success'
        case 'CANCELLED': return 'danger'
        default: return ''
      }
    }

    // 获取随机学习计划
    const fetchRandomPlans = async () => {
      recommendLoading.value = true
      recommendError.value = ''
      
      try {
        const response = await getRandomStudyPlans(recommendLimit.value)
        if (response.success) {
          recommendPlans.value = response.data
        } else {
          recommendError.value = response.message || '获取推荐计划失败'
        }
      } catch (error) {
        recommendError.value = `获取推荐计划失败: ${error.message}`
        console.error('获取推荐计划错误:', error)
      } finally {
        recommendLoading.value = false
      }
    }

    // 选择推荐计划
    const selectRecommendPlan = (plan) => {
      searchId.value = plan.id
      handleSearch()
    }

    // 组件挂载时获取推荐计划
    onMounted(() => {
      fetchRandomPlans()
    })

    return {
      searchId,
      loading,
      errorMessage,
      currentPlan,
      Plus,
      recommendPlans,
      recommendLoading,
      recommendError,
      Refresh,
      handleSearch,
      resetSearch,
      handleCreateCustomPlan,
      formatDate,
      formatWordIds,
      getPlanTypeTag,
      getStatusTag,
      fetchRandomPlans,
      selectRecommendPlan
    }
  }
}
</script>

<style scoped>
.study-plan-search {
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.page-title {
  text-align: center;
  margin-bottom: 30px;
  color: #333;
}

.search-form {
  margin-bottom: 30px;
  display: flex;
  justify-content: center;
}

.loading-state {
  text-align: center;
  padding: 40px 0;
  color: #666;
}

.loading-icon {
  animation: rotating 2s linear infinite;
  margin-right: 10px;
}

@keyframes rotating {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.error-message {
  margin: 20px 0;
}

.plan-detail {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.1);
}

.plan-title {
  text-align: center;
  margin-bottom: 20px;
  color: #409eff;
}

.word-ids-list {
  max-height: 200px;
  overflow-y: auto;
  word-break: break-all;
}

.empty-state {
  margin-top: 50px;
}

/* 新增推荐计划样式 */
.recommend-plan-section {
  margin-top: 40px;
  padding: 20px;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.recommend-title {
  display: inline-block;
  margin-right: 20px;
  color: #333;
}

.recommend-plans {
  margin-top: 20px;
}

.plan-card {
  margin-bottom: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.plan-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 4px 12px 0 rgba(0,0,0,0.15);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.card-header h4 {
  margin: 0;
  color: #409eff;
}

.card-content {
  margin: 10px 0;
}

.card-content p {
  margin: 5px 0;
  color: #666;
}

.card-content .label {
  display: inline-block;
  width: 80px;
  color: #999;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #eee;
}

.plan-date {
  font-size: 12px;
  color: #999;
}
</style>