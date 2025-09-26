<template>
  <div class="listen-learning">
    <!-- 页面头部 -->
    <header class="page-header">
      <h1>听力训练</h1>
      <p>按考试级别、年份或关键词筛选听力真题，点击卡片进入练习模式</p>
      <div class="user-stats">
        <el-tag type="success" size="large" effect="light">
          <el-icon class="stats-icon"><Trophy /></el-icon>
          您已完成 {{ completedPracticesCount }} 次听力练习
        </el-tag>
      </div>
    </header>

    <!-- 列表视图 -->
    <div v-if="!selectedPaper" class="list-view">
      <!-- 筛选卡片 -->
      <el-card class="filter-card" shadow="never">
        <el-space wrap>
          <el-select v-model="filters.level" placeholder="考试级别" clearable>
            <el-option label="CET-4" value="CET-4" />
            <el-option label="CET-6" value="CET-6" />
          </el-select>
          <el-select v-model="filters.year" placeholder="年份" clearable>
            <el-option v-for="y in years" :key="y" :label="y.toString()" :value="y" />
          </el-select>
          <el-input
            v-model="searchTerm"
            placeholder="关键词搜索"
            clearable
            class="search-input"
            @keyup.enter="searchPapers"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-button type="primary" @click="searchPapers" :loading="loading">
            <el-icon class="el-icon--left"><Search /></el-icon>搜索
          </el-button>
        </el-space>
      </el-card>

      <!-- 空状态或加载 -->
      <el-empty v-if="papers.length===0 && !loading" description="暂无听力真题" />
      <div v-if="loading" class="loading-container">
        <el-skeleton rows="3" animated />
        <el-skeleton rows="3" animated />
      </div>

      <!-- 真题卡片列表 -->
      <el-row v-if="papers.length>0" :gutter="20" class="list-grid">
        <el-col v-for="paper in papers" :key="paper.id" :xs="24" :sm="12" :md="8" :lg="6">
          <el-card class="list-item" shadow="hover" @click="selectPaper(paper)">
            <div class="item-info">
              <div class="item-title">{{ paper.level }} {{ paper.year }} {{ paper.session }}</div>
              <div class="item-meta">共 {{ paper.sectionCount }} 部分</div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 分页 -->
      <div v-if="papers.length>0" class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[8,16,24]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="totalPapers"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          background
        />
      </div>
    </div>

    <!-- 练习视图 -->
    <div v-else class="practice-view">
      <el-card class="detail-card" shadow="hover">
        <!-- 头部：返回+标题+进度 -->
        <div class="detail-header">
          <el-button plain @click="backToList">
            <el-icon class="el-icon--left"><ArrowLeft /></el-icon>返回列表
          </el-button>
          <div class="center-title">{{ selectedPaper.level }} {{ selectedPaper.year }} {{ selectedPaper.session }} 听力</div>
          <div class="right-info">
            <span>第 {{ currentSectionIndex+1 }} / {{ sections.length }} 部分</span>
          </div>
        </div>

        <!-- 音频+题目 -->
        <div class="detail-content">
          <!-- 统一使用试卷级别的 audioUrl -->
          <audio controls class="audio-player" :src="selectedPaper.audioUrl">
            您的浏览器不支持 audio 元素。
          </audio>
          <div v-for="(q, qi) in currentSection.questions" :key="q.id" class="question-item">
            <div class="question-stem"><span>{{ q.order }}.</span> {{ q.stem }}</div>
            <el-radio-group v-model="answers[currentSectionIndex][qi]">
              <el-radio
                v-for="opt in q.options"
                :key="opt.label"
                :value="opt.label"
                :class="{
                  'correct-answer': submitted && opt.label === q.correctOption,
                  'wrong-answer': submitted
                    && answers[currentSectionIndex][qi] === opt.label
                    && opt.label !== q.correctOption
                }"
              >
                {{ opt.label }}) {{ opt.content }}
              </el-radio>
            </el-radio-group>
          </div>
        </div>

        <!-- 底部导航+提交 -->
        <div class="detail-footer">
          <el-button @click="prevSection" :disabled="currentSectionIndex===0">上一部分</el-button>
          <el-button type="primary" @click="submitAnswers" v-if="!submitted">提交答案</el-button>
          <div v-if="submitted" class="score-display">得分：{{ score }} / {{ totalQuestions }}</div>
          <el-button @click="nextSection" :disabled="currentSectionIndex>=sections.length-1">下一部分</el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { Search, ArrowLeft, Trophy } from '@element-plus/icons-vue';
import { useRouter, useRoute } from 'vue-router';
import { completeLevel as completeAdventureLevel } from '../utils/localProgress';
import api from '../utils/axios'
// —— 定义几个接口，避免 any —— 
interface Paper {
  id: number;
  level: string;
  year: number;
  session: string;
  audioUrl: string;
  sectionCount: number;
}
interface Option {
  label: string;
  content: string;
}
interface Question {
  id: number;
  order: number;
  stem: string;
  correctOption: string;
  options: Option[];
}
interface Section {
  id: number;
  order: number;
  questions: Question[];
}

// —— 状态 —— 
const loading = ref(false);
const papers = ref<Paper[]>([]);
const totalPapers = ref(0);
const currentPage = ref(1);
const pageSize = ref(8);
const filters = reactive<{ level?: string; year?: number }>({});
const searchTerm = ref('');
const years = Array.from({ length: 5 }).map((_, i) => new Date().getFullYear() - i);

const completedPracticesCount = ref(0);

const selectedPaper = ref<Paper | null>(null);
const sections = ref<Section[]>([]);
const currentSectionIndex = ref(0);
const answers = reactive<string[][]>([]);
const submitted = ref(false);
const score = ref(0);

// —— 计算属性 —— 
const totalQuestions = computed(() =>
  sections.value.reduce((sum, sec) => sum + sec.questions.length, 0)
);

const currentSection = computed<Section>(() =>
  sections.value[currentSectionIndex.value] || { id: 0, order: 0, questions: [] }
);

// —— 方法 —— 
async function searchPapers(): Promise<void> {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    params.set('page', currentPage.value.toString());
    params.set('size', pageSize.value.toString());
    if (filters.level) params.set('level', filters.level);
    if (filters.year !== undefined) params.set('year', filters.year.toString());
    if (searchTerm.value) params.set('q', searchTerm.value);

    const res = await api.get('/api/listening-papers?' + params.toString())
    papers.value = res.data.items
    totalPapers.value = res.data.total

    const cntRes = await api.get('/api/listening-practice/completed/count')
    completedPracticesCount.value = cntRes.data.count
  } finally {
    loading.value = false;
  }
}

function handleSizeChange(newSize: number) {
  pageSize.value = newSize;
  searchPapers();
}

function handleCurrentChange(newPage: number) {
  currentPage.value = newPage;
  searchPapers();
}

async function selectPaper(paper: Paper): Promise<void> {
  selectedPaper.value = paper;
  loading.value = true;
  try {
    const res = await fetch(`/api/listening-papers/${paper.id}/sections`);
    const data = await res.json();
    sections.value = data.sections as Section[];
    // 初始化答案数组
    answers.splice(0, answers.length);
    sections.value.forEach(sec => answers.push(Array(sec.questions.length).fill('')));
  } finally {
    loading.value = false;
    currentSectionIndex.value = 0;
  }
}

function backToList() {
  selectedPaper.value = null;
  submitted.value = false;
  score.value = 0;
  currentSectionIndex.value = 0;
}

function prevSection() {
  if (currentSectionIndex.value > 0) currentSectionIndex.value--;
}

function nextSection() {
  if (currentSectionIndex.value < sections.value.length - 1)
    currentSectionIndex.value++;
}

async function submitAnswers(): Promise<void> {
  // 1) 本地算分
  let cnt = 0;
  sections.value.forEach((sec, si) =>
    sec.questions.forEach((q, qi) => {
      if (answers[si][qi] === q.correctOption) cnt++;
    })
  );
  score.value = cnt;


  submitted.value = true;

  // 2) 提交给后端
  await api.post('/api/listening-practice/submit', {
    paperId: selectedPaper.value!.id,
    answers: sections.value.flatMap((sec, si) =>
      sec.questions.map((q, qi) => ({
        questionId: q.id,
        response: answers[si][qi],
      }))
    ),
  });

  // 3) 再次拉取最新完成次数
  const cntRes = await api.get('/api/listening-practice/completed/count');
  completedPracticesCount.value = cntRes.data.count;
}

onMounted(async () => {
  await searchPapers()
})

</script>


  
<style scoped>
  .listen-learning {
    width: 80%;
    height: 100%;
    margin: 0;
    padding: 0;
  }
  .page-header {
    text-align: center;
    margin-bottom: 30px;
  }
  .page-header h1 {
    color: #409EFF;
    font-size: 28px;
    margin-bottom: 10px;
  }
  .page-header p {
    color: #606266;
    font-size: 16px;
  }
  .user-stats {
    margin-top: 15px;
  }
  .filter-card {
    margin-bottom: 20px;
    border-radius: 8px;
  }
  .search-input {
    width: 250px;
  }
  .list-grid {
    margin-top: 20px;
  }
  .list-item {
    cursor: pointer;
    transition: transform .3s;
  }
  .list-item:hover {
    transform: translateY(-4px);
  }
  .item-info {
    text-align: center;
    padding: 20px;
  }
  .item-title {
    font-size: 16px;
    font-weight: 500;
  }
  .item-meta {
    color: #909399;
    margin-top: 8px;
  }
  .loading-container {
    padding: 20px;
  }
  .pagination-container {
    text-align: center;
    margin-top: 20px;
  }
  .detail-card {
    border-radius: 8px;
    margin-bottom: 20px;
  }
  .detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    border-bottom: 1px solid #ebeef5;
    padding-bottom: 10px;
  }
  .center-title {
    font-size: 20px;
    font-weight: bold;
    color: #303133;
  }
  .right-info {
    color: #909399;
  }
  .detail-content {
    padding: 20px 0;
  }
  .audio-player {
    width: 100%;
    margin-bottom: 20px;
  }
  .question-item {
    margin-bottom: 20px;
    text-align: left;
  }
  .question-stem {
    font-size: 16px;
    margin-bottom: 8px;
  }
  .question-item .el-radio-group {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px 20px;
  }

  .question-item .el-radio-group .el-radio__label {
    text-align: left;
  }

  .detail-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-top: 10px;
    border-top: 1px solid #ebeef5;
  }
  .score-display {
    color: #67C23A;
    font-weight: bold;
  }
  /* 提交后正确答案变绿 */
  ::v-deep(.correct-answer) .el-radio__label {
    color: #67C23A;
    font-weight: bold;
  }

  /* 提交后用户选错的答案变红 */
  ::v-deep(.wrong-answer .el-radio__input.is-checked + .el-radio__label) {
    color: #F56C6C !important;
    font-weight: bold;
  }
</style>
  