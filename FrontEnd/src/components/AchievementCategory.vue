<template>
    <div class="category-section">
        <div class="category-header">
            <div class="category-title">
                <span class="category-emoji">🏆</span>
                <h2>{{ category.title }}</h2>
            </div>
            <div class="category-progress">
                {{ unlockedCount }}/{{ category.achievements.length }} 已解锁
            </div>
        </div>

        <div class="achievements-grid">
            <AchievementCard 
                v-for="achievement in category.achievements" 
                :key="achievement.id"
                :achievement="achievement"
                :current-value="getCurrentValue(achievement.category)"
                @click="$emit('achievement-click', achievement)"
            />
        </div>
    </div>
</template>

<script setup>
import { computed } from 'vue'
import AchievementCard from './AchievementCard.vue'

const props = defineProps({
    category: {
        type: Object,
        required: true
    },
    userStats: {
        type: Object,
        required: true
    }
})

const emit = defineEmits(['achievement-click'])

const getCurrentValue = (categoryKey) => {
    return props.userStats[categoryKey] || 0
}

const isUnlocked = (achievement) => {
    return getCurrentValue(achievement.category) >= achievement.target
}

const unlockedCount = computed(() => {
    return props.category.achievements.filter(achievement => isUnlocked(achievement)).length
})
</script>

<style scoped>
.category-section {
    background: white;
    border-radius: 16px;
    padding: 30px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.category-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
    padding-bottom: 15px;
    border-bottom: 2px solid #f5f5f5;
}

.category-title {
    display: flex;
    align-items: center;
    gap: 12px;
}

.category-emoji {
    font-size: 24px;
    margin-right: 8px;
}

.category-title h2 {
    margin: 0;
    color: #303133;
    font-size: 20px;
    font-weight: 600;
}

.category-progress {
    background: linear-gradient(135deg, #ff66b3, #ff99cc);
    color: white;
    padding: 6px 15px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 500;
}

.achievements-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
    gap: 20px;
}

/* 响应式设计 */
@media (max-width: 768px) {
    .achievements-grid {
        grid-template-columns: 1fr;
    }
    
    .category-header {
        flex-direction: column;
        gap: 15px;
        align-items: flex-start;
    }
}
</style>
