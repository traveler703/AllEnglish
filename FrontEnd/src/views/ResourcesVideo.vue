<template>
    <div class="video-resource-detail">
        <div class="detail-container">
            <!-- 左侧信息区域 -->
            <div class="info-section">
                <el-card shadow="hover" class="info-card">
                    <div class="video-cover">
                        <img :src="resource.previewImage || defaultCoverImage"
                             alt="视频封面"
                             @error="(e: any) => { e.currentTarget.src = defaultCoverImage }" />
                    </div>
                    <div class="resource-meta">
                        <h2 class="resource-title">{{ resource.title }}</h2>
                        <div class="meta-tags">
                            <el-tag type="success">{{ formatType(resource.type) }}</el-tag>
                            <el-tag type="info">视频</el-tag>
                            <el-tag type="warning">{{ resource.level }}</el-tag>
                        </div>
                        <div class="price-wrapper">
                            <span class="price">¥{{ resource.price }}</span>
                        </div>
                        <div class="description">
                            <h3>课程简介</h3>
                            <p>{{ resource.description }}</p>
                        </div>
                    </div>
                </el-card>
            </div>

            <!-- 右侧播放区域 -->
            <div class="play-section">
                <el-card shadow="hover" class="play-card">
                    <h3>课程预览（30秒）</h3>
                    <div class="video-container">
                        <video ref="videoRef"
                               class="video-player"
                               controls
                               controlsList="nodownload"
                               :poster="resource.previewImage || defaultCoverImage"
                               @timeupdate="checkTime"
                               oncontextmenu="return false">
                            <source :src="resource.url" type="video/mp4" />
                        </video>
                    </div>
                </el-card>
            </div>
        </div>

        <!-- 底部按钮 -->
        <div class="footer-actions">
            <el-button 
                v-if="isPurchased"
                type="success" 
                disabled>
                已购买
            </el-button>
            <el-button 
                v-else
                type="primary" 
                @click="handlePurchase">
                购买资源
            </el-button>
            <el-button @click="goBack">返回</el-button>
        </div>

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
</template>

<script lang="ts">
    import { defineComponent, ref, onMounted } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import { ElMessage } from 'element-plus';
    import { getCoinBalance, purchaseMaterial, checkMaterialInInventory } from '@/api';

    export default defineComponent({
        name: 'ResourcesVideo',
        setup() {
            const route = useRoute();
            const router = useRouter();

            const defaultCoverImage = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTlq4b9QpuZJYMIq_Z8gksyR-NtNf19aF9isw&s';
            const resource = ref(JSON.parse(route.query.resourceData as string || '{}'));

            console.log('获取的资源', resource);

            // 购买相关状态
            const showPurchaseDialog = ref(false);
            const purchaseResource = ref<any>({});
            const userCoins = ref(0);
            const user = ref({
                Id: '',
                coins: 0
            });

            // 用户是否已购买当前资源
            const isPurchased = ref(false);

            const formatType = (type: string) => {
                const map: Record<string, string> = {
                    '听力': '听力',
                    '口语': '口语',
                    '阅读': '阅读',
                    '写作': '写作'
                };
                return map[type?.trim()] || '其他';
            };

            const goBack = () => {
                router.push('/learningresources');
            };

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

                        // 检查用户是否已购买当前资源
                        await checkPurchaseStatus();
                    }
                } catch (error: any) {
                    console.error('获取用户信息失败:', error);
                    ElMessage.error('获取用户信息失败');
                }
            };

            // 检查用户是否已购买当前资源
            const checkPurchaseStatus = async () => {
                try {
                    if (!user.value.Id || !resource.value.id) return;
                    
                    const response = await checkMaterialInInventory(user.value.Id, resource.value.id);
                    isPurchased.value = response.data.isInInventory;
                } catch (error: any) {
                    console.error('检查购买状态失败:', error);
                }
            };

            const handlePurchase = () => {
                console.log('点击购买按钮:', resource.value);
                purchaseResource.value = resource.value;
                showPurchaseDialog.value = true;
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
                        // 更新购买状态
                        isPurchased.value = true;
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

            // 视频试看逻辑
            const videoRef = ref<HTMLVideoElement | null>(null);
            const maxPreviewTime = 30; 

            const checkTime = () => {
                const video = videoRef.value;
                if (video && video.currentTime > maxPreviewTime) {
                    video.pause();
                    video.currentTime = maxPreviewTime;
                    ElMessage.warning('试看已结束，购买后可观看完整视频。');
                }
            };

            onMounted(() => {
                fetchUserInfo();
            });

            return {
                resource,
                defaultCoverImage,
                formatType,
                goBack,
                handlePurchase,
                confirmPurchase,
                videoRef,
                checkTime,
                showPurchaseDialog,
                purchaseResource,
                userCoins,
                isPurchased
            };
        }
    });
</script>

<style scoped>
    .video-resource-detail {
        max-width: 1500px;
        min-width: 1200px;
        margin: 0 auto;
        padding: 20px;
        display: flex;
        flex-direction: column;
        min-height: 100vh;
    }

    .detail-container {
        display: flex;
        gap: 20px;
        flex: 1;
    }

    .info-section {
        flex: 1;
        min-width: 0;
    }

    .play-section {
        flex: 2;
        min-width: 0;
    }

    .info-card, .play-card {
        height: 100%;
    }

    .video-cover {
        text-align: center;
        margin-bottom: 20px;
    }

        .video-cover img {
            max-width: 100%;
            max-height: 300px;
            border-radius: 4px;
            object-fit: contain;
        }

    .resource-meta {
        padding: 0 15px;
    }

    .resource-title {
        color: #333;
        margin-bottom: 15px;
        font-size: 22px;
        word-break: break-word;
    }

    .meta-tags {
        display: flex;
        justify-content: center; 
        flex-wrap: wrap;
        gap: 8px;
        margin-bottom: 10px;
    }

    .price-wrapper {
        text-align: center;
        margin-top: 10px;
    }

    .price {
        font-size: 18px;
        font-weight: bold;
        color: #ff66b3;
    }

    .description {
        margin: 25px 0;
    }

        .description h3 {
            color: #555;
            margin-bottom: 10px;
            font-size: 16px;
        }

        .description p {
            color: #666;
            line-height: 1.6;
            font-size: 14px;
        }

    .play-card h3 {
        color: #555;
        margin-bottom: 15px;
        font-size: 18px;
    }

    .video-container {
        width: 100%;
        margin-top: 10px;
    }

    .video-player {
        width: 100%;
        height: calc(100vh - 300px);
        min-height: 400px;
        background-color: #000;
        border-radius: 4px;
    }

    .footer-actions {
        margin-top: 20px;
        text-align: center;
        padding: 20px 0;
        border-top: 1px solid #eee;
    }

        .footer-actions .el-button {
            margin: 0 10px;
        }

    /* 响应式 */
    @media (max-width: 992px) {
        .detail-container {
            flex-direction: column;
        }

        .video-player {
            height: 400px;
        }
    }

    @media (max-width: 768px) {
        .video-cover img {
            max-height: 200px;
        }

        .video-player {
            height: 300px;
            min-height: 300px;
        }

        .footer-actions {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

            .footer-actions .el-button {
                margin: 5px 0;
                width: 100%;
            }
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
</style>
