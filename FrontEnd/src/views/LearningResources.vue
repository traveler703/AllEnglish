<template>
    <div class="learning-resources">
        <div class="resources-container">
            <!-- 顶部标题 -->
            <div class="page-header">
                <h1>学习资源</h1>
                <p>按听说读写、学习层次、资料类型分类筛选学习资源</p>
            </div>

            <!-- 筛选区域 -->
            <el-card class="filter-card" shadow="hover">
                <div class="filter-section" style="display: flex; justify-content: space-between; align-items: center;">
                    <el-select v-model="filters.speakingListening" placeholder="选择技能类型" clearable>
                        <el-option label="听力" value="听力"></el-option>
                        <el-option label="口语" value="口语"></el-option>
                        <el-option label="阅读" value="阅读"></el-option>
                        <el-option label="写作" value="写作"></el-option>
                    </el-select>

                    <el-select v-model="filters.examLevel" placeholder="选择考试层次" clearable>
                        <el-option label="四级" value="CET-4"></el-option>
                        <el-option label="六级" value="CET-6"></el-option>
                        <el-option label="考研英语" value="考研英语"></el-option>
                    </el-select>

                    <el-select v-model="filters.resourceType" placeholder="选择资料类型" clearable>
                        <el-option label="文章" value="文章"></el-option>
                        <el-option label="视频" value="视频"></el-option>
                    </el-select>
                </div>
            </el-card>

            <!-- 资源列表 -->
            <div class="resource-list">
                <el-row v-if="filteredResources.length > 0" :gutter="20" class="resource-grid">
                    <el-col v-for="resource in filteredResources"
                            :key="resource.id"
                            :xs="24"
                            :sm="12"
                            :md="8"
                            :lg="6"
                            class="resource-column">
                        <el-card class="resource-item" shadow="hover" @click="viewDetails(resource)">
                            <!-- 封面图片区域 -->
                            <div class="resource-cover">
                                <img :src="resource.previewImage || defaultCoverImage"
                                     alt="预览图"
                                     @error="(e: any) => { e.currentTarget.src = defaultCoverImage }" />
                                <div class="resource-level">{{ resource.level || '未知' }}</div>
                            </div>

                            <!-- 资源信息区域 -->
                            <div class="resource-info">
                                <!-- 标题区域 -->
                                <div class="resource-title-wrapper" :title="resource.title">
                                    <span class="resource-title">{{ resource.title || '无标题' }}</span>
                                </div>

                                <!-- 标签和价格区域 -->
                                <div class="resource-tags-price">
                                    <div class="resource-tags">
                                        <el-tag size="small" type="success">
                                            {{
                                                resource.type === '听力' ? '听力' :
                                                resource.type === '口语' ? '口语' :
                                                resource.type === '写作' ? '写作' :
                                                resource.type === '阅读' ? '阅读' : '其他'
                                            }}
                                        </el-tag>
                                        <el-tag size="small" type="info">
                                            {{ resource.resourceType === '文章' ? '文章' : resource.resourceType === '视频' ? '视频' : '其他' }}
                                        </el-tag>
                                    </div>
                                    <div class="resource-price">
                                        <span class="price-value">{{ resource.price || 0 }} 虚拟币</span>
                                    </div>
                                </div>

                                <!-- 底部按钮区域 -->
                                <div class="resource-actions">
                                    <el-button type="text" @click.stop="viewDetails(resource)" size="small">查看详情</el-button>
                                    <el-button 
                                        v-if="purchasedMaterialIds.has(resource.id)"
                                        type="success" 
                                        size="small" 
                                        disabled>
                                        已购买
                                    </el-button>
                                    <el-button 
                                        v-else
                                        type="primary" 
                                        @click.stop="handlePurchase(resource)" 
                                        size="small">
                                        购买
                                    </el-button>
                                </div>
                            </div>
                        </el-card>
                    </el-col>
                </el-row>

                <el-empty v-else description="暂无学习资源" />
            </div>


            <!-- 查看详情的弹出框 -->
            <el-dialog v-model="showDetailsDialog" width="60%">
                <template #title>
                    {{ detailsResource.title }}
                </template>
                <div>
                    <p>{{ detailsResource.description }}</p>
                    <p><strong>资源类型：</strong>{{ detailsResource.resourceType === 'document' ? '文章' : '视频' }}</p>
                    <p><strong>考试层次：</strong>{{ detailsResource.level }}</p>
                    <p><strong>价格：</strong>{{ detailsResource.price || 0 }} 虚拟币</p>
                </div>
                <template #footer>
                    <el-button @click="showDetailsDialog = false">关闭</el-button>
                    <el-button 
                        v-if="purchasedMaterialIds.has(detailsResource.id)"
                        type="success" 
                        disabled>
                        已购买
                    </el-button>
                    <el-button 
                        v-else
                        type="primary" 
                        @click="handlePurchase(detailsResource)">
                        购买
                    </el-button>
                </template>
            </el-dialog>

            <!-- 购买确认弹窗 -->
            <el-dialog
                title="确认购买"
                v-model="showPurchaseDialog"
                width="500px"
                class="purchase-dialog">
                <div class="purchase-content">
                    <div class="resource-info-purchase">
                        <h3>{{ purchaseResource.title }}</h3>
                        <p class="resource-description">{{ purchaseResource.description }}</p>
                        <div class="price-info">
                            <span class="price-label">价格：</span>
                            <span class="price-value">{{ purchaseResource.price }} 虚拟币</span>
                        </div>
                    </div>
                    
                    <div class="user-balance">
                        <span class="balance-label">您的余额：</span>
                        <span class="balance-value">{{ userCoins }} 虚拟币</span>
                    </div>
                    
                </div>
                <template #footer>
                    <el-button @click="showPurchaseDialog = false">取消</el-button>
                    <el-button 
                        type="primary" 
                        @click="confirmPurchase"
                        :disabled="userCoins < purchaseResource.price">
                        {{ userCoins < purchaseResource.price ? '余额不足' : '确认购买' }}
                    </el-button>
                </template>
            </el-dialog>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, reactive, computed, onMounted } from 'vue';
    import { ElMessage, ElMessageBox } from 'element-plus';
    import { useRouter } from 'vue-router';
    import { getCoinBalance, purchaseMaterial, getAvailableMaterials, checkMaterialInInventory } from '@/api';

    export default defineComponent({
        name: 'LearningResources',
        setup() {

            const router = useRouter();

            // 默认封面图片
            const defaultCoverImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTlq4b9QpuZJYMIq_Z8gksyR-NtNf19aF9isw&s";

            // 筛选条件
            const filters = reactive({
                speakingListening: '',
                examLevel: '',
                resourceType: ''
            });

            // 用户信息
            const user = ref({
                Id: '',
                coins: 0
            });

            // 学习资源数据
            const resources = ref<any[]>([]);

            // 当前显示详情的资源
            const detailsResource = ref<any>(null);
            const showDetailsDialog = ref(false);

            // 购买相关状态
            const showPurchaseDialog = ref(false);
            const purchaseResource = ref<any>({});
            const userCoins = ref(0);

            // 用户已购买的资源ID列表
            const purchasedMaterialIds = ref<Set<string>>(new Set());

            // 筛选后的资源
            const filteredResources = computed(() => {
                return resources.value.filter((resource) => {
                    const matchesSpeakingListening = filters.speakingListening
                        ? resource.type === filters.speakingListening
                        : true;
                    const matchesExamLevel = filters.examLevel
                        ? resource.level === filters.examLevel
                        : true;
                    const matchesResourceType = filters.resourceType
                        ? resource.resourceType === filters.resourceType
                        : true;

                    return matchesSpeakingListening && matchesExamLevel && matchesResourceType;
                });
            });

            // 获取用户信息和虚拟币余额
            const fetchUserInfo = async () => {
                try {
                    const userData = localStorage.getItem('user');
                    if (userData) {
                        const parsedUser = JSON.parse(userData);
                        user.value.Id = parsedUser.Id;
                        
                        // 获取虚拟币余额
                        const response = await getCoinBalance(user.value.Id);
                        userCoins.value = response.data.coin;
                    }
                } catch (error: any) {
                    console.error('获取用户信息失败:', error);
                    ElMessage.error('获取用户信息失败');
                }
            };

            // 获取可购买的资源列表
            const fetchAvailableMaterials = async () => {
                try {
                    const response = await getAvailableMaterials();
                    const materials = response.data.materials;
                    
                    console.log('从后端获取的原始数据:', materials);
                    
                    // 转换数据格式以匹配前端显示需求
                    resources.value = materials.map((material: any) => {
                        const mappedResource = {
                            id: material.id,
                            title: material.description || '未命名资源',
                            description: material.description || '暂无描述',
                            type: material.skillType,
                            level: material.examType,
                            resourceType: material.materialType,
                            previewImage: material.previewUrl || defaultCoverImage,
                            price: material.price,
                            url: material.url
                        };
                        console.log('映射后的资源数据:', mappedResource);
                        return mappedResource;
                    });

                    // 获取用户已购买的资源列表
                    await fetchUserPurchasedMaterials();
                } catch (error: any) {
                    console.error('获取资源列表失败:', error);
                    ElMessage.error('获取资源列表失败');
                }
            };

            // 获取用户已购买的资源列表
            const fetchUserPurchasedMaterials = async () => {
                try {
                    if (!user.value.Id) return;
                    
                    // 检查每个资源是否已购买
                    for (const resource of resources.value) {
                        try {
                            const response = await checkMaterialInInventory(user.value.Id, resource.id);
                            if (response.data.isInInventory) {
                                purchasedMaterialIds.value.add(resource.id);
                            }
                        } catch (error) {
                            console.error(`检查资源 ${resource.id} 购买状态失败:`, error);
                        }
                    }
                } catch (error: any) {
                    console.error('获取用户已购买资源列表失败:', error);
                }
            };

            // 查看详情的处理
            const viewDetails = (resource: any) => {
                console.log('点击查看详情，资源信息:', resource);
                console.log('资源类型:', resource.resourceType);
                
                // 根据资源类型跳转到相应的详情页面
                if (resource.resourceType === '视频' || resource.resourceType === 'video') {
                    console.log('跳转到视频详情页面');
                    router.push({
                        name: 'ResourcesVideo',
                        query: { resourceData: JSON.stringify(resource) }
                    });
                } else if (resource.resourceType === 'document' || resource.resourceType === '文章') {
                    console.log('跳转到文章详情页面');
                    router.push({
                        name: 'ResourcesDocument',
                        query: { resourceData: JSON.stringify(resource) }
                    });
                } else {
                    console.log('资源类型不明确，显示弹窗');
                    // 如果资源类型不明确，显示弹窗
                    detailsResource.value = resource;
                    showDetailsDialog.value = true;
                }
            };

            // 处理购买
            const handlePurchase = (resource: any) => {
                console.log('点击购买按钮:', resource);
                purchaseResource.value = resource;
                showPurchaseDialog.value = true;
                console.log('showPurchaseDialog:', showPurchaseDialog.value);
            };

            // 确认购买
            const confirmPurchase = async () => {
                try {
                    console.log('发送购买请求:', {
                        userId: user.value.Id,
                        materialId: purchaseResource.value.id,
                        resource: purchaseResource.value
                    });
                    
                    const response = await purchaseMaterial(user.value.Id, purchaseResource.value.id);
                    
                    if (response.data.message) {
                        ElMessage.success(response.data.message);
                        // 更新用户虚拟币余额
                        userCoins.value = response.data.remainingCoins;
                        // 将资源添加到已购买列表
                        purchasedMaterialIds.value.add(purchaseResource.value.id);
                        showPurchaseDialog.value = false;
                    }
                } catch (error: any) {
                    console.error('购买失败:', error);
                    if (error.response?.data?.message) {
                        ElMessage.error(error.response.data.message);
                    } else {
                        ElMessage.error('购买失败，请稍后重试');
                    }
                }
            };

            onMounted(() => {
                fetchUserInfo();
                fetchAvailableMaterials();
            });

            return {
                filters,
                filteredResources,
                showDetailsDialog,
                detailsResource,
                showPurchaseDialog,
                purchaseResource,
                userCoins,
                purchasedMaterialIds,
                viewDetails,
                handlePurchase,
                confirmPurchase,
                defaultCoverImage
            };
        }
    });
</script>

<style scoped>
    .learning-resources {
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

    .filter-card {
        margin-bottom: 20px;
        border-radius: 8px;
    }

    .filter-section {
        display: flex;
        gap: 15px;
        align-items: center;
        flex-wrap: wrap;
        width: 100%;
    }

        .filter-section .el-select {
            width: 180px;
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

    .resource-tags-price {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin: 8px 0;
    }

    .resource-tags {
        display: flex;
        gap: 6px;
    }

    .resource-price {
        font-weight: bold;
        color: #ff66b3;
    }

    .price-value {
        font-size: 16px;
    }

    .resource-preview {
        color: #666;
        font-size: 14px;
        line-height: 1.4;
        margin: 8px 0 10px;
        flex: 1;
        height: 60px;
        overflow: hidden;
        display: -webkit-box;
        -webkit-box-orient: vertical;
        -webkit-line-clamp: 3;
        line-clamp: 3;
    }

    .resource-actions {
        margin-top: auto;
        display: flex;
        justify-content: space-between;
        padding: 8px 0;
    }

    /* 购买弹窗样式 */
    .purchase-dialog .el-dialog {
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
    }

    .purchase-dialog .el-dialog__body {
        padding: 30px;
        background: #ffffff;
    }

    .purchase-dialog .el-dialog__header {
        padding: 20px 30px 0 30px;
        background: #ffffff;
        border-bottom: 1px solid #f0f0f0;
    }

    .purchase-dialog .el-dialog__footer {
        padding: 20px 30px 30px 30px;
        background: #ffffff;
        border-top: 1px solid #f0f0f0;
        display: flex;
        justify-content: flex-end;
        gap: 12px;
    }

    .purchase-dialog .el-dialog__footer .el-button {
        min-width: 80px;
        height: 40px;
        font-size: 14px;
        border-radius: 6px;
    }

    .purchase-content {
        display: flex;
        flex-direction: column;
        gap: 30px;
        padding: 10px 0;
    }

    .resource-info-purchase h3 {
        margin: 0 0 15px 0;
        color: #333;
        font-size: 22px;
        font-weight: 600;
        line-height: 1.3;
    }

    .resource-description {
        color: #666;
        margin: 0 0 20px 0;
        line-height: 1.6;
        font-size: 15px;
    }

    .price-info {
        display: flex;
        align-items: center;
        gap: 10px;
        margin-bottom: 5px;
    }

    .price-label {
        color: #666;
        font-size: 14px;
    }

    .price-value {
        color: #ff66b3;
        font-weight: bold;
        font-size: 18px;
    }

    .user-balance {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 15px;
        background-color: #f5f7fa;
        border-radius: 8px;
        border: 1px solid #e4e7ed;
    }

    .balance-label {
        color: #666;
        font-size: 14px;
    }

    .balance-value {
        color: #409EFF;
        font-weight: bold;
        font-size: 18px;
    }

    .purchase-summary {
        border-top: 1px solid #eee;
        padding-top: 20px;
    }

    .summary-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        color: #666;
        font-size: 14px;
    }

    .remaining-coins {
        color: #67C23A;
        font-weight: bold;
        font-size: 16px;
    }

    .purchase-detail {
        margin-top: 15px;
        padding: 10px;
        background-color: #f5f7fa;
        border-radius: 4px;
    }

    @media (max-width: 768px) {
        .filter-section {
            flex-direction: column;
            align-items: flex-start;
        }

            .filter-section .el-select {
                width: 100%;
            }
    }
</style>

<style>
    .purchase-confirm-messagebox {
        min-width: 400px;
    }

        .purchase-confirm-messagebox .el-message-box__content {
            padding: 20px;
        }

        .purchase-confirm-messagebox .el-message-box__status {
            font-size: 24px !important;
        }
</style>