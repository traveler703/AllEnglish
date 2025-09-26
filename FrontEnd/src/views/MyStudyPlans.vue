<template>
  <div class="my-study-plans">
    <div class="page-header">
      <div class="header-left">
        <el-button type="text" @click="handleBack" class="back-button">
          <el-icon><ArrowLeft /></el-icon>
          返回日历
        </el-button>
      </div>
      <div class="header-center">
        <h1>我的学习计划</h1>
      </div>
      <div class="header-right"></div>
    </div>

    <div v-if="loading" class="loading-state">
      <el-skeleton :rows="5" animated />
    </div>

    <div v-else-if="studyPlans.length > 0" class="plans-container">
      <el-row :gutter="20">
        <el-col 
          v-for="plan in studyPlans" 
          :key="plan.planId"
          :xs="24" 
          :sm="12" 
          :md="8"
          :lg="6"
        >
          <el-card class="plan-card" shadow="hover">
            <div class="card-header">
              <div class="plan-type" :style="{ backgroundColor: getPlanColor(plan) }">
                {{ getPlanType(plan) }}
              </div>
              <h3 class="plan-title">{{ plan.planTitle || `学习计划 #${plan.planId}` }}</h3>
            </div>
            
            <div class="card-content">
              <p class="plan-dates">
                <el-icon><Calendar /></el-icon>
                {{ formatDate(plan.startDate) }}
              </p>
              
              <div class="progress-stats">
                <div class="stat-item">
                  <span class="stat-label">单词:</span>
                  <span class="stat-value">{{ plan.learnedWordCount }}/{{ plan.wordCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">文章:</span>
                  <span class="stat-value">{{ plan.learnedArticleCount }}/{{ plan.articleCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">听力:</span>
                  <span class="stat-value">{{ plan.listeningTime }}/{{ plan.duration }}分钟</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">口语:</span>
                  <span class="stat-value">{{ plan.learnedOralTime }}/{{ plan.oralTime }}分钟</span>
                </div>
              </div>
            </div>
            
            <div class="card-footer">
              <el-progress 
                :percentage="calculateProgress(plan)" 
                :status="getProgressStatus(calculateProgress(plan))" 
                text-inside
                :stroke-width="20"
                :text-color="'#000000'" 
              />
              
              <div class="actions">
                <el-button type="primary" size="small" @click="handleViewDetails(plan)">
                  详情
                </el-button>
                <el-button type="danger" size="small" @click="handleDeletePlan(plan)">
                  删除
                </el-button>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <div v-else class="empty-state">
      <el-empty description="暂无学习计划">
        <el-button type="primary" @click="navigateToAdd">创建学习计划</el-button>
      </el-empty>
    </div>
  </div>
  <el-dialog 
    v-model="dialogVisible" 
    :title="selectedPlan ? `${selectedPlan.planTitle || '未命名计划'} #${selectedPlan.planId}` : '学习计划详情'"
    width="50%"
  >
    <div v-if="selectedPlan" class="plan-detail">
      <div class="plan-header">
        <div class="plan-icon" :style="{ backgroundColor: getPlanColor(selectedPlan) + '20', color: getPlanColor(selectedPlan) }">
          <i class="fas fa-book"></i>
        </div>
        <div class="plan-meta">
          <p><strong>计划类型:</strong> {{ getPlanType(selectedPlan) }}</p>
          <p><strong>开始日期:</strong> {{ formatDate(selectedPlan.startDate) }}</p>
          <p><strong>持续时间:</strong> {{ selectedPlan.duration }}天</p>
          <p><strong>单词目标:</strong> {{ selectedPlan.wordCount }}</p>
          <p><strong>文章目标:</strong> {{ selectedPlan.articleCount }}</p>
          <p><strong>口语目标:</strong> {{ selectedPlan.oralTime }}分钟</p>
        </div>
      </div>
      
      <el-divider />
      
      <div class="plan-progress">
        <p><strong>当前进度:</strong></p>
        <div class="progress-item">
          <span>单词: {{ selectedPlan.learnedWordCount }}/{{ selectedPlan.wordCount }}</span>
          <el-progress 
            :percentage="Math.round((selectedPlan.learnedWordCount / selectedPlan.wordCount) * 100)" 
            :status="getProgressStatus(Math.round((selectedPlan.learnedWordCount / selectedPlan.wordCount) * 100))" 
          />
        </div>
        <div class="progress-item">
          <span>文章: {{ selectedPlan.learnedArticleCount }}/{{ selectedPlan.articleCount }}</span>
          <el-progress 
            :percentage="Math.round((selectedPlan.learnedArticleCount / selectedPlan.articleCount) * 100)" 
            :status="getProgressStatus(Math.round((selectedPlan.learnedArticleCount / selectedPlan.articleCount) * 100))" 
          />
        </div>
        <div class="progress-item">
          <span>听力: {{ selectedPlan.listeningTime }}/{{ selectedPlan.duration }}分钟</span>
          <el-progress 
            :percentage="Math.round((selectedPlan.listeningTime / selectedPlan.duration) * 100)" 
            :status="getProgressStatus(Math.round((selectedPlan.listeningTime / selectedPlan.duration) * 100))" 
          />
        </div>
        <div class="progress-item">
          <span>口语: {{ selectedPlan.learnedOralTime }}/{{ selectedPlan.oralTime }}分钟</span>
          <el-progress 
            :percentage="Math.round((selectedPlan.learnedOralTime / selectedPlan.oralTime) * 100)" 
            :status="getProgressStatus(Math.round((selectedPlan.learnedOralTime / selectedPlan.oralTime) * 100))" 
          />
        </div>
        <div class="progress-total">
          <p><strong>总体进度:</strong></p>
          <el-progress 
            :percentage="calculateProgress(selectedPlan)" 
            :status="getProgressStatus(calculateProgress(selectedPlan))" 
            :stroke-width="16" 
            text-inside
          />
        </div>
      </div>
    </div>
    
    <template #footer>
      <el-button @click="dialogVisible = false">关闭</el-button>
      <el-button type="danger" @click="handleDeletePlan">删除计划</el-button>
    </template>
  </el-dialog>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { ArrowLeft, Calendar } from '@element-plus/icons-vue';
import dayjs from 'dayjs';

export default {
  name: 'MyStudyPlans',
  components: {
    ArrowLeft,
    Calendar
  },
  setup() {
    const router = useRouter();
    const loading = ref(true);
    const studyPlans = ref([]);
    const userId = ref('');
    const dialogVisible = ref(false);
    const selectedPlan = ref(null);

    const getUserId = () => {
      const userData = localStorage.getItem('user');
      if (userData) {
        const parsedData = JSON.parse(userData);
        userId.value = parsedData.Id || '114514';
      }
    };

    const fetchStudyPlans = async () => {
      try {
        loading.value = true;
        const response = await fetch(`https://localhost:7071/api/UserStudyPlan/DetailsByUser/${userId.value}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        });

        if (!response.ok) {
          const errorData = await response.json().catch(() => null);
          const errorMsg = errorData?.title || errorData?.message || '获取学习计划详情失败';
          throw new Error(`${errorMsg} (状态码: ${response.status})`);
        }

        const data = await response.json();
        studyPlans.value = data;
      } catch (error) {
        console.error('获取学习计划详情错误:', error);
        ElMessage.error(error.message || '获取学习计划详情时出错');
      } finally {
        loading.value = false;
      }
    };

    const formatDate = (dateString) => {
      return dayjs(dateString).format('YYYY-MM-DD');
    };

    const calculateProgress = (plan) => {
      const totalItems = [
        { completed: plan.learnedWordCount, total: plan.wordCount },
        { completed: plan.learnedArticleCount, total: plan.articleCount },
        { completed: plan.listeningTime, total: plan.duration },
        { completed: plan.learnedOralTime, total: plan.oralTime }
      ];
      
      const validItems = totalItems.filter(item => item.total > 0);
      if (validItems.length === 0) return 0;
      
      const totalProgress = validItems.reduce((sum, item) => {
        return sum + (item.completed / item.total * 100);
      }, 0);
      
      return Math.round(totalProgress / validItems.length);
    };

    const getPlanType = (plan) => {
      return plan.planType === 'Auto' ? '自动' : '自定义';
    };

    const getPlanColor = (plan) => {
      const colors = ['#3498db', '#2ecc71', '#9b59b6', '#e67e22', '#e74c3c', '#1abc9c'];
      return colors[plan.planId % colors.length];
    };

    const getProgressStatus = (progress) => {
      if (progress >= 80) return 'success';
      if (progress >= 50) return '';
      if (progress >= 20) return 'warning';
      return 'exception';
    };

    const handleBack = () => {
      router.push({ name: 'StudyPlansView' });
    };

    const navigateToAdd = () => {
      router.push({ name: 'AddNewStudyPlan' });
    };

    const handleViewDetails = (plan) => {
      selectedPlan.value = plan;
      dialogVisible.value = true;
    };

    const handleDeletePlan = async (plan) => {
      try {
        await ElMessageBox.confirm(
          `确定要删除学习计划 "${plan.planTitle || '未命名'}" 吗？此操作不可恢复。`,
          '确认删除',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning',
          }
        );
        
        const response = await fetch(`https://localhost:7071/api/UserStudyPlan/${plan.planId}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        });

        if (!response.ok) {
          throw new Error('删除失败');
        }

        await fetchStudyPlans();
        ElMessage.success('学习计划删除成功');
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error(error.message || '删除计划时出错');
        }
      }
    };

    onMounted(() => {
      getUserId();
      fetchStudyPlans();
    });

    return {
      studyPlans,
      loading,
      handleBack,
      navigateToAdd,
      handleViewDetails,
      handleDeletePlan,
      getProgressStatus,
      calculateProgress,
      getPlanType,
      getPlanColor,
      formatDate,
      dialogVisible,
      selectedPlan
    };
  }
};
</script>

<style scoped>
.my-study-plans {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  width: 100%;
  box-sizing: border-box;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 1px solid #ebeef5;
  position: relative;
}

.header-left {
  flex: 0 0 200px; /* 固定宽度，不拉伸 */
  text-align: left;
}

.header-center {
  flex: 1; /* 占据剩余空间并居中 */
  display: flex;
  justify-content: center;
  align-items: center;
}

.header-right {
  flex: 0 0 200px; /* 固定宽度，用于平衡布局 */
}

.back-button {
  display: flex;
  align-items: center;
  color: #409eff;
  font-size: 14px;
  padding: 8px 12px;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.back-button:hover {
  background-color: #ecf5ff;
  color: #337ecc;
}

.back-button .el-icon {
  margin-right: 6px;
  font-size: 16px;
}

.page-header h1 {
  margin: 0;
  color: #303133;
  font-size: 24px;
  font-weight: 600;
}

.plans-container {
  margin-top: 20px;
}

.plan-card {
  margin-bottom: 20px;
  transition: transform 0.3s ease;
  height: 280px;
  display: flex;
  flex-direction: column;
}

.plan-card:hover {
  transform: translateY(-5px);
}

.card-header {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.plan-type {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  margin-right: 10px;
  flex-shrink: 0;
}

.plan-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.plan-dates {
  font-size: 12px;
  color: #606266;
  margin: 0 0 15px 0;
  display: flex;
  align-items: center;
}

.plan-dates .el-icon {
  margin-right: 5px;
}

.progress-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 15px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

.stat-label {
  color: #909399;
}

.stat-value {
  color: #303133;
  font-weight: 500;
}

.card-footer {
  margin-top: auto;
  padding-top: 15px;
}

.actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 10px;
}

.empty-state {
  margin-top: 50px;
  text-align: center;
}

.loading-state {
  margin-top: 20px;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .my-study-plans {
    max-width: 100%;
    padding: 15px;
  }
}

@media (max-width: 768px) {
  .my-study-plans {
    padding: 10px;
  }
  
  .header-left,
  .header-right {
    flex: 0 0 auto;
  }
  
  .header-center {
    flex: 1;
    margin: 0 10px;
  }
  
  .page-header h1 {
    font-size: 20px;
  }
  
  .back-button {
    padding: 6px 8px;
    font-size: 13px;
  }
}

@media (max-width: 480px) {
  .my-study-plans {
    padding: 8px;
  }
  
  .page-header {
    flex-direction: column;
    gap: 10px;
    text-align: center;
  }
  
  .header-left,
  .header-right {
    flex: none;
  }
  
  .header-center {
    order: -1;
  }
}
</style>