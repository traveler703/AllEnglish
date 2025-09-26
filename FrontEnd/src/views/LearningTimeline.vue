<template>
  <div class="timeline-container">
    <div class="timeline">
      <div
        v-for="(item, index) in timelineItems"
        :key="index"
        class="timeline-item"
        @mouseenter="hoverIndex = index"
        @mouseleave="hoverIndex = null"
      >
        <!-- 时间点 -->
        <div class="timeline-point" :class="{ active: hoverIndex === index }">
          <span class="time">{{ item.time }}</span>
        </div>
        
        <!-- 支线内容 (悬停时显示) -->
        <div
          class="timeline-branch"
          :class="{ active: hoverIndex === index }"
        >
          <div class="branch-content">
            <!-- 显示模式 -->
            <div v-if="!item.editing" class="content-display">
              <p>{{ item.content }}</p>
              <button v-if="editable" @click.stop="toggleEdit(index)">编辑</button>
            </div>
            
            <!-- 编辑模式 -->
            <div v-else class="content-edit">
              <textarea v-model="item.content" rows="4"></textarea>
              <div class="edit-buttons">
                <button @click.stop="saveEdit(index)">保存</button>
                <button @click.stop="cancelEdit(index)">取消</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue';

export default {
  props: {
    items: {
      type: Array,
      required: true,
      default: () => []
    },
    editable: {
      type: Boolean,
      default: false
    }
  },
  setup(props) {
    const hoverIndex = ref(null);
    const timelineItems = ref([...props.items.map(item => ({
      ...item,
      editing: false,
      originalContent: item.content
    }))]);

    const toggleEdit = (index) => {
      if (!props.editable) return;
      
      timelineItems.value[index].editing = !timelineItems.value[index].editing;
      // 保存原始内容以便取消编辑时恢复
      if (timelineItems.value[index].editing) {
        timelineItems.value[index].originalContent = timelineItems.value[index].content;
      }
    };

    const saveEdit = (index) => {
      timelineItems.value[index].editing = false;
      // 这里可以添加保存到服务器的逻辑
    };

    const cancelEdit = (index) => {
      timelineItems.value[index].content = timelineItems.value[index].originalContent;
      timelineItems.value[index].editing = false;
    };

    return {
      hoverIndex,
      timelineItems,
      toggleEdit,
      saveEdit,
      cancelEdit
    };
  }
};
</script>

<style scoped>
.timeline-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.timeline {
  position: relative;
  display: flex;
  justify-content: space-between;
  padding: 20px 0;
}

.timeline::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 2px;
  background: #ddd;
  z-index: 1;
}

.timeline-item {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  z-index: 2;
}

.timeline-point {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: #ccc;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: all 0.3s ease;
  cursor: pointer;
}

.timeline-point.active {
  background: #42b983;
  transform: scale(1.2);
}

.time {
  position: absolute;
  top: -25px;
  font-size: 14px;
  color: #666;
}

.timeline-branch {
  position: absolute;
  top: 40px;
  width: 200px;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 10px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
  opacity: 0;
  visibility: hidden;
  transform: translateY(10px);
  transition: all 0.3s ease;
  z-index: 10;
}

.timeline-branch.active {
  opacity: 1;
  visibility: visible;
  transform: translateY(0);
}

.branch-content {
  position: relative;
}

.content-display p {
  margin: 0 0 10px 0;
}

.content-edit textarea {
  width: 100%;
  margin-bottom: 10px;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

button {
  padding: 5px 10px;
  background: #42b983;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 5px;
}

button:hover {
  background: #3aa876;
}

.edit-buttons {
  display: flex;
  justify-content: flex-end;
}
</style>