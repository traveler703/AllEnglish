<template>
  <el-card class="plan-card" shadow="hover">
    <div class="card-header">
      <div class="plan-type" :style="{ backgroundColor: plan.color }">
        {{ plan.type }}
      </div>
      <h3 class="plan-title">{{ plan.title }}</h3>
    </div>
    
    <div class="card-content">
      <p class="plan-dates">
        <el-icon><Calendar /></el-icon>
        {{ plan.startDate }} 至 {{ plan.endDate }}
      </p>
      <p class="plan-content">{{ plan.content }}</p>
    </div>
    
    <div class="card-footer">
      <el-progress 
        :percentage="plan.progress" 
        :status="getProgressStatus(plan.progress)" 
        text-inside
      />
      
      <div class="actions">
        <el-button type="primary" size="small" @click.stop="$emit('edit', plan)">
          编辑
        </el-button>
        <el-button type="danger" size="small" @click.stop="$emit('delete', plan)">
          删除
        </el-button>
      </div>
    </div>
  </el-card>
</template>

<script>
import { Calendar } from '@element-plus/icons-vue';

export default {
  name: 'PlanCard',
  components: {
    Calendar
  },
  props: {
    plan: {
      type: Object,
      required: true
    }
  },
  setup() {
    const getProgressStatus = (progress) => {
      if (progress >= 80) return 'success';
      if (progress >= 50) return '';
      if (progress >= 20) return 'warning';
      return 'exception';
    };

    return {
      getProgressStatus
    };
  }
};
</script>

<style scoped>
.plan-card {
  margin-bottom: 20px;
  transition: transform 0.3s ease;
  height: 220px;
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
  margin: 0 0 10px 0;
  display: flex;
  align-items: center;
}

.plan-dates .el-icon {
  margin-right: 5px;
}

.plan-content {
  font-size: 13px;
  color: #606266;
  margin: 0;
  height: 60px;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
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
</style>