<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="$emit('update:visible', $event)"
    :title="editingPlan ? '编辑学习计划' : '创建新学习计划'"
    width="600px"
    top="5vh"
    :close-on-click-modal="false"
  >
    <el-form 
      ref="formRef" 
      :model="form" 
      label-width="100px"
      label-position="top"
      :rules="rules"
    >
      <el-form-item label="计划标题" prop="title">
        <el-input v-model="form.title" placeholder="请输入计划标题" />
      </el-form-item>
      
      <el-form-item label="计划类型">
        <el-select v-model="form.planType" placeholder="请选择计划类型" disabled>
          <el-option label="Auto" value="Auto" />
        </el-select>
      </el-form-item>
      
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="开始日期" prop="startDate">
            <el-date-picker
              v-model="form.startDate"
              type="date"
              placeholder="选择开始日期"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="结束日期" prop="endDate">
            <el-date-picker
              v-model="form.endDate"
              type="date"
              placeholder="选择结束日期"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>
        </el-col>
      </el-row>
      
      <el-form-item label="单词数量">
        <el-input-number v-model="form.wordCount" :min="0" :step="10" />
      </el-form-item>
      
      <el-form-item label="文章数量">
        <el-input-number v-model="form.articleCount" :min="0" :step="1" />
      </el-form-item>      
      
      <el-form-item label="口语练习时间(分钟)">
        <el-input-number v-model="form.oralTime" :min="0" :step="10" />
      </el-form-item>

      <el-form-item label="听力练习时间(分钟)">
        <el-input-number v-model="form.listeningTime" :min="0" :step="10" />
      </el-form-item>

      <el-form-item label="是否公开">
        <el-switch v-model="form.isPublic" />
      </el-form-item>
    </el-form>
    
    <template #footer>
      <el-button @click="$emit('update:visible', false)">取消</el-button>
      <el-button type="primary" @click="handleSubmit" :loading="loading">保存计划</el-button>
    </template>
  </el-dialog>
</template>

<script>
import { ref, reactive, watch, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import dayjs from 'dayjs';

export default {
  name: 'CustomStudyPlan',
  props: {
    visible: {
      type: Boolean,
      required: true
    },
    plan: {
      type: Object,
      default: null
    }
  },
  emits: ['update:visible', 'save'],
  setup(props, { emit }) {
    const formRef = ref(null);
    const loading = ref(false);
    const userId = ref('');
    
    // 初始化用户ID
    onMounted(() => {
      const userData = localStorage.getItem('user');
      if (userData) {
        const parsedData = JSON.parse(userData);
        userId.value = parsedData.Id || '114514'; // 默认id
      }
    });
    
    // 表单数据
    const form = reactive({
      userId: '',
      wordIds: [],
      wordCount: 0,
      planType: 'Auto',
      title: '',
      duration: 0, // 根据日期计算
      isPublic: true,
      articleCount: 0,
      oralTime: 0,
      listeningTime: 0,
      startDate: dayjs().format('YYYY-MM-DD'),
      endDate: dayjs().add(7, 'day').format('YYYY-MM-DD')
    });
    
    // 表单验证规则
    const rules = {
      title: [
        { required: true, message: '请输入计划标题', trigger: 'blur' }
      ],
      startDate: [
        { required: true, message: '请选择开始日期', trigger: 'change' }
      ],
      endDate: [
        { required: true, message: '请选择结束日期', trigger: 'change' },
        {
          validator: (rule, value, callback) => {
            if (value < form.startDate) {
              callback(new Error('结束日期不能早于开始日期'));
            } else {
              callback();
            }
          },
          trigger: 'change'
        }
      ]
    };
    
    // 编辑计划时初始化表单
    watch(() => props.plan, (newPlan) => {
      if (newPlan) {
        Object.assign(form, newPlan);
      } else {
        // 重置表单
        form.title = '';
        form.planType = 'Auto';
        form.startDate = dayjs().format('YYYY-MM-DD');
        form.endDate = dayjs().add(7, 'day').format('YYYY-MM-DD');
        form.wordCount = 0;
        form.articleCount = 0;
        form.oralTime = 0;
        form.listeningTime = 0;
        form.isPublic = true;
        form.wordIds = [];
      }
    }, { immediate: true });
    
    // 计算持续时间（天数）
    const calculateDuration = () => {
      const start = dayjs(form.startDate);
      const end = dayjs(form.endDate);
      form.duration = end.diff(start, 'day') + 1; // 包含开始和结束当天
    };
    
    // 处理表单提交
    const handleSubmit = async () => {
      try {
        await formRef.value.validate();
        calculateDuration();
        loading.value = true;

        // 检查各指标是否均空
        if (form.wordCount + form.articleCount + form.oralTime + form.listeningTime == 0) {
          throw new Error('保存失败，未设置计划内容');
        }
        
        // 准备提交数据
        const submitData = {
          userId: userId.value,
          wordIds: form.wordIds,
          wordCount: form.wordCount,
          planType: form.planType,
          title: form.title,
          duration: form.duration,
          isPublic: form.isPublic,
          articleCount: form.articleCount,
          oralTime: form.oralTime,
          listeningTime: form.listeningTime,
          startDate: form.startDate, // 直接使用YYYY-MM-DD格式
          endDate: form.endDate,      // 直接使用YYYY-MM-DD格式
          completed: 0
        };
        
        // 调用API
        const response = await fetch('https://localhost:7071/api/StudyPlan', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(submitData)
        });
        
        if (!response.ok) {
          throw new Error('保存失败');
        }
        
        const result = await response.json();

        // Call the second API to associate user with the plan
        const userStudyPlanData = {
          userId: userId.value,
          planId: result.id, // Assuming the plan ID is in result.id
          startDate: new Date(form.startDate).toISOString()
        };

        const userPlanResponse = await fetch('https://localhost:7071/api/UserStudyPlan', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(userStudyPlanData)
        });

        if (!userPlanResponse.ok) {
          throw new Error('关联用户学习计划失败');
        }

        emit('save', result);
        emit('update:visible', false);
        ElMessage.success('计划保存成功');
      } catch (error) {
        ElMessage.error(error.message || '请填写完整的计划信息');
      } finally {
        loading.value = false;
      }
    };

    return {
      formRef,
      loading,
      form,
      rules,
      handleSubmit
    };
  }
};
</script>

<style scoped>
.type-color-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-right: 8px;
}
</style>