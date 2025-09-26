<template>
  <div class="word-learning">
    <div class="learning-container">
      <!-- 顶部标题 -->
      <div class="page-header">
        <h1>单词学习</h1>
        <p>按考试大纲、难度或分类筛选单词，系统记录您的学习进度</p>
      </div>
      
      <div v-if="!isLearning">
        <!-- 学习统计卡片 -->
        <el-card class="stats-card" shadow="hover">
          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-label">总单词量</div>
              <div class="stat-value">{{ learningStats.totalWords }}</div>
            </div>
            <div class="stat-card">
              <div class="stat-label">已学单词</div>
              <div class="stat-value">{{ learningStats.learnedWords }}</div>
            </div>
            <div class="stat-card">
              <div class="stat-label">生词本</div>
              <div class="stat-value">{{ learningStats.bookmarkedWords }}</div>
            </div>
          </div>
          
          <el-progress 
            :percentage="Math.round((learningStats.learnedWords / learningStats.totalWords) * 100)" 
            :stroke-width="10"
            :color="progressColor">
          </el-progress>

          <div class="start-learning-btn">
            <el-button 
              type="primary" 
              @click="startLearning" 
              :disabled="wordsToLearn.length === 0"
              round>
              <el-icon class="el-icon--left"><Reading /></el-icon>
              开始学习
            </el-button>
          </div>
        </el-card>

        <!-- 筛选区域 -->
        <el-card class="filter-card" shadow="hover">
          <div class="filter-section" style="display: flex; justify-content: space-between; align-items: center;">
            <el-select v-model="filters.examType" placeholder="考试大纲" clearable>
              <el-option label="CET-4" value="CET-4"></el-option>
              <el-option label="CET-6" value="CET-6"></el-option>
              <el-option label="考研英语" value="考研英语"></el-option>
            </el-select>
            
            <el-button-group>
              <el-button 
                type="primary" 
                @click="searchWords('new')" 
                :loading="loading"
                :plain="activeTab !== 'new'">
                新词学习
              </el-button>
              <el-button 
                type="primary" 
                @click="searchWords('review')" 
                :loading="loading"
                :plain="activeTab !== 'review'">
                复习单词
              </el-button>
              <el-button 
                type="primary" 
                @click="searchWords('bookmark')" 
                :loading="loading"
                :plain="activeTab !== 'bookmark'">
                生词本
              </el-button>
            </el-button-group>
          </div>
        </el-card>

        <!-- 单词卡片列表 -->
        <WordList
          :words="filteredWords"
          :loading="loading"
          :active-tab="activeTab"
          @toggle-bookmark="toggleBookmark"
        />
      </div>

      <!-- 学习模式 -->
      <div v-else class="learning-mode">
        <!-- 添加学习模式切换 -->
        <div class="mode-switcher" v-if="!showResult">
          <el-button 
            class="mode-button" 
            :class="{ active: currentMode === 'meaning' }"
            @click="currentMode = 'meaning'">
            英选中
          </el-button>
          <el-button 
            class="mode-button" 
            :class="{ active: currentMode === 'word' }"
            @click="currentMode = 'word'">
            中选英
          </el-button>
          <el-button 
            class="mode-button" 
            :class="{ active: currentMode === 'spelling' }"
            @click="currentMode = 'spelling'">
            拼写
          </el-button>
        </div>
        
        <LearningMode
          :words-to-learn="wordsToLearn"
          :current-word-index="currentWordIndex"
          :current-mode="currentMode"
          :meaning-options="meaningOptions"
          :word-options="wordOptions"
          :user-spelling="userSpelling"
          :show-result="showResult"
          :is-answer-correct="isAnswerCorrect"
          @stop-learning="stopLearning"
          @prev-word="prevWord"
          @next-word="nextWord"
          @check-answer="checkAnswer"
          @update:userSpelling="val => userSpelling = val"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, reactive, computed, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { Reading } from '@element-plus/icons-vue';
import WordList from './WordList.vue';
import LearningMode from './LearningMode.vue';
import { Word, Option, ApiWordResponse } from './types';
import { useRouter, useRoute } from 'vue-router';
import { completeLevel as completeAdventureLevel } from '../utils/localProgress';

export default defineComponent({
  name: 'WordLearning',
  components: {
    Reading,
    WordList,
    LearningMode
  },
  setup() {
    const router = useRouter();
    const route = useRoute();
    
    // 获取闯关参数
    const levelId = ref(route.query.levelId ? parseInt(route.query.levelId as string) : null);
    const isAdventureMode = ref(!!levelId.value);
    
    // 状态管理
    const activeTab = ref<'new' | 'review' | 'bookmark'>('new');
    const isLearning = ref(false);
    const showResult = ref(false);
    const isAnswerCorrect = ref(false);
    const userSpelling = ref('');
    const currentWordIndex = ref(0);
    const loading = ref(false);
    const currentMode = ref<'meaning' | 'word' | 'spelling'>('meaning');
    
    // 筛选条件
    const filters = reactive({
      examType: '',
    });
    
    // 单词数据
    const words = ref<Word[]>([]);
    
    // 用户ID
    const userId = ref('114514'); // 可以从登录状态获取

    // 学习统计
    const learningStats = reactive({
      totalWords: 0,
      learnedWords: 0,
      bookmarkedWords: 0,
      todayLearned: 0,
      correctRate: 0,
      streakDays: 0
    });

    // API调用 - 更新单词学习状态
    const updateWordLearnStatus = async (wordId: number, learned: boolean) => {
      try {
        const response = await fetch(
          `/api/Word/user/learn/${userId.value}/${wordId}`, 
          {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({ learned })
          }
        );
        
        if (!response.ok) throw new Error('Network response was not ok');
        //return await response.json();
      } catch (error) {
        console.error('Error updating learn status:', error);
        throw error;
      }
    };

    // API调用 - 更新单词收藏状态
    const updateWordBookmarkStatus = async (wordId: number, bookmarked: boolean) => {
      try {
        const response = await fetch(
          `/api/Word/user/bookmark/${userId.value}/${wordId}`, 
          {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({ bookmarked })
          }
        );
        
        if (!response.ok) throw new Error('Network response was not ok');
        //return await response.json();
      } catch (error) {
        console.error('Error updating bookmark status:', error);
        throw error;
      }
    };

    // 获取用户单词ID列表
    const fetchUserWordIds = async (params: {
      hasLearned?: number;
      hasBookmarked?: number;
      minCorrectCount?: number;
      maxCorrectCount?: number;
      minLearnCount?: number;
      maxLearnCount?: number;
    }) => {
      try {
        loading.value = true;
        const queryParams = new URLSearchParams();
        
        // 设置查询参数
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== -1) {
            queryParams.set(key, value.toString());
          }
        });
        
        const response = await fetch(
          `/api/Word/user/${userId.value}?${queryParams.toString()}`
        );
        
        if (!response.ok) throw new Error('Network response was not ok');
        
        const data = await response.json();
        // 确保返回的是wordIds数组
        return Array.isArray(data.wordIds) ? data.wordIds : (data.wordIds ? [data.wordIds] : []);
      } catch (error) {
        console.error('Error fetching word IDs:', error);
        ElMessage.error('获取单词列表失败');
        return [];
      } finally {
        loading.value = false;
      }
    };

    // 获取学习统计数据
    const fetchLearningStats = async () => {
      try {
        const [totalRes, learnedRes, bookmarkedRes] = await Promise.all([
          fetchUserWordIds({}),
          fetchUserWordIds({ hasLearned: 1 }),
          fetchUserWordIds({ hasBookmarked: 1 })
        ]);
        
        learningStats.totalWords = totalRes.length;
        learningStats.learnedWords = learnedRes.length;
        learningStats.bookmarkedWords = bookmarkedRes.length;
        
        // 这里可以添加获取今日学习数据和连续学习天数的逻辑
        // 可以从本地存储或后端API获取
        learningStats.todayLearned = parseInt(localStorage.getItem('todayLearned') || '0');
        learningStats.streakDays = parseInt(localStorage.getItem('streakDays') || '0');
      } catch (error) {
        console.error('Error fetching learning stats:', error);
      }
    };

    // 从后端获取单词数据
    const fetchWord = async (id: number): Promise<Word> => {
      try {
        const response = await fetch(`/api/Word/by-id/${id}`);
        if (!response.ok) throw new Error('Network response was not ok');
        
        const data: ApiWordResponse = await response.json();
        
        // 处理翻译字符串，去除前后双引号
        const cleanTranslation = data.chineseTranslations[0]?.replace(/^"|"$/g, '') || 'n. 核心，中心';
        
        // 确定性考试类型分配
        const examTypes = ['CET-4', 'CET-6', '考研英语'];
        const examType = examTypes[id % examTypes.length];
        
        return {
          id,
          text: data.wordName,
          pronunciation: generateStablePronunciation(data.wordName, id),
          translation: cleanTranslation, // 使用处理后的翻译
          examType,
          example: generateStableExample(data.wordName, id),
          // learned: data.hasLearned || false,
          // bookmarked: data.hasBookmarked || false,
          // errorCount: data.errorCount || 0
          learned: false,
          bookmarked: false,
          errorCount: 0
        };
      } catch (error) {
        console.error('Error fetching word:', error);
        throw error;
      }
    };

    // const fetchWord = async (id: number): Promise<Word> => {
    //   try {
    //     const response = await fetch(`/api/Word/by-id/${id}`);
    //     if (!response.ok) throw new Error('Network response was not ok');
        
    //     const data: ApiWordResponse = await response.json();
        
    //     // 确保返回的数据结构正确
    //     const hasLearned = data.hasLearned || false;
    //     const hasBookmarked = data.hasBookmarked || false;
        
    //     return {
    //       id,
    //       text: data.wordName,
    //       pronunciation: generateStablePronunciation(data.wordName, id),
    //       translation: data.chineseTranslations[0]?.replace(/^"|"$/g, '') || 'n. 核心，中心',
    //       examType: data.examType || 'CET-4',
    //       example: generateStableExample(data.wordName, id),
    //       learned: hasLearned,
    //       bookmarked: hasBookmarked,
    //       errorCount: data.errorCount || 0
    //     };
    //   } catch (error) {
    //     console.error('Error fetching word:', error);
    //     throw error;
    //   }
    // };
    
    // 稳定的发音生成
    const generateStablePronunciation = (word: string, id: number): string => {
      // 音标替换规则（元音+辅音）
      const vowelMap: Record<string, string[]> = {
        a: ["æ", "ɑ", "ʌ", "ə", "eɪ"],
        e: ["ɛ", "ə", "i", "eɪ̯", "ɜ"],
        i: ["ɪ", "i", "aɪ"],
        o: ["ɒ", "ɔ", "ə", "oʊ", "aʊ"],
        u: ["ʊ", "u", "ju", "ʌ", "ɚ"]
      };
      
      const consonantMap: Record<string, string[]> = {
        t: ["t", "θ", "tʃ"],
        d: ["d", "dʒ"],
        s: ["s", "ʃ", "ʒ"],
        g: ["ɡ", "ʒ", "dʒ", "ŋ"],
        r: ["r", "ɾ"],
        y: ["j", "ɪ", "i"],
        l: ["l", "əl"],
        n: ["n", "ŋ", "ən"],
        c: ["k", "s", "tʃ"]
      };

      // 稳定随机选择器（基于ID+位置）
      const getStableChoice = (arr: string[], seed: number): string => {
        return arr[(seed + id) % arr.length];
      };

      // 转小写处理
      const lowerWord = word.toLowerCase();
      let pronunciation = "";
      
      // 确定音节数（1-3个）
      const syllableCount = (id % 3) + 1; 
      const syllableLength = Math.max(1, Math.floor(lowerWord.length / syllableCount));
      
      // 主重音位置（第一个音节）
      pronunciation += "ˈ";
      
      for (let i = 0; i < lowerWord.length; i++) {
        const char = lowerWord[i];
        let replacement = char;
        
        // 元音替换（概率60%）
        if (vowelMap[char] && i % 3 !== id % 3) {
          replacement = getStableChoice(vowelMap[char], i);
        } 
        // 辅音替换（概率40%）
        else if (consonantMap[char] && (i + 1) % 4 === id % 4) {
          replacement = getStableChoice(consonantMap[char], i);
        }
        
        pronunciation += replacement;
        
        // 在音节分界处添加分隔符
        const isSyllableBreak = i > 0 && 
                                i < lowerWord.length - 1 && 
                                (i % syllableLength === syllableLength - 1) && 
                                (syllableCount > 1);
        
        if (isSyllableBreak) {
          // 次重音（20%概率）
          const useSecondary = (id + i) % 5 === 0;
          pronunciation += useSecondary ? "ˌ" : ".";
        }
      }

      return pronunciation;
    };
    
    // 稳定的例句生成
    const generateStableExample = (word: string, id: number): string => {
      const examples = [
        `She paused to consider the meaning of ${word} in her notebook.`,
        `The professor emphasized understanding ${word} in our studies.`,
        `Children often struggle to pronounce ${word} correctly on first attempt.`,
        `His essay used ${word} to convey a particularly complex idea.`,
        `The dictionary definition of ${word} surprised most students in class.`,
        `Can you think of a situation where ${word} would be appropriate?`,
        `The poet's clever use of ${word} created unexpected imagery.`,
        `Journalists should avoid overusing ${word} in their reports.`,
        `My vocabulary notebook has ${word} highlighted as a key term.`,
        `This academic text features ${word} frequently throughout.`,
        `I encountered ${word} while reading yesterday's newspaper.`,
        `Language learners sometimes confuse ${word} with similar terms.`,
        `The crossword puzzle clue pointed directly to ${word}.`,
        `During the debate, she strategically incorporated ${word}.`,
        `The etymology of ${word} traces back to ancient Latin roots.`,
        `In technical contexts, ${word} carries a specialized meaning.`,
        `Can you detect subtle nuances in how ${word} functions here?`,
        `The toddler repeated ${word} with surprising clarity.`,
        `Translating ${word} requires attention to cultural context.`,
        `His eloquent speech lacked concrete examples of ${word}.`,
        `${word} suddenly slipped my mind during the presentation.`,
        `Editors often debate the proper usage of ${word}.`,
        `This demonstrates a practical application of ${word}.`,
        `Native speakers use ${word} more casually than learners expect.`,
        `The distinction between ${word} and its synonym matters greatly.`,
        `Literature enthusiasts appreciate when authors reinvent ${word}.`,
        `In casual conversation, people rarely employ ${word} deliberately.`,
        `Throughout the textbook, ${word} appears as a key concept.`,
        `Linguists study how ${word} evolved over several centuries.`,
        `Can you identify the grammatical role of ${word} in this sentence?`
      ];
      
      return examples[id % examples.length];
    };
    
    // 初始化加载单词数据
    const loadWords = async () => {
      loading.value = true;
      try {
        let wordIds: number[] = [];
        
        // 根据当前标签页获取不同的单词ID列表
        if (activeTab.value === 'new') {
          wordIds = await fetchUserWordIds({ hasLearned: 0 });
        } else if (activeTab.value === 'review') {
          wordIds = await fetchUserWordIds({ hasLearned: 1 });
        } else if (activeTab.value === 'bookmark') {
          wordIds = await fetchUserWordIds({ hasBookmarked: 1 });
        }
        
        // 清空当前单词列表
        words.value = [];
        
        // 如果没有任何单词ID，直接返回
        if (wordIds.length === 0) {
          return;
        }
        
        // 分批加载单词详情（每次加载20个）
        const batchSize = 10;
        
        for (let i = 0; i < wordIds.length && i < 50; i += batchSize) {
          const batchIds = wordIds.slice(i, i + batchSize);
          const batchPromises = batchIds.map(id => fetchWord(id));
          const batchWords = await Promise.all(batchPromises);
          
          // 更新单词的学习状态和收藏状态
          const updatedWords = batchWords.map(word => {
            // 从API获取实际的学习状态
            const learned = activeTab.value === 'review' || word.learned;
            const bookmarked = activeTab.value === 'bookmark' || word.bookmarked;
            return {
              ...word,
              learned,
              bookmarked
            };
          });
          
          // 使用concat而不是直接赋值，避免UI跳动
          words.value = words.value.concat(updatedWords);
          
          // 小延迟让UI更新
          await new Promise(resolve => setTimeout(resolve, 50));
        }
        
      } catch (error) {
        ElMessage.error('加载单词失败');
      } finally {
        loading.value = false;
      }
    };
    
    // 计算属性
    const filteredWords = computed(() => {
      let result = words.value;
      
      if (activeTab.value === 'new') {
        result = result.filter(word => !word.learned);
      } else if (activeTab.value === 'review') {
        result = result.filter(word => word.learned);
      } else if (activeTab.value === 'bookmark') {
        result = result.filter(word => word.bookmarked);
      }
      
      if (filters.examType) {
        result = result.filter(word => word.examType === filters.examType);
      }
      
      return result;
    });
    
    const wordsToLearn = computed(() => {
      let result = filteredWords.value;
      
      if (activeTab.value === 'review') {
        result = [...result].sort((a, b) => b.errorCount - a.errorCount);
      }
      
      // 每次只取10个单词
      const count = Math.min(10, result.length);
      return result.slice(0, count);
    });
    
    const currentWord = computed(() => {
      return wordsToLearn.value[currentWordIndex.value];
    });
    
    const meaningOptions = computed<Option[]>(() => {
      if (!currentWord.value || words.value.length < 4) return [];
      
      // 确保有足够的错误选项
      const wrongOptions = [...words.value]
        .filter(word => word.id !== currentWord.value.id)
        .sort(() => 0.5 - Math.random())
        .slice(0, 3)
        .map(word => ({
          text: word.translation,
          isCorrect: false
        }));
      
      // 如果错误选项不足，使用默认选项补充
      while (wrongOptions.length < 3) {
        wrongOptions.push({
          text: `释义${wrongOptions.length + 1}`,
          isCorrect: false
        });
      }
      
      return [
        ...wrongOptions,
        {
          text: currentWord.value.translation,
          isCorrect: true
        }
      ].sort(() => 0.5 - Math.random());
    });

    const wordOptions = computed<Option[]>(() => {
      if (!currentWord.value || words.value.length < 4) return [];
      
      const wrongOptions = [...words.value]
        .filter(word => word.id !== currentWord.value.id)
        .sort(() => 0.5 - Math.random())
        .slice(0, 3)
        .map(word => ({
          text: word.text,
          isCorrect: false
        }));
      
      // 如果错误选项不足，使用默认选项补充
      while (wrongOptions.length < 3) {
        wrongOptions.push({
          text: `word${wrongOptions.length + 1}`,
          isCorrect: false
        });
      }
      
      return [
        ...wrongOptions,
        {
          text: currentWord.value.text,
          isCorrect: true
        }
      ].sort(() => 0.5 - Math.random());
    });
    
    const learnedWordsCount = computed(() => {
      return words.value.filter(word => word.learned).length;
    });
    
    const learnedToday = ref(7);
    const learningProgress = computed(() => {
      return Math.round((currentWordIndex.value / wordsToLearn.value.length) * 100);
    });
    
    const progressColor = computed(() => {
      const percentage = Math.round((learnedToday.value / learnedWordsCount.value) * 100);
      if (percentage < 30) return '#F56C6C';
      if (percentage < 70) return '#E6A23C';
      return '#67C23A';
    });
    
    // 方法
    const searchWords = async (tab: 'new' | 'review' | 'bookmark') => {
      activeTab.value = tab;
      await loadWords();
    };
    
    const toggleBookmark = async (word: Word) => {
      try {
        // 调用API更新收藏状态
        await updateWordBookmarkStatus(word.id, !word.bookmarked);
        
        // 更新本地状态
        word.bookmarked = !word.bookmarked;
        const message = word.bookmarked ? '已添加到生词本' : '已从生词本移除';
        ElMessage.success(message);
        
        // 更新学习统计
        if (word.bookmarked) {
          learningStats.bookmarkedWords++;
        } else {
          learningStats.bookmarkedWords--;
        }
        
        // 如果是生词本标签页且取消了收藏，重新加载单词
        if (activeTab.value === 'bookmark' && !word.bookmarked) {
          await loadWords();
        }
      } catch (error) {
        console.error('Error toggling bookmark:', error);
        ElMessage.error('操作失败，请重试');
      }
    };
    
    const startLearning = () => {
      if (wordsToLearn.value.length === 0) {
        ElMessage.warning('没有可学习的单词，请调整筛选条件');
        return;
      }
      
      // 重置学习状态
      isLearning.value = true;
      currentWordIndex.value = 0;
      userSpelling.value = '';
      showResult.value = false;
      
      // 根据单词难度选择学习模式
      selectAppropriateMode();
    };
    
    // 根据单词难度选择合适的学习模式
    const selectAppropriateMode = () => {
      const word = currentWord.value;
      
      // 根据错误次数选择不同的学习模式概率
      const modes: ('meaning' | 'word' | 'spelling')[] = ['meaning', 'word', 'spelling'];
      let weights: number[];
      
      if (word.errorCount === 0) {
        // 新词或正确率高的词，增加拼写模式概率
        weights = [0.4, 0.4, 0.2];
      } else if (word.errorCount <= 2) {
        // 中等难度的词，均衡分配
        weights = [0.5, 0.3, 0.2];
      } else {
        // 高错误率的词，增加释义选择模式概率
        weights = [0.6, 0.3, 0.1];
      }
      
      // 根据权重随机选择模式
      const random = Math.random();
      let cumulativeWeight = 0;
      
      for (let i = 0; i < modes.length; i++) {
        cumulativeWeight += weights[i];
        if (random <= cumulativeWeight) {
          currentMode.value = modes[i];
          break;
        }
      }
    };
    
    const stopLearning = () => {
      isLearning.value = false;
      ElMessage.info('已退出学习模式');
    };
    
    const updateStudyPlans = async () => {
      try {
        // 1. 获取用户所有学习计划
        const response = await fetch(
          `/api/UserStudyPlan/DetailsByUser/${userId.value}`
        );
        
        if (!response.ok) throw new Error('获取学习计划失败');
        const plans = await response.json();

        // 2. 筛选当前活跃的学习计划
        const today = new Date();
        today.setHours(0, 0, 0, 0); // 设置为当天开始时间
        
        // const activePlans = plans.filter((plan: any) => {
        //   const startDate = new Date(plan.startDate);
        //   const endDate = new Date(startDate);
        //   endDate.setDate(startDate.getDate() + plan.duration);
          
        //   return today >= startDate && today <= endDate;
        // });

        const activePlans = plans.filter((plan: any) => {
          return plan.completed == 0;
        });

        // 3. 更新每个活跃计划
        for (const plan of activePlans) {
          const updatedPlan = {
            ...plan,
            learnedWordCount: plan.learnedWordCount + 1
          };
          
          // ElMessage.success(updatedPlan.planId);

          const updateResponse = await fetch(
            `/api/UserStudyPlan/${userId.value}/${plan.planId}`,
            {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                userId: updatedPlan.userId,
                planId: updatedPlan.planId,
                learnedWordCount: updatedPlan.learnedWordCount,
                learnedArticleCount: updatedPlan.learnedArticleCount,
                listeningTime: updatedPlan.listeningTime,
                learnedOralTime: updatedPlan.learnedOralTime
              })
            }
          );

          if (!updateResponse.ok) throw new Error('更新学习计划失败');
        }
      } catch (error) {
        console.error('更新学习计划出错:', error);
      }
    };

    const checkAnswer = async (correct: boolean) => {
      isAnswerCorrect.value = correct;
      showResult.value = true;
      
      const word = currentWord.value;
      try {
        if (correct) {
          // 调用API更新学习状态
          await updateWordLearnStatus(word.id, true);
          
          // 更新本地状态
          words.value = words.value.map(w => 
            w.id === word.id ? { ...w, learned: true, errorCount: 0 } : w
          );
          
          learnedToday.value++;
          learningStats.todayLearned++;
          learningStats.learnedWords++;
          
          // 更新本地存储
          localStorage.setItem('todayLearned', learningStats.todayLearned.toString());

          // 更新学习计划状态
          await updateStudyPlans();

        } else {
          // 更新本地错误计数
          words.value = words.value.map(w => 
            w.id === word.id ? { ...w, errorCount: w.errorCount + 1 } : w
          );
        }
      } catch (error) {
        console.error('Error updating word status:', error);
        ElMessage.error('保存学习状态失败');
      }
    };
    
    const nextWord = async () => {
      if (currentWordIndex.value < wordsToLearn.value.length - 1) {
        currentWordIndex.value++;
        selectAppropriateMode();
        userSpelling.value = '';
        showResult.value = false;
      } else {
        // 学习结束后重置状态
        isLearning.value = false;
        currentWordIndex.value = 0;
        userSpelling.value = '';
        showResult.value = false;
        
        let completionMessage = '恭喜完成本次学习!';
        
        console.log('=== 学习完成，检查闯关模式 ===')
        console.log('isAdventureMode:', isAdventureMode.value);
        console.log('levelId:', levelId.value);
        console.log('levelId 类型:', typeof levelId.value);
        
        // 如果是闯关模式，完成对应关卡
        if (isAdventureMode.value && levelId.value) {
          console.log(`准备完成关卡 ${levelId.value}...`)
          try {
            // 注意：completeAdventureLevel 现在是异步函数
            const success = await completeAdventureLevel(levelId.value);
            console.log(`关卡 ${levelId.value} 完成结果:`, success);
            
            if (success) {
              completionMessage = `恭喜完成关卡 ${levelId.value}！`;
              console.log('关卡完成成功，准备跳转...')
              // 延迟跳转回闯关页面（数据已在completeLevel中自动刷新）
              setTimeout(() => {
                console.log('跳转回闯关页面')
                router.push('/adventure');
              }, 2000);
            } else {
              console.warn(`关卡 ${levelId.value} 完成失败或已经完成过`)
              // 即使失败也跳转回去，可能是重复完成
              setTimeout(() => {
                router.push('/adventure');
              }, 2000);
            }
          } catch (error) {
            console.error('调用 completeAdventureLevel 时发生错误:', error)
            ElMessage.error('完成关卡时发生错误，但学习已完成')
          }
        }
        
        ElMessage.success(completionMessage);
        
        // 更新连续学习天数
        const lastLearnDate = localStorage.getItem('lastLearnDate');
        const today = new Date().toDateString();
        
        if (lastLearnDate !== today) {
          if (lastLearnDate && new Date(lastLearnDate).getTime() > new Date().getTime() - 86400000 * 2) {
            learningStats.streakDays++;
          } else {
            learningStats.streakDays = 1;
          }
          localStorage.setItem('lastLearnDate', today);
          localStorage.setItem('streakDays', learningStats.streakDays.toString());
        }
      }
    };
    
    const prevWord = () => {
      if (currentWordIndex.value > 0) {
        currentWordIndex.value--;
        selectAppropriateMode();
        userSpelling.value = '';
        showResult.value = false;
      }
    };
    
    // 初始化
    onMounted(() => {
      const userData = localStorage.getItem('user');
      if (userData) {
        const parsedData = JSON.parse(userData);
        userId.value = parsedData.Id || '114514'; // 默认id
      } else {
        // 如果未登录，跳转到登录页
        //router.push('/login');
      }
      loadWords();
      fetchLearningStats();
    });
    
    return {
      // 状态管理
      activeTab,
      isLearning,
      showResult,
      isAnswerCorrect,
      userSpelling,
      currentWordIndex,
      loading,
      currentMode,
      filters,
      
      // 数据
      words,
      filteredWords,
      wordsToLearn,
      currentWord,
      meaningOptions,
      wordOptions,
      
      // 学习统计
      learningStats,
      learnedToday,
      learnedWordsCount,
      learningProgress,
      progressColor,
      
      // 方法
      searchWords,
      toggleBookmark,
      startLearning,
      stopLearning,
      checkAnswer,
      nextWord,
      prevWord,
      updateStudyPlans
    };
  }
});
</script>

<style scoped>
.word-learning {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
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

.filter-card {
  margin-bottom: 20px;
}

.filter-section {
  display: flex;
  gap: 15px;
  align-items: center;
  flex-wrap: wrap;
  width: 100%;
}

.filter-section .el-button-group {
  flex-shrink: 0;
}

.filter-section .el-select {
  width: 180px;
}

/* 学习统计卡片 */
.stats-card {
  margin-bottom: 20px;
  border-radius: 12px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 15px;
  margin-bottom: 15px;
}

.stat-card {
  padding: 15px;
  border-radius: 8px;
  background: #f5f7fa;
  text-align: center;
  transition: all 0.3s;
}

.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: var(--el-color-primary);
  margin: 5px 0;
}

.stat-label {
  font-size: 14px;
  color: #909399;
}

/* 开始学习按钮 */
.action-card {
  margin-bottom: 20px;
  text-align: center;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 20px;
  padding: 10px 0;
}

.action-buttons .el-button {
  padding: 12px 24px;
  font-size: 16px;
}

/* 学习模式切换按钮 */
.mode-switcher {
  margin: 20px 0;
  text-align: center;
}

.mode-button {
  margin: 0 10px;
  padding: 12px 24px;
  border-radius: 20px;
  font-weight: bold;
  transition: all 0.3s;
}

.mode-button.active {
  background-color: var(--el-color-primary);
  color: white;
}

.learning-mode {
  margin-bottom: 30px;
}

.start-learning-btn {
  margin-top: 20px;
  text-align: center;
}

@media (max-width: 768px) {
  .filter-section {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .filter-section .el-select {
    width: 100%;
  }
  
  .stats-grid {
    grid-template-columns: 1fr 1fr;
  }
  
  .mode-switcher {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }
  
  .mode-button {
    margin: 0;
    width: 100%;
  }
}
</style>