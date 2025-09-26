<template>
  <div class="word-list-container">
    <el-empty v-if="words.length === 0 && !loading" :description="emptyDescription" />
    
    <div v-if="loading" class="loading-container">
      <el-skeleton :rows="3" animated />
      <el-skeleton :rows="3" animated />
    </div>
    
    <el-row :gutter="20" class="word-list-grid">
      <el-col 
        :xs="24" 
        :sm="12" 
        :md="8" 
        :lg="6" 
        v-for="word in paginatedWords" 
        :key="word.id"
        class="word-card-col">
        <WordCard 
          :word="word" 
          @toggle-bookmark="$emit('toggle-bookmark', word)" />
      </el-col>
    </el-row>
    
    <!-- 分页 -->
    <div class="pagination-container" v-if="words.length > 0">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[8, 16, 24, 32]"
        layout="total, sizes, prev, pager, next, jumper"
        :total="totalWords"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType, computed, ref } from 'vue';
import WordCard from './WordCard.vue';
import { Word } from './types';

export default defineComponent({
  name: 'WordList',
  components: {
    WordCard
  },
  props: {
    words: {
      type: Array as PropType<Word[]>,
      required: true,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    },
    activeTab: {
      type: String as PropType<'new' | 'review' | 'bookmark'>,
      default: 'new'
    },
    initialPageSize: {
      type: Number,
      default: 8
    }
  },
  emits: ['toggle-bookmark', 'page-change', 'size-change'],
  setup(props, { emit }) {
    const currentPage = ref(1);
    const pageSize = ref(props.initialPageSize);
    const totalWords = computed(() => props.words.length);

    const paginatedWords = computed(() => {
      const start = (currentPage.value - 1) * pageSize.value;
      const end = start + pageSize.value;
      return props.words.slice(start, end);
    });

    const emptyDescription = computed(() => {
      return props.activeTab === 'bookmark' ? '生词本为空' : '暂无单词数据';
    });

    const handleSizeChange = (size: number) => {
      pageSize.value = size;
      emit('size-change', size);
    };

    const handleCurrentChange = (page: number) => {
      currentPage.value = page;
      emit('page-change', page);
    };

    return {
      currentPage,
      pageSize,
      totalWords,
      paginatedWords,
      emptyDescription,
      handleSizeChange,
      handleCurrentChange
    };
  }
});
</script>

<style scoped>
.word-list-container {
  margin-top: 20px;
}

.word-list-grid {
  margin-bottom: 20px;
}

.word-card-col {
  margin-bottom: 20px;
}

.loading-container {
  padding: 20px;
}

.pagination-container {
  margin-top: 30px;
  text-align: center;
}

@media (max-width: 768px) {
  .word-card-col {
    margin-bottom: 15px;
  }
}
</style>