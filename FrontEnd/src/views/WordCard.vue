<template>
  <el-card class="word-item" shadow="hover">
    <!-- 单词卡片标题部分 -->
    <div class="word-header">
      <h3 class="word-title">{{ word.text }}</h3>
      <el-tag size="small" :type="getExamType(word.examType)">{{ word.examType }}</el-tag>
    </div>
    <!-- 单词卡片内容部分 -->
    <div class="word-content">
      <p class="pronunciation">/{{ word.pronunciation }}/</p>
      <p class="translation">{{ word.translation }}</p>
      <p class="example">{{ word.example }}</p>
    </div>
    <!-- 单词卡片脚注部分 -->
    <div class="word-footer">
      <el-button 
        size="small" 
        :type="word.bookmarked ? 'warning' : 'text'" 
        @click="toggleBookmark(word)"
        class="bookmark-btn">
        <el-icon class="bookmark-icon">
          <StarFilled v-if="word.bookmarked" />
          <Star v-else />
        </el-icon>
      </el-button>
      
      <div class="word-meta">
        <el-tag v-if="word.learned" type="success" size="small">已学</el-tag>
        <el-tag v-else type="info" size="small">未学</el-tag>
      </div>
    </div>
  </el-card>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import { Star, StarFilled } from '@element-plus/icons-vue';
import { Word } from './types';

export default defineComponent({
  name: 'WordCard',
  components: {
    Star,
    StarFilled
  },
  props: {
    word: {
      type: Object as PropType<Word>,
      required: true
    }
  },
  methods: {
    getExamType(examType: string) {
      switch (examType) {
        case 'CET-4': return 'success';
        case 'CET-6': return '';
        case '考研英语': return 'danger';
        default: return 'info';
      }
    },
    toggleBookmark(word: Word) {
      this.$emit('toggle-bookmark', word);
    }
  }
});
</script>

<style scoped>
.word-item {
  display: flex;
  flex-direction: column;
  height: 100%; /* 确保卡片填满容器高度 */
  min-height: 280px; /* 设置最小高度 */
  transition: all 0.3s;
  position: relative; /* 为内部布局提供定位上下文 */
}

.word-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 15px rgba(0, 0, 0, 0.1);
}

.word-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  flex-shrink: 0; /* 防止标题区域被压缩 */
}

.word-title {
  margin: 0;
  font-size: 18px;
  color: #333;
  font-weight: bold;
}

.word-content {
  flex: 1; /* 关键属性 - 填充剩余空间 */
  display: flex;
  flex-direction: column;
  padding: 8px 0;
  /* 确保内容区域至少有一定高度 */
  min-height: 150px;
}

/* 内容区域内的元素样式 */
.pronunciation {
  color: #909399;
  font-style: italic;
  margin: 4px 0;
  font-size: 16px;
  line-height: 1.4;
  flex-shrink: 0; /* 防止被压缩 */
}

.translation {
  color: #303133;
  font-weight: 500;
  margin: 8px 0;
  font-size: 18px;
  line-height: 1.5;
  flex-shrink: 0; /* 防止被压缩 */
}

.example {
  color: #606266;
  font-size: 14px;
  margin: 8px 0 0; /* 顶部有间距，底部无间距 */
  line-height: 1.5;
  flex: 1; /* 关键属性 - 填充剩余空间 */
  min-height: 42px; /* 最小高度保证 */
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.word-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto; /* 关键属性 - 推到底部 */
  padding-top: 12px;
  border-top: 1px solid #f0f0f0;
  flex-shrink: 0; /* 防止页脚被压缩 */
}

.bookmark-btn {
  padding: 4px;
  border: none;
  background: transparent !important;
  color: var(--el-color-warning) !important;
}

.bookmark-icon {
  font-size: 18px;
  width: 1em;
  height: 1em;
}

.bookmark-btn:hover {
  transform: scale(1.1);
}

.word-meta {
  display: flex;
  gap: 5px;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .word-item {
    min-height: 240px;
  }
  
  .word-content {
    min-height: 120px;
  }
  
  .example {
    min-height: 36px;
    -webkit-line-clamp: 2;
  }
}
</style>