<template>
  <div class="study-plans-view">

    <!-- 页面头部：标题和说明 -->
    <header class="page-header">
      <h1>学习计划</h1>
      <p>规划你的英语学习旅程，每天进步一点点</p>
    </header>

    <!-- 日历组件区域 -->
    <div class="calendar-section">
      <el-card>
        <template #header>
          <span>学习计划日历</span>
        </template>
        
        <!-- 完整的日历组件 -->
        <StudyPlanCalendar
          :initial-plans="studyPlans"
          :colors="predefinedColors"
          @plan-added="handlePlanAdded"
          @plan-updated="handlePlanUpdated"
          @plan-deleted="handlePlanDeleted"
          @plan-clicked="handlePlanClicked"
        />
      </el-card>
    </div>

    <!-- 计划列表
    <div class="plans-list-section">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>学习计划列表</span>
            <el-button type="text" @click="handleExportPlans">
              导出计划
            </el-button>
          </div>
        </template>
        
        <el-table :data="studyPlans" style="width: 100%">
          <el-table-column prop="title" label="计划标题" width="200" />
          <el-table-column prop="startDate" label="开始日期" width="120" />
          <el-table-column prop="endDate" label="结束日期" width="120" />
          <el-table-column prop="content" label="内容" show-overflow-tooltip />
          <el-table-column label="操作" width="150">
            <template #default="{ row }">
              <el-button type="text" @click="handleEditPlan(row)">
                编辑
              </el-button>
              <el-button type="text" @click="handleDeletePlan(row)">
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div> -->
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

// 导入 StudyPlanCalendar 组件
import StudyPlanCalendar from '../components/StudyPlanCalendar.vue'

export default {
  name: 'CustomStudyPlan',
  components: {
    StudyPlanCalendar
  },
  setup() {
    const router = useRouter()
    const saving = ref(false)
    const useSimpleCalendar = ref(false)
    const currentTime = ref(new Date().toLocaleString())
    
    const studyPlans = ref([])

    // 预定义颜色
    const predefinedColors = [
      '#409EFF',
      '#67C23A',
      '#E6A23C',
      '#F56C6C',
      '#909399',
      '#36CFC9',
      '#722ED1',
      '#EB2F96'
    ]

    // 返回查询页面
    const handleBack = () => {
      console.log('返回按钮被点击')
      router.back()
    }

    // 切换日历显示
    const toggleCalendar = () => {
      useSimpleCalendar.value = !useSimpleCalendar.value
    }

    // 保存所有计划
    const handleSaveAll = async () => {
      console.log('保存按钮被点击')
      saving.value = true
      try {
        await new Promise(resolve => setTimeout(resolve, 1000))
        ElMessage.success('保存成功')
      } catch (error) {
        ElMessage.error('保存失败')
      } finally {
        saving.value = false
      }
    }

    // 计划相关事件处理
    const handlePlanAdded = (plan) => {
      console.log('新增计划:', plan)
      ElMessage.success(`成功添加计划: ${plan.title}`)
    }

    const handlePlanUpdated = (plan) => {
      console.log('更新计划:', plan)
      ElMessage.success(`成功更新计划: ${plan.title}`)
    }

    const handlePlanDeleted = (plan) => {
      console.log('删除计划:', plan)
      ElMessage.success(`成功删除计划: ${plan.title}`)
    }

    const handlePlanClicked = (plan) => {
      console.log('点击计划:', plan)
    }

    // 编辑计划
    const handleEditPlan = (plan) => {
      console.log('编辑计划:', plan)
      ElMessage.info('编辑功能待实现')
    }

    // 删除计划
    const handleDeletePlan = async (plan) => {
      console.log('删除计划:', plan)
      try {
        await ElMessageBox.confirm(
          `确定要删除学习计划"${plan.title}"吗？`,
          '确认删除',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning',
          }
        )
        
        const index = studyPlans.value.findIndex(p => p.id === plan.id)
        if (index > -1) {
          studyPlans.value.splice(index, 1)
          ElMessage.success('计划删除成功')
        }
      } catch {
        ElMessage.info('已取消删除')
      }
    }

    // 导出计划
    const handleExportPlans = () => {
      console.log('导出计划')
      ElMessage.info('导出功能待实现')
    }

    // 更新时间
    const updateTime = () => {
      currentTime.value = new Date().toLocaleString()
    }

    // 初始化
    onMounted(() => {
      console.log('CustomStudyPlan 组件已挂载')
      
      // 每秒更新时间
      setInterval(updateTime, 1000)
      
      // 初始化示例数据
      studyPlans.value = [
        {
          id: 1,
          title: '英语词汇强化',
          startDate: '2025-07-28',
          endDate: '2025-08-03',
          content: '每天背诵50个新单词，复习100个旧单词',
          color: '#409EFF',
          progress: 65
        },
        {
          id: 2,
          title: '数学基础巩固',
          startDate: '2025-07-30',
          endDate: '2025-08-10',
          content: '复习高中数学基础知识，完成练习题',
          color: '#67C23A',
          progress: 30
        }
      ]
      
      console.log('初始化数据:', studyPlans.value)
    })

    return {
      saving,
      useSimpleCalendar,
      currentTime,
      studyPlans,
      predefinedColors,
      handleBack,
      toggleCalendar,
      handleSaveAll,
      handlePlanAdded,
      handlePlanUpdated,
      handlePlanDeleted,
      handlePlanClicked,
      handleEditPlan,
      handleDeletePlan,
      handleExportPlans
    }
  }
}
</script>

<style scoped>
.study-plans-view {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
  min-height: 100vh;
}

.debug-info {
  background: #f0f9ff;
  border: 1px solid #0ea5e9;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
}

.debug-info h2 {
  color: #0369a1;
  margin: 0 0 10px 0;
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

.header-left {
  flex: 1;
}

.page-title {
  text-align: center;
  margin: 0;
  color: #303133;
  font-size: 28px;
  font-weight: 600;
}

.header-right {
  flex: 1;
  display: flex;
  justify-content: flex-end;
}

.stats-section {
  margin-bottom: 30px;
}

.stats-card {
  text-align: center;
}

.stats-item {
  padding: 15px 0;
}

.stats-number {
  font-size: 32px;
  font-weight: bold;
  color: #409EFF;
  line-height: 1;
  margin-bottom: 8px;
}

.stats-label {
  font-size: 14px;
  color: #909399;
}

.calendar-section {
  margin-bottom: 30px;
}

.simple-calendar {
  text-align: center;
  padding: 40px;
}

.plans-list-section {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>