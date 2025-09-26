<template>
  <div class="page-wrap">
    <!-- 顶部横幅 -->
    <div class="hero">
      <h1>学习排行榜</h1>
      <p>查看学习成绩与活跃度，冲榜赢奖励</p>
    </div>

    <!-- 控制条 -->
    <div class="toolbar glass">
      <el-tabs v-model="activeTab" class="tabs" stretch>
        <el-tab-pane label="总榜" name="overall" />
        <el-tab-pane label="日排行" name="daily" />
        <el-tab-pane label="周排行" name="weekly" />
        <el-tab-pane label="月排行" name="monthly" />
      </el-tabs>

      <div class="ctrl">
        <el-radio-group v-model="rankType" size="small">
          <el-radio-button label="score">学习成绩</el-radio-button>
          <el-radio-button label="activity">活跃度</el-radio-button>
        </el-radio-group>
        <el-button size="small" :loading="refreshing" @click="refreshCurrent" type="primary" plain round>
          刷新当前榜单
        </el-button>
      </div>
    </div>

    <!-- 内容两列：左榜单 / 右侧我的 -->
    <div class="content-grid">
      <!-- 榜单卡片 -->
      <el-card class="card leaderboard-card" shadow="hover" :body-style="{padding:'0'}">
        <div class="card-header">
          <h3 class="title">
            {{ titleByTab[activeTab] }}
            <small>· {{ rankType === 'score' ? '学习成绩' : '活跃度' }}</small>
          </h3>
        </div>

        <div class="table-wrap">
          <el-skeleton v-if="loading" :rows="6" animated style="padding: 20px" />
          <template v-else>
            <el-empty 
              v-if="rows.length === 0"
              :description="emptyText(activeTab, rankType)"
            />
            <el-table
              v-else
              :data="rows"
              border
              size="large"
              :row-class-name="rowClass"
              class="rank-table"
            >
              <el-table-column prop="rank" label="排名" width="90" align="center">
                <template #default="{ row }">
                  <div class="rank-badge" :class="badgeClass(row.rank)">{{ row.rank }}</div>
                </template>
              </el-table-column>

              <el-table-column label="用户" min-width="260">
                <template #default="{ row }">
                  <div class="user">
                    <img class="avatar" :src="row.avatar" alt="avatar" />
                    <div class="meta">
                      <div class="name">{{ row.username }}</div>
                      <el-tag
                        v-if="row.rank<=3"
                        :type="row.rank===1?'danger':row.rank===2?'warning':'success'"
                        size="small"
                        round
                        effect="light"
                      >
                        {{ row.rank===1?'冠军':row.rank===2?'亚军':'季军' }}
                      </el-tag>
                    </div>
                  </div>
                </template>
              </el-table-column>

              <el-table-column
                :label="rankType === 'score' ? '学习成绩' : '活跃度'"
                min-width="260"
                align="center"
              >
                <template #default="{ row }">
                  <div class="metric">
                    <span class="metric-num">{{ metricOf(row) }}</span>
                    <el-progress
                      :percentage="progressOf(row)"
                      :stroke-width="10"
                      :show-text="false"
                      striped
                      class="progress"
                    />
                  </div>
                </template>
              </el-table-column>
            </el-table>
          </template>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { descriptionProps, ElMessage } from 'element-plus';
import { User, ArrowUp, ArrowDown, Minus } from '@element-plus/icons-vue';
import api from '@/utils/axios';

/** ===== 类型 ===== */
type Period = 'overall' | 'daily' | 'weekly' | 'monthly';
type RankType = 'score' | 'activity';

// —— 总榜返回 ——
// 旧结构：包了一层，里面是 rankings 数组
interface OverallRankingUser {
  userId: string;
  username: string;                  // 注意是 username（不是 userName）
  avartar?: string;                   // 可能相对路径
  score: number;                     // 后端把对应值放在 score
  change?: string;
  trend?: 'up' | 'down' | 'stable';
  rank?: number;
}
interface OverallResponse {
  total: number;
  page: number;
  size: number;
  type: 'score' | 'activity';
  updateTime: string;
  rankings: OverallRankingUser[];
}

// —— 日/周/月返回 ——
// 新结构：直接数组
interface PeriodRow {
  userId: string;                    // 可能是 GUID
  userName: string;                  // 注意是 userName（不是 username）
  avatarUrl: string;                 // 可能相对路径
  totalScore: number;
  totalActivity: number;
  rankNo: number;                    // 名次
}


interface LeaderboardUser {
  userId: string;
  username: string;
  rank: number;
  score: number; // 后端用 score 承载具体指标
  change?: string;
  trend?: 'up' | 'down' | 'stable';
}

interface LeaderboardResponse {
  total: number;
  page: number;
  size: number;
  type: RankType;
  updateTime: string;
  rankings: LeaderboardUser[];
}

interface UserRankResponse {
  rank: number;
  score: number;
  totalUsers: number;
  percentile: number;
  change: string;
  trend: 'up' | 'down' | 'stable';
  bestRanking: {
    bestRankScore: number;
    bestRankActivity: number;
    bestScore: number;
    bestActivityScore: number;
    achievedAt: string;
  };
}

/** ===== UI/状态 ===== */
const activeTab = ref<Period>('overall');
const rankType = ref<RankType>('score');
const loading = ref(false);
const refreshing = ref(false);
const updateTime = ref('');
const userRankLoading = ref(false);

const userId = ref<number>(0);
const userRankData = ref<UserRankResponse | null>(null);

/** 缓存：key = period:type */
type Row = {
  rank: number;
  userId: string;
  username: string;
  avatar: string;
  score: number;
  activityPoints: number;
  trend: 'up' | 'down' | 'stable';
  change: string;
};
const cache = ref<Record<string, { rows: Row[]; updateTime?: string }>>({});

/** 标题映射 */
const titleByTab: Record<Period, string> = {
  overall: '综合排行榜',
  daily: '今日学习之星',
  weekly: '本周学习之星',
  monthly: '本月学习之星'
};

/** period -> 路径段 */
const segMap: Record<Exclude<Period, 'overall'>, 'day' | 'week' | 'month'> = {
  daily: 'day',
  weekly: 'week',
  monthly: 'month'
};

/** 统一拼接接口 */
const buildUrl = (type: RankType, period: Period) => {
  if (period === 'overall') return `/api/Leaderboard/${type}`;
  return `/api/Leaderboard/${segMap[period as Exclude<Period,'overall'>]}/${type}`;
};

const normalizeAvatar = (url: string | undefined, idx: number) => {
  if (!url) return `/avatars/user${(idx % 4) + 1}.png`;
  if (url.startsWith('http')) return url;
  return url.startsWith('/') ? url : `/${url}`;
};

const emptyTextMap: Record<Period, Record<RankType, string>> = {
  overall: {
    score: '暂无综合"学习成绩"榜单数据',
    activity: '暂无综合"活跃度"榜单数据'
  },
  daily: {
    score: '暂无今日"学习成绩"榜单数据',
    activity: '暂无今日"活跃度"榜单数据'
  },
  weekly: {
    score: '暂无本周"学习成绩"榜单数据',
    activity: '暂无本周"活跃度"榜单数据'
  },
  monthly: {
    score: '暂无本月"学习成绩"榜单数据',
    activity: '暂无本月"活跃度"榜单数据'
  }
};

const emptyText = (p: Period, t: RankType) => emptyTextMap[p]?.[t] ?? '暂无数据';

const fetchBoard = async (type: RankType, period: Period) => {
  const key = `${period}:${type}`;
  loading.value = true;
  try {
    if (period === 'overall') {
      // —— 总榜（旧结构）——
      const res = await api.get<OverallResponse>(buildUrl(type, period));
      const list = res.data?.rankings ?? [];

      // ✅ 合法空数据：不报错，只写空缓存
      if (!Array.isArray(list) || list.length === 0) {
        cache.value[key] = { rows: [], updateTime: res.data?.updateTime || '' };
        updateTime.value = res.data?.updateTime || '';
        return;
      }

      const rows: Row[] = list.map((u, idx) => {
        const r = u.rank && u.rank > 0 ? u.rank : (idx + 1);
        const avatar = normalizeAvatar((u.avartar as string | undefined), idx);
        return {
          rank: r,
          userId: u.userId,
          username: u.username || `用户${idx + 1}`,
          avatar,
          score: type === 'score' ? (u.score ?? 0) : Math.floor(Math.random() * 100 + 60),
          activityPoints: type === 'activity' ? (u.score ?? 0) : Math.floor(Math.random() * 500 + 100),
          change: u.change ?? '0',
          trend: u.trend ?? 'stable'
        };
      });

      cache.value[key] = { rows, updateTime: res.data.updateTime || '' };
      updateTime.value = res.data.updateTime || '';
    } else {
      // —— 日/周/月（新结构）——
      const res = await api.get<PeriodRow[]>(buildUrl(type, period));
      const list = Array.isArray(res.data) ? res.data : [];

      // ✅ 合法空数据：不报错，只写空缓存
      if (list.length === 0) {
        cache.value[key] = { rows: [], updateTime: '' };
        updateTime.value = '';
        return;
      }

      const rows: Row[] = list.map((u, idx) => {
        const r = (typeof u.rankNo === 'number' && u.rankNo > 0) ? u.rankNo : (idx + 1);
        const avatar = normalizeAvatar(u.avatarUrl, idx);

        return {
          rank: r,
          userId: u.userId,
          username: u.userName || `用户${idx + 1}`,
          avatar,
          score: type === 'score' ? (u.totalScore ?? 0) : Math.floor(Math.random() * 100 + 60),
          activityPoints: type === 'activity' ? (u.totalActivity ?? 0) : Math.floor(Math.random() * 500 + 100),
          change: '0',
          trend: 'stable'
        };
      });

      cache.value[key] = { rows, updateTime: '' };
      updateTime.value = '';
    }
  } catch (e) {
    // ❗只有真正异常才提示
    cache.value[key] = { rows: [] };
    updateTime.value = '';
    ElMessage.error('排行榜数据获取失败');
  } finally {
    loading.value = false;
  }
};



/** 拉个人排名（总榜-学习成绩） */
const fetchMe = async () => {
  if (!userId.value) return;
  userRankLoading.value = true;
  try {
    const res = await api.get<UserRankResponse>(`/api/Leaderboard/score/user/${userId.value}`);
    if (!res.data || (res.data.rank <= 0 && res.data.score <= 0)) throw new Error('invalid');
    userRankData.value = res.data;
  } catch {
    userRankData.value = null;
    ElMessage.warning('个人排名获取失败');
  } finally {
    userRankLoading.value = false;
  }
};

/** 切换时自动拉取（有缓存则不拉） */
watch([activeTab, rankType], async ([p, t]) => {
  const key = `${p}:${t}`;
  if (!cache.value[key]) {
    await fetchBoard(t, p);
  } else {
    updateTime.value = cache.value[key].updateTime || '';
  }
});

/** 页面显示的行 */
const rows = computed<Row[]>(() => {
  const key = `${activeTab.value}:${rankType.value}`;
  const d = cache.value[key]?.rows ?? [];
  // 兜底排序（服务器通常已排好）
  return [...d].sort((a, b) => {
    const va = rankType.value === 'score' ? a.score : a.activityPoints;
    const vb = rankType.value === 'score' ? b.score : b.activityPoints;
    return vb - va;
  });
});

/** 进度和数值 */
const metricOf = (row: Row) => (rankType.value === 'score' ? row.score : row.activityPoints);
const progressOf = (row: Row) => {
  const val = metricOf(row);
  // 简单归一：按可见列表最大值
  const max = Math.max(...rows.value.map(metricOf), 1);
  return Math.min(100, Math.round((val / max) * 100));
};

/** 样式辅助 */
const badgeClass = (rank: number) => (rank === 1 ? 'gold' : rank === 2 ? 'silver' : rank === 3 ? 'bronze' : '');
const rowClass = ({ row }: { row: Row }) => (row.rank <= 3 ? `row-top${row.rank}` : '');

/** 工具 */
const refreshCurrent = async () => {
  refreshing.value = true;
  try {
    const key = `${activeTab.value}:${rankType.value}`;
    delete cache.value[key];
    await fetchBoard(rankType.value, activeTab.value);
    ElMessage.success('已刷新');
  } finally {
    refreshing.value = false;
  }
};
const formatDate = (iso: string) => {
  if (!iso) return '未知';
  const d = new Date(iso);
  if (isNaN(d.getTime())) return '未知';
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

/** 初始化 */
onMounted(async () => {
  // 取 userId
  try {
    const raw = localStorage.getItem('user');
    userId.value = raw ? (JSON.parse(raw)?.Id ?? 0) : 0;
  } catch { userId.value = 0; }

  await fetchBoard(rankType.value, activeTab.value); // 默认：总榜-学习成绩
  fetchMe();
});
</script>

<style scoped>
/* 背景与基础 */
.page-wrap {
  min-height: 100vh;
  background: linear-gradient(180deg, #eaf6ff 0%, #f9fbff 100%);
  padding: 28px 20px 40px;
  box-sizing: border-box;
}

/* 顶部横幅 */
.hero {
  max-width: 1440px;
  margin: 0 auto 14px;
  text-align: center;
  background: #ffffffcc;
  border-radius: 20px;
  padding: 28px 20px;
  backdrop-filter: blur(6px);
  box-shadow: 0 6px 24px rgba(46, 91, 255, 0.06);
}
.hero h1 {
  margin: 0;
  color: #ff66b3;
  font-size: 32px;
  letter-spacing: 1px;
}
.hero p { margin: 8px 0 0; color: #7a7a7a; }

/* 工具条 */
.toolbar {
  max-width: 1440px;
  margin: 0 auto 16px;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 12px;
  align-items: center;
  padding: 14px 20px;
  border-radius: 16px;
}
.glass { background: #fff; box-shadow: 0 6px 20px rgba(0,0,0,.05); }
.tabs :deep(.el-tabs__item) { border-radius: 10px; }
.ctrl { display: flex; align-items: center; gap: 10px; }

/* 两列布局 */
.content-grid {
  max-width: 1440px;
  margin: 0 auto 20px;
  display: grid;
  grid-template-columns: 1.7fr;
  gap: 16px;
}

/* 卡片通用 */
.card { border-radius: 18px; overflow: hidden; }
.card :deep(.el-card__header) { border-bottom: none; }
.card-header {
  display: flex; justify-content: space-between; align-items: baseline;
  padding: 20px 24px;
  border-bottom: 1px solid #f2f3f5;
}
.card-header .title { margin: 0; color: #ff66b3; }
.card-header .title small { color: #999; font-weight: 400; margin-left: 6px; }
.card-header .sub { color: #a8abb2; font-size: 20px; }

/* 榜单表格 */
.table-wrap { width: 100%; }
.rank-table { --el-table-border-color: #f1f2f3; --el-table-header-text-color:#606266; --el-table-row-hover-bg-color:#fafcff; }

.rank-badge {
  display: inline-grid; place-items: center;
  width: 42px; height: 42px; border-radius: 999px; font-weight: 800; font-size: 16px;
  background: #f2f3f5; color: #606266;
}
.rank-badge.gold   { background: linear-gradient(135deg,#ffb900,#ff7730); color: #fff; }
.rank-badge.silver { background: linear-gradient(135deg,#cfcfcf,#f5f5f5); color: #333; }
.rank-badge.bronze { background: linear-gradient(135deg,#cd7f32,#e9967a); color: #fff; }

.rank-table :deep(.el-table__row) { height: 64px; }
.rank-table :deep(.cell) { font-size: 16px; }

.user { display: flex; align-items: center; gap: 12px; }
.user .avatar { width: 52px; height: 52px; border-radius: 50%; object-fit: cover; }
.user .meta .name { font-weight: 600; }

.metric { display: grid; grid-template-columns: 90px 1fr; align-items: center; gap: 14px; }
.metric .metric-num { font-weight: 800; font-size: 18px; color: #606266; }
.metric .progress { margin-right: 20px; }
.rank-table :deep(.el-progress-bar__outer) { height: 12px !important; }

.rewards { display: inline-flex; gap: 8px; }

/* 三强底色 */
.row-top1 { --el-table-tr-bg-color: #fffaf0; }
.row-top2 { --el-table-tr-bg-color: #f9fbff; }
.row-top3 { --el-table-tr-bg-color: #fff8f5; }

/* 个人卡片 */
.me-card .me-head { display: flex; align-items: center; gap: 8px; color: #ff66b3; font-weight: 600; }
.me-body { padding: 6px 2px; }
.me-metrics { display: grid; grid-template-columns: repeat(2,1fr); gap: 12px; }
.me-metrics .box { background: #f8fafc; border-radius: 14px; padding: 12px; box-shadow: inset 0 0 0 1px #f1f5f9; }
.me-metrics .lab { color: #909399; font-size: 12px; }
.me-metrics .val { font-weight: 800; font-size: 20px; }
.me-metrics .val { font-size: 22px; }
.me-metrics .lab { font-size: 13px; }
.me-metrics .val.trend { display: inline-flex; gap: 6px; align-items: center; }
.best .title-sm { font-weight: 700; margin-bottom: 6px; }
.best .list { margin: 0; padding-left: 18px; color: #606266; }
.best .ts { color: #a8abb2; }

/* 奖励卡片 */
.reward-card .reward-head { display: flex; align-items: center; gap: 8px; color: #ff66b3; font-weight: 700; }
.reward-grid { display: grid; grid-template-columns: repeat(4,minmax(180px,1fr)); gap: 12px; }
.reward-item { background: #f8fafc; padding: 12px; border-radius: 14px; display: flex; gap: 12px; align-items: center; }
.badge { width: 40px; height: 40px; display: grid; place-items: center; border-radius: 50%; color: #fff; font-weight: 800; }
.badge-1 { background: linear-gradient(135deg,#ffb900,#ff7730); }
.badge-2 { background: linear-gradient(135deg,#cfcfcf,#f5f5f5); color:#333; }
.badge-3 { background: linear-gradient(135deg,#cd7f32,#e9967a); }
.badge-oth { background: linear-gradient(135deg,#4ab8ff,#91d5ff); }
.r-title { font-weight: 700; }
.r-sub { color: #909399; font-size: 13px; }

@media (max-width: 992px) {
  .content-grid { grid-template-columns: 1fr; max-width: 96vw; }
  .hero, .toolbar { max-width: 96vw; }
  .reward-grid { grid-template-columns: repeat(2,1fr); }
}
@media (max-width: 560px) {
  .toolbar { grid-template-columns: 1fr; }
  .ctrl { justify-content: space-between; }
  .reward-grid { grid-template-columns: 1fr; }
}
</style>