<template>
    <div class="user-center-layout">
        <!-- 主体容器 -->
        <div class="main-container">
            <!-- 左侧区域 -->
            <div class="left-section">
                <div class="header-section">
                    <div class="top-row">
                        <!-- 头像显示 -->
                        <el-avatar :size="80" :src="user.avatarUrl" class="avatar" />
                        <div class="user-info">
                            <h2>{{ user.username }}</h2>
                        </div>
                        <p class="level-badge"><strong>等级：</strong>Lv.{{ user.level }}</p>
                        <div class="coins-section">
                            <el-button type="primary" class="coins-button" plain>
                                <strong>虚拟币：</strong>{{ user.coins }} 💰
                            </el-button>
                            <el-button type="primary" @click="handleSignIn" class="signin-button">
                                每日签到
                            </el-button>
                        </div>
                    </div>

                    <div class="document-row">
                        <el-button type="primary" @click="goToEditPage">编辑资料</el-button>
                    </div>

                    <div class="info-row">
                        <p><strong>类型：</strong>{{ user.Category }}</p>
                    </div>
                </div>

                <!-- 按钮区 -->
                <el-button type="primary" plain class="block-button" @click="goToMyResources">📦 我的资源</el-button>
                <el-button type="primary" plain class="block-button" @click="goToMyAchievements">🏆 我的成就</el-button>
                <el-button type="primary" plain class="block-button" @click="goToMyFriends">👥 我的好友</el-button>
                <el-button type="primary" plain class="block-button" @click="showLearningRecord">📊 我的记录</el-button>
            </div>

            <!-- 右侧区域 -->
            <div class="right-section">
                <div class="posts-container">
                    <h3>我的帖子</h3>
                    <div class="posts-list">
                        <div class="post-item" v-for="post in posts" :key="post.id">
                            <h4>{{ post.title }}</h4>
                            <p>{{ post.content }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>


        <!-- 右下角反馈入口按钮 -->
        <el-tooltip content="点击这里反馈问题" placement="top">
            <el-button type="primary" class="feedback-button" @click="onFeedbackClick">
                💬 反馈入口
            </el-button>
        </el-tooltip>

        <!-- 学习记录对话框 -->
        <el-dialog
            v-model="showRecordDialog"
            title="我的学习记录"
            width="80%"
            :before-close="handleCloseRecordDialog"
            @open="onDialogOpen"
        >
            <div class="record-container">
                <div class="chart-container" ref="chartContainer"></div>
                <div class="record-summary">
                    <div class="summary-item">
                        <span class="summary-label">近7日总学习单词数：</span>
                        <span class="summary-value">{{ totalWords }}</span>
                    </div>
                    <div class="summary-item">
                        <span class="summary-label">平均每日学习：</span>
                        <span class="summary-value">{{ averageWords }}</span>
                    </div>
                </div>
            </div>
        </el-dialog>
    </div>
</template>

<script setup>

    import { ref, onMounted, watch, nextTick } from 'vue'
    import { useRoute, useRouter } from 'vue-router' // 引入 router
    import { getCoinBalance, dailySignIn } from '@/api';
    import api from '../utils/axios'
    import { ElMessage } from 'element-plus'

    const editing = ref(false)
    const route = useRoute()
    const router = useRouter() // 初始化 router

    const user = ref({
        username: '',
        Id: '',
        Email: '',
        DateOfBirth: new Date(),
        Gender: '',
        PhoneNumber: '',
        Category: '',
        level: 8,
        coins: 0, // 先设为0，后面异步获取
        avatarUrl: ''
    })


    const posts = ref([
        { id: 1, title: '欢迎大家！', content: '这是我的第一个帖子，欢迎大家交流讨论。' },
        { id: 2, title: '分享技巧', content: '分享一个很实用的技巧，希望对大家有帮助。' },
        { id: 3, title: '求助', content: '遇到了一个问题，求教大家怎么解决。' },
        { id: 4, title: '闲聊', content: '今天心情不错，来这里聊聊天吧。' },
        { id: 5, title: '推荐好书', content: '最近看了一本书，非常推荐给大家。' },
        { id: 6, title: '学习记录', content: '这是今天的学习成果！。' },
    ])

    // 学习记录相关数据
    const showRecordDialog = ref(false)
    const chartContainer = ref(null)
    const totalWords = ref(0)
    const averageWords = ref(0)
    const learningData = ref({
        dates: [],
        wordCounts: []
    })

    onMounted(async () => {
        
        let data = localStorage.getItem('user')
        if (!data || !data.userName) {
            const stored = localStorage.getItem('user')
            if (stored) {
                const parsed = JSON.parse(stored)
                if (parsed.Category=== 'admin') {
                localStorage.setItem('user', JSON.stringify(parsed))
                localStorage.setItem('isAuthenticated', 'true')
                localStorage.setItem('role', parsed.Category)
                localStorage.setItem('email', parsed.Email)

                router.push('/admin')
            }
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
                coins: 0, // 先设为0，后面异步获取
                avatarUrl: data.avatarUrl || ''
            }

            // 获取虚拟币余额
            try {
                const res = await api.get('/api/paying/coin-balance', { params: { userId: user.value.Id } });
                user.value.coins = res.data.coin;
            } catch (e) {
                user.value.coins = 0;
            }

            // 获取用户类型
            try {
                const response = await api.get('/api/paying/category', { params: { userId: user.value.Id } });
                user.value.Category = response.data.category;
            } catch (e) {
                console.error('获取用户类型失败：', e);
            }

            if(user.value.username===undefined)
            router.push('/')
        }
    })

    watch(() => route.fullPath, () => {
        const stored = localStorage.getItem('user')
        if (stored) {
            const data = JSON.parse(stored)
            user.value = {
                username: data.userName || data.username,
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

    function goToEditPage() {
        router.push({
            name: 'Editdocument',
            query: {
                id: user.value.Id,
                username: user.value.username,
                email: user.value.Email,
                gender: user.value.Gender,
                phone: user.value.PhoneNumber,
                category: user.value.Category,
                avatarUrl: user.value.avatarUrl
            }
        })
    }

    function onFeedbackClick() {
        alert('打开反馈入口')
    }

    function logout() {
        // 清除 localStorage 中的用户信息
        localStorage.removeItem('user')

        // 跳转到登录页
        router.push('/')
    }

    // 签到功能
    async function handleSignIn() {
        if (!user.value.Id) return;
        try {
            const res = await dailySignIn(user.value.Id);
            if (res.data && res.data.message) {
                ElMessage.success(res.data.message);
                // 更新虚拟币余额
                const coinRes = await api.get('/api/paying/coin-balance', { params: { userId: user.value.Id } });
                user.value.coins = coinRes.data.coin;
            }
        } catch (e) {
            ElMessage.error('签到失败，今日已签到');
        }
    }
    
    function goToMyFriends() {
        router.push('/user/friends')
    }
    function goToMyResources() {
        router.push('/user/resources')
    }
    
    function goToMyAchievements() {
        router.push('/user/achievements')
    }

    // 显示学习记录
    async function showLearningRecord() {
        showRecordDialog.value = true
    }

    // 对话框打开时的处理
    async function onDialogOpen() {
        await fetchLearningData()
    }

    // 获取学习数据
    async function fetchLearningData() {
        if (!user.value.Id) {
            ElMessage.error('用户信息获取失败')
            return
        }

        console.log('开始获取学习数据，用户ID:', user.value.Id)

        try {
            const dates = []
            const wordCounts = []
            let total = 0

            // 获取近7天的日期（包含当天）
            for (let i = 6; i >= 0; i--) {
                const date = new Date()
                date.setDate(date.getDate() - i)
                const dateStr = date.toISOString().split('T')[0]
                dates.push(dateStr)
                
                try {
                    const response = await api.get('/api/UserDailyWords', {
                        params: {
                            userId: user.value.Id,
                            studyDate: dateStr
                        }
                    })
                    
                    console.log(`API响应 ${dateStr}:`, response.data)
                    
                    // 解析API响应，获取wordCount字段
                    let count = 0
                    if (response.data && response.data.success && response.data.data) {
                        count = parseInt(response.data.data.wordCount) || 0
                    }
                    wordCounts.push(count)
                    total += count
                    console.log(`日期 ${dateStr} 学习单词数: ${count}`)
                } catch (error) {
                    console.error(`获取${dateStr}的学习数据失败:`, error)
                    wordCounts.push(0)
                }
            }

            console.log('最终数据:', { dates, wordCounts, total })
            
            learningData.value = { dates, wordCounts }
            totalWords.value = total
            averageWords.value = Math.round(total / 7)

            console.log('数据更新完成:', {
                totalWords: totalWords.value,
                averageWords: averageWords.value,
                learningData: learningData.value
            })

            // 延迟渲染图表，确保对话框完全打开
            setTimeout(() => {
                renderChart()
            }, 300)
        } catch (error) {
            console.error('获取学习数据失败:', error)
            ElMessage.error('获取学习数据失败')
        }
    }

    // 渲染图表
    function renderChart() {
        if (!chartContainer.value || !learningData.value.dates.length) {
            console.log('图表容器或数据不存在')
            return
        }

        console.log('开始渲染图表，数据:', learningData.value)

        // 尝试使用ECharts，如果失败则使用Canvas绘制简单图表
        import('echarts').then((echarts) => {
            // 确保容器存在
            if (!chartContainer.value) {
                console.error('图表容器不存在')
                return
            }

            const chart = echarts.init(chartContainer.value)
            
            const option = {
                title: {
                    text: '近7日学习单词数量',
                    left: 'center',
                    textStyle: {
                        color: '#333',
                        fontSize: 18,
                        fontWeight: 'bold'
                    }
                },
                tooltip: {
                    trigger: 'axis',
                    formatter: function(params) {
                        const data = params[0]
                        return `${data.name}<br/>学习单词数：${data.value}个`
                    }
                },
                xAxis: {
                    type: 'category',
                    data: learningData.value.dates.map(date => {
                        const d = new Date(date)
                        return `${d.getMonth() + 1}/${d.getDate()}`
                    }),
                    axisLabel: {
                        color: '#666',
                        fontSize: 12
                    },
                    axisLine: {
                        lineStyle: {
                            color: '#ddd'
                        }
                    }
                },
                yAxis: {
                    type: 'value',
                    name: '单词数量',
                    nameTextStyle: {
                        color: '#666',
                        fontSize: 12
                    },
                    axisLabel: {
                        color: '#666',
                        fontSize: 12
                    },
                    axisLine: {
                        lineStyle: {
                            color: '#ddd'
                        }
                    },
                    splitLine: {
                        lineStyle: {
                            color: '#f0f0f0'
                        }
                    }
                },
                series: [{
                    name: '学习单词数',
                    type: 'line',
                    data: learningData.value.wordCounts,
                    smooth: true,
                    lineStyle: {
                        color: '#409EFF',
                        width: 3
                    },
                    itemStyle: {
                        color: '#409EFF',
                        borderWidth: 2,
                        borderColor: '#fff'
                    },
                    areaStyle: {
                        color: {
                            type: 'linear',
                            x: 0,
                            y: 0,
                            x2: 0,
                            y2: 1,
                            colorStops: [{
                                offset: 0,
                                color: 'rgba(64, 158, 255, 0.3)'
                            }, {
                                offset: 1,
                                color: 'rgba(64, 158, 255, 0.1)'
                            }]
                        }
                    },
                    symbol: 'circle',
                    symbolSize: 8
                }],
                grid: {
                    left: '10%',
                    right: '10%',
                    top: '15%',
                    bottom: '15%'
                }
            }

            chart.setOption(option)
            console.log('ECharts图表渲染完成')

            // 监听窗口大小变化
            window.addEventListener('resize', () => {
                chart.resize()
            })
        }).catch(error => {
            console.error('加载ECharts失败，使用Canvas绘制:', error)
            renderSimpleChart()
        })
    }

    // 使用Canvas绘制简单图表
    function renderSimpleChart() {
        if (!chartContainer.value) return

        const canvas = document.createElement('canvas')
        canvas.width = chartContainer.value.clientWidth
        canvas.height = chartContainer.value.clientHeight
        canvas.style.width = '100%'
        canvas.style.height = '100%'
        
        // 清空容器
        chartContainer.value.innerHTML = ''
        chartContainer.value.appendChild(canvas)
        
        const ctx = canvas.getContext('2d')
        const width = canvas.width
        const height = canvas.height
        const padding = 60
        
        // 清空画布
        ctx.clearRect(0, 0, width, height)
        
        // 绘制标题
        ctx.fillStyle = '#333'
        ctx.font = 'bold 18px Arial'
        ctx.textAlign = 'center'
        ctx.fillText('近7日学习单词数量', width / 2, 30)
        
        // 计算数据范围
        const maxValue = Math.max(...learningData.value.wordCounts, 1)
        const minValue = 0
        
        // 绘制坐标轴
        ctx.strokeStyle = '#ddd'
        ctx.lineWidth = 1
        
        // Y轴
        ctx.beginPath()
        ctx.moveTo(padding, padding)
        ctx.lineTo(padding, height - padding)
        ctx.stroke()
        
        // X轴
        ctx.beginPath()
        ctx.moveTo(padding, height - padding)
        ctx.lineTo(width - padding, height - padding)
        ctx.stroke()
        
        // 绘制数据点和连线
        const dataWidth = width - 2 * padding
        const dataHeight = height - 2 * padding
        const stepX = dataWidth / (learningData.value.dates.length - 1)
        
        ctx.strokeStyle = '#409EFF'
        ctx.lineWidth = 3
        ctx.fillStyle = '#409EFF'
        
        ctx.beginPath()
        learningData.value.dates.forEach((date, index) => {
            const x = padding + index * stepX
            const y = height - padding - (learningData.value.wordCounts[index] / maxValue) * dataHeight
            
            if (index === 0) {
                ctx.moveTo(x, y)
            } else {
                ctx.lineTo(x, y)
            }
            
            // 绘制数据点
            ctx.beginPath()
            ctx.arc(x, y, 4, 0, 2 * Math.PI)
            ctx.fill()
            ctx.beginPath()
        })
        ctx.stroke()
        
        // 绘制X轴标签
        ctx.fillStyle = '#666'
        ctx.font = '12px Arial'
        ctx.textAlign = 'center'
        learningData.value.dates.forEach((date, index) => {
            const x = padding + index * stepX
            const d = new Date(date)
            const label = `${d.getMonth() + 1}/${d.getDate()}`
            ctx.fillText(label, x, height - padding + 20)
        })
        
        // 绘制Y轴标签
        ctx.textAlign = 'right'
        for (let i = 0; i <= 5; i++) {
            const y = height - padding - (i / 5) * dataHeight
            const value = Math.round((i / 5) * maxValue)
            ctx.fillText(value.toString(), padding - 10, y + 4)
        }
        
        console.log('Canvas图表渲染完成')
    }

    // 关闭记录对话框
    function handleCloseRecordDialog() {
        showRecordDialog.value = false
    }

</script>

<style scoped>
    .user-center-layout {
        width: 70%;
        min-height: 90vh;
        margin: 10px auto 30px auto;
        padding: 30px;
        background-color: #e6f7ff;
        border-radius: 20px;
    }

    .header-section {
        background-color: transparent;
        padding: 0;
        border-radius: 0;
        margin-bottom: 0;
        box-shadow: none;
    }

    .top-row {
        display: flex;
        flex-wrap: wrap; 
        align-items: center;
        gap: 12px;
    }

    /* 用户名容器 */
    .user-info {
        min-width: 100px;
        flex-shrink: 1;
    }

    /* 等级徽章 */
    .level-badge {
        background-color: #ff66b3;
        color: #fff;
        padding: 4px 12px;
        border-radius: 12px;
        font-weight: 600;
        font-size: 14px;
        white-space: nowrap;
        flex-shrink: 0;
    }

    /* 虚拟币区域 */
    .coins-section {
        display: flex;
        align-items: center;
        gap: 12px;
        flex-shrink: 0;
    }

    .coins-button {
        font-size: 14px;
        white-space: nowrap;
        flex-shrink: 0;
        border-radius: 6px;
        padding: 4px 12px;
    }

    .coins-button:hover {
        background-color: #ff66b3;
        color: white;
        border-color: #ff66b3;
    }

    .signin-button {
        font-size: 14px;
        padding: 6px 16px;
        border-radius: 6px;
        background-color: #409EFF;
        border-color: #409EFF;
        color: white;
        transition: all 0.3s;
        height: 32px;
    }

    .signin-button:hover {
        background-color: #66b1ff;
        border-color: #66b1ff;
    }

    .document-row {
        display: flex;
        align-items: center;
        margin-top: 20px;
        margin-bottom: 10px;
    }

    .user-info h2 {
        font-size: 24px;
        margin: 0;
        color: #000000;
    }

    .info-row {
        font-size: 16px;
        color: #606266;
        justify-content: flex-start;
        gap: 10px;
        text-align: left;
    }

    .divider {
        border: none;
        border-top: 1px solid #dcdfe6;
        margin: 20px 0;
    }

    .main-container {
        display: flex;
        gap: 40px;
        align-items: flex-start; 
        flex-wrap: wrap; 
    }

    /* 左侧区域 */
    .left-section {
        flex: 1.3;
        background-color: #fff;
        border-radius: 16px;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 10px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
        overflow: auto;
        height: 30%;
    }

    .block-button {
        width: auto !important;
        height: 70px !important;
        font-size: 16px;
        border: none;
        border-bottom: 1px solid #eee;
        border-radius: 0;
        color: #909399;
        background-color: #fff;
        display: flex !important;
        align-items: center !important;
        justify-content: flex-start !important;
        padding-left: 20px !important;
        padding-right: 0 !important;
        margin: 0 !important;
        box-sizing: border-box !important;
        --el-button-padding-left: 20px;
        --el-button-padding-right: 0;
    }

        .block-button:hover,
        .block-button.active {
            color: #409EFF;
            background-color: #f5f7fa;
        }

    /* 右侧区域 */
    .right-section {
        flex: 1.7;
        height: 75vh;
        background-color: #fff;
        border-radius: 16px;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 20px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    }

    /* 滚动帖子容器 */
    .posts-container {
        display: flex;
        flex-grow: 1;
        flex-direction: column;
        height: 400px;
        border: 1px solid #dcdfe6;
        border-radius: 12px;
        overflow-y: auto;
        background-color: #fafafa;
        padding: 0;
        color: #000;
    }

        .posts-container h3 {
            position: sticky;
            top: 0;
            background-color: #fafafa;
            margin: 0;
            padding: 12px 16px;
            font-size: 20px;
            font-weight: 600;
            border-bottom: 1px solid #dcdfe6;
            z-index: 10;
        }

    .posts-list {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 16px;
    }

    .post-item {
        background: #fff;
        padding: 12px 16px;
        border-radius: 8px;
        box-shadow: 0 1px 4px rgb(0 0 0 / 0.1);
    }

        .post-item h4 {
            margin: 0 0 6px 0;
            font-size: 18px;
            color: #333;
        }

        .post-item p {
            margin: 0;
            color: #666;
            font-size: 14px;
        }

    /* 右下角反馈按钮 */
    .feedback-button {
        position: fixed;
        right: 30px;
        bottom: 30px;
        z-index: 1000;
        min-width: 120px;
        height: 50px;
        font-size: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
        gap: 8px;
        padding: 0 16px;
    }

    .avatar {
        width: 60px !important;
        height: 60px !important;
        border-radius: 50% !important;
        object-fit: cover;
        flex-shrink: 0; 
    }

    .logout-button {
        margin: auto auto 0 auto;
        width: 80%;
        height: 40px;
        font-size: 16px;
        color: #f56c6c;
        border-color: #f56c6c;
    }

        .logout-button:hover {
            background-color: #f56c6c;
            color: white;
        }

    /* 学习记录对话框样式 */
    .record-container {
        padding: 20px;
    }

    .chart-container {
        width: 100%;
        height: 400px;
        margin-bottom: 20px;
        border-radius: 8px;
        background-color: #fafafa;
    }

    .record-summary {
        display: flex;
        justify-content: space-around;
        padding: 20px;
        background-color: #f8f9fa;
        border-radius: 8px;
        border: 1px solid #e9ecef;
    }

    .summary-item {
        text-align: center;
    }

    .summary-label {
        display: block;
        font-size: 14px;
        color: #666;
        margin-bottom: 8px;
    }

    .summary-value {
        display: block;
        font-size: 24px;
        font-weight: bold;
        color: #409EFF;
    }

    /* 对话框样式优化 */
    :deep(.el-dialog) {
        border-radius: 12px;
        overflow: hidden;
    }

    :deep(.el-dialog__header) {
        background-color: #f8f9fa;
        border-bottom: 1px solid #e9ecef;
        padding: 20px;
    }

    :deep(.el-dialog__title) {
        font-size: 18px;
        font-weight: bold;
        color: #333;
    }

    :deep(.el-dialog__body) {
        padding: 0;
    }

    :deep(.el-dialog__footer) {
        padding: 15px 20px;
        border-top: 1px solid #e9ecef;
        background-color: #f8f9fa;
    }

</style>

