<template>
  <div class="leaderboard-table">
    <el-table
      :data="data"
      stripe
      style="width: 100%"
      :header-cell-style="{ background: '#e6f7ff', color: '#333' }"
    >
      <!-- 排名列 -->
      <el-table-column label="排名" width="80" align="center">
        <template #default="{ row }">
          <div class="rank-cell">
            <!-- 前三名显示特殊图标 -->
            <div v-if="row.rank === 1" class="rank-badge top1">1</div>
            <div v-else-if="row.rank === 2" class="rank-badge top2">2</div>
            <div v-else-if="row.rank === 3" class="rank-badge top3">3</div>
            <div v-else class="rank-badge normal">{{ row.rank }}</div>
          </div>
        </template>
      </el-table-column>

      <!-- 用户信息列 -->
      <el-table-column label="用户" min-width="180">
        <template #default="{ row }">
          <div class="user-cell">
            <el-avatar :src="row.avatar" :size="40"></el-avatar>
            <div class="user-info">
              <div class="username">{{ row.username }}</div>
              <div v-if="row.rank <= 3" class="user-tag">
                <el-tag size="small" effect="dark" :type="getTagType(row.rank)">
                  {{ getTagText(row.rank) }}
                </el-tag>
              </div>
            </div>
          </div>
        </template>
      </el-table-column>

      <!-- 分数列 -->
      <el-table-column :label="rankType === 'score' ? '学习成绩' : '活跃度'" min-width="120" align="center">
        <template #default="{ row }">
          <div class="score-cell">
            <div class="score-value">
              {{ rankType === 'score' ? row.score : row.activityPoints }}
            </div>
            <div class="score-progress">
              <el-progress 
                :percentage="getPercentage(row)" 
                :color="getProgressColor(row.rank)" 
                :show-text="false" 
                :stroke-width="8" 
              />
            </div>
          </div>
        </template>
      </el-table-column>

      <!-- 奖励列 -->
      <el-table-column label="奖励" min-width="120" align="center">
        <template #default="{ row }">
          <div class="reward-cell">
            <div class="reward-item">
              <el-icon class="coin-icon"><Money /></el-icon>
              <span>{{ row.coins }}</span>
            </div>
            <div class="reward-item">
              <el-icon class="points-icon"><Star /></el-icon>
              <span>{{ row.points }}</span>
            </div>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <!-- 我的排名信息 -->
    <div class="my-rank">
      <div class="my-rank-header">我的{{ timePeriod }}排名</div>
      <div class="my-rank-content">
        <el-avatar src="/avatars/user.png" :size="40"></el-avatar>
        <div class="my-info">
          <div class="my-name">林芯w (我)</div>
          <div class="my-rank-stats">
            <div class="my-rank-item">
              <span>当前排名</span>
              <strong class="my-value">15</strong>
            </div>
            <div class="my-rank-item">
              <span>{{ rankType === 'score' ? '学习成绩' : '活跃度' }}</span>
              <strong class="my-value">{{ rankType === 'score' ? '85' : '320' }}</strong>
            </div>
            <div class="my-rank-item">
              <span>距离前10名</span>
              <strong class="my-value gap">↑ 5</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Money, Star } from '@element-plus/icons-vue';

// 定义props
const props = defineProps({
  data: {
    type: Array,
    required: true,
    default: () => []
  },
  rankType: {
    type: String,
    default: 'score'
  },
  timePeriod: {
    type: String,
    default: '日'
  }
});

// 根据排名获取标签类型
const getTagType = (rank: number): string => {
  switch (rank) {
    case 1:
      return 'danger';
    case 2:
      return 'warning';
    case 3:
      return 'success';
    default:
      return 'info';
  }
};

// 根据排名获取标签文本
const getTagText = (rank: number): string => {
  switch (rank) {
    case 1:
      return '冠军';
    case 2:
      return '亚军';
    case 3:
      return '季军';
    default:
      return '';
  }
};

// 获取进度条百分比
const getPercentage = (row: any): number => {
  if (props.rankType === 'score') {
    // 假设满分是100
    return row.score;
  } else {
    // 活跃度值相对于第一名的百分比
    const maxPoints = Math.max(...props.data.map(item => item.activityPoints));
    return Math.min(100, (row.activityPoints / maxPoints) * 100);
  }
};

// 根据排名获取进度条颜色
const getProgressColor = (rank: number): string => {
  switch (rank) {
    case 1:
      return '#ff66b3'; // 粉色
    case 2:
      return '#67c23a'; // 绿色
    case 3:
      return '#409eff'; // 蓝色
    default:
      return '#909399'; // 灰色
  }
};
</script>

<style scoped>
.leaderboard-table {
  width: 100%;
}

.rank-cell {
  display: flex;
  justify-content: center;
  align-items: center;
}

.rank-badge {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-weight: bold;
  color: white;
}

.rank-badge.top1 {
  background: linear-gradient(135deg, #ffb900, #ff7730);
  box-shadow: 0 2px 8px rgba(255, 119, 48, 0.3);
}

.rank-badge.top2 {
  background: linear-gradient(135deg, #b4b4b4, #efefef);
  color: #333;
  box-shadow: 0 2px 8px rgba(180, 180, 180, 0.3);
}

.rank-badge.top3 {
  background: linear-gradient(135deg, #cd7f32, #e9967a);
  box-shadow: 0 2px 8px rgba(205, 127, 50, 0.3);
}

.rank-badge.normal {
  background: #e6f7ff;
  color: #409eff;
  border: 1px solid #b3d8ff;
}

.user-cell {
  display: flex;
  align-items: center;
}

.user-info {
  margin-left: 10px;
}

.username {
  font-weight: bold;
  color: #333;
  margin-bottom: 4px;
}

.user-tag {
  margin-top: 2px;
}

.score-cell {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
}

.score-value {
  font-weight: bold;
  color: #ff66b3;
  font-size: 16px;
}

.score-progress {
  width: 100%;
  padding: 0 5px;
}

.reward-cell {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 10px;
}

.reward-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.coin-icon {
  color: #ff9800;
}

.points-icon {
  color: #ff66b3;
}

/* 我的排名样式 */
.my-rank {
  margin-top: 20px;
  background-color: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
}

.my-rank-header {
  font-weight: bold;
  color: #333;
  margin-bottom: 15px;
  padding-bottom: 10px;
  border-bottom: 1px solid #ebeef5;
}

.my-rank-content {
  display: flex;
  align-items: center;
}

.my-info {
  margin-left: 15px;
  flex: 1;
}

.my-name {
  font-weight: bold;
  margin-bottom: 8px;
}

.my-rank-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
}

.my-rank-item {
  display: flex;
  flex-direction: column;
  min-width: 80px;
}

.my-rank-item span {
  font-size: 12px;
  color: #909399;
  margin-bottom: 2px;
}

.my-value {
  font-size: 16px;
  color: #ff66b3;
}

.my-value.gap {
  color: #67c23a;
}

@media (max-width: 768px) {
  .user-cell {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .user-info {
    margin-left: 0;
    margin-top: 5px;
    text-align: center;
  }
  
  .my-rank-stats {
    flex-direction: column;
    gap: 8px;
  }
}
</style> 