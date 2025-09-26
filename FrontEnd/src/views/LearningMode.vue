<template>
  <el-card class="learning-card" shadow="hover">
    <!-- 顶部操作栏 -->
    <div class="detail-header">
      <div class="left-actions">
        <el-button @click="stopLearning" plain>
          <el-icon class="el-icon--left"><ArrowLeft /></el-icon>返回列表
        </el-button>
      </div>
      
      <div class="center-title">
        ({{ currentWordIndex + 1 }}/{{ wordsToLearn.length }})
      </div>
      
      <div class="right-actions">
        <el-button-group>
          <el-button plain title="上一个" :disabled="currentWordIndex <= 0" @click="prevWord">
            <el-icon><ArrowLeft /></el-icon>
          </el-button>
          <el-button plain title="下一个" :disabled="currentWordIndex >= wordsToLearn.length - 1" @click="nextWord">
            <el-icon><ArrowRight /></el-icon>
          </el-button>
        </el-button-group>
      </div>
    </div>
    
    <!-- 学习内容 -->
    <div class="learning-content">
      <div v-if="!showResult" class="question-area">
        <!-- 模式1: 英选中 -->
        <div v-if="currentMode === 'meaning'" class="mode-meaning">
          <h2 class="word-text">{{ currentWord.text }}</h2>
          <p class="pronunciation">/{{ currentWord.pronunciation }}/</p>
          <p class="question">请选择正确的中文释义:</p>
          
          <div class="options">
            <el-button 
              v-for="(option, index) in meaningOptions" 
              :key="index"
              class="option-button"
              @click="checkAnswer(option.isCorrect)"
              size="large">
              {{ option.text }}
            </el-button>
          </div>
        </div>
        
        <!-- 模式2: 中选英 -->
        <div v-else-if="currentMode === 'word'" class="mode-word">
          <h2 class="translation">{{ currentWord.translation }}</h2>
          <p class="question">请选择对应的英文单词:</p>
          
          <div class="options">
            <el-button 
              v-for="(option, index) in wordOptions" 
              :key="index"
              class="option-button"
              @click="checkAnswer(option.isCorrect)"
              size="large">
              {{ option.text }}
            </el-button>
          </div>
        </div>
        
        <!-- 模式3: 拼写 -->
        <div v-else class="mode-spelling">
          <h2 class="translation">{{ currentWord.translation }}</h2>
          <p class="question">请拼写对应的英文单词:</p>
          
          <el-input 
            v-model="localSpelling" 
            placeholder="输入单词拼写"
            @keyup.enter="handleSpellingCheck"
            size="large">
          </el-input>
          
          <el-button 
            type="primary" 
            @click="handleSpellingCheck"
            :disabled="!localSpelling"
            size="large"
            class="check-button">
            检查
          </el-button>
        </div>
      </div>
      <!-- 结果反馈 -->
      <div v-else class="result-feedback" :class="isAnswerCorrect ? 'correct' : 'wrong'">
        <div class="feedback-content">
          <el-icon class="feedback-icon">
            <CircleCheck v-if="isAnswerCorrect" />
            <CircleClose v-else />
          </el-icon>
          <h3 v-if="isAnswerCorrect" class="feedback-title">回答正确!</h3>
          <h3 v-else class="feedback-title">回答错误</h3>
          
          <div class="word-info">
            <p><span class="label">单词:</span> {{ currentWord.text }}</p>
            <p><span class="label">发音:</span> /{{ currentWord.pronunciation }}/</p>
            <p><span class="label">释义:</span> {{ currentWord.translation }}</p>
            <p><span class="label">例句:</span> {{ currentWord.example }}</p>
          </div>
        </div>
        
        <el-button 
          type="primary" 
          @click="nextWord"
          class="next-button"
          size="large">
          继续学习
        </el-button>
      </div>
    </div>
    
    <!-- 底部进度条 -->
    <div class="progress-bar">
      <el-progress 
        :percentage="learningProgress" 
        :stroke-width="8" 
        :color="progressColor"
        :show-text="false">
      </el-progress>
    </div>
  </el-card>
</template>

<script lang="ts">
import { defineComponent, PropType, computed, ref, watch } from 'vue';
import { 
  ArrowLeft, 
  ArrowRight,
  CircleCheck, 
  CircleClose
} from '@element-plus/icons-vue';
import { Word, Option } from './types';

export default defineComponent({
  name: 'LearningMode',
  components: {
    ArrowLeft,
    ArrowRight,
    CircleCheck,
    CircleClose
  },
  props: {
    wordsToLearn: {
      type: Array as PropType<Word[]>,
      required: true
    },
    currentWordIndex: {
      type: Number,
      required: true
    },
    currentMode: {
      type: String as PropType<'meaning' | 'word' | 'spelling'>,
      required: true
    },
    meaningOptions: {
      type: Array as PropType<Option[]>,
      required: true
    },
    wordOptions: {
      type: Array as PropType<Option[]>,
      required: true
    },
    userSpelling: {
      type: String,
      default: ''
    },
    showResult: {
      type: Boolean,
      default: false
    },
    isAnswerCorrect: {
      type: Boolean,
      default: false
    }
  },
  emits: [
    'stop-learning',
    'prev-word',
    'next-word',
    'check-answer',
    'update:userSpelling'
  ],
  setup(props, { emit }) {
    const localSpelling = ref(props.userSpelling);

    // 监听props.userSpelling变化，同步到localSpelling
    watch(() => props.userSpelling, (newVal) => {
      localSpelling.value = newVal;
    });

    // 监听currentWordIndex变化，重置localSpelling
    watch(() => props.currentWordIndex, () => {
      localSpelling.value = '';
    });

    const currentWord = computed(() => {
      return props.wordsToLearn[props.currentWordIndex];
    });

    const learningProgress = computed(() => {
      return Math.round((props.currentWordIndex / props.wordsToLearn.length) * 100);
    });

    const progressColor = computed(() => {
      const percentage = learningProgress.value;
      if (percentage < 30) return '#F56C6C';
      if (percentage < 70) return '#E6A23C';
      return '#67C23A';
    });

    const stopLearning = () => {
      emit('stop-learning');
    };

    const prevWord = () => {
      emit('prev-word');
    };

    const nextWord = () => {
      emit('next-word');
    };

    const checkAnswer = (correct: boolean) => {
      emit('check-answer', correct);
    };

    const handleSpellingCheck = () => {
      emit('update:userSpelling', localSpelling.value);
      const isCorrect = localSpelling.value.trim().toLowerCase() === currentWord.value.text.toLowerCase();
      checkAnswer(isCorrect);
    };

    return {
      localSpelling,
      currentWord,
      learningProgress,
      progressColor,
      stopLearning,
      prevWord,
      nextWord,
      checkAnswer,
      handleSpellingCheck
    };
  }
});
</script>

<style scoped>
.learning-card {
  border-radius: 12px;
  width: 100%;
  min-width: 0;
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
  font-size: 18px;
  font-weight: bold;
  color: #333;
  text-align: center;
  flex: 1;
  margin: 0 20px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.learning-content {
  padding: 20px;
  min-height: 400px;
  display: flex;
  flex-direction: column;
}

.question-area {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.word-text {
  font-size: 32px;
  color: #303133;
  margin-bottom: 10px;
}

.translation {
  font-size: 24px;
  color: #303133;
  margin-bottom: 10px;
}

.pronunciation {
  color: #909399;
  font-style: italic;
  margin-bottom: 30px;
  font-size: 18px;
}

.question {
  color: #606266;
  margin-bottom: 30px;
  font-size: 16px;
}

.options {
  display: grid;
  grid-template-columns: 1fr;
  gap: 15px;
  margin-top: 20px;
  width: 100%;
  max-width: 500px;
  margin-left: auto;
  margin-right: auto;
}

.option-button {
  width: 100%;
  padding: 15px;
  font-size: 16px;
  text-align: center;
  box-sizing: border-box;
}

.option-button.el-button {
  justify-content: center;
  margin: 0;
}

.mode-spelling {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  flex: 1;
  justify-content: center;
}

.mode-spelling .el-input {
  width: 100%;
  max-width: 400px;
  margin-bottom: 20px;
}

.check-button {
  width: 120px;
}

.result-feedback {
  margin-top: 20px;
  padding: 30px;
  border-radius: 8px;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.result-feedback.correct {
  background-color: #f0f9eb;
  color: #67c23a;
}

.result-feedback.wrong {
  background-color: #fef0f0;
  color: #f56c6c;
}

.feedback-content {
  margin-bottom: 20px;
}

.feedback-icon {
  font-size: 48px;
  margin-bottom: 20px;
}

.feedback-title {
  margin: 0 0 20px 0;
  font-size: 24px;
}

.word-info {
  text-align: left;
  margin-top: 20px;
}

.word-info p {
  margin: 10px 0;
  line-height: 1.6;
}

.word-info .label {
  font-weight: bold;
  margin-right: 10px;
}

.next-button {
  margin-top: 30px;
  width: 200px;
  align-self: center;
}

.progress-bar {
  margin-top: 20px;
}
</style>