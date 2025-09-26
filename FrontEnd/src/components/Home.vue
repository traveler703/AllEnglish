<template>
  <div class="home">
    <!-- 用户信息横幅 -->
    <div class="user-banner" v-if="user">
      <el-avatar :src="user.avatarUrl" size="large" />
      <div class="user-info">
        <div class="user-name">{{ user.userName }}</div>
        <div class="user-stats">
          <span>Lv.{{ user.level }}</span>
          <span>{{ user.coins }} 金币</span>
        </div>
      </div>
    </div>

    <!-- 主视觉（Hero） -->
    <section class="hero">
      <div class="hero__bg">
        <span class="blob blob--1"></span>
        <span class="blob blob--2"></span>
        <span class="blob blob--3"></span>
      </div>

      <div class="hero__content">
        <div class="brand">
          <img src="/ALogo.png" alt="AllEn Logo" class="brand__logo" />
        </div>
        <h1 class="hero__title">
          AllEn, <span>ALL ENGLISH</span>
        </h1>
      </div>
    </section>

    <!-- 每日一句 -->
    <section class="daily-quote">
      <!-- 用户欢迎信息 -->
      <div class="user-welcome" v-if="user">
        欢迎回来，{{ user.userName }}！今日学习目标已完成 <el-progress :percentage="65" :show-text="false" />
      </div>
      
      <el-card class="quote-card" shadow="hover">
        <div class="quote-header">
          <span class="quote-title">
            <span class="quote-icon">💡</span>
            每日一句
          </span>
          <el-button 
            text 
            size="small" 
            @click="refreshQuote"
            :loading="isLoadingQuote"
            :disabled="isLoadingQuote"
            class="refresh-btn"
          >
            <span class="refresh-icon" :class="{ 'spinning': isLoadingQuote }">🔄</span>
            {{ isLoadingQuote ? '换句中' : '换一句' }}
          </el-button>
        </div>
        
        <div class="quote-content" :class="{ 'loading': isLoadingQuote }">
          <div class="quote-text">{{ dailyQuote.en }}</div>
          <div class="quote-divider"></div>
          <div class="quote-translation">{{ dailyQuote.cn }}</div>
        </div>
        
        <!-- loading 状态指示器 -->
        <div v-if="isLoadingQuote" class="quote-loading">
          <div class="loading-dots">
            <span></span>
            <span></span>
            <span></span>
          </div>
          <span>正在为你挑选新句子...</span>
        </div>
      </el-card>
    </section>

    <!-- 功能/课程 快速入口 -->
    <section class="quick">
      <h2 class="section-title">快速开始</h2>
      <el-row :gutter="16">
        <el-col v-for="item in quickEntries" :key="item.path" :xs="12" :sm="12" :md="6" :lg="6">
          <el-card class="quick-card" shadow="hover" @click="goModule(item.path)">
            <div class="quick-card__icon" v-html="item.icon"></div>
            <div class="quick-card__title">{{ item.title }}</div>
            <div class="quick-card__desc">{{ item.desc }}</div>
          </el-card>
        </el-col>
      </el-row>
    </section>

    <!-- 推荐内容（轮播） -->
    <section class="recommend">
      <h2 class="section-title">为你推荐</h2>
      <el-carousel height="220px" arrow="always" indicator-position="outside" :interval="4500">
        <el-carousel-item v-for="rec in recommends" :key="rec.id">
          <div class="rec-item" @click="goModule(rec.to)">
            <div class="rec-item__left">
              <h3>{{ rec.title }}</h3>
              <p>{{ rec.subtitle }}</p>
              <div class="rec-item__tags">
                <el-tag v-for="t in rec.tags" :key="t" size="small" effect="dark" round>{{ t }}</el-tag>
              </div>
            </div>
            <div class="rec-item__right">
              <img :src="rec.cover" alt="cover" />
            </div>
          </div>
        </el-carousel-item>
      </el-carousel>
    </section>

    <!-- 热门课程 -->
    <section class="popular">
      <h2 class="section-title">热门课程</h2>
      <el-row :gutter="16">
        <el-col v-for="c in popularCourses" :key="c.id" :xs="24" :sm="24" :md="24" :lg="24">
          <el-card class="course-card" shadow="hover">
            <div class="course-card__cover">
              <img :src="c.cover" alt="course" />
              <el-tag class="course-card__badge" type="danger" effect="dark" round>HOT</el-tag>
            </div>
            <div class="course-card__body">
              <h3 class="course-card__title">{{ c.title }}</h3>
              <p class="course-card__desc">{{ c.desc }}</p>
              <div class="course-card__meta">
                <el-rate :model-value="c.rate" disabled allow-half />
                <span class="course-card__meta__hours">{{ c.hours }}h</span>
              </div>
              <div class="course-card__actions">
                <el-button type="primary" @click="goModule('course/'+c.id)">开始学习</el-button>
                <el-button plain @click="goModule('preview/'+c.id)">试听</el-button>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </section>

    <!-- API 示例/后端连通区 -->
    <section class="backend">
      <el-card shadow="never" class="backend-card">
        <div class="backend-row">
          <div id="welcome">Welcome to AllEn</div>
          <el-button type="info" @click="callBackend">调用后端</el-button>
        </div>
        <p v-if="backendMsg" class="backend-msg">{{ backendMsg }}</p>
      </el-card>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios' 
import { Loading } from '@element-plus/icons-vue'

// 定义后端返回数据的类型
interface BackendMaterial {
  id: string      
  title: string   
  skillType: string
  examType: string
  price: number
  previewUrl: string
}

interface BackendResponse {
  hot: BackendMaterial[];    
  forYou: BackendMaterial[]; 
}

export default defineComponent({
  name: 'Home',
  setup() {
    const backendMsg = ref<string>('')
    const router = useRouter()
    const isLoading = ref(false)
    const API_BASE = 'https://localhost:7071'
    
    // 从本地存储获取用户信息
    const user = computed(() => {
      const userData = localStorage.getItem('user')
      return userData ? JSON.parse(userData) : null
    })

    // 获取认证token
    const getAuthToken = () => {
      return localStorage.getItem('token') || '';
    }

    // 路由跳转方法
    const goModule = (path: string) => {
      router.push(path)
    }

    // 调用后端示例方法 - 使用axios重写
    const callBackend = async () => {
      try {
        const token = getAuthToken();
        const response = await axios.get('/api/english/hello', {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        
        backendMsg.value = response.data.message;
      } catch (error) {
        console.error('调用后端出错：', error)
        backendMsg.value = '调用后端出错'
      }
    }

    // 每日一句（保持不变）
    const dailyQuote = ref({
      en: "Loading...",
      cn: "加载中..."
    })

    const isLoadingQuote = ref(false)
    const currentQuoteIndex = ref(0)

    // 精选中英文对照句子库（30个）
    const quotesLibrary = [
      {
        en: "The best way to predict the future is to create it.",
        cn: "预测未来的最好方式就是创造它。"
      },
      {
        en: "Success is not final, failure is not fatal: it is the courage to continue that counts.",
        cn: "成功不是终点，失败也不是致命的：重要的是继续前进的勇气。"
      },
      {
        en: "The future belongs to those who believe in the beauty of their dreams.",
        cn: "未来属于那些相信梦想之美的人。"
      },
      {
        en: "It is during our darkest moments that we must focus to see the light.",
        cn: "在我们最黑暗的时刻，我们必须专注于寻找光明。"
      },
      {
        en: "The way to get started is to quit talking and begin doing.",
        cn: "开始的方法就是停止空谈，开始行动。"
      },
      {
        en: "Don't let yesterday take up too much of today.",
        cn: "不要让昨天占据今天太多的时间。"
      },
      {
        en: "You learn more from failure than from success.",
        cn: "你从失败中学到的比从成功中学到的更多。"
      },
      {
        en: "If you are working on something that you really care about, you don't have to be pushed.",
        cn: "如果你正在做你真正关心的事情，你不需要被推动。"
      },
      {
        en: "Life is what happens when you're busy making other plans.",
        cn: "生活就是在你忙于制定其他计划时发生的事情。"
      },
      {
        en: "The only impossible journey is the one you never begin.",
        cn: "唯一不可能的旅程是你从未开始的那一个。"
      },
      {
        en: "In the middle of difficulty lies opportunity.",
        cn: "在困难的中间蕴藏着机会。"
      },
      {
        en: "Everything you've ever wanted is on the other side of fear.",
        cn: "你想要的一切都在恐惧的另一边。"
      },
      {
        en: "Believe you can and you're halfway there.",
        cn: "相信你能做到，你就已经成功了一半。"
      },
      {
        en: "The only person you are destined to become is the person you decide to be.",
        cn: "你注定成为的唯一的人，就是你决定成为的人。"
      },
      {
        en: "What lies behind us and what lies before us are tiny matters compared to what lies within us.",
        cn: "我们身后和面前的事物与我们内心的力量相比，都是微不足道的。"
      },
      {
        en: "Do not go where the path may lead, go instead where there is no path and leave a trail.",
        cn: "不要走别人走过的路，而要开辟新路，留下足迹。"
      },
      {
        en: "The greatest glory in living lies not in never falling, but in rising every time we fall.",
        cn: "生命中最大的荣耀不在于从未跌倒，而在于每次跌倒后都能重新站起来。"
      },
      {
        en: "Education is the most powerful weapon which you can use to change the world.",
        cn: "教育是你可以用来改变世界的最强大的武器。"
      },
      {
        en: "The mind is everything. What you think you become.",
        cn: "思想决定一切。你想什么，就会成为什么。"
      },
      {
        en: "Twenty years from now you will be more disappointed by the things you didn't do than by the ones you did do.",
        cn: "二十年后，你会为那些你没有做的事情感到更失望，而不是你做过的事情。"
      },
      {
        en: "A person who never made a mistake never tried anything new.",
        cn: "从不犯错误的人从来不会尝试任何新东西。"
      },
      {
        en: "The journey of a thousand miles begins with one step.",
        cn: "千里之行，始于足下。"
      },
      {
        en: "It does not matter how slowly you go as long as you do not stop.",
        cn: "只要你不停下来，走得多慢都没关系。"
      },
      {
        en: "Our greatest weakness lies in giving up. The most certain way to succeed is always to try just one more time.",
        cn: "我们最大的弱点在于放弃。成功最可靠的方法就是再试一次。"
      },
      {
        en: "Yesterday is history, tomorrow is a mystery, today is a gift of God, which is why we call it the present.",
        cn: "昨天是历史，明天是谜团，今天是上帝的礼物，这就是为什么我们称之为现在。"
      },
      {
        en: "The difference between ordinary and extraordinary is that little extra.",
        cn: "平凡与非凡的区别在于那一点点额外的努力。"
      },
      {
        en: "Innovation distinguishes between a leader and a follower.",
        cn: "创新区分了领导者和追随者。"
      },
      {
        en: "Your limitation—it's only your imagination.",
        cn: "你的局限——它只存在于你的想象中。"
      },
      {
        en: "Push yourself, because no one else is going to do it for you.",
        cn: "推动自己前进，因为没有人会为你这样做。"
      },
      {
        en: "Great things never come from comfort zones.",
        cn: "伟大的事物从来不会来自舒适区。"
      }
    ]

    // 快速入口（保持不变）
    const quickEntries = ref([
      { 
        path: '/word_learning', 
        title: '词汇挑战', 
        desc: '生词学习+错题本', 
        icon: icons.word 
      },
      { 
        path: '/read_learning', 
        title: '阅读理解', 
        desc: '各种主题和难度挑战', 
        icon: icons.grammar 
      },
      { 
        path: '/listening_practice', 
        title: '听力训练', 
        desc: 'CET 真题练习', 
        icon: icons.listening 
      },
      { 
        path: '/adventure', 
        title: 'AI 口语实战', 
        desc: 'AI 对话实时反馈', 
        icon: icons.speaking 
      }
    ])

    // 动态数据：从后端获取
    const recommends = ref<any[]>([])
    const popularCourses = ref<any[]>([])

    // 获取随机句子索引（避免连续重复）
    const getRandomQuoteIndex = (excludeIndex?: number): number => {
      let randomIndex: number
      do {
        randomIndex = Math.floor(Math.random() * quotesLibrary.length)
      } while (randomIndex === excludeIndex && quotesLibrary.length > 1)
      
      return randomIndex
    }

    // 设置每日一句
    const setDailyQuote = (index: number) => {
      currentQuoteIndex.value = index
      dailyQuote.value = quotesLibrary[index]
    }

    // 获取今日一句（带日期缓存）
    const getTodayQuote = () => {
      const today = new Date().toDateString()
      const cachedDate = localStorage.getItem('dailyQuote_date')
      const cachedIndex = localStorage.getItem('dailyQuote_index')
      
      // 如果是同一天且有缓存，使用缓存的索引
      if (cachedDate === today && cachedIndex !== null) {
        const index = parseInt(cachedIndex, 10)
        if (index >= 0 && index < quotesLibrary.length) {
          setDailyQuote(index)
          return
        }
      }
      
      // 否则生成新的随机句子
      const randomIndex = getRandomQuoteIndex()
      setDailyQuote(randomIndex)
      
      // 缓存今日句子
      localStorage.setItem('dailyQuote_date', today)
      localStorage.setItem('dailyQuote_index', randomIndex.toString())
    }

    // 刷新句子（换一句功能）
    const refreshQuote = async () => {
      isLoadingQuote.value = true
      
      // 随机延迟 800-2000ms，增加真实感
      const randomDelay = Math.random() * 400 + 500
      
      await new Promise(resolve => setTimeout(resolve, randomDelay))
      
      try {
        // 获取新的随机句子（避免重复当前句子）
        const newIndex = getRandomQuoteIndex(currentQuoteIndex.value)
        setDailyQuote(newIndex)
        
        // 更新缓存（这样"换一句"后的句子会成为今天的句子）
        const today = new Date().toDateString()
        localStorage.setItem('dailyQuote_date', today)
        localStorage.setItem('dailyQuote_index', newIndex.toString())
        
      } catch (error) {
        console.error('刷新句子失败:', error)
        // 即使出错也要显示一个句子
        const fallbackIndex = getRandomQuoteIndex(currentQuoteIndex.value)
        setDailyQuote(fallbackIndex)
      } finally {
        isLoadingQuote.value = false
      }
    }

    // 初始化每日一句
    const initDailyQuote = () => {
      // 先显示加载状态
      dailyQuote.value = {
        en: "Loading daily inspiration...",
        cn: "加载每日灵感..."
      }
      
      // 模拟短暂加载时间，让用户感知到内容是"新鲜"的
      setTimeout(() => {
        getTodayQuote()
      }, 300)
    }

    // 获取句子库统计信息（可选，用于调试或展示）
    const getQuoteStats = () => {
      return {
        total: quotesLibrary.length,
        current: currentQuoteIndex.value + 1,
        percentage: Math.round(((currentQuoteIndex.value + 1) / quotesLibrary.length) * 100)
      }
    }


    // 获取首页卡片数据
    const fetchHomeCards = async () => {
      isLoading.value = true;
      try {
        const userId = user.value?.Id;
        if (!userId) {
          throw new Error('用户ID不存在');
        }

        const token = getAuthToken();
        const response = await axios.get(
          `${API_BASE}/api/home/home/cards/${userId}`,
          { headers: { Authorization: `Bearer ${token}` } }
        );
        
        console.log("API响应数据:", response.data); // 调试用
        
        const backendData: BackendResponse = response.data;

        
        popularCourses.value = (backendData?.hot || []).map(item => ({
          id: item.id, 
          title: item.title, 
          desc: `${item.skillType}精品课程`,
          hours: Math.floor(Math.random() * 10) + 1,
          rate: 4.5 + Math.random() * 0.5,
          cover: item.previewUrl || `https://picsum.photos/seed/${item.id}/600/360`
        }));

        
        recommends.value = (backendData?.forYou || []).map(item => ({
          id: item.id, 
          title: item.title, 
          subtitle: `${item.examType}精选课程`,
          tags: [item.skillType],
          to: `pathway/${item.id}`,
          cover: item.previewUrl || `https://picsum.photos/seed/${item.id}/540/300`
        }));

      } catch (error) {
        console.error('获取首页卡片数据失败:', error);
        popularCourses.value = fallbackPopularCourses;
        recommends.value = fallbackRecommends;
      } finally {
        isLoading.value = false;
      }
    }

    // 默认数据（当后端请求失败时使用）
    const fallbackPopularCourses = [
      {
        id: 'c101',
        title: '日常口语 100 句',
        desc: '从问候寒暄到出行购物，一网打尽',
        hours: 8, rate: 4.5,
        cover: 'https://picsum.photos/seed/allen4/600/360'
      },
      {
        id: 'c102',
        title: '听力场景特训 · 旅行篇',
        desc: '机场/酒店/交通全覆盖，配套听写',
        hours: 6, rate: 4.6,
        cover: 'https://picsum.photos/seed/allen5/600/360'
      },
      {
        id: 'c103',
        title: '雅思写作 Task2',
        desc: '高分结构与常见话题模板',
        hours: 10, rate: 4.7,
        cover: 'https://picsum.photos/seed/allen6/600/360'
      },
      {
        id: 'c104',
        title: '商务邮件高效写作',
        desc: '正式、得体、清晰的英文邮件',
        hours: 5, rate: 4.4,
        cover: 'https://picsum.photos/seed/allen7/600/360'
      }
    ]

    const fallbackRecommends = [
      {
        id: 'rec1',
        title: '零基础也能开口说',
        subtitle: '14 天口语启程 · 每天 15 分钟',
        tags: ['口语', '打卡', '初级'],
        to: 'pathway/speaking',
        cover: 'https://picsum.photos/seed/allen1/540/300'
      },
      {
        id: 'rec2',
        title: '雅思 6.5 直通车',
        subtitle: '词汇+听说读写全链路提分',
        tags: ['IELTS', '进阶'],
        to: 'pathway/ielts',
        cover: 'https://picsum.photos/seed/allen2/540/300'
      },
      {
        id: 'rec3',
        title: '商务英语速成',
        subtitle: '邮件沟通 + 会议表达 + 演示汇报',
        tags: ['Business', '口语'],
        to: 'pathway/business',
        cover: 'https://picsum.photos/seed/allen3/540/300'
      }
    ]

    onMounted(() => {
      // 进入动画
      requestAnimationFrame(() => document.documentElement.classList.add('allen-ready'))
      
      // 初始化每日一句
      initDailyQuote()

      // 获取首页卡片数据
      fetchHomeCards()
    })

    return {
      user,
      goModule,
      callBackend,
      backendMsg,
      quickEntries,
      recommends,
      popularCourses,
      dailyQuote,
      isLoadingQuote,
      refreshQuote, // 导出刷新函数供模板使用
      getQuoteStats 
    }
  }
})

// SVG 图标（保持不变）
const icons = {
  word: `<svg width="40" height="40" viewBox="0 0 24 24" fill="none">
    <rect x="3" y="4" width="18" height="16" rx="2" stroke="currentColor" stroke-width="1.5"/>
    <path d="M7 8h10M7 12h6M7 16h8" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
  </svg>`,
  grammar: `<svg width="40" height="40" viewBox="0 0 24 24" fill="none">
    <circle cx="8" cy="8" r="3.5" stroke="currentColor" stroke-width="1.5"/>
    <circle cx="16" cy="16" r="3.5" stroke="currentColor" stroke-width="1.5"/>
    <path d="M10.5 10.5l3 3" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
  </svg>`,
  listening: `<svg width="40" height="40" viewBox="0 0 24 24" fill="none">
    <rect x="5" y="4" width="14" height="16" rx="7" stroke="currentColor" stroke-width="1.5"/>
    <path d="M3 12h2M19 12h2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
  </svg>`,
  speaking: `<svg width="40" height="40" viewBox="0 0 24 24" fill="none">
    <path d="M7 10a5 5 0 1010 0" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
    <rect x="9" y="4" width="6" height="8" rx="3" stroke="currentColor" stroke-width="1.5"/>
    <path d="M12 14v4M9 18h6" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
  </svg>`
}
</script>

<style scoped>
/* 主题色 */
:root,
:host {
  --allen-pink: #ff66b3;
  --allen-blue: #4e7fff;
  --allen-blue-2: #7aa8ff;
  --allen-bg: #f7f8ff;
  --glass: rgba(255,255,255,0.6);
  --text-1: #1f2d3d;
  --text-2: #606266;
}

.home { background: var(--allen-bg); }

/* 用户信息横幅 */
.user-banner {
  display: flex;
  align-items: center;
  padding: 16px 24px;
  background: linear-gradient(135deg, var(--allen-pink) 0%, var(--allen-blue) 100%);
  color: white;
  border-radius: 16px;
  margin-bottom: 20px;
  box-shadow: 0 8px 24px rgba(78, 127, 255, 0.25);
}

.user-info {
  margin-left: 16px;
}

.user-name {
  font-size: 18px;
  font-weight: 700;
}

.user-stats {
  display: flex;
  gap: 16px;
  margin-top: 6px;
  font-size: 14px;
}

.user-stats span {
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 10px;
  border-radius: 12px;
}

/* 用户欢迎信息 */
.user-welcome {
  background: rgba(255, 255, 255, 0.85);
  border-radius: 16px;
  padding: 12px 16px;
  margin-bottom: 12px;
  font-size: 14px;
  color: var(--text-1);
  backdrop-filter: blur(4px);
}

.user-welcome .el-progress {
  margin-top: 8px;
}

/* Section 标题 */
.section-title{
  font-size: 24px;
  font-weight: 800;
  color: var(--text-1);
  margin: 12px 0 18px;
  letter-spacing: .2px;
}

/*Hero */
.hero{
  position: relative;
  overflow: hidden;
  border-radius: 24px;
  margin: 0 auto 24px;
  padding: clamp(18px, 6vw, 28px) clamp(10px, 3vw, 28px);
  background: linear-gradient(135deg, var(--allen-pink) 0%, var(--allen-blue) 100%);
  color: black;
  box-shadow: 0 15px 40px rgba(78,127,255,0.25);
}

.hero__bg .blob{
  position: absolute;
  filter: blur(30px);
  opacity: .45;
  pointer-events: none;
  transform: scale(0.9);
  transition: transform .8s ease;
}

.blob--1{
  width: 280px; height: 280px; border-radius: 50%;
  background: radial-gradient(closest-side, #fff, transparent);
  top: -60px; left: -60px;
}
.blob--2{
  width: 320px; height: 320px; border-radius: 50%;
  background: radial-gradient(closest-side, #ffe0f1, transparent);
  right: -80px; bottom: -80px;
}
.blob--3{
  width: 220px; height: 220px; border-radius: 50%;
  background: radial-gradient(closest-side, #d6e3ff, transparent);
  left: 40%; top: -40px;
}

/* 进入动画（在 onMounted 添加 allen-ready 类名触发） */
:global(.allen-ready) .blob { transform: scale(1); }

.hero__content{
  position: relative;
  z-index: 1;
  text-align: center;
}

.brand__logo {
  height: 60px; 
  object-fit: contain;
  display: block;
  margin: 0 auto; 
}

.hero__title{
  margin: 12px 0 8px;
  font-size: clamp(28px, 5.2vw, 56px);
  font-weight: 900;
  letter-spacing: .4px;
}
.hero__title span { 
  background: linear-gradient(90deg, #ffb3c6, #ff4d94);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

/* 每日一句 */
.daily-quote {
  margin: 12px auto 20px;
}

.quote-card {
  text-align: center;
  padding: 20px;
  border-radius: 18px;
  background: rgba(255,255,255,0.7);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255,255,255,0.4);
}

.quote-text {
  font-size: 14px;
  color: var(--text-2);
}

.quote-translation {
  font-size: 14px;
  color: var(--text-2);
}


/* 快速开始 */
.quick{
  padding: 4px 2px 8px;
}
.quick-card{
  cursor: pointer;
  border-radius: 18px;
  background: linear-gradient(180deg, #ffffffaa, #ffffff 70%);
  backdrop-filter: blur(6px);
  border: 1px solid #eef1ff;
  transition: transform .25s ease, box-shadow .25s ease;
  text-align: center;
  min-height: 200px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}
.quick-card:hover{
  transform: translateY(-6px);
  box-shadow: 0 20px 40px rgba(78,127,255,0.2);
}
.quick-card__icon{
  color: var(--allen-blue);
  margin: 8px auto 6px;
}
.quick-card__title{
  font-size: 24px; font-weight: 700; color: var(--text-1); margin-top: 2px;
  padding-bottom: 6px;
}
.quick-card__desc{
  font-size: 13px; color: var(--text-2); margin-top: 4px;
}


/* 推荐 */
.recommend{ margin-top: 10px; }
.rec-item{
  display: grid; grid-template-columns: 1.2fr .8fr; gap: 12px;
  background: linear-gradient(135deg, #fff, #f7f8ff);
  border-radius: 18px; height: 100%; overflow: hidden; cursor: pointer;
  border: 1px solid #eef1ff;
}
.rec-item__left{ padding: 18px 18px; display:flex; flex-direction:column; justify-content:center; }
.rec-item__left h3{ font-size: 20px; font-weight: 800; margin: 4px 0; color: var(--text-1);}
.rec-item__left p{ color: var(--text-2); margin-bottom: 8px;}
.rec-item__tags .el-tag{ margin-right: 6px; background: linear-gradient(90deg, var(--allen-pink), var(--allen-blue)); border: none;}
.rec-item__right{ position: relative; }
.rec-item__right img{ width: 100%; height: 100%; object-fit: cover; }


/* 热门课程 */
.popular{ margin-top: 8px; }
.course-card{
  border-radius: 16px; overflow: hidden;
  transition: transform .25s ease, box-shadow .25s ease;
}
.course-card:hover{
  transform: translateY(-6px); box-shadow: 0 16px 36px rgba(0,0,0,0.08);
}
.course-card__cover{ position: relative; height: 160px; overflow: hidden; }
.course-card__cover img{ width: 100%; height: 100%; object-fit: cover; }
.course-card__badge{
  position: absolute; top: 10px; right: 10px;
  background: linear-gradient(90deg, var(--allen-pink), var(--allen-blue));
  border: none;
}
.course-card__body{ padding-top: 10px; }
.course-card__title{ font-size: 16px; font-weight: 800; margin-bottom: 4px; color: var(--text-1); }
.course-card__desc{ color: var(--text-2); min-height: 36px; }
.course-card__meta{
  display:flex; align-items:center; justify-content: space-between; margin: 8px 0 6px;
}
.course-card__meta__hours{ font-size: 12px; color: var(--text-2); }
.course-card__actions{ display:flex; gap: 8px; }

/* ========== Backend area (保留) ========== */
.backend{ margin-top: 18px; }
.backend-card{ border-radius: 16px; }
.backend-row{
  display:flex; align-items:center; justify-content: space-between; gap: 12px;
}
#welcome{
  font-size: clamp(22px, 3.6vw, 32px);
  font-weight: 900;
  color: var(--allen-pink);
}
.backend-msg{ color: var(--text-2); }

/* 响应式 */
@media (max-width: 768px){
  .rec-item{ grid-template-columns: 1fr; }
  .hero__stats{ grid-template-columns: repeat(3, 1fr); }
  .user-banner {
    padding: 12px 16px;
  }
  .quick-card {
    min-height: 180px;
  }
}

/* 每日一句样式 - 添加到原有样式中 */
.daily-quote {
  margin: 12px auto 20px;
}

.quote-card {
  border-radius: 18px;
  background: rgba(255,255,255,0.7);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255,255,255,0.4);
  transition: all 0.3s ease;
  overflow: hidden;
}

.quote-card:hover {
  background: rgba(255,255,255,0.85);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(78, 127, 255, 0.15);
}

.quote-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 8px;
  border-bottom: 1px solid rgba(78, 127, 255, 0.1);
}

.quote-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--allen-blue);
  display: flex;
  align-items: center;
  gap: 8px;
}

.quote-icon {
  font-size: 18px;
  animation: glow 2s ease-in-out infinite alternate;
}

.quote-counter {
  font-size: 12px;
  color: var(--text-2);
  font-weight: 400;
  margin-left: 4px;
}

.refresh-btn {
  color: var(--allen-pink) !important;
  font-size: 14px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 4px;
}

.refresh-btn:hover:not(:disabled) {
  background: linear-gradient(90deg, var(--allen-pink), var(--allen-blue));
  color: white !important;
  border-radius: 12px;
  transform: scale(1.05);
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.refresh-icon {
  display: inline-block;
  transition: transform 0.3s ease;
}

.refresh-icon.spinning {
  animation: spin 1s linear infinite;
}

.quote-content {
  text-align: center;
  padding: 8px 0;
  transition: all 0.4s ease;
}

.quote-content.loading {
  opacity: 0.5;
  transform: translateY(10px);
}

.quote-text {
  font-size: 16px;
  font-weight: 500;
  color: var(--text-1);
  line-height: 1.6;
  margin-bottom: 12px;
  font-style: italic;
  transition: all 0.4s ease;
  position: relative;
}

.quote-text::before {
  content: '"';
  position: absolute;
  left: -16px;
  top: -8px;
  font-size: 32px;
  color: var(--allen-pink);
  opacity: 0.3;
  font-family: serif;
}

.quote-text::after {
  content: '"';
  position: absolute;
  right: -16px;
  bottom: -8px;
  font-size: 32px;
  color: var(--allen-pink);
  opacity: 0.3;
  font-family: serif;
}

.quote-divider {
  width: 60px;
  height: 2px;
  background: linear-gradient(90deg, var(--allen-pink), var(--allen-blue));
  margin: 16px auto;
  border-radius: 1px;
  opacity: 0.6;
}

.quote-translation {
  font-size: 14px;
  color: var(--text-2);
  line-height: 1.5;
  margin-top: 8px;
  transition: all 0.4s ease;
}

.quote-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-top: 12px;
  color: var(--allen-blue);
  font-size: 14px;
}

.loading-dots {
  display: flex;
  gap: 4px;
}

.loading-dots span {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--allen-pink);
  animation: bounce 1.4s ease-in-out infinite both;
}

.loading-dots span:nth-child(1) { animation-delay: -0.32s; }
.loading-dots span:nth-child(2) { animation-delay: -0.16s; }
.loading-dots span:nth-child(3) { animation-delay: 0s; }

/* 响应式调整 */
@media (max-width: 768px) {
  .quote-header {
    flex-direction: column;
    gap: 8px;
    align-items: flex-start;
  }
  
  .refresh-btn {
    align-self: flex-end;
  }
  
  .quote-text {
    font-size: 15px;
    margin: 0 20px 12px 20px;
  }
  
  .quote-translation {
    font-size: 13px;
  }
  
  .quote-text::before,
  .quote-text::after {
    display: none;
  }
}

/* 动画定义 */
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes bounce {
  0%, 80%, 100% { 
    transform: scale(0);
    opacity: 0.5;
  } 
  40% { 
    transform: scale(1);
    opacity: 1;
  }
}

@keyframes glow {
  0% { opacity: 0.7; }
  100% { opacity: 1; }
}

@keyframes fadeIn {
  from { 
    opacity: 0;
    transform: translateY(20px);
  }
  to { 
    opacity: 1;
    transform: translateY(0);
  }
}
</style>