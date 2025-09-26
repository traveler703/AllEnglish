<template>
    <div class="video-resource-detail">
        <!-- 广告弹窗 -->
        <el-dialog v-model="showAdDialog"
                   title="推荐视频课程"
                   width="40%"
                   :close-on-click-modal="false"
                   :show-close="canCloseAd"
                   :before-close="handleAdClose">
            <div class="ad-content">
                <h3>您可能会感兴趣</h3>
                <div class="ad-preview" @click="goToAdResource">
                    <img :src="adData.previewUrl || defaultCoverImage"
                         alt="广告预览图"
                         class="ad-image"
                         @error="e => { e.currentTarget.src = defaultCoverImage }" />
                    <div class="ad-info">
                        <h4>{{ adData.title }}</h4>
                        <div class="meta-tags">
                            <el-tag type="success">{{ formatType(adData.skillType) }}</el-tag>
                            <el-tag type="info">{{ adData.examType }}</el-tag>
                            <el-tag type="warning">{{ adData.materialType }}</el-tag>
                        </div>
                        <p class="ad-description">{{ adData.description }}</p>
                    </div>
                </div>
                <p v-if="!canCloseAd" class="countdown-tip">广告将在 {{ countdown }} 秒后关闭</p>
            </div>
            <template #footer>
                <el-button @click="closeAd" :disabled="!canCloseAd">关闭</el-button>
            </template>
        </el-dialog>

        <div class="detail-container">
            <!-- 左侧信息区域 -->
            <div class="info-section">
                <el-card shadow="hover" class="info-card">
                    <div class="video-cover">
                        <img :src="resource.previewUrl || defaultCoverImage"
                             alt="视频封面"
                             @error="e => { e.currentTarget.src = defaultCoverImage }" />
                    </div>
                    <div class="resource-meta">
                        <h2 class="resource-title">{{ resource.title }}</h2>
                        <div class="meta-tags">
                            <el-tag type="success">{{ formatType(resource.skillType) }}</el-tag>
                            <el-tag type="info">视频</el-tag>
                            <el-tag type="warning">{{ resource.examType }}</el-tag>
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
                    <h3>课程内容</h3>
                    <div class="video-container">
                        <video ref="videoRef"
                               class="video-player"
                               controls
                               controlsList="nodownload"
                               :poster="resource.previewUrl || defaultCoverImage"
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
            <el-button @click="goBack">返回</el-button>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, onMounted } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import api from '../utils/axios'
    import axios from 'axios';

    export default defineComponent({
        name: 'ResourcesVideo',
        setup() {
            const route = useRoute();
            const router = useRouter();

            const defaultCoverImage = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTlq4b9QpuZJYMIq_Z8gksyR-NtNf19aF9isw&s';
            const resource = ref(JSON.parse(route.query.resourceData as string || '{}'));

            // 广告相关状态
            const showAdDialog = ref(false);
            const canCloseAd = ref(false);
            const countdown = ref(5);
            let timer: number | null = null;

            const adData = ref({
                id: "",
                title: "",
                description: "",
                skillType: "",
                examType: "",
                materialType: "",
                price: 0,
                previewUrl: "",
                url: ""
            });

            onMounted(async () => {
                // 获取广告列表
                const res = await api.get('/api/ads/available-ad');
                const adDatas = res.data.ad;

                console.log('全部广告：', adDatas);

                if (adDatas && adDatas.length > 0) {
                    const randomIndex = Math.floor(Math.random() * adDatas.length);
                    const temp_id = adDatas[randomIndex].targetId
                    const selectedAdRes = await api.get('/api/ads/available-material', { params: { id: temp_id } });
                    const selectedAd = selectedAdRes.data.material?.[0]; 

                    console.log('选中的广告：', selectedAd);

                    if (selectedAd) {
                        adData.value = {
                            id: selectedAd.id,
                            title: selectedAd.title,
                            description: selectedAd.description,
                            skillType: selectedAd.skillType,
                            examType: selectedAd.examType,
                            materialType: selectedAd.materialType,
                            price: selectedAd.price,
                            previewUrl: selectedAd.previewUrl && selectedAd.previewUrl.startsWith("http")
                                ? selectedAd.previewUrl
                                : defaultCoverImage,
                            url: selectedAd.url
                        };
                    }

                }
            });

            // 检查用户是否为VIP
            const isVip = async () => {
                const stored = JSON.parse(localStorage.getItem('user') || '{}'); 
                const res = await api.get('/api/paying/category', { params: { userId: stored.Id } });
                // 判断 category 是否为 VIP
                console.log('用户类型：', res.data.category);
                return res.data.category === 'VIP';
            };


            const startCountdown = () => {
                timer = window.setInterval(() => {
                    countdown.value -= 1;
                    if (countdown.value <= 0) {
                        canCloseAd.value = true;
                        if (timer) {
                            clearInterval(timer);
                            timer = null;
                        }
                    }
                }, 1000);
            };

            // 跳转到广告资源页面
            const goToAdResource = () => {
                if (!adData.value) return;

                // 判断广告资源类型
                if (adData.value.materialType === '视频' || adData.value.materialType === 'video') {
                    console.log('跳转到视频详情页面');
                    router.push({
                        name: 'ResourcesVideo',
                        query: {
                            resourceData: JSON.stringify({
                                id: adData.value.id,
                                title: adData.value.title,
                                description: adData.value.description,
                                type: adData.value.skillType,
                                level: adData.value.examType,
                                previewImage: adData.value.previewUrl,
                                price: adData.value.price,
                                url: adData.value.url
                            })
                        }
                    });
                } else if (adData.value.materialType === '文章' || adData.value.materialType === 'document') {
                    console.log('跳转到文章详情页面');
                    router.push({
                        name: 'ResourcesDocument',
                        query: {
                            resourceData: JSON.stringify({
                                id: adData.value.id,
                                title: adData.value.title,
                                description: adData.value.description,
                                type: adData.value.skillType,
                                level: adData.value.examType,
                                previewImage: adData.value.previewUrl,
                                price: adData.value.price,
                                url: adData.value.url
                            })
                        }
                    });
                } else {
                    console.warn('未知资源类型，无法跳转');
                }
            };

            const showAdIfNeeded = async () => {
                const vip = await isVip();
                console.log("isVip 返回：", vip);
                if (!vip) {
                    showAdDialog.value = true;
                    startCountdown();
                }
            };

            const closeAd = () => {
                showAdDialog.value = false;
            };

            const handleAdClose = (done: () => void) => {
                if (canCloseAd.value) {
                    done();
                }
            };

            const formatType = (type: string) => {
                const map: Record<string, string> = {
                    听力: '听力',
                    口语: '口语',
                    阅读: '阅读',
                    写作: '写作'
                };
                return map[type?.trim()] || '其他';
            };


            const goBack = () => {
                router.push('/learningresources');
            };

            onMounted(() => {
                showAdIfNeeded();
            });

            return {
                resource,
                defaultCoverImage,
                formatType,
                goBack,
                showAdDialog,
                canCloseAd,
                countdown,
                closeAd,
                handleAdClose,
                adData,
                goToAdResource
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

    /* 广告弹窗样式 */
    .ad-content {
        text-align: center;
        padding: 20px;
    }

    /* 广告预览区域 */
    .ad-preview {
        display: flex;
        flex-direction: column;
        align-items: center;
        overflow: hidden; /* 防止内容溢出 */
        max-width: 100%;
    }

    /* 广告图片 */
    .ad-image {
        width: 100%; /* 图片宽度不超过父元素 */
        height: auto; /* 高度自适应 */
        max-height: 300px; /* 限制图片最大高度 */
        object-fit: contain; /* 保持比例且不裁剪 */
        border-radius: 4px;
        margin-bottom: 15px;
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
</style>