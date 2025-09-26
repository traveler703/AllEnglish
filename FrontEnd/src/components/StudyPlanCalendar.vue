<template>
  <div class="study-plan-container">

    <!-- 操作区域 -->
    <div class="action-bar">
      <div class="action-buttons">
        <el-button type="success" @click="showMyPlans">
          <el-icon><Document /></el-icon>
          查看我的计划
        </el-button>
        <el-button type="primary" @click="handleAddPlan">
          <el-icon><Plus /></el-icon>
          添加学习计划
        </el-button>
      </div>
      <div class="date-display">
        <el-icon><Calendar /></el-icon>
        <span>{{ currentYear }}年{{ currentMonth }}</span>
      </div>
    </div>

    <!-- 日历区域 -->
    <el-calendar v-model="currentDate">
      <template #date-cell="{ data }">
        <div 
          class="cell-content" 
          :class="{ 'current-day': isCurrentDay(data.day) }" 
          @click="handleDateClick(data.day)"
        >
          <div class="day-number">{{ data.day.split('-')[2] }}</div>
          
          <!-- 学习计划条显示 -->
          <div v-if="hasPlansForDate(data.day)" class="plan-indicator"></div>
          
          <template v-if="getPlansForDate(data.day).length > 0">
            <div 
              v-for="plan in getPlansForDate(data.day)" 
              :key="`${plan.planId}-${data.day}`"
              class="plan-bar"
              :style="{ backgroundColor: getPlanColor(plan) }"
              @click.stop="handlePlanClick(plan)"
            >
              <span class="plan-title">{{ plan.planTitle || `计划 #${plan.planId}` }}</span>
            </div>
          </template>
          <div v-else class="empty-cell">
            <i class="fas fa-book-open"></i>
          </div>
        </div>
      </template>
    </el-calendar>

    <!-- 统计和图例区域 -->
    <div class="stats-container">
      <div class="stat-card" v-for="stat in stats" :key="stat.title">
        <h3>{{ stat.value }}</h3>
        <p>{{ stat.title }}</p>
      </div>
    </div>

    <div class="legend">
      <div class="legend-item" v-for="(color, type) in planTypeColors" :key="type">
        <div class="legend-color" :style="{ backgroundColor: color }"></div>
        <span>{{ type }}学习</span>
      </div>
    </div>

    <!-- 计划详情对话框 -->
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
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Plus, Document, Calendar, ArrowLeft } from '@element-plus/icons-vue';
import dayjs from 'dayjs';

export default {
  name: 'StudyPlanCalendar',
  components: {
    Plus,
    Document,
    Calendar,
    ArrowLeft
  },
  setup() {
    const router = useRouter();
    const currentDate = ref(new Date());
    const dialogVisible = ref(false);
    const selectedPlan = ref(null);
    const clickedDate = ref('');
    const loading = ref(true);
    const studyPlans = ref([]);
    const userId = ref('');

    const planTypeColors = {
      '词汇': '#3498db',
      '语法': '#2ecc71',
      '阅读': '#9b59b6',
      '听力': '#e67e22',
      '口语': '#e74c3c',
      '写作': '#1abc9c'
    };

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
          throw new Error('获取学习计划失败');
        }

        studyPlans.value = await response.json();
      } catch (error) {
        ElMessage.error(error.message || '获取学习计划时出错');
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
      return plan.planType === 'Auto' ? '自动生成' : '自定义';
    };

    const getPlanColor = (plan) => {
      const colors = ['#3498db', '#2ecc71', '#9b59b6', '#e67e22', '#e74c3c', '#1abc9c'];
      return colors[plan.planId % colors.length];
    };

    const currentYear = computed(() => {
      return currentDate.value.getFullYear();
    });
    
    const currentMonth = computed(() => {
      const months = ['1月', '2月', '3月', '4月', '5月', '6月', 
                      '7月', '8月', '9月', '10月', '11月', '12月'];
      return months[currentDate.value.getMonth()];
    });

    const stats = computed(() => [
      { value: studyPlans.value.length, title: '总学习计划' },
      { 
        value: studyPlans.value.filter(plan => 
          dayjs().isSameOrAfter(dayjs(plan.startDate)) && 
          dayjs().isSameOrBefore(dayjs(plan.startDate).add(plan.duration, 'day'))
        ).length, 
        title: '进行中的计划' 
      },
      { 
        value: studyPlans.value.filter(plan => calculateProgress(plan) === 100).length, 
        title: '已完成计划' 
      },
      { 
        value: studyPlans.value.reduce((sum, plan) => sum + plan.learnedWordCount, 0),
        title: '已学单词' 
      }
    ]);

    const isCurrentDay = (date) => {
      return date === dayjs().format('YYYY-MM-DD');
    };

    const getPlansForDate = (date) => {
      return studyPlans.value.filter(plan => 
        dayjs(date).isSameOrAfter(dayjs(plan.startDate)) && 
        dayjs(date).isSameOrBefore(dayjs(plan.startDate).add(plan.duration, 'day'))
      );
    };

    const hasPlansForDate = (date) => {
      return getPlansForDate(date).length > 0;
    };

    const handlePlanClick = (plan) => {
      selectedPlan.value = plan;
      dialogVisible.value = true;
    };

    const handleDateClick = (date) => {
      clickedDate.value = date;
    };

    const handleAddPlan = () => {
      router.push({ name: 'AddNewStudyPlan' });
    };

    const showMyPlans = () => {
      router.push({ name: 'MyStudyPlans' });
    };

    const handleBack = () => {
      router.go(-1);
    };

    const handleDeletePlan = async () => {
          if (!selectedPlan.value) return;
          
          try {
            await ElMessageBox.confirm(
              `确定要删除学习计划 "${selectedPlan.value.planTitle || '未命名'}" 吗？此操作不可恢复。`,
              '确认删除',
              {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
              }
            );
            
            const response = await fetch(`https://localhost:7071/api/UserStudyPlan/${userId.value}/${selectedPlan.value.planId}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
              }
            });

            if (!response.ok) {
              throw new Error('删除失败');
            }

            await fetchStudyPlans();
            dialogVisible.value = false;
            // updateStudyPlans
          } catch (error) {
            if (error !== 'cancel') {
              ElMessage.error(error.message || '删除计划时出错');
            }
          }
        };

    const getProgressStatus = (progress) => {
      if (progress >= 80) return 'success';
      if (progress >= 50) return '';
      if (progress >= 20) return 'warning';
      return 'exception';
    };

    onMounted(() => {
      getUserId();
      fetchStudyPlans();
      clickedDate.value = dayjs().format('YYYY-MM-DD');
    });

    return {
      currentDate,
      dialogVisible,
      selectedPlan,
      clickedDate,
      loading,
      studyPlans,
      planTypeColors,
      currentYear,
      currentMonth,
      stats,
      formatDate,
      calculateProgress,
      getPlanType,
      getPlanColor,
      getPlansForDate,
      hasPlansForDate,
      handlePlanClick,
      handleDateClick,
      handleAddPlan,
      showMyPlans,
      handleBack,
      handleDeletePlan,
      getProgressStatus,
      isCurrentDay
    };
  }
};
</script>

<style scoped>
.study-plan-container {
  max-width: 1200px;
  margin: 0 auto;
  background: white;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  padding: 20px;
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
  flex: 1;
  text-align: left;
}

.header-center {
  flex: 1;
  text-align: center;
}

.header-right {
  flex: 1;
}

.page-header h1 {
  margin: 0;
  color: #303133;
  font-size: 24px;
  font-weight: 600;
}

.action-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 0;
  flex-wrap: wrap;
  gap: 15px;
}

.date-display {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #f8fafc;
  padding: 8px 16px;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
  font-weight: 500;
}

.action-buttons {
  display: flex;
  gap: 15px;
}

.el-calendar {
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  background: #fff;
  border: 1px solid #ebeef5;
  margin: 0 0 30px 0;
}

:deep(.el-calendar__header) {
  padding: 15px 20px;
  background: #f8fafc;
  border-bottom: 1px solid #ebeef5;
}

:deep(.el-calendar__title) {
  color: #2c3e50;
  font-size: 18px;
  font-weight: 600;
}

.cell-content {
  position: relative;
  height: 100%;
  padding: 8px 5px;
  min-height: 110px;
  cursor: pointer;
  transition: background-color 0.2s;
  border-radius: 8px;
}

.cell-content:hover {
  background-color: rgba(236, 245, 255, 0.6);
}

.day-number {
  font-weight: bold;
  margin-bottom: 8px;
  position: relative;
  z-index: 5;
  text-align: center;
  width: 26px;
  height: 26px;
  line-height: 26px;
  border-radius: 50%;
  font-size: 13px;
}

.current-day .day-number {
  background: #3498db;
  color: white;
}

.plan-bar {
  height: 22px;
  border-radius: 6px;
  color: white;
  font-size: 11px;
  padding: 0 8px;
  box-sizing: border-box;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-bottom: 4px;
  position: relative;
  z-index: 2;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.08);
  width: 100%;
}

.plan-bar:hover {
  transform: translateX(3px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
  z-index: 20;
}

.plan-title {
  font-size: 11px;
  line-height: 22px;
  font-weight: 600;
  letter-spacing: 0.3px;
}

:deep(.el-calendar-table .el-calendar-day) {
  height: 125px;
  padding: 0;
  vertical-align: top;
}

:deep(.el-calendar-table td.is-today) {
  background-color: rgba(236, 245, 255, 0.4) !important;
}

:deep(.el-calendar-table td) {
  border: 1px solid #ebeef5;
  vertical-align: top;
}

.plan-indicator {
  position: absolute;
  top: 5px;
  right: 5px;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #3498db;
  z-index: 10;
}

.stats-container {
  display: flex;
  justify-content: space-around;
  margin: 30px 0;
  padding: 20px;
  background: #f8fafc;
  border-radius: 12px;
  flex-wrap: wrap;
  gap: 15px;
}

.stat-card {
  text-align: center;
  padding: 20px 15px;
  border-radius: 10px;
  background: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  flex: 1;
  min-width: 180px;
  transition: transform 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-card h3 {
  font-size: 28px;
  color: #3498db;
  margin-bottom: 5px;
}

.stat-card p {
  color: #7f8c8d;
  font-size: 14px;
}

.legend {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin: 0 0 30px 0;
  padding: 0 15px;
  justify-content: center;
}

.legend-item {
  display: flex;
  align-items: center;
  font-size: 13px;
  background: #f8fafc;
  padding: 6px 12px;
  border-radius: 20px;
  box-shadow: 0 2px 5px rgba(0,0,0,0.05);
}

.legend-color {
  width: 16px;
  height: 16px;
  border-radius: 4px;
  margin-right: 8px;
}

.empty-cell {
  color: #95a5a6;
  font-size: 12px;
  text-align: center;
  padding-top: 30px;
}

/* 计划详情样式 */
.plan-detail {
  padding: 10px;
}

.plan-header {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.plan-icon {
  width: 80px;
  height: 80px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
}

.plan-meta p {
  margin: 8px 0;
  font-size: 15px;
}

.plan-progress {
  margin-top: 25px;
}

.progress-item {
  margin-bottom: 15px;
}

.progress-item span {
  display: block;
  margin-bottom: 5px;
  font-size: 14px;
}

.progress-total {
  margin-top: 25px;
}

@media (max-width: 768px) {
  .action-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  :deep(.el-calendar-table .el-calendar-day) {
    height: 100px;
  }
  
  .el-calendar {
    margin: 0;
  }
  
  .stats-container,
  .legend {
    margin: 15px 0;
  }
  
  .plan-header {
    flex-direction: column;
    gap: 15px;
  }
}
</style>