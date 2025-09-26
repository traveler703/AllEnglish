<template>
    <div 
        class="achievement-card" 
        :class="{ 'unlocked': isUnlocked, 'locked': !isUnlocked }"
        @click="$emit('click', achievement)"
    >
        <div class="achievement-icon">
            <span class="achievement-emoji">🎯</span>
            <div v-if="isUnlocked" class="unlock-badge">
                <el-icon><Check /></el-icon>
            </div>
        </div>
        <div class="achievement-info">
            <h3>{{ achievement.title }}</h3>
            <p>{{ achievement.description }}</p>
            <div class="achievement-progress">
                <el-progress 
                    :percentage="progress" 
                    :status="isUnlocked ? 'success' : ''"
                    :stroke-width="8"
                />
                <span class="progress-text">
                    {{ currentValue }}/{{ achievement.target }}
                </span>
            </div>
        </div>
        <div v-if="isUnlocked" class="unlock-date">
            {{ formatUnlockDate(achievement.unlockedAt) }}
        </div>
    </div>
</template>

<script setup>
import { computed } from 'vue'
import { Check } from '@element-plus/icons-vue'

const props = defineProps({
    achievement: {
        type: Object,
        required: true
    },
    currentValue: {
        type: Number,
        required: true
    }
})

const emit = defineEmits(['click'])

const isUnlocked = computed(() => {
    return props.currentValue >= props.achievement.target
})

const progress = computed(() => {
    const progressValue = Math.min((props.currentValue / props.achievement.target) * 100, 100)
    return Math.round(progressValue)
})

const formatUnlockDate = (dateString) => {
    if (!dateString) return ''
    const date = new Date(dateString)
    return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    })
}
</script>

<style scoped>
.achievement-card {
    background: #fafafa;
    border: 2px solid #e8e8e8;
    border-radius: 12px;
    padding: 20px;
    transition: all 0.3s;
    cursor: pointer;
    position: relative;
    overflow: hidden;
}

.achievement-card.unlocked {
    background: linear-gradient(135deg, #f0f9ff, #e0f2fe);
    border-color: #67c23a;
    box-shadow: 0 4px 12px rgba(103, 194, 58, 0.2);
}

.achievement-card.unlocked::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: linear-gradient(90deg, #67c23a, #85ce61);
}

.achievement-card.locked {
    opacity: 0.7;
}

.achievement-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.achievement-icon {
    position: relative;
    width: 60px;
    height: 60px;
    margin: 0 auto 15px auto;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    background: linear-gradient(135deg, #ff66b3, #ff99cc);
    color: white;
    font-size: 24px;
}

.achievement-emoji {
    font-size: 28px;
}

.achievement-card.unlocked .achievement-icon {
    background: linear-gradient(135deg, #67c23a, #85ce61);
}

.unlock-badge {
    position: absolute;
    bottom: -5px;
    right: -5px;
    width: 24px;
    height: 24px;
    background: #67c23a;
    border: 2px solid white;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    color: white;
}

.achievement-info {
    text-align: center;
}

.achievement-info h3 {
    margin: 0 0 8px 0;
    font-size: 18px;
    font-weight: 600;
    color: #303133;
}

.achievement-info p {
    margin: 0 0 15px 0;
    font-size: 14px;
    color: #606266;
    line-height: 1.4;
}

.achievement-progress {
    margin-bottom: 10px;
}

.progress-text {
    font-size: 12px;
    color: #909399;
    margin-top: 5px;
    display: block;
}

.unlock-date {
    text-align: center;
    font-size: 12px;
    color: #67c23a;
    font-weight: 500;
    margin-top: 10px;
    padding-top: 10px;
    border-top: 1px solid #e8e8e8;
}
</style>
