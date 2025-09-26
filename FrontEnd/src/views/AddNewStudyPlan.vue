<template>
  <div class="add-new-study-plan">
    <div class="page-header">
      <el-button type="text" @click="handleBack">
        <el-icon><ArrowLeft /></el-icon>
        返回日历
      </el-button>
      <h1>添加学习计划</h1>
    </div>

    <!-- 搜索区域 -->
    <div class="search-section">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>搜索学习计划</span>
            <el-button 
              type="primary" 
              @click="openCustomPlanDialog"
            >
              <el-icon><Plus /></el-icon>
              自定义学习计划
            </el-button>
          </div>
        </template>
        
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
        </el-form>
      </el-card>
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
          <el-descriptions-item label="持续时间">{{ currentPlan.duration }}天</el-descriptions-item>
          <el-descriptions-item label="计划类型">
            <el-tag :type="getPlanTypeTag(currentPlan.planType)">
              {{ getPlanTypeDisplayName(currentPlan.planType) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="公开状态">
            <el-tag :type="currentPlan.isPublic ? 'success' : 'info'">
              {{ currentPlan.isPublic ? '公开' : '私有' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="单词数量">{{ currentPlan.wordCount }}</el-descriptions-item>
          <el-descriptions-item label="文章数量">{{ currentPlan.articleCount }}</el-descriptions-item>
          <el-descriptions-item label="听力时间">{{ currentPlan.listeningTime }}分钟</el-descriptions-item>
          <el-descriptions-item label="口语练习">{{ currentPlan.oralTime }}分钟</el-descriptions-item>
        </el-descriptions>
      </div>
      
      <div class="action-buttons">
        <el-button type="primary" @click="addToMyPlans" :loading="addingPlan">
          {{ addingPlan ? '正在添加...' : '添加到我的计划' }}
        </el-button>
      </div>
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
                  {{ getPlanTypeDisplayName(plan.planType) }}
                </el-tag>
              </div>
              <div class="card-content">
                <p><span class="label">持续时间:</span> {{ plan.duration }}天</p>
                <p><span class="label">单词数量:</span> {{ plan.wordCount }}</p>
                <p><span class="label">文章数量:</span> {{ plan.articleCount }}</p>
                <p><span class="label">听力时间:</span> {{ plan.listeningTime }}分钟</p>
                <p><span class="label">口语练习:</span> {{ plan.oralTime }}分钟</p>
                <p><span class="label">公开状态:</span> 
                  <el-tag :type="plan.isPublic ? 'success' : 'info'" size="small">
                    {{ plan.isPublic ? '公开' : '私有' }}
                  </el-tag>
                </p>
              </div>
              <div class="card-footer">
                <el-tag type="info" size="small">
                  {{ getPlanTypeDisplayName(plan.planType) }}
                </el-tag>
                <span class="user-id">用户: {{ plan.userId.substring(0, 8) }}...</span>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>
      
      <div v-if="!recommendLoading && recommendPlans.length === 0 && !recommendError" class="empty-state">
        <el-empty description="暂无推荐学习计划" />
      </div>
    </div>

    <!-- 自定义学习计划弹窗 -->
    <CustomStudyPlan 
      v-model:visible="customPlanDialogVisible" 
      :plan="editingPlan"
      @save="handleSaveCustomPlan"
    />
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { ArrowLeft, Plus, Loading, Refresh } from '@element-plus/icons-vue';
import CustomStudyPlan from './CustomStudyPlan.vue';
import dayjs from 'dayjs';

export default {
  name: 'AddNewStudyPlan',
  components: {
    ArrowLeft,
    Loading,
    Refresh,
    CustomStudyPlan
  },
  setup() {
    const router = useRouter();
    const searchId = ref('');
    const loading = ref(false);
    const errorMessage = ref('');
    const currentPlan = ref(null);
    const customPlanDialogVisible = ref(false);
    const editingPlan = ref(null);
    const userId = ref('');
    const addingPlan = ref(false);
    
    // 推荐计划相关状态
    const recommendPlans = ref([]);
    const recommendLoading = ref(false);
    const recommendError = ref('');
    const recommendLimit = ref(3); // 默认获取3条推荐计划

    // 返回日历
    const handleBack = () => {
      router.push({ name: 'StudyPlansView' });
    };

    // 打开自定义计划弹窗
    const openCustomPlanDialog = () => {
      editingPlan.value = null;
      customPlanDialogVisible.value = true;
    };

    // 处理搜索 - 使用真实API
    const handleSearch = async () => {
      if (!searchId.value) return;
      
      loading.value = true;
      errorMessage.value = '';
      currentPlan.value = null;

      try {
        const response = await fetch(`https://localhost:7071/api/StudyPlan/${searchId.value}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        });

        if (!response.ok) {
          if (response.status === 404) {
            errorMessage.value = `未找到ID为 ${searchId.value} 的学习计划`;
          } else if (response.status === 401) {
            errorMessage.value = '认证失败，请重新登录';
          } else if (response.status === 403) {
            errorMessage.value = '没有权限访问该学习计划';
          } else {
            errorMessage.value = `查询失败，服务器错误 (${response.status})`;
          }
          return;
        }

        const planData = await response.json();
        
        // 映射API返回的字段到前端使用的字段
        currentPlan.value = {
          ...planData,
          wordCount: planData.wordCount || planData.totalWords || 0,
          articleCount: planData.articleCount || planData.totalArticles || 0,
          oralTime: planData.oralTime || planData.speakingTime || planData.totalSpeakingTime || 0,
          listeningTime: planData.listeningTime || planData.totalListeningTime || 0
        };
        
      } catch (error) {
        console.error('搜索学习计划失败:', error);
        
        if (error.name === 'TypeError' && error.message.includes('fetch')) {
          errorMessage.value = '网络连接失败，请检查网络连接或服务器状态';
        } else {
          errorMessage.value = `查询失败: ${error.message}`;
        }
      } finally {
        loading.value = false;
      }
    };

    // 重置搜索
    const resetSearch = () => {
      searchId.value = '';
      currentPlan.value = null;
      errorMessage.value = '';
    };

    // 获取计划类型显示名称
    const getPlanTypeDisplayName = (type) => {
      const typeMap = {
        'Auto': '自动计划',
        'Manual': '手动计划',
        'Vocabulary': '词汇计划',
        'Grammar': '语法计划',
        'Reading': '阅读计划',
        'Listening': '听力计划',
        'Speaking': '口语计划',
        'Writing': '写作计划'
      };
      return typeMap[type] || type;
    };

    // 获取计划类型标签样式
    const getPlanTypeTag = (type) => {
      switch (type) {
        case 'Vocabulary': 
        case '词汇':
          return 'success';
        case 'Grammar':
        case '语法': 
          return 'info';
        case 'Reading':
        case '阅读': 
          return 'warning';
        case 'Listening':
        case '听力': 
          return '';
        case 'Auto':
          return 'primary';
        case 'Speaking':
        case '口语':
          return 'danger';
        case 'Writing':
        case '写作':
          return 'warning';
        default: 
          return '';
      }
    };

    // 获取随机学习计划 - 真实API调用
    const fetchRandomPlans = async () => {
      recommendLoading.value = true;
      recommendError.value = '';
      
      try {
        const response = await fetch(`https://localhost:7071/api/StudyPlan/Random/${recommendLimit.value}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        recommendPlans.value = data;
        
        if (data.length === 0) {
          recommendError.value = '暂无可用的推荐学习计划';
        }
        
      } catch (error) {
        console.error('获取推荐计划失败:', error);
        recommendError.value = `获取推荐计划失败: ${error.message}`;
        
        if (error.name === 'TypeError' && error.message.includes('fetch')) {
          recommendError.value = '网络连接失败，请检查网络连接或服务器状态';
        }
      } finally {
        recommendLoading.value = false;
      }
    };

    // 选择推荐计划
    const selectRecommendPlan = (plan) => {
      searchId.value = plan.id;
      handleSearch();
    };

    // 添加到我的计划 - 使用真实API
    const addToMyPlans = async () => {
      if (!currentPlan.value || !userId.value) return;
      
      addingPlan.value = true;
      
      try {
        // 修复：使用本地日期格式，避免时区转换问题
        const localDate = dayjs().format('YYYY-MM-DD');
        
        const requestData = {
          userId: userId.value,
          planId: currentPlan.value.id,
          startDate: localDate
        };

        const response = await fetch('https://localhost:7071/api/UserStudyPlan', {
          method: 'POST',
          headers: {
            'accept': 'text/plain',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          },
          body: JSON.stringify(requestData)
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.text();
        ElMessage.success(`计划添加成功: ${result}`);
        
        // 添加成功后返回日历页面
        setTimeout(() => {
          router.push({ name: 'StudyPlansView' });
        }, 1000);
        
      } catch (error) {
        console.error('添加学习计划失败:', error);
        ElMessage.error(`添加学习计划失败: ${error.message}`);
      } finally {
        addingPlan.value = false;
      }
    };

    // 处理保存自定义计划
    const handleSaveCustomPlan = (plan) => {
      ElMessage.success(`自定义计划保存成功: ${plan.title}`);
      customPlanDialogVisible.value = false;
    };

    // 组件挂载时获取用户ID和推荐计划
    onMounted(() => {
      const userData = localStorage.getItem('user');
      if (userData) {
        const parsedData = JSON.parse(userData);
        userId.value = parsedData.Id || '114514'; // 默认id
      } else {
        // 如果未登录，跳转到登录页
        router.push('/login');
      }
      fetchRandomPlans();
    });

    return {
      searchId,
      loading,
      errorMessage,
      currentPlan,
      recommendPlans,
      recommendLoading,
      recommendError,
      customPlanDialogVisible,
      editingPlan,
      addingPlan,
      ArrowLeft,
      Plus,
      Loading,
      Refresh,
      handleBack,
      openCustomPlanDialog,
      handleSearch,
      resetSearch,
      getPlanTypeTag,
      getPlanTypeDisplayName,
      fetchRandomPlans,
      selectRecommendPlan,
      addToMyPlans,
      handleSaveCustomPlan
    };
  }
};
</script>

<style scoped>
.add-new-study-plan {
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 1px solid #ebeef5;
}

.page-header h1 {
  margin: 0 auto;
  color: #303133;
  font-size: 24px;
  font-weight: 600;
}

.search-section {
  margin-bottom: 30px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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
  margin-bottom: 30px;
}

.plan-title {
  text-align: center;
  margin-bottom: 20px;
  color: #409eff;
}

.action-buttons {
  margin-top: 20px;
  text-align: center;
}

/* 推荐计划样式 */
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

.user-id {
  font-size: 12px;
  color: #999;
}

.empty-state {
  margin-top: 50px;
}
</style>