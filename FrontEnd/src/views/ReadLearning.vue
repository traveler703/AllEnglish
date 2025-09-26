<template>
  <div class="read-learning">
    <!-- 页面头部：标题和说明 -->
    <header class="page-header">
      <h1>阅读训练</h1>
      <p v-if="!isAdventureMode">按课程类别、文章类型或难度筛选阅读材料，提升您的阅读理解能力</p>
      <p v-else class="adventure-mode-tip">🗺️ 闯关模式 - 关卡 {{ levelId }}：完成阅读练习并获得60%以上的分数即可通过关卡</p>
      <!-- 用户完成文章数显示区域 -->
      <div class="user-stats" v-if="!isAdventureMode">
        <el-tag type="success" size="large" effect="light">
          <el-icon class="stats-icon"><Trophy /></el-icon>
          您已完成 <span class="completed-count">{{ completedArticlesCount }}</span> 篇文章阅读练习
        </el-tag>
      </div>
    </header>

    <!-- 移除调试信息 -->

    <!-- 筛选条件卡片：课程类别、文章类型、难度等级和关键词搜索 -->
    <el-card class="filter-card" shadow="never">
      <el-space wrap>
        <!-- 课程类别筛选下拉框 -->
        <el-select v-model="filters.courseId" placeholder="课程类别" clearable>
          <template #prefix>
            <span class="select-prefix">课程类别:</span>
          </template>
          <el-option label="基础课程" :value="1" />
          <el-option label="进阶课程" :value="2" />
          <el-option label="高级课程" :value="3" />
        </el-select>

        <!-- 文章类型筛选下拉框 -->
        <el-select v-model="filters.category" placeholder="文章类型" clearable>
          <template #prefix>
            <span class="select-prefix">文章类型:</span>
          </template>
          <el-option label="新闻" value="新闻" />
          <el-option label="故事" value="故事" />
          <el-option label="说明文" value="说明文" />
          <el-option label="科技" value="科技" />
        </el-select>

        <!-- 难度等级筛选下拉框 -->
        <el-select v-model="filters.difficulty" placeholder="难度等级" clearable>
          <template #prefix>
            <span class="select-prefix">难度等级:</span>
          </template>
          <el-option label="初级" :value="1" />
          <el-option label="中级" :value="2" />
          <el-option label="高级" :value="3" />
        </el-select>

        <!-- 关键词搜索输入框 -->
        <el-input 
          v-model="searchTerm" 
          placeholder="关键词搜索" 
          clearable 
          class="search-input"
          @keyup.enter="searchReadings"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>

        <!-- 搜索按钮 -->
        <el-button type="primary" @click="searchReadings" :loading="loading">
          <el-icon class="el-icon--left"><Search /></el-icon>搜索
        </el-button>
      </el-space>
    </el-card>

    <!-- 阅读材料列表视图 - 当没有选中文章时显示 -->
    <div v-if="!selectedReading" class="reading-list">
      <!-- 空状态显示 - 当没有文章且不在加载状态时显示 -->
      <el-empty v-if="readings.length === 0 && !loading" description="暂无阅读材料" />

      <!-- 加载状态骨架屏 - 数据加载中时显示 -->
      <div v-if="loading" class="loading-container">
        <el-skeleton :rows="3" animated />
        <el-skeleton :rows="3" animated />
      </div>

      <!-- 阅读材料网格布局 -->
      <el-row v-if="readings.length > 0" :gutter="20" class="article-grid">
        <!-- 在极小屏幕上一行显示1个，小屏幕2个，中等屏幕3个，大屏幕4个 -->
        <el-col
          v-for="(article, index) in readings"
          :key="article.articleId || index"
          :xs="24"
          :sm="12"
          :md="8"
          :lg="6"
          class="article-column"
        >
          <!-- 阅读材料卡片 -->
          <el-card class="reading-item" shadow="hover" @click="selectReading(article)">
            <!-- 封面图片区域 -->
            <div class="reading-cover">
              <img
                :src="article.coverImage || defaultCoverImage"
                alt="封面"
                 @error="e => {
                  (e.currentTarget as HTMLImageElement).src = defaultCoverImage
                }"
              />
              <div class="reading-difficulty">{{ article.difficultyLevel || '未知' }}</div>
            </div>

            <!-- 阅读信息区域 -->
            <div class="reading-info">
              <!-- 标题区域 -->
              <div class="reading-title-wrapper" :title="article.title">
                <span class="reading-title">{{ article.title || '无标题' }}</span>
              </div>

              <!-- 标签区域 -->
              <div class="reading-tags">
                <el-tag size="small" type="success">{{ (article.category || '').trim() }}</el-tag>
                <el-tag size="small" type="info">{{ article.difficultyLevel || '未知难度' }}</el-tag>
                <!-- 显示最高分标签，如果有的话 -->
                <el-tag size="small" type="primary" v-if="article.highestScore !== undefined && article.highestScore > 0" class="score-tag" effect="light">
                  <el-icon><Trophy /></el-icon>
                  最高分: {{ article.highestScore }}
                </el-tag>
              </div>

              <!-- 内容预览 -->
              <p class="reading-preview">{{ article.description || '暂无描述' }}</p>

              <!-- 阅读时间和字数信息 -->
              <div class="reading-meta">
                <span>
                  <el-icon><Clock /></el-icon>{{ article.readingTime || 0 }}分钟
                </span>
                <span>
                  <el-icon><Document /></el-icon>{{ article.wordCount || 0 }}字
                </span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 分页控件 -->
      <div class="pagination-container" v-if="readings.length > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[8, 16, 24, 32]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="totalReadings"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          background
        />
      </div>
    </div>

    <!-- 阅读详情页视图 - 选中文章后显示 -->
    <div v-else class="reading-detail">
      <!-- 详情页加载状态 -->
      <div v-if="detailLoading" class="detail-loading-overlay">
        <div class="detail-loading-spinner">
          <el-icon class="is-loading"><Loading /></el-icon>
          <p>加载文章内容中...</p>
        </div>
      </div>

      <el-card class="detail-card" shadow="hover">
        <!-- 详情页头部：返回按钮、标题和翻页控件 -->
        <div class="detail-header">
          <!-- 左侧返回按钮 -->
          <div class="left-actions">
            <el-button @click="backToList" plain>
              <el-icon class="el-icon--left"><ArrowLeft /></el-icon>返回列表
            </el-button>
          </div>

          <!-- 中间标题 -->
          <div class="center-title">{{ selectedReading?.title || '加载中...' }}</div>

          <!-- 右侧翻页控件 -->
          <div class="right-actions">
            <el-button-group>
              <el-button plain title="上一页" :disabled="currentPageInBook <= 1" @click="prevPage">
                <el-icon><ArrowLeft /></el-icon>
              </el-button>
              <el-button plain>{{ currentPageInBook }}/{{ totalPages }}</el-button>
              <el-button plain title="下一页" :disabled="currentPageInBook >= totalPages" @click="nextPage">
                <el-icon><ArrowRight /></el-icon>
              </el-button>
            </el-button-group>
          </div>
        </div>

        <!-- 文章信息和标签 -->
        <div class="article-meta">
          <div class="article-info">
            <span><el-icon><Clock /></el-icon>{{ selectedReading?.readingTime || 15 }}分钟</span>
            <span><el-icon><Document /></el-icon>{{ selectedReading?.wordCount || 500 }}字</span>
            <span><el-icon><Calendar /></el-icon>{{ formatDate(selectedReading?.createdAt) }}</span>
            <!-- 显示用户最高分 -->
            <span v-if="selectedReading?.highestScore" class="highest-score">
              <el-icon><Trophy /></el-icon>最高分: {{ selectedReading.highestScore }}
            </span>
          </div>
          
          <!-- 文章标签 -->
          <div class="article-tags" v-if="selectedReading?.tags && selectedReading.tags.length">
            <el-tag 
              v-for="(tag, index) in selectedReading.tags" 
              :key="index" 
              size="small" 
              effect="plain" 
              class="tag-item"
            >
              {{ tag }}
            </el-tag>
          </div>
        </div>

        <!-- 电子书内容容器 -->
        <div class="e-book-container">
          <!-- 电子书内容区，使用transform实现翻页效果 -->
          <div class="e-book-content" :style="{ transform: `translateX(-${(currentPageInBook - 1) * 100}%)` }">
            <!-- 内容页面循环 -->
            <div v-for="(page, index) in contentPages" :key="index" class="e-book-page">
              <!-- 使用v-html渲染HTML内容 -->
              <div class="page-content" v-html="page" />
            </div>
          </div>
        </div>

        <!-- 详情页底部 -->
        <div class="detail-footer">
          <!-- 未开始练习时显示开始按钮 -->
          <el-button type="primary" @click="startPractice" v-if="!practiceStarted">
            开始练习 {{ hasQuestions ? `(${selectedReading?.questions?.length || 0}题)` : '' }}
          </el-button>
          
          <!-- 练习中显示计时器和按钮 -->
          <div v-else-if="practiceStarted && !showingQuestions" class="practice-timer">
            <el-icon><Timer /></el-icon>
            <span>{{ formatTime(practiceTime) }}</span>
            <!-- 如有练习题，显示开始答题按钮 -->
            <el-button type="warning" @click="showQuestions" v-if="hasQuestions">开始答题</el-button>
            <el-button type="success" @click="finishPractice" :loading="practiceSubmitting">完成练习</el-button>
          </div>
        </div>

        <!-- 练习题部分 -->
        <div v-if="showingQuestions && selectedReading?.questions && selectedReading.questions.length > 0" class="questions-section">
          <h3>练习题</h3>
          
          <!-- 练习结果总分区域 -->
          <div v-if="practiceCompleted && practiceResult" class="practice-result-summary">
            <el-result
              v-if="practiceCompleted && practiceResult"
              :icon="practiceResult.percentage >= 60 ? 'success' : 'warning'"
              :title="`练习完成！得分: ${practiceResult.totalScore}/${practiceResult.maxPossibleScore}`"
              :sub-title="`正确率: ${practiceResult.percentage.toFixed(1)}%，用时: ${formatTime(practiceTime)}`"
            >
              <template #extra>
                <el-progress 
                  :percentage="Number(practiceResult.percentage.toFixed(1))" 
                  :status="practiceResult.percentage >= 60 ? 'success' : 'exception'"
                  :stroke-width="15"
                  :format="() => `${practiceResult!.totalScore}/${practiceResult!.maxPossibleScore}`"
                />
              </template>
            </el-result>
          </div>
          
          <!-- 循环渲染每个练习题 -->
          <div 
            v-for="(question, index) in selectedReading.questions" 
            :key="question?.id || index" 
            class="question-item"
            :class="{
              'answered-correct': practiceCompleted && getQuestionResult(question?.id)?.isCorrect,
              'answered-wrong': practiceCompleted && !getQuestionResult(question?.id)?.isCorrect && userAnswers[question?.id]
            }"
          >
            <!-- 题目头部 -->
            <div class="question-header">
              <span class="question-number">第{{ question?.seqo || (index + 1) }}题</span>
              <div class="question-score-info">
                <!-- 练习完成后显示得分 -->
                <span v-if="practiceCompleted && question && question.id">
                  <span class="score-value">{{ getQuestionResult(question.id)?.score || 0 }}分</span>
                  <el-tag size="small" :type="getQuestionResult(question.id)?.isCorrect ? 'success' : 'danger'" class="result-tag">
                    {{ getQuestionResult(question.id)?.isCorrect ? '正确' : '错误' }}
                  </el-tag>
                </span>
                <!-- 未完成练习显示总分 -->
                <span v-else class="question-score">{{ question?.score || 0 }}分</span>
              </div>
            </div>
            
            <!-- 题干 -->
            <div class="question-stem">{{ question?.stem || '题目内容加载失败' }}</div>
            
            <!-- 题目选项区域 -->
            <div class="question-options">
              <!-- 单选题 -->
              <template v-if="question && question.kind === 1">
                <div class="custom-radio-group">
                  <div 
                    v-for="option in parseOptions(question.options || '')" 
                    :key="option.value" 
                    class="custom-radio-item"
                    :class="{
                      'correct-option': practiceCompleted && getQuestionResult(question.id)?.correctAnswer === option.value,
                      'wrong-option': practiceCompleted && userAnswers[question.id] === option.value && getQuestionResult(question.id)?.correctAnswer !== option.value
                    }"
                  >
                    <el-radio 
                      v-model="userAnswers[question.id]" 
                      :label="option.value"
                      :disabled="practiceCompleted"
                    >
                      {{ option.text }}
                    </el-radio>
                  </div>
                </div>
                
                <!-- 练习完成后显示正确答案 -->
                <div v-if="practiceCompleted && question.id" class="correct-answer-hint">
                  <strong>正确答案:</strong> {{ getQuestionResult(question.id)?.correctAnswer || '未知' }}
                </div>
              </template>
              
              <!-- 填空题 -->
              <template v-else-if="question && question.kind === 2">
                <el-input 
                  v-model="userAnswers[question.id]" 
                  placeholder="请输入答案" 
                  class="fill-blank-input"
                  :disabled="practiceCompleted"
                />
                <!-- 练习完成后显示正确答案 -->
                <div v-if="practiceCompleted && question.id" class="correct-answer-hint">
                  <strong>正确答案:</strong> {{ getQuestionResult(question.id)?.correctAnswer || '未知' }}
                </div>
              </template>
              
              <!-- 其他题型 -->
              <template v-else>
                <div class="option-placeholder">暂不支持的题型 ({{ question?.kind || '未知' }})</div>
              </template>
            </div>
          </div>
          
          <!-- 练习题底部操作区 -->
          <div class="questions-footer">
            <!-- 返回阅读按钮 -->
            <el-button @click="showingQuestions = false">返回阅读</el-button>
            <!-- 完成练习按钮 - 提交后禁用 -->
            <el-button 
              type="success" 
              @click="finishPractice" 
              :loading="practiceSubmitting"
              :disabled="practiceCompleted"
            >
              {{ practiceCompleted ? '已完成' : practiceSubmitting ? '提交中...' : '完成练习' }}
            </el-button>
            <!-- 重新练习按钮 - 完成练习后显示 -->
            <el-button 
              type="primary" 
              @click="restartPractice"
              v-if="practiceCompleted"
            >
              重新练习
            </el-button>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted, onUnmounted, onActivated, computed, nextTick } from 'vue';
import { ElMessage } from 'element-plus';
import { Search, ArrowLeft, ArrowRight, Clock, Document, Timer, Calendar, Loading, Trophy } from '@element-plus/icons-vue';
import { useRouter, useRoute } from 'vue-router';
import { completeLevel as completeAdventureLevel } from '../utils/localProgress';
import { getArticles, getArticleById, submitArticleAnswers, getUserCompletedArticlesCount, getUserArticleHighestScore, type Article, type ArticleDetail, type ArticleParams, type Question, type SubmitAnswersRequest } from '../api';

// 默认封面图片 - Base64编码的内联图片（蓝色背景+文字标识）
const defaultCoverImage = 'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMTUwIiB2aWV3Qm94PSIwIDAgMzAwIDE1MCIgZmlsbD0ibm9uZSI+CiAgPHJlY3Qgd2lkdGg9IjMwMCIgaGVpZ2h0PSIxNTAiIGZpbGw9IiNlMGYyZmUiLz4KICA8cmVjdCB4PSIxMjAiIHk9IjMwIiB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIGZpbGw9IiM2N2UzZjMiIHJ4PSIxMCIgLz4KICA8cGF0aCBkPSJNMTUwIDY1IEExMCAxMCAwIDAgMCAxNTAgOTUiIHN0cm9rZT0id2hpdGUiIHN0cm9rZS13aWR0aD0iNCIgZmlsbD0ibm9uZSIgLz4KICA8dGV4dCB4PSI1MCUiIHk9IjEyMCIgZG9taW5hbnQtYmFzZWxpbmU9Im1pZGRsZSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZm9udC1mYW1pbHk9IkFyaWFsLCBzYW5zLXNlcmlmIiBmb250LXNpemU9IjE2IiBmaWxsPSIjMDA3OWI4Ij7pl6/mnaXlm77niYc8L3RleHQ+Cjwvc3ZnPg==';

// 扩展Article接口以包含最高分字段
interface ExtendedArticle extends Article {
  highestScore?: number;
}

// ===== 响应式状态 =====

const router = useRouter();
const route = useRoute();

// 获取闯关参数
const levelId = ref(route.query.levelId ? parseInt(route.query.levelId as string) : null);
const isAdventureMode = ref(!!levelId.value);

/**
 * 用户ID
 * 从localStorage获取登录用户的ID
 */
const userId = ref<number>(0);

/**
 * 用户完成的文章数量
 * 从API获取，显示在页面头部
 */
const completedArticlesCount = ref(0);

/**
 * 文章列表数据
 * 用于存储从API获取的文章列表
 */
const readings = ref<ExtendedArticle[]>([]);

/**
 * 加载状态标志
 * 控制加载动画和骨架屏显示
 */
const loading = ref(false);

/**
 * 筛选条件对象
 * 包含课程类别、文章类型、难度等级筛选
 * 注意：courseId和difficulty是数字类型，与后端API对应
 * - courseId: 1=基础课程, 2=进阶课程, 3=高级课程
 * - difficulty: 1=初级, 2=中级, 3=高级
 */
const filters = reactive<ArticleParams>({
  // 不设置默认值，避免发送空字符串
});

/**
 * 搜索关键词
 * 用于全文搜索功能
 */
const searchTerm = ref('');

/**
 * 分页相关状态
 * 控制列表分页显示
 */
const currentPage = ref(1);
const pageSize = ref(8);
const totalReadings = ref(0);

// ===== 阅读详情相关 =====

/**
 * 当前选中的阅读材料详情
 * 包含完整的文章内容、练习题等信息
 */
const selectedReading = ref<ArticleDetail | null>(null);

/**
 * 文章内容分页数据
 * 每个元素是一页的HTML内容，用于电子书翻页效果
 */
const contentPages = ref<string[]>([]);

/**
 * 电子书阅读状态
 * 控制电子书翻页和页码显示
 */
const currentPageInBook = ref(1);
const totalPages = ref(1);
const detailLoading = ref(false);

/**
 * 练习题相关状态
 * 控制练习题显示和答题功能
 */
const showingQuestions = ref(false);

/**
 * 用户答案记录
 * 键为题目ID，值为用户选择/输入的答案
 */
const userAnswers = ref<Record<number, string>>({});

/**
 * 练习相关状态
 */
const practiceStarted = ref(false);
const practiceTime = ref(0);
let timer: number | null = null;
// 记录练习的开始时间
const practiceStartTime = ref<Date | null>(null);
const practiceSubmitting = ref(false); // 提交中状态标志

/**
 * 练习结果状态
 * 用于存储后端返回的得分结果
 */
interface QuestionResult {
  questionId: number;
  isCorrect: boolean;
  score: number;
  correctAnswer: string;
}

interface PracticeResult {
  totalScore: number;
  maxPossibleScore: number;
  percentage: number;
  questionResults: QuestionResult[];
}

const practiceResult = ref<PracticeResult | null>(null);
const practiceCompleted = ref(false); // 标记练习是否已完成

// ===== 计算属性 =====

/**
 * 判断是否有激活的筛选条件
 * 用于UI条件显示
 */
const hasActiveFilters = computed(() => {
  return filters.courseId || filters.category || filters.difficulty;
});

/**
 * 判断是否有练习题
 * 用于条件显示练习功能按钮
 */
const hasQuestions = computed(() => {
  return selectedReading.value?.questions && selectedReading.value.questions.length > 0;
});

// ===== 方法 =====

/**
 * 获取用户完成的文章数量
 * 调用后端API: GET /api/Articles/user/{userId}/completed-articles/count
 */
const fetchCompletedArticlesCount = async () => {
  try {
    const result = await getUserCompletedArticlesCount(userId.value);
    completedArticlesCount.value = result.completedCount;
  } catch (error) {
    console.error('获取用户完成文章数量失败:', error);
    completedArticlesCount.value = 0;
  }
};

/**
 * 搜索阅读材料
 * 根据筛选条件和分页参数获取文章列表
 * 调用后端API: GET /api/Articles
 */
const searchReadings = async () => {
  loading.value = true;
  
  try {
    // 构建参数，确保不发送无效值
    const params: ArticleParams = {
      pageNumber: currentPage.value,
      pageSize: pageSize.value
    };

    // 只添加有值的参数，避免发送undefined
    if (filters.courseId) params.courseId = filters.courseId;
    if (filters.category) params.category = filters.category;
    if (filters.difficulty) params.difficulty = filters.difficulty;
    if (searchTerm.value) params.searchTerm = searchTerm.value;

    // 调用API获取文章列表
    const response = await getArticles(params);
    
    // 确保我们有数据
    if (response && Array.isArray(response.items)) {
      // 处理数据，确保关键字段存在
      readings.value = response.items.map(article => ({
        ...article,
        // 确保category是字符串并去除空白
        category: article.category?.trim() || '未分类',
        // 默认值
        readingTime: article.readingTime || 5,
        wordCount: article.wordCount || 0,
        description: article.description || '暂无描述'
      }));
      
      totalReadings.value = response.totalItems || 0;
      
      // 获取每篇文章的最高分
      await fetchArticlesHighestScores();
      
    } else {
      console.warn('API响应格式不符合预期:', response);
      readings.value = [];
      totalReadings.value = 0;
    }
  } catch (error) {
    console.error('获取阅读材料失败:', error);
    ElMessage.error('获取阅读材料失败，请稍后重试');
    readings.value = [];
    totalReadings.value = 0;
  } finally {
    loading.value = false;
  }
};

/**
 * 获取所有文章的最高分
 * 为每篇文章添加highestScore属性
 */
const fetchArticlesHighestScores = async () => {
  if (readings.value.length === 0) return;
  
  try {
    const promises = readings.value.map(async article => {
      if (!article.articleId) return;
      
      const result = await getUserArticleHighestScore(userId.value, article.articleId);
      article.highestScore = result.highestScore;
    });
    
    await Promise.all(promises);
  } catch (error) {
    console.error('获取文章最高分失败:', error);
  }
};

/**
 * 处理每页数量变化
 * 更新页面大小并重新加载数据
 * @param size 新的每页数量
 */
const handleSizeChange = (size: number) => {
  pageSize.value = size;
  searchReadings();
};

/**
 * 处理页码变化
 * 更新当前页码并重新加载数据
 * @param page 新的页码
 */
const handleCurrentChange = (page: number) => {
  currentPage.value = page;
  searchReadings();
};

/**
 * 格式化日期
 * 将ISO日期字符串转换为易读格式
 * @param dateStr ISO日期字符串
 * @returns 格式化的日期字符串 (YYYY-MM-DD)
 */
const formatDate = (dateStr?: string): string => {
  if (!dateStr) return '未知日期';
  try {
    const date = new Date(dateStr);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  } catch (e) {
    console.error('日期格式化错误:', e);
    return '日期格式错误';
  }
};

/**
 * 显示练习题
 * 切换到练习题显示模式，并记录开始时间
 */
const showQuestions = () => {
  if (selectedReading.value?.questions && selectedReading.value.questions.length > 0) {
    // 完全重置所有练习相关状态
    practiceStartTime.value = new Date();
    practiceCompleted.value = false;
    practiceResult.value = null;
    practiceTime.value = 0;
    userAnswers.value = {};
    
    // 确保计时器状态正确
    stopTimer(); // 先停止可能存在的计时器
    startTimer(); // 然后重新开始计时
    
    // 显示练习题
    showingQuestions.value = true;
    ElMessage.info('开始答题，完成后点击"完成练习"提交所有答案');
  } else {
    ElMessage.warning('当前文章没有练习题');
  }
};

/**
 * 解析选项字符串为选项数组
 * 处理后端返回的选项文本，提取选项值和文本
 * 支持两种格式：
 * 1. "A) 选项1 B) 选项2 C) 选项3" (单行格式)
 * 2. "A. 选项1\nB. 选项2" (多行格式)
 * 
 * @param optionsStr 选项字符串
 * @returns 解析后的选项数组
 */
const parseOptions = (optionsStr: string): { value: string; text: string }[] => {
  if (!optionsStr) return [];
  
  try {
    // 定义更精确的选项匹配正则表达式，匹配A)、B)等形式的选项
    const optionRegex = /([A-Z])\)\s*([^A-Z\)]+?)(?=(?:\s+[A-Z]\))|$)/g;
    const options: { value: string; text: string }[] = [];
    let match;
    
    // 尝试使用正则表达式匹配所有选项
    let tempStr = optionsStr;
    let matches = [];
    
    // 先尝试找出所有可能的选项标记
    const possibleMarkers = tempStr.match(/[A-Z]\)/g);
    
    if (possibleMarkers && possibleMarkers.length > 0) {
      console.log('找到可能的选项标记:', possibleMarkers);
      
      // 对每个标记处理
      for (let i = 0; i < possibleMarkers.length; i++) {
        const currentMarker = possibleMarkers[i];
        const nextMarker = possibleMarkers[i + 1];
        const markerPos = tempStr.indexOf(currentMarker);
        
        if (markerPos >= 0) {
          let endPos;
          if (nextMarker) {
            endPos = tempStr.indexOf(nextMarker, markerPos + 1);
          } else {
            endPos = tempStr.length;
          }
          
          if (endPos > markerPos) {
            const optionText = tempStr.substring(markerPos, endPos).trim();
            const value = currentMarker.charAt(0); // 如 "A"
            options.push({
              value,
              text: optionText // 如 "A) 选项内容"
            });
          }
        }
      }
      
      return options;
    }
    
    // 如果上面的方法失败，回退到分行处理
    if (options.length === 0 && optionsStr.includes('\n')) {
      return optionsStr.split('\n')
        .filter(option => option.trim())
        .map(option => {
          // 匹配 A.、A)、(A) 等格式
          const match = option.match(/^(?:\()?([A-Z])[\.\)]?\)?\s*(.+)$/);
          if (match) {
            return {
              value: match[1], // 选项值，如 "A"
              text: option     // 完整文本，如 "A. 选项1"
            };
          }
          return { value: '', text: option };
        });
    }
    
    // 如果依然没有解析出选项，尝试直接以空格分隔
    if (options.length === 0) {
      const parts = optionsStr.split(' ').filter(p => p.trim());
      const letterOptions = parts.filter(p => /^[A-Z]\)/.test(p));
      
      if (letterOptions.length > 0) {
        return letterOptions.map(opt => ({
          value: opt.charAt(0),
          text: opt
        }));
      }
    }
    
    // 返回解析结果，或空数组
    return options;
    
  } catch (e) {
    console.error('选项解析错误:', e);
    return [];
  }
};

/**
 * 判断答案是否正确
 * 用于显示答题结果标签
 * 
 * @param question 当前问题
 * @returns 是否正确
 */
const isCorrect = (question: Question): boolean => {
  if (!question) return false;
  
  const userAnswer = userAnswers.value[question.id];
  const correctAnswer = question.answerKey?.trim().toUpperCase() || '';
  
  return userAnswer.trim().toUpperCase() === correctAnswer;
};

/**
 * 选择阅读材料并获取详情
 * 当用户点击文章卡片时调用，加载文章完整内容
 * 调用后端API: GET /api/Articles/{id}/detail
 * 
 * @param reading 阅读材料基本信息
 */
const selectReading = async (reading: Article) => {
  if (!reading || !reading.articleId) {
    ElMessage.error('无效的文章信息');
    return;
  }
  
  try {
    // 首先设置loading状态，使用内联加载指示器
    detailLoading.value = true;
    
    // 先设置选中状态，让视图切换到详情页
    selectedReading.value = { ...reading } as ArticleDetail;
    
    // 重置所有练习相关状态
    showingQuestions.value = false;
    practiceStarted.value = false;
    practiceCompleted.value = false;
    practiceResult.value = null;
    practiceTime.value = 0;
    userAnswers.value = {};
    if (timer) {
      stopTimer();
    }
    contentPages.value = []; // 清空内容页
    
    // 确保DOM已更新，再进行下一步操作
    await nextTick();
    
    // 调用详情API获取完整内容
    const articleDetail = await getArticleById(reading.articleId);
    
    if (!articleDetail) {
      throw new Error('获取文章详情失败');
    }
    
    // 更新详情数据
    selectedReading.value = articleDetail;
    
    // 获取用户在此文章的最高分
    if (articleDetail && articleDetail.articleId) {
      try {
        const scoreResult = await getUserArticleHighestScore(userId.value, articleDetail.articleId);
        if (selectedReading.value) { // 确保selectedReading还存在
          selectedReading.value.highestScore = scoreResult.highestScore;
        }
      } catch (error) {
        console.error('获取文章最高分失败:', error);
      }
    }
    
    // 处理文章内容（不再分页，显示为单页）
    const content = articleDetail.content || articleDetail.description || '';
    if (content) {
      try {
        // 按段落分割并清理空段落
        const paragraphs = content.split('\n').filter(p => p.trim());
        // 将所有段落包装为单页内容
        const singlePageContent = paragraphs.map(p => `<p>${p}</p>`).join('');
        contentPages.value = [`<div>${singlePageContent}</div>`];
      } catch (e) {
        console.error('内容处理失败:', e);
        contentPages.value = [`<div><p>${content}</p></div>`];
      }
    } else {
      contentPages.value = [`<div><p>暂无内容</p></div>`];
    }
    
    // 设置为单页，移除翻页相关状态
    totalPages.value = 1;
    currentPageInBook.value = 1;
  } catch (error) {
    console.error('获取文章详情失败:', error);
    ElMessage.error('获取文章详情失败，请稍后重试');
    
    // 确保至少有基本内容显示
    if (!contentPages.value.length) {
      contentPages.value = [`<div><p>${reading.description || '无法加载文章内容'}</p></div>`];
      totalPages.value = 1;
      currentPageInBook.value = 1;
    }
  } finally {
    // 延迟关闭加载状态，确保DOM已完全更新
    setTimeout(() => {
      detailLoading.value = false;
    }, 200);
  }
};

/**
 * 返回文章列表
 * 清除选中的文章，停止练习计时
 */
const backToList = () => {
  if (practiceStarted.value) {
    stopTimer();
  }
  practiceStarted.value = false;
  selectedReading.value = null;
};

/**
 * 翻到上一页
 * 控制电子书阅读视图的页面切换
 */
const prevPage = () => {
  if (currentPageInBook.value > 1) {
    currentPageInBook.value--;
  }
};

/**
 * 翻到下一页
 * 控制电子书阅读视图的页面切换
 */
const nextPage = () => {
  if (currentPageInBook.value < totalPages.value) {
    currentPageInBook.value++;
  }
};

/**
 * 开始阅读练习
 * 启动计时器记录练习时长
 */
const startPractice = () => {
  // 重置练习状态
  practiceStarted.value = true;
  practiceCompleted.value = false;
  practiceResult.value = null;
  
  // 停止可能已经存在的计时器
  if (timer) {
    stopTimer();
  }
  
  // 重置计时器和答案
  practiceTime.value = 0;
  userAnswers.value = {};
  startTimer();
  
  ElMessage.success('练习已开始，计时开始');
};

/**
 * 完成阅读练习
 * 停止计时并提交答案
 */
const finishPractice = async () => {
  if (practiceCompleted.value) {
    ElMessage.info('您已完成本次练习，请返回阅读或选择其他文章');
    return;
  }

  stopTimer();

  // 如果有答题记录，则提交到服务器
  if (Object.keys(userAnswers.value).length > 0 && selectedReading.value?.articleId) {
    // 提交答案
    try {
      practiceSubmitting.value = true;

      // 构造答案提交数据
      const endTime = new Date();
      const submitData: SubmitAnswersRequest = {
        userId: userId.value,
        articleID: selectedReading.value.articleId,
        startTime: practiceStartTime.value?.toISOString() || new Date().toISOString(),
        endTime: endTime.toISOString(),
        answers: Object.entries(userAnswers.value).map(([questionId, response]) => ({
          questionId: parseInt(questionId),
          userResponse: response
        }))
      };

      // 调用API提交答案
      const response = await submitArticleAnswers(submitData);

      // 保存练习结果
      practiceResult.value = response;
      practiceCompleted.value = true;

      let completionMessage = `练习已完成！您的得分是: ${response.totalScore}/${response.maxPossibleScore}`;

      // 如果是闯关模式且得分达到要求，完成对应关卡
      if (isAdventureMode.value && levelId.value && response.totalScore >= response.maxPossibleScore * 0.6) {
        const success = completeAdventureLevel(levelId.value);
        if (await success) {
          completionMessage = `恭喜完成关卡 ${levelId.value}！得分: ${response.totalScore}/${response.maxPossibleScore}`;
          // 延迟跳转回闯关页面
          setTimeout(() => {
            router.push('/adventure');
          }, 3000);
        }
      }

      ElMessage.success(completionMessage);

      // 完成练习后更新用户完成文章数量
      fetchCompletedArticlesCount();

      // 更新当前文章的最高分显示
      if (selectedReading.value && selectedReading.value.articleId) {
        try {
          const result = await getUserArticleHighestScore(userId.value, selectedReading.value.articleId);
          if (selectedReading.value) {
            selectedReading.value.highestScore = result.highestScore;
          }
        } catch (error) {
          console.error('更新当前文章最高分失败:', error);
        }
      }

      // 更新所有活跃的学习计划
      if (response.totalScore == response.maxPossibleScore) {
        await updateAllActiveStudyPlans();
      }
      
    } catch (error) {
      console.error('提交答案失败:', error);
      ElMessage.error('提交答案失败，请稍后重试');
    } finally {
      practiceSubmitting.value = false;
    }
  } else {
    // 如果没有答案，也标记为完成，但不提交
    practiceCompleted.value = true;
    ElMessage.info('练习已结束');
  }
};

/**
 * 更新用户所有活跃的学习计划，将 learnedArticleCount 加一
 */
import axios from 'axios';
const updateAllActiveStudyPlans = async () => {
  try {
    // 假设 userId 是可用的，例如从 store 或 ref 中获取
    var currentUserId = "0";
    const userData = localStorage.getItem('user');
    if (userData) {
      const parsedData = JSON.parse(userData);
      currentUserId = parsedData.Id || '114514'; // 默认id
    }

    // 1. 获取用户所有学习计划详情
    const response = await axios.get(`/api/UserStudyPlan/DetailsByUser/${currentUserId}`);
    const plans = response.data;

    // 2. 筛选出未完成的学习计划 (completed == 0)
    const activePlans = plans.filter((plan: any) => plan.completed === 0);

    // 3. 遍历并更新每个活跃的学习计划
    for (const plan of activePlans) {
      const updatedPlanData = {
        ...plan,
        learnedArticleCount: (plan.learnedArticleCount || 0) + 1
      };

      await axios.put(`/api/UserStudyPlan/${currentUserId}/${plan.planId}`, updatedPlanData, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
    }

    if (activePlans.length > 0) {
        ElMessage.success('所有活跃学习计划的已读文章数已更新！');
    }

  } catch (error) {
    console.error('更新学习计划时出错:', error);
    ElMessage.error('更新学习计划失败。');
  }
};

/**
 * 重新开始练习
 * 清空用户答案，重新开始计时
 */
const restartPractice = () => {
  practiceCompleted.value = false; // 确保练习未完成状态
  userAnswers.value = {}; // 清空用户答案
  practiceTime.value = 0; // 重置练习时间
  startTimer(); // 重新开始计时
  ElMessage.info('已重新开始练习，请继续答题');
};

/**
 * 启动计时器
 * 每秒递增practiceTime，用于记录阅读练习时间
 */
const startTimer = () => {
  practiceTime.value = 0;
  timer = window.setInterval(() => {
    practiceTime.value++;
  }, 1000);
};

/**
 * 停止计时器
 * 清除计时器并释放资源
 */
const stopTimer = () => {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
};

/**
 * 格式化时间为分:秒格式
 * @param seconds 总秒数
 * @returns 格式化后的时间字符串 (MM:SS)
 */
const formatTime = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${String(minutes).padStart(2, '0')}:${String(remainingSeconds).padStart(2, '0')}`;
};

/**
 * 获取特定题目的结果
 * 根据题目ID从结果中获取对应的题目得分信息
 * 
 * @param questionId 题目ID
 * @returns 题目结果对象，如果未找到则返回undefined
 */
const getQuestionResult = (questionId?: number): QuestionResult | undefined => {
  if (!questionId || !practiceResult.value || !practiceResult.value.questionResults) return undefined;
  
  return practiceResult.value.questionResults.find(result => result.questionId === questionId);
};

// ===== 生命周期钩子 =====

/**
 * 组件挂载时加载文章列表
 * 页面初始化时自动加载第一页数据
 */
onMounted(() => {
  // 从localStorage获取用户ID
  const userData = localStorage.getItem('user');
  if (userData) {
    try {
      const parsedData = JSON.parse(userData);
      userId.value = parsedData.Id || 0;
    } catch (error) {
      console.error('解析用户数据失败:', error);
      userId.value = 0;
    }
  }
  
  searchReadings();
  fetchCompletedArticlesCount(); // 在组件挂载时获取用户完成文章数量
});

/**
 * 组件被 keep-alive 重新激活时调用
 * 确保每次重新激活组件时，练习状态都是新的
 */
const handleActivated = () => {
  // 如果有选中的文章，但还未开始练习，则重置所有状态
  if (selectedReading.value && !practiceStarted.value) {
    practiceCompleted.value = false;
    practiceResult.value = null;
    userAnswers.value = {};
    practiceTime.value = 0;
    showingQuestions.value = false;
    
    // 确保计时器已停止
    if (timer) {
      stopTimer();
    }
  }
};

// 注册组件激活时的钩子
onActivated(handleActivated);

/**
 * 组件卸载时清理所有资源
 */
onUnmounted(() => {
  // 停止计时器
  stopTimer();
  
  // 重置状态，避免在组件销毁后访问DOM
  readings.value = [];
  selectedReading.value = null;
  showingQuestions.value = false;
  contentPages.value = [];
  detailLoading.value = false;
  practiceResult.value = null;
});
</script>

<style scoped>
.read-learning {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #ff66b3;
  font-size: 28px;
  margin-bottom: 10px;
}

.page-header p {
  color: #606266;
  font-size: 16px;
}

.user-stats {
  margin-top: 15px;
  text-align: center;
}

.stats-icon {
  margin-right: 5px;
}

.completed-count {
  font-weight: bold;
  color: #ff66b3;
}

.filter-card {
  margin-bottom: 20px;
  border-radius: 8px;
}

.select-prefix {
  color: #606266;
  margin-right: 5px;
  font-size: 14px;
}

.search-input {
  width: 250px;
}

.reading-list {
  margin-bottom: 30px;
}

.loading-container {
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
  margin-bottom: 20px;
}

.reading-item {
  height: 100%;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  flex-direction: column;
  margin-bottom: 20px;
  overflow: hidden; /* 确保内容不溢出 */
  border-radius: 8px;
}

.reading-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

.article-grid {
  margin-top: 20px;
}

.article-column {
  margin-bottom: 20px;
}

.reading-cover {
  position: relative;
  height: 150px;
  overflow: hidden;
  border-radius: 4px;
  margin-bottom: 10px;
}

/* 确保图片显示正确 */
.reading-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  background-color: #f5f5f5;
}

.reading-difficulty {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(255, 102, 179, 0.85);
  color: #fff;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.reading-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0 10px;
}

.reading-title-wrapper {
  height: 24px;
  overflow: hidden;
  position: relative;
  margin-bottom: 8px;
}

.reading-title {
  display: inline-block;
  white-space: nowrap;
  font-size: 16px;
  color: #333;
  line-height: 24px;
  font-weight: 500;
}

.reading-title-wrapper:hover .reading-title {
  animation: marquee 8s linear infinite;
}

@keyframes marquee {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-100%);
  }
}

.reading-tags {
  margin: 8px 0;
  display: flex;
  gap: 6px;
}

.reading-preview {
  color: #666;
  font-size: 14px;
  line-height: 1.4;
  margin: 8px 0 10px;
  flex: 1;
  height: 60px;
  overflow: hidden;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
  line-clamp: 3;
}

.reading-meta {
  margin-top: auto;
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  color: #999;
  padding: 8px 0;
}

.reading-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.pagination-container {
  margin-top: 30px;
  text-align: center;
}

/* 详情页样式 */
.detail-card {
  border-radius: 8px;
  margin-bottom: 20px;
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #ebeef5;
}

.center-title {
  font-size: 20px;
  font-weight: bold;
  color: #333;
  text-align: center;
  flex: 1;
  margin: 0 20px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.article-meta {
  padding: 10px 0;
  margin-bottom: 15px;
  border-bottom: 1px dashed #ebeef5;
}

.article-info {
  display: flex;
  gap: 15px;
  font-size: 14px;
  color: #666;
}

.article-info span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.highest-score {
  color: #409EFF;
  font-weight: bold;
}

.article-tags {
  margin-top: 10px;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.tag-item {
  cursor: pointer;
}

.score-tag {
  display: flex;
  align-items: center;
  gap: 4px;
}

.e-book-container {
  width: 100%;
  height: 500px;
  overflow: hidden;
  position: relative;
  margin-bottom: 20px;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  background-color: #f9f9f9;
}

.e-book-content {
  display: flex;
  width: 100%;
  height: 100%;
  transition: transform 0.3s ease;
}

.e-book-page {
  min-width: 100%;
  height: 100%;
  padding: 30px;
  box-sizing: border-box;
}

.page-content {
  line-height: 1.8;
  font-size: 16px;
  color: #333;
  height: 100%;
  overflow-y: auto;
}

.detail-footer {
  display: flex;
  justify-content: center;
  margin-top: 20px;
  padding-top: 15px;
  border-top: 1px solid #ebeef5;
}

.practice-timer {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #ff66b3;
  font-weight: bold;
  font-size: 16px;
}

/* 详情页加载样式 */
.detail-loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
  border-radius: 8px;
}

.detail-loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.detail-loading-spinner .el-icon {
  font-size: 32px;
  color: #ff66b3;
}

.reading-detail {
  position: relative; /* 添加这个以使加载层相对于此定位 */
}

.questions-section {
  margin-top: 20px;
  border-top: 1px solid #ebeef5;
  padding-top: 20px;
}

.question-item {
  margin-bottom: 20px;
  padding: 20px;
  border: 1px solid #ebeef5;
  border-radius: 8px;
  background-color: #fff;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.03);
  transition: all 0.3s ease;
}

.question-item:hover {
  box-shadow: 0 4px 16px 0 rgba(0, 0, 0, 0.08);
}

.question-options {
  margin: 15px 0;
  padding: 15px;
  border-radius: 8px;
  background-color: #f9f9f9;
}

/* 选项样式 - 确保完全左对齐 */
.option-radio-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 100%;
}

.option-item {
  margin-bottom: 0;
  padding: 10px 12px;
  border-radius: 4px;
  transition: all 0.2s ease;
  background-color: #fff;
  border: 1px solid #e0e0e0;
  text-align: left;
  display: flex;
}

.option-item:hover {
  background-color: #f0f9ff;
  border-color: #a0cfff;
}

/* 覆盖Element UI默认样式，确保选项文本左对齐 */
.option-item :deep(.el-radio) {
  margin-right: 0;
  width: 100%;
  display: flex;
  align-items: flex-start;
  color: #606266;
}

/* 单选按钮的圆圈部分 */
.option-item :deep(.el-radio__input) {
  margin-top: 3px;
  flex-shrink: 0;
}

/* 单选按钮的文本部分 */
.option-item :deep(.el-radio__label) {
  padding-left: 10px;
  text-align: left;
  white-space: normal !important;
  line-height: 1.5;
  display: block;
  color: inherit;
}

/* 选中状态样式 */
.option-item :deep(.el-radio.is-checked) {
  color: #409eff;
}

.questions-footer {
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #ebeef5;
  display: flex;
  justify-content: center;
  gap: 20px;
}

.question-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.question-number {
  font-weight: bold;
  color: #333;
}

.question-score {
  color: #ff66b3;
  font-weight: bold;
}

.question-score-info {
  display: flex;
  align-items: center;
  gap: 5px;
}

.score-value {
  font-size: 14px;
  color: #ff66b3;
  font-weight: bold;
}

.result-tag {
  font-size: 12px;
}

.question-stem {
  font-size: 16px;
  margin-bottom: 15px;
  line-height: 1.5;
}

.fill-blank-input {
  width: 100%;
  max-width: 300px;
}

.raw-options {
  padding: 10px;
  border: 1px dashed #ccc;
  border-radius: 4px;
  color: #666;
  font-style: italic;
  background-color: #fff;
}

.practice-result-summary {
  margin: 20px 0 30px;
  background-color: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.answered-correct {
  border-left: 4px solid #67c23a;
}

.answered-wrong {
  border-left: 4px solid #f56c6c;
}

.correct-option {
  border: 1px solid #67c23a;
  background-color: rgba(103, 194, 58, 0.1);
}

.wrong-option {
  border: 1px solid #f56c6c;
  background-color: rgba(245, 108, 108, 0.1);
}

.correct-answer-hint {
  margin-top: 15px;
  padding: 10px;
  color: #67c23a;
  font-size: 14px;
  border-top: 1px dashed #e6e6e6;
}

/* 完全重写的单选题组样式 */
.custom-radio-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 100%;
  margin: 10px 0;
}

.custom-radio-item {
  padding: 10px 12px;
  border-radius: 4px;
  border: 1px solid #e0e0e0;
  background-color: #fff;
  transition: all 0.2s ease;
}

.custom-radio-item:hover {
  background-color: #f0f9ff;
  border-color: #a0cfff;
}

/* 完整重写单选按钮样式，强制左对齐 */
.custom-radio-item :deep(.el-radio) {
  margin: 0;
  padding: 0;
  display: flex;
  width: 100%;
  align-items: flex-start;
  justify-content: flex-start;
}

/* 单选按钮的圆圈部分置于顶部 */
.custom-radio-item :deep(.el-radio__input) {
  margin-top: 3px;
  margin-right: 5px;
  flex: 0 0 auto;
}

/* 单选按钮的文本部分强制左对齐和自动折行 */
.custom-radio-item :deep(.el-radio__label) {
  padding: 0 0 0 5px;
  text-align: left;
  white-space: normal;
  display: inline;
  width: auto;
  flex: 1;
}

/* 已选中项的样式 */
.custom-radio-item :deep(.el-radio.is-checked) {
  color: #409eff;
}

/* 正确和错误选项样式 */
.correct-option {
  border: 1px solid #67c23a !important;
  background-color: rgba(103, 194, 58, 0.1);
}

.wrong-option {
  border: 1px solid #f56c6c !important;
  background-color: rgba(245, 108, 108, 0.1);
}

/* 闯关模式样式 */
.adventure-mode-tip {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  text-align: center;
  margin: 15px 0;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}
</style>