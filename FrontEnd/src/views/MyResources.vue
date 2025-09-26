<template>
    <div class="my-resources">
        <div class="resources-container">
            <!-- 顶部标题 -->
            <div class="page-header">
                <h1>我的资源</h1>
                <p>您已购买的学习资源</p>
            </div>

            <!-- 资源列表 -->
            <div class="resource-list">
                <el-row v-if="userResources.length > 0" :gutter="20" class="resource-grid">
                    <el-col v-for="resource in userResources"
                            :key="resource.id"
                            :xs="24"
                            :sm="12"
                            :md="8"
                            :lg="6"
                            class="resource-column">
                        <el-card class="resource-item" shadow="hover">
                            <!-- 封面图片区域 -->
                            <div class="resource-cover">
                                <img :src="resource.previewUrl || defaultCoverImage"
                                     alt="预览图"
                                     @error="handleImageError">
                                <div class="resource-level">{{ resource.examType || '未知' }}</div>
                                <div class="purchased-badge">已购买</div>
                            </div>

                            <!-- 资源信息区域 -->
                            <div class="resource-info">
                                <!-- 标题区域 -->
                                <div class="resource-title-wrapper" :title="resource.description">
                                    <span class="resource-title">{{ resource.description || '无标题' }}</span>
                                </div>

                                <!-- 标签区域 -->
                                <div class="resource-tags">
                                    <el-tag size="small" type="success">
                                        {{
                        resource.skillType === '听力' ? '听力' :
                        resource.skillType === '口语' ? '口语' :
                        resource.skillType === '写作' ? '写作' :
                        resource.skillType === '阅读' ? '阅读' : '其他'
                                        }}
                                    </el-tag>
                                    <el-tag size="small" type="info">
                                        {{ resource.materialType === '文章' ? '文章' : '视频'}}
                                    </el-tag>
                                </div>

                                <!-- 购买信息 -->
                                <div class="purchase-info">
                                    <p class="purchase-date">购买时间：{{ formatDate(resource.purchaseDate) }}</p>
                                    <p class="purchase-price">购买价格：{{ resource.purchasePrice }} 虚拟币</p>
                                </div>

                                <!-- 底部按钮区域 -->
                                <div class="resource-actions">
                                    <el-button type="primary"
                                               @click="openResource(resource)"
                                               size="small">
                                        查看资源
                                    </el-button>
                                </div>
                            </div>
                        </el-card>
                    </el-col>
                </el-row>

                <el-empty v-else description="暂无已购买资源" />
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';
    import { ElMessage } from 'element-plus';
    import { getUserInventory } from '@/api';
    import { useRouter } from 'vue-router';

    const router = useRouter();

    // 默认封面图片
    const defaultCoverImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTlq4b9QpuZJYMIq_Z8gksyR-NtNf19aF9isw&s";

    // 用户资源列表
    const userResources = ref([]);

    // 获取用户资源
    const fetchUserResources = async () => {
        try {
            const userData = localStorage.getItem('user');
            if (userData) {
                const parsedUser = JSON.parse(userData);
                const response = await getUserInventory(parsedUser.Id);
                userResources.value = response.data.inventory || [];
            }
        } catch (error) {
            console.error('获取用户资源失败:', error);
            ElMessage.error('获取用户资源失败');
        }
    };

    // 打开资源链接（新标签页）
    const openResource = (resource) => {
        if (!resource.url) {
            ElMessage.warning('资源链接不可用');
            return;
        }

        // 根据资源类型跳转到相应的详情页面
        if (resource.materialType === '视频' || resource.materialType === 'video') {
            router.push({
                name: 'ResourcesVideoOwn',
                query: { resourceData: JSON.stringify(resource) }
            });
        }
        else if (resource.materialType === 'document' || resource.materialType === '文章') {
            router.push({
                name: 'ResourcesDocumentOwn',
                query: { resourceData: JSON.stringify(resource) }
            });
        }
        else {
            // 默认处理方式
            window.open(resource.url, '_blank');
        }
    };

    // 格式化日期
    const formatDate = (dateString) => {
        if (!dateString) return '未知';
        const date = new Date(dateString);
        return date.toLocaleDateString('zh-CN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    // 处理图片加载错误
    const handleImageError = (e) => {
        e.target.src = defaultCoverImage;
    };

    onMounted(() => {
        fetchUserResources();
    });
</script>

<style scoped>
    .my-resources {
        padding: 20px;
        max-width: 1200px;
        margin: 0 auto;
        width: 100%;
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

    .resource-list {
        margin-bottom: 30px;
    }

    .resource-grid {
        margin-top: 20px;
    }

    .resource-column {
        margin-bottom: 20px;
    }

    .resource-item {
        height: 100%;
        cursor: pointer;
        transition: all 0.3s;
        display: flex;
        flex-direction: column;
        margin-bottom: 20px;
        overflow: hidden;
        border-radius: 8px;
    }

        .resource-item:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
        }

    .resource-cover {
        position: relative;
        height: 150px;
        overflow: hidden;
        border-radius: 4px;
        margin-bottom: 10px;
    }

        .resource-cover img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
            background-color: #f5f5f5;
        }

    .resource-level {
        position: absolute;
        top: 10px;
        right: 10px;
        background: rgba(255, 102, 179, 0.85);
        color: #fff;
        padding: 2px 8px;
        border-radius: 4px;
        font-size: 12px;
    }

    .purchased-badge {
        position: absolute;
        top: 10px;
        left: 10px;
        background: rgba(103, 194, 58, 0.85);
        color: #fff;
        padding: 2px 8px;
        border-radius: 4px;
        font-size: 12px;
    }

    .resource-info {
        flex: 1;
        display: flex;
        flex-direction: column;
        padding: 0 10px;
    }

    .resource-title-wrapper {
        height: 24px;
        overflow: hidden;
        position: relative;
        margin-bottom: 8px;
    }

    .resource-title {
        display: inline-block;
        white-space: nowrap;
        font-size: 16px;
        color: #333;
        line-height: 24px;
        font-weight: 500;
    }

    .resource-title-wrapper:hover .resource-title {
        animation: marquee 8s linear infinite;
    }

    @keyframes marquee {
        0% {
            transform: translateX(0);
        }

        100% {
            transform: translateX(-100%);
        }
    }

    .resource-tags {
        display: flex;
        gap: 6px;
        margin: 8px 0;
    }

    .purchase-info {
        margin: 8px 0;
        font-size: 12px;
        color: #666;
    }

    .purchase-date, .purchase-price {
        margin: 2px 0;
    }

    .resource-actions {
        margin-top: auto;
        display: flex;
        justify-content: center;
        padding: 8px 0;
    }

    @media (max-width: 768px) {
        .resource-actions {
            flex-direction: column;
            gap: 8px;
        }
    }
</style>