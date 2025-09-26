<template>
  <div class="admin-container">
    <div class="admin-header">
      <div class="admin-title-box">
        <h1>系统管理控制台</h1>
        <div class="admin-user">
          <span>欢迎，{{ userName }} ({{ role }})</span>
        </div>
      </div>
    </div>
    
    <div class="admin-content">
      <el-menu 
        mode="horizontal"
        :default-active="currentView"
        @select="handleMenuSelect"
        class="admin-menu"
      >
        <el-menu-item index="dashboard">控制面板</el-menu-item>
        <el-menu-item index="feedback">用户反馈</el-menu-item>
        <el-menu-item index="users">用户管理</el-menu-item>
        <el-menu-item index="materials">课程管理</el-menu-item>
        <el-menu-item index="ads">广告管理</el-menu-item>
      </el-menu>
      
      <div class="admin-section">
        <div class="admin-main">
          <!-- 控制面板视图 -->
          <div v-if="currentView === 'dashboard'" class="dashboard-view">
            <h2>管理员控制台</h2>
            <p>欢迎回来，{{ userName }}！今天是 {{ currentDate }}</p>
            
            <div class="action-buttons">
              <el-card class="action-card" @click="currentView = 'feedback'">
                <div class="card-content">
                  <i class="el-icon-chat-line-round"></i>
                  <h3>用户反馈管理</h3>
                  <p>查看和处理用户提交的反馈</p>
                </div>
              </el-card>
              
              <el-card class="action-card" @click="currentView = 'users'">
                <div class="card-content">
                  <i class="el-icon-user"></i>
                  <h3>用户账户管理</h3>
                  <p>管理系统用户账户和权限</p>
                </div>
              </el-card>
              
              <el-card class="action-card" @click="currentView = 'materials'">
                <div class="card-content">
                  <i class="el-icon-collection"></i>
                  <h3>课程管理</h3>
                  <p>管理课程的上架和下架</p>
                </div>
              </el-card>

              <el-card class="action-card" @click="currentView = 'ads'">
                <div class="card-content">
                  <i class="el-icon-collection"></i>
                  <h3>广告管理</h3>
                  <p>管理广告的上架和下架</p>
                </div>
              </el-card>
            </div>
          </div>
          
          <!-- 用户反馈视图 -->
          <div v-if="currentView === 'feedback'" class="feedback-view">
            <h2>用户反馈管理</h2>
            <el-button @click="currentView = 'dashboard'" class="back-btn">
              <i class="el-icon-back"></i> 返回控制面板
            </el-button>
            
            <el-table :data="feedbackData" style="width: 100%" class="feedback-table">
             <el-table-column prop="id" label="ID" width="230" align="center" header-align="center"></el-table-column>
<el-table-column prop="user" label="用户" width="100" align="center" header-align="center"></el-table-column>
<el-table-column prop="content" label="反馈内容" width="200" align="center" header-align="center"></el-table-column>
<el-table-column prop="date" label="提交时间" width="200" align="center" header-align="center"></el-table-column>
<el-table-column label="状态" width="120" align="center" header-align="center">
  <template #default="scope">
    <el-tag :type="scope.row.status === '已处理' ? 'success' : 'warning'">
      {{ scope.row.status }}
    </el-tag>
  </template>
</el-table-column>
<el-table-column label="操作" width="200" align="center" header-align="center">
  <template #default="scope">
    <div style="display: flex; flex-direction: column; align-items: center; gap: 8px;">
      <el-button 
        size="small" 
        style="width: 120px; text-align: center;margin: 0;" 
        @click="handleViewFeedback(scope.row)"
      >
        查看详情
      </el-button>
      <el-button 
        v-if="scope.row.status === '待处理'" 
        size="small" 
        type="primary" 
        style="width: 120px; text-align: center;margin: 0;" 
        @click="handleResolveFeedback(scope.row)"
      >
        标记为已处理
      </el-button>
    </div>
  </template>
</el-table-column>
            </el-table>
            
            <div class="feedback-detail" v-if="selectedFeedback">
              <h3>反馈详情 #{{ selectedFeedback.id }}</h3>
<p><strong>用户:</strong> {{ selectedFeedback.user }}</p>
<p><strong>提交时间:</strong> {{ selectedFeedback.date }}</p>
<p><strong>内容:</strong></p>
<div class="feedback-content">{{ selectedFeedback.content }}</div>
<el-button type="primary" @click="selectedFeedback = null">关闭详情</el-button>
            </div>
          </div>
          
          <!-- 用户管理视图 -->
          <div v-if="currentView === 'users'" class="users-view">
            <h2>用户账户管理</h2>
            <div class="user-header">
              <el-button @click="currentView = 'dashboard'" class="back-btn">
                <i class="el-icon-back"></i> 返回控制面板
              </el-button>
              <el-button type="primary" @click="fetchUsers">
                <i class="el-icon-refresh"></i> 刷新数据
              </el-button>
            </div>
            
            <div class="user-search">
              <el-input
                v-model="searchUser"
                placeholder="搜索用户（用户名/邮箱）"
                prefix-icon="el-icon-search"
                style="width: 300px; margin-right: 15px;"
                @keyup.enter="fetchUsers"
              ></el-input>
              <el-button type="primary" @click="fetchUsers">搜索</el-button>
            </div>
            
            <el-table 
              :data="userData" 
              v-loading="loadingUsers"
              style="width: 100%" 
              class="user-table"
            >
              <el-table-column prop="id" label="ID" width="330" align="center" header-align="center"></el-table-column>
<el-table-column prop="userName" label="用户名" width="80" align="center" header-align="center"></el-table-column>
<el-table-column prop="email" label="邮箱" width="150" align="center" header-align="center"></el-table-column>
<el-table-column label="角色" width="100" align="center" header-align="center">
  <template #default="scope">
    <el-tag :type="scope.row.category === 'admin' ? 'danger' : 'primary'">
      {{ scope.row.category === 'admin' ? '管理员' : '普通用户' }}
    </el-tag>
  </template>
</el-table-column>
<el-table-column prop="dateOfBirth" label="生日" width="120" align="center" header-align="center">
  <template #default="scope">
    {{ formatDate(scope.row.dateOfBirth) }}
  </template>
</el-table-column>
<el-table-column label="操作" width="150" align="center" header-align="center">
  <template #default="scope">
    <el-button size="small" @click="handleEditUser(scope.row)">编辑</el-button>
    <el-button 
      size="small" 
      type="danger" 
      @click="handleDeleteUser(scope.row)"
    >
      删除
    </el-button>
  </template>
</el-table-column>
            </el-table>
            
            <div class="pagination-container">
              <el-pagination
                v-model:current-page="userPagination.currentPage"
                v-model:page-size="userPagination.pageSize"
                :total="userPagination.total"
                :page-sizes="[5, 10, 20, 50]"
                layout="total, sizes, prev, pager, next, jumper"
                @size-change="fetchUsers"
                @current-change="fetchUsers"
              />
            </div>
            
            <!-- 编辑用户对话框 -->
              <el-dialog 
                v-model="editDialogVisible" 
                title="编辑用户信息" 
                width="500px"
                class="edit-dialog"
              >
                <div class="form-header">
                  <div class="user-avatar">
                    <i class="el-icon-user-solid"></i>
                  </div>
                  <h3>{{ editingUser.userName || '用户' }}</h3>
                  <div class="user-id">ID: {{ editingUser.id }}</div>
                </div>
                
                <el-form 
                  :model="editingUser" 
                  label-width="80px"
                  class="user-form"
                >
                  <el-form-item label="用户名">
                    <el-input v-model="editingUser.userName" placeholder="请输入用户名"></el-input>
                  </el-form-item>
                  
                  <el-form-item label="电子邮箱">
                    <el-input v-model="editingUser.email" placeholder="请输入电子邮箱" type="email"></el-input>
                  </el-form-item>
                  
                  <el-form-item label="用户角色">
                    <el-select v-model="editingUser.category" placeholder="请选择用户角色">
                      <el-option label="普通用户" value="user"></el-option>
                      <el-option label="管理员" value="admin"></el-option>
                    </el-select>
                  </el-form-item>
                </el-form>
                
                <template #footer>
                  <div class="form-footer">
                    <el-button @click="editDialogVisible = false">取消</el-button>
                    <el-button type="primary" @click="saveUser" :loading="savingUser">保存修改</el-button>
                  </div>
                </template>
              </el-dialog>
            </div>
          

            <!-- 广告管理） -->
<div v-if="currentView === 'ads'" class="ads-view">
  <el-card shadow="hover">
    <div class="card-header">
      <span>广告管理</span>
      <div class="actions">
        <el-button type="primary" @click="openAdDialog()">新建广告</el-button>
        <el-button @click="fetchAds">刷新</el-button>
      </div>
    </div>

    <el-table
      :data="adList.slice((adPagination.currentPage-1)*adPagination.pageSize, adPagination.currentPage*adPagination.pageSize)"
      v-loading="adLoading"
      border
    >
      <el-table-column prop="id" label="ID" width="220" />
      <el-table-column prop="mediaUrl" label="媒体预览" width="200">
        <template #default="{ row }">
          <img v-if="row.mediaUrl?.match(/\.(png|jpg|jpeg|gif|webp)$/i)"
               :src="row.mediaUrl" style="width:120px;height:70px;object-fit:cover;border-radius:8px" />
          <a v-else :href="row.mediaUrl" target="_blank">打开媒体</a>
        </template>
      </el-table-column>
      <el-table-column prop="targetId" label="对应资源ID" min-width="220">
        <template #default="{ row }">
          <a :href="row.targetId" target="_blank">{{ row.targetId }}</a>
        </template>
      </el-table-column>
      <el-table-column prop="context" label="文案" min-width="220" />
      <el-table-column prop="status" label="状态" width="140">
        <template #default="{ row }">
          <el-switch :model-value="row.status === 1"
                     active-text="启用" inactive-text="停用"
                     @change="() => toggleAdStatus(row)" />
        </template>
      </el-table-column>
      <el-table-column prop="clickCount" label="点击数" width="100" />
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button size="small" @click="openAdDialog(row)">编辑</el-button>
          <el-button size="small" type="danger" @click="removeAd(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="display:flex;justify-content:flex-end;margin-top:12px">
      <el-pagination
        v-model:current-page="adPagination.currentPage"
        v-model:page-size="adPagination.pageSize"
        :total="adPagination.total"
        layout="prev, pager, next, sizes, total" />
    </div>
  </el-card>

  <!-- 新建/编辑 广告弹窗 -->
  <el-dialog v-model="adFormVisible" title="广告信息" width="680px">
    <el-form :model="adForm" label-width="100px">
      <el-form-item label="媒体地址">
        <div style="width:100%">
          <el-input v-model="adForm.mediaUrl" placeholder="例如 /uploads/ads/xxx.png 或 https://..." />
          <div style="margin-top:8px">
            <el-upload :show-file-list="false" :auto-upload="false" :on-change="handleAdUpload">
              <el-button>上传媒体</el-button>
            </el-upload>
          </div>
        </div>
      </el-form-item>
      <el-form-item label="资源ID">
        <el-input v-model="adForm.targetId" placeholder="请输入资源ID" />
      </el-form-item>
      <el-form-item label="广告文案">
        <el-input v-model="adForm.context" type="textarea" :rows="3" />
      </el-form-item>
      <el-form-item label="状态">
        <el-switch v-model="adForm.status" :active-value="1" :inactive-value="0" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="adFormVisible=false">取消</el-button>
      <el-button type="primary" @click="saveAd">保存</el-button>
    </template>
  </el-dialog>
</div>

          <!-- 课程管理视图 -->
          <div v-if="currentView === 'materials'" class="materials-view">
            <h2>课程管理</h2>
            <div class="materials-header">
              <el-button @click="currentView = 'dashboard'" class="back-btn">
                <i class="el-icon-back"></i> 返回控制面板
              </el-button>
              <div class="materials-actions">
                <el-button type="primary" @click="fetchMaterials">
                  <i class="el-icon-refresh"></i> 刷新数据
                </el-button>
                <el-button type="success" @click="openUploadDialog">
                  <i class="el-icon-upload"></i> 上传新课程
                </el-button>
              </div>
            </div>
            
            <el-table 
              :data="materialData" 
              v-loading="loadingMaterials"
              style="width: 100%" 
              class="materials-table"
              empty-text="暂无课程数据"
            >
              <el-table-column prop="id" label="课程ID" width="280" align="center" header-align="center"></el-table-column>
<el-table-column prop="description" label="课程描述" min-width="200"></el-table-column>
<el-table-column label="类型" width="120" align="center" header-align="center">
  <template #default="scope">
    <el-tag>{{ scope.row.materialType }}</el-tag>
  </template>
</el-table-column>
<el-table-column label="技能类型" width="120" align="center" header-align="center">
  <template #default="scope">
    <el-tag type="info">{{ scope.row.skillType }}</el-tag>
  </template>
</el-table-column>
<el-table-column label="考试类型" width="120" align="center" header-align="center">
  <template #default="scope">
    <el-tag type="info">{{ scope.row.examType }}</el-tag>
  </template>
</el-table-column>
<el-table-column prop="price" label="价格" width="100" align="center" header-align="center">
  <template #default="scope">
    ¥{{ scope.row.price }}
  </template>
</el-table-column>
<el-table-column label="状态" width="100" align="center" header-align="center">
  <template #default="scope">
    <el-tag :type="scope.row.isActive===1 ? 'success' : 'danger'">
      {{ scope.row.isActive===1 ? '已上架' : '已下架' }}
    </el-tag>
  </template>
</el-table-column>
<el-table-column label="操作" width="180" align="center" header-align="center">
  <template #default="scope">
    <el-button 
      size="small" 
      :type="scope.row.isActive===1 ? 'warning' : 'success'" 
      @click="toggleMaterialStatus(scope.row)"
    >
      {{ scope.row.isActive===1 ? '下架' : '上架' }}
    </el-button>
    <el-button 
      size="small" 
      type="danger" 
      @click="handleDeleteMaterial(scope.row)"
    >
      删除
    </el-button>
  </template>
</el-table-column>
            </el-table>
            
            <div v-if="materialData.length === 0 && !loadingMaterials" class="empty-state">
              <i class="el-icon-document-remove empty-icon"></i>
              <p class="empty-text">暂无课程数据，请上传新课程</p>
              <el-button type="success" @click="openUploadDialog">
                <i class="el-icon-upload"></i> 上传课程
              </el-button>
            </div>
            
            <div class="pagination-container">
              <el-pagination
                v-model:current-page="materialPagination.currentPage"
                v-model:page-size="materialPagination.pageSize"
                :total="materialPagination.total"
                :page-sizes="[5, 10, 20]"
                layout="total, sizes, prev, pager, next, jumper"
                @size-change="fetchMaterials"
                @current-change="fetchMaterials"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 上传课程对话框 -->
    <el-dialog 
      v-model="uploadDialogVisible" 
      title="上传新课程" 
      width="600px"
      class="upload-dialog"
    >
      <el-form 
        :model="newMaterial" 
        label-width="100px"
        class="upload-form"
        :rules="uploadRules"
        ref="uploadFormRef"
      >
        <div class="form-row">
          <div class="form-group">
            <el-form-item label="课程类型" prop="materialType">
              <el-select v-model="newMaterial.materialType" placeholder="请选择课程类型">
                <el-option label="视频" value="视频"></el-option>
                <el-option label="文章" value="文章"></el-option>
              </el-select>
            </el-form-item>
          </div>
          <div class="form-group">
            <el-form-item label="技能类型" prop="skillType">
              <el-select v-model="newMaterial.skillType" placeholder="请选择技能类型">
                <el-option label="听力" value="听力"></el-option>
                <el-option label="阅读" value="阅读"></el-option>
                <el-option label="口语" value="口语"></el-option>
                <el-option label="写作" value="写作"></el-option>
              </el-select>
            </el-form-item>
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <el-form-item label="考试类型" prop="examType">
              <el-select v-model="newMaterial.examType" placeholder="请选择考试类型">
                <el-option label="CET-4" value="CET-4"></el-option>
                <el-option label="CET-6" value="CET-6"></el-option>
                <el-option label="托福" value="托福"></el-option>
              </el-select>
            </el-form-item>
          </div>
          <div class="form-group">
            <el-form-item label="价格" prop="price">
              <el-input v-model.number="newMaterial.price" type="number" placeholder="请输入价格">
                <template #prefix>¥</template>
              </el-input>
            </el-form-item>
          </div>
        </div>
        
        <el-form-item label="课程描述" prop="description">
          <el-input 
            v-model="newMaterial.description" 
            type="textarea" 
            :rows="3" 
            placeholder="请输入课程描述"
          ></el-input>
        </el-form-item>
        
        
        <el-form-item label="课程文件" prop="file">
          <el-upload
  class="upload-area"
  :action="uploadAction"
  :show-file-list="false"
  :on-success="handleFileSuccess"
  :before-upload="beforeFileUpload"
  :on-error="handleUploadError"
>
            <div class="upload-area-inner">
              <i class="el-icon-upload upload-icon"></i>
              <p>点击或拖拽文件到此处上传</p>
              <p class="el-upload__tip">支持视频(MP4, AVI)或文章(PDF, DOCX)格式</p>
            </div>
          </el-upload>
          <div v-if="fileInfo.name" class="file-info">
            <i class="el-icon-document"></i> {{ fileInfo.name }} ({{ formatFileSize(fileInfo.size) }})
            <el-button 
              v-if="fileInfo.name" 
              type="danger" 
              size="mini" 
              circle 
              @click.stop="removeFile"
              class="remove-file-btn"
            >
              <i class="el-icon-close"></i>
            </el-button>
          </div>
        </el-form-item>

        
<el-form-item label="预览图" prop="previewImage">
  <el-upload
    class="upload-area"
    :action="previewUploadAction"
    :show-file-list="false"
    :on-success="handlePreviewSuccess"
    :before-upload="beforePreviewUpload"
    :on-error="handleUploadError"
  >
    <div class="upload-area-inner">
      <i class="el-icon-picture-outline upload-icon"></i>
      <p>点击或拖拽图片到此处上传</p>
      <p class="el-upload__tip">支持JPG、PNG格式，大小不超过2MB</p>
    </div>
  </el-upload>
  <div v-if="previewImageInfo.url" class="file-info">
    <img :src="previewImageInfo.url" alt="预览图" style="max-width: 200px; max-height: 150px; margin-top: 10px;">
    <el-button 
      v-if="previewImageInfo.url" 
      type="danger" 
      size="mini" 
      circle 
      @click.stop="removePreviewImage"
      class="remove-file-btn"
    >
      <i class="el-icon-close"></i>
    </el-button>
  </div>
</el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="uploadDialogVisible = false">取消</el-button>
        <el-button 
          type="primary" 
          @click="submitMaterialForm"
          :loading="uploading"
        >
          上传课程
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import axios from 'axios'  
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

// 定义API基础路径
const USERS_API = '/api/admin'
const MATERIALS_API = '/api/admin/material'

const currentView = ref('dashboard')
const userName = ref(localStorage.getItem('username') || '管理员')
const role = ref(localStorage.getItem('role') || 'unknown')


const currentDate = computed(() => {
  return new Date().toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  })
})

const user = ref({
  username: '',
  Id: '',
  Email: '',
  DateOfBirth: new Date(),
  Gender: '',
  PhoneNumber: '',
  Category: '',
  level: 8,
  coins: 1500,
  avatarUrl: ''
})

const handleMenuSelect = (view) => {
  currentView.value = view
  if (view === 'users') {
    fetchUsers()
  } else if (view === 'materials') {
    fetchMaterials()
  }
}

const feedbackData = ref([
  { id: 12316546465616645154684, user: '张三', content: '学习模块加载太慢了，希望能优化', date: '2023-10-15 14:30', status: '待处理' },
  { id: 21565186456156161651516561618618, user: '李四', content: '单词发音功能有问题', date: '2023-10-16 09:15', status: '已处理' },
  { id: 315615161686156116168446, user: '王五', content: '建议增加更多练习题目', date: '2023-10-17 16:45', status: '待处理' },
  { id: 4597237537859785389597927529, user: '赵六', content: '账户登录有问题', date: '2023-10-18 11:20', status: '待处理' },
])

const selectedFeedback = ref(null)

const handleViewFeedback = (feedback) => {
  selectedFeedback.value = feedback
}

const handleResolveFeedback = (feedback) => {
  feedback.status = '已处理'
  ElMessage.success(`已处理反馈 #${feedback.id}`)
}

// 用户数据相关逻辑
const userData = ref([])
const searchUser = ref('')
const loadingUsers = ref(false)
const userPagination = ref({
  currentPage: 1,
  pageSize: 10,
  total: 0
})

const editingUser = ref(null)
const editDialogVisible = ref(false)
const savingUser = ref(false)

// 格式化日期
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

// 获取用户列表
const fetchUsers = async () => {
  try {
    loadingUsers.value = true
    const params = {
      page: userPagination.value.currentPage,
      limit: userPagination.value.pageSize
    }
    
    if (searchUser.value) {
      params.search = searchUser.value
    }
    
    const response = await axios.get(`${USERS_API}/users`, {
      params,
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    
    userData.value = response.data.map(user => ({
      ...user,
      active: user.isActive === 1, // 将整数转换为布尔值
      
    }))
    
    // 更新分页总数
    userPagination.value.total = userData.value.length
  } catch (error) {
    if (error.response?.status === 401) {
      ElMessage.error('认证失败，请重新登录')
      logout()
    } else {
      ElMessage.error(error.response?.data?.message || '获取用户数据失败')
    }
  } finally {
    loadingUsers.value = false
  }
}

// 打开编辑对话框
const handleEditUser = (user) => {
  // 创建编辑对象的副本
  editingUser.value = { ...user }
  editDialogVisible.value = true
}

// 保存用户信息
const saveUser = async () => {
  try {
    savingUser.value = true
    
    const updateData = {
      UserName: editingUser.value.userName,
      Email: editingUser.value.email,
      Category: editingUser.value.category,
    };
    
    await axios.put(
      `${USERS_API}/users/${editingUser.value.id}`,
      updateData, 
      {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      }
    );
    
    ElMessage.success('用户信息已更新')
    
    // 更新本地数据
    const index = userData.value.findIndex(u => u.id === editingUser.value.id)
    if (index !== -1) {
      userData.value[index] = {
        ...editingUser.value,
      }
    }
    
    editDialogVisible.value = false
  } catch (error) {
    if (error.response?.status === 401) {
      ElMessage.error('认证失败，请重新登录')
      logout()
    } else {
      ElMessage.error(error.response?.data?.message || '保存用户信息失败')
    }
  } finally {
    savingUser.value = false
  }
}

// 删除用户（使用邮箱）
const handleDeleteUser = async (user) => {
  try {
    await ElMessageBox.confirm(
      `确定要永久删除用户 "${user.username}" 吗?`,
      '警告',
      {
        confirmButtonText: '确定删除',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    await axios.delete(`${USERS_API}/users/${user.email}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    userData.value = userData.value.filter(u => u.email !== user.email)
    ElMessage.success(`用户 "${user.username}" 已删除`)
  } catch (error) {
    if (error === 'cancel') return
    
    if (error.response?.status === 401) {
      ElMessage.error('认证失败，请重新登录')
      logout()
    } else {
      ElMessage.error(error.response?.data?.message || '删除用户失败')
    }
  }
}

// 课程管理相关逻辑
const materialData = ref([])
const loadingMaterials = ref(false)
const materialPagination = ref({
  currentPage: 1,
  pageSize: 10,
  total: 0
})

// 获取课程列表
const fetchMaterials = async () => {
  try {
    loadingMaterials.value = true
    const params = {
      page: materialPagination.value.currentPage,
      limit: materialPagination.value.pageSize
    }
    
    const response = await axios.get(`${MATERIALS_API}/list`, {
      params,
    })
    
    materialData.value = response.data
    
    // 更新分页总数
    materialPagination.value.total = materialData.value.length
  } catch (error) {
    if (error.response?.status === 401) {
      ElMessage.error('认证失败，请重新登录')
    } else {
      ElMessage.error('获取课程数据失败: ' + (error.response?.data?.message || error.message))
    }
  } finally {
    loadingMaterials.value = false
  }
}

// 切换课程状态（上架/下架）
const toggleMaterialStatus = async (material) => {
  try {
    const newStatus = material.isActive === 1 ? 0 : 1;
    const action = newStatus === 1 ? '上架' : '下架'
    
    await ElMessageBox.confirm(
      `确定要${action}课程 "${material.description}" 吗?`,
      '操作确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    // 调用后端接口更新状态
    await axios.patch(`${MATERIALS_API}/${material.id}/status`, )
    
    // 更新本地状态
    material.IsActive = newStatus
    ElMessage.success(`课程已${action}`)
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败: ' + (error.response?.data?.message || error.message))
    }
  }
}

// 删除课程
const handleDeleteMaterial = async (material) => {
  try {
    await ElMessageBox.confirm(
      `确定要永久删除课程 "${material.description}" 吗?`,
      '删除确认',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'error'
      }
    )
    
    // 调用后端删除接口
    await axios.delete(`${MATERIALS_API}/${material.id}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    // 更新本地数据
    materialData.value = materialData.value.filter(m => m.id !== material.id)
    ElMessage.success('课程已删除')
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败: ' + (error.response?.data?.message || error.message))
    }
  }
}

// 上传课程相关逻辑
const uploadDialogVisible = ref(false)
const uploading = ref(false)
const uploadFormRef = ref(null)

const newMaterial = ref({
  materialType: '',
  skillType: '',
  examType: '',
  price: null,
  description: '',
  file: null,
  IsActive: true,
  url: '' ,
  previewImageUrl: ''
})

const fileInfo = ref({})
const uploadAction = ref('/api/admin/upload-material-file')
const fileUploadData = ref({}) // 文件上传附加数据


const previewImageInfo = ref({}) // 预览图信息
const previewUploadAction = ref('/api/admin/upload-preview-image') // 预览图上传接口

// 预览图上传前的验证
const beforePreviewUpload = (file) => {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif']
  const isImage = allowedTypes.includes(file.type)
  const isLt2M = file.size / 1024 / 1024 < 2

  if (!isImage) {
    ElMessage.error('请上传图片文件（JPG/PNG）!')
    return false
  }
  if (!isLt2M) {
    ElMessage.error('图片大小不能超过2MB!')
    return false
  }

  // 保存文件信息（如果需要显示）
  previewImageInfo.value = {
    name: file.name,
    size: file.size,
    type: file.type
  }
  return true
}

// 预览图上传成功
const handlePreviewSuccess = (response) => {
  if (response && response.url) {
    newMaterial.value.previewImageUrl = response.url
    previewImageInfo.value.url = response.url // 用于显示图片
    ElMessage.success('预览图上传成功!')
  } else {
    ElMessage.error('预览图上传失败')
  }
}

// 移除预览图
const removePreviewImage = () => {
  previewImageInfo.value = {}
  newMaterial.value.previewImageUrl = ''
}

const uploadRules = {
  materialType: [
    { required: true, message: '请选择课程类型', trigger: 'change' }
  ],
  skillType: [
    { required: true, message: '请选择技能类型', trigger: 'change' }
  ],
  examType: [
    { required: true, message: '请选择考试类型', trigger: 'change' }
  ],
  price: [
    { required: true, message: '请输入价格', trigger: 'blur' },
    { type: 'number', min: 0, message: '价格必须大于0', trigger: 'blur' }
  ],
  description: [
    { required: true, message: '请输入课程描述', trigger: 'blur' }
  ]
}

const openUploadDialog = () => {
  uploadDialogVisible.value = true
  // 重置表单
  newMaterial.value = {
    materialType: '',
    skillType: '',
    examType: '',
    price: null,
    description: '',
    file: null,
    isActive: true,
    url: ''
  }
  fileInfo.value = {}
  
  // 准备文件上传数据
  fileUploadData.value = {
    MaterialType: newMaterial.value.materialType,
    SkillType: newMaterial.value.skillType,
    ExamType: newMaterial.value.examType,
    Price: newMaterial.value.price,
    Description: newMaterial.value.description,
    IsActive: 1
  }
}

// 文件上传前的验证
const beforeFileUpload = (file) => {
  // 允许的文件类型
  const allowedVideoTypes = ['video/mp4', 'video/avi'];
  const allowedDocTypes = ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
  
  const isVideo = allowedVideoTypes.includes(file.type);
  const isDocument = allowedDocTypes.includes(file.type);
  
  if (!isVideo && !isDocument) {
    ElMessage.error('只支持视频(MP4, AVI)或文章(PDF, DOCX)格式!');
    return false;
  }
  
  // 文件大小限制 (100MB)
  const isLt100M = file.size / 1024 / 1024 < 100;
  if (!isLt100M) {
    ElMessage.error('文件大小不能超过 100MB!');
    return false;
  }
  
  // 更新文件信息
  fileInfo.value = {
    name: file.name,
    size: file.size,
    type: file.type
  }
  
  // 保存文件引用
  newMaterial.value.file = file;
  
  // 更新上传数据
  fileUploadData.value = {
    MaterialType: newMaterial.value.materialType,
    SkillType: newMaterial.value.skillType,
    ExamType: newMaterial.value.examType,
    Price: newMaterial.value.price,
    Description: newMaterial.value.description,
    IsActive: true
  }
  
  return true;
}

// 文件上传成功处理
const handleFileSuccess = (response, file) => {
  if (response && response.url) {
    // 保存文件URL到表单数据
    newMaterial.value.url = response.url;
    ElMessage.success('文件上传成功!');
  } else {
    ElMessage.error(response?.message || '文件上传失败');
  }
}

// 文件上传错误处理
const handleUploadError = (err, file) => {
  console.error('文件上传错误:', err);
  ElMessage.error('文件上传失败: ' + (err.message || '服务器错误'));
}

// 移除已选文件
const removeFile = () => {
  fileInfo.value = {};
  newMaterial.value.file = null;
  newMaterial.value.url = '';
  ElMessage.info('已移除文件');
}

// 提交整个课程表单
const submitMaterialForm = async () => {
  if (!uploadFormRef.value) return
  
  try {
    // 验证表单
    await uploadFormRef.value.validate()
    
    // 检查文件是否上传
    if (!newMaterial.value.url) {
      ElMessage.error('请先上传课程文件');
      return;
    }
    
    uploading.value = true
    
    // 使用 FormData 格式提交数据
    const formData = new FormData();
    formData.append('MaterialType', newMaterial.value.materialType);
    formData.append('SkillType', newMaterial.value.skillType);
    formData.append('ExamType', newMaterial.value.examType);
    formData.append('Price', newMaterial.value.price);
    formData.append('Description', newMaterial.value.description);
    formData.append('fileUrl', newMaterial.value.url);  
    formData.append('PreviewUrl', newMaterial.value.previewImageUrl || '');
    formData.append('IsActive', true);
    
    // 调用创建课程接口
    const response = await axios.post(`${MATERIALS_API}/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    // 添加到本地列表
    materialData.value.unshift({
      ...response.data,
      isActive: response.data.IsActive === 1 || response.data.IsActive === true
    })
    
    ElMessage.success('课程上传成功!')
    uploadDialogVisible.value = false
  } catch (error) {
    console.error('课程上传错误:', error);
    ElMessage.error('课程上传失败: ' + (error.response?.data?.message || error.message))
  } finally {
    uploading.value = false
  }
}

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i])
}


const uploadMaterial = async () => {
  if (!uploadFormRef.value) return
  
  try {
    // 验证表单
    await uploadFormRef.value.validate()
    
    // 检查文件
    if (!newMaterial.value.file) {
      ElMessage.error('请选择课程文件')
      return
    }
    
    uploading.value = true
    
    // 创建FormData对象
    const formData = new FormData()
    formData.append('MaterialType', newMaterial.value.materialType)
    formData.append('SkillType', newMaterial.value.skillType)
    formData.append('ExamType', newMaterial.value.examType)
    formData.append('Price', newMaterial.value.price)
    formData.append('Description', newMaterial.value.description)
    formData.append('IsActive', true)
    formData.append('file', newMaterial.value.file)
    
    // 调用上传接口
    const response = await axios.post(`${MATERIALS_API}/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    // 添加到本地列表
    materialData.value.unshift({
      ...response.data,
      isActive: response.data.IsActive === 1 || response.data.IsActive === true
    })
    
    ElMessage.success('课程上传成功')
    uploadDialogVisible.value = false
  } catch (error) {
    ElMessage.error('上传失败: ' + (error.response?.data?.message || error.message))
  } finally {
    uploading.value = false
  }
}

onMounted(() => {
  window.addEventListener('unload', handlePageUnload)
  let data = route.query
        if (!data || !data.userName) {
            const stored = localStorage.getItem('user')
            if (stored) {
                data = JSON.parse(stored)
            }
        }

        // 初始化 user 数据
        if (data) {
            user.value = {
                username: data.userName,
                Id: data.Id,
                Email: data.Email || '',
                DateOfBirth: new Date(data.DateOfBirth),
                Gender: data.Gender || '',
                PhoneNumber: data.PhoneNumber || '',
                Category: data.Category || '',
                level: parseInt(data.level || 0),
                coins: parseInt(data.coins || 0),
                avatarUrl: data.avatarUrl || ''
            }
          }
})

const handlePageUnload = () => {
  const AEmail = localStorage.getItem('email')
  if (!AEmail) return
}

// —— 广告管理

const adList = ref([])
const adLoading = ref(false)
const adPagination = ref({ currentPage: 1, pageSize: 10, total: 0 })

const adFormVisible = ref(false)
const adForm = ref({
  id: '',
  mediaUrl: '',
  targetId: '',
  context: '',
  status: 1
})

const fetchAds = async () => {
  try {
    adLoading.value = true
    const res = await axios.get(`/api/admin/ads/list`)
    adList.value = res.data || []
    adPagination.value.total = adList.value.length
  } catch (e) {
    ElMessage.error('获取广告列表失败：' + (e.response?.data || e.message))
  } finally {
    adLoading.value = false
  }
}

const openAdDialog = (row = null) => {
  if (row) {
    adForm.value = {
      id: row.id,
      mediaUrl: row.mediaUrl,
      targetId: row.targetId,
      context: row.context,
      status: row.status
    }
  } else {
    adForm.value = { id: '', mediaUrl: '', targetId: '', context: '', status: 1 }
  }
  adFormVisible.value = true
}

const saveAd = async () => {
  try {
    await axios.post(`/api/admin/ads`, {
      id: adForm.value.id || null,
      mediaUrl: adForm.value.mediaUrl,
      targetId: adForm.value.targetId,
      context: adForm.value.context,
      status: adForm.value.status
    })
    ElMessage.success('保存成功')
    adFormVisible.value = false
    fetchAds()
  } catch (e) {
    ElMessage.error('保存失败：' + (e.response?.data || e.message))
  }
}

const toggleAdStatus = async (row) => {
  try {
    await axios.patch(`/api/admin/ads/${row.id}/status`)
    ElMessage.success('状态已更新')
    fetchAds()
  } catch (e) {
    ElMessage.error('更新状态失败：' + (e.response?.data || e.message))
  }
}

const removeAd = async (row) => {
  try {
    await ElMessageBox.confirm('确认删除该广告吗？', '提示', { type: 'warning' })
    await axios.delete(`/api/admin/ads/${row.id}`)
    ElMessage.success('已删除')
    fetchAds()
  } catch (e) {
    if (e !== 'cancel') {
      ElMessage.error('删除失败：' + (e.response?.data || e.message))
    }
  }
}

const handleAdUpload = async (file) => {
  const form = new FormData()
  form.append('file', file.raw)
  try {
    const res = await axios.post(`/api/admin/upload-ad-media`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    adForm.value.mediaUrl = res.data.url
    ElMessage.success('上传成功')
  } catch (e) {
    ElMessage.error('上传失败：' + (e.response?.data || e.message))
  }
}


</script>

<style scoped>
.admin-container {
   background-color: #ffffff00;
  border-radius: 12px;
  padding: 30px;
  width: 60%;
  margin: 40px auto;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px 20px;
  border-bottom: 1px solid #ebeef5;
}

.admin-title-box {
  background-color: white;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  width: 100%;  
  margin: 0 auto;        
  display: flex;
  flex-direction: column; 
  align-items: center;    
  text-align: center;
  box-sizing: border-box;
}

.admin-user {
  display: flex;
  align-items: center;
  gap: 10px;
}

.admin-content {
  margin-top: 20px;
  margin: 0 auto;
}

.admin-menu {
  margin-bottom: 20px;
   border-radius: 12px;
}

.admin-section {
  width: 100%;
  display: flex;
  justify-content: center;
}

.admin-main {
  background: white;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  width: 100%;
  box-sizing: border-box;
}

.dashboard-view h2 {
  width: 100%;
  margin-bottom: 15px;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-top: 30px;
}

.action-card {
  width: 100%;
  cursor: pointer;
  transition: transform 0.3s;
}

.action-card:nth-child(1) {
  background-color: #fffbfe; 
}

.action-card:nth-child(2) {
  background-color: #fff6fc; 
}

.action-card:nth-child(3) {
  background-color: #fff0f9; 
}

.action-card:hover {
  transform: translateY(-5px);
}

.card-content {
  text-align: center;
  padding: 20px;
}

.card-content i {
  font-size: 48px;
  color: #409EFF;
  margin-bottom: 15px;
}

.feedback-view, .users-view, .materials-view {
  position: relative;
}

.back-btn {
  margin-bottom: 20px;
}

.feedback-detail {
  margin-top: 30px;
  padding: 20px;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  background-color: #fafafa;
}

.feedback-content {
  padding: 10px;
  background-color: white;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  margin: 0px 0 20px;
  min-height: 100px;
}

.user-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}

.user-search {
  display: flex;
  margin-bottom: 20px;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

/* 课程管理样式 */
.materials-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 15px;
}

.materials-actions {
  display: flex;
  gap: 15px;
}

.materials-table {
  margin-top: 20px;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.status-badge {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.status-active {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.status-inactive {
  background-color: #ffebee;
  color: #c62828;
}

.empty-state {
  text-align: center;
  padding: 50px 20px;
}

.empty-icon {
  font-size: 64px;
  color: #c0c4cc;
  margin-bottom: 20px;
}

.empty-text {
  color: #909399;
  margin-bottom: 30px;
}

/* 上传对话框样式 */
.upload-dialog .el-dialog {
  border-radius: 12px;
  overflow: hidden;
}

.upload-form {
  padding: 20px;
}

.form-row {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.form-group {
  flex: 1;
}

.upload-area {
  border: 2px dashed #dcdfe6;
  border-radius: 8px;
  padding: 30px;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.3s;
  margin-bottom: 20px;
}

.upload-area:hover {
  border-color: #409eff;
}

.upload-icon {
  font-size: 48px;
  color: #409eff;
  margin-bottom: 15px;
}

.file-info {
  margin-top: 15px;
  padding: 10px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

@media (max-width: 768px) {
  .materials-header {
    flex-direction: column;
  }
  
  .materials-actions {
    width: 100%;
    justify-content: flex-start;
  }
  
  .form-row {
    flex-direction: column;
    gap: 15px;
  }
}
</style>