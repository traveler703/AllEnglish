import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import Home from '../components/Home.vue';
import WordLearning from '../views/WordLearning.vue';
import ReadLearning from '../views/ReadLearning.vue';
import OralPractice from '../views/OralPractice.vue';
import Login from '../views/Login.vue';
import Admin from '../views/Admin.vue'
import Register from '../views/Register.vue'
import UserCenter from '../views/UserCenter.vue'
import EditDocument from '../views/EditDocument.vue'
import ForgetPassword from '../views/ForgetPassword.vue'
import LearningResources from '../views/LearningResources.vue';
import ResourcesVideo from '../views/ResourcesVideo.vue';
import ResourcesDocument from '../views/ResourcesDocument.vue';
import ListeningPractice from '../views/ListeningPractice.vue';
import Leaderboard from '../views/Leaderboard.vue';
import MyFriendsPage from '../views/MyFriends.vue';
import MyResources from '../views/MyResources.vue';
import MyAchievements from '../views/MyAchievements.vue';
import StudyPlanSearch from '../views/StudyPlanSearch.vue'
import CustomStudyPlan from '../views/CustomStudyPlan.vue'
import MyStudyPlans from '../views/MyStudyPlans.vue' 
import StudyPlansView from '../views/StudyPlansView.vue' 
import AddNewStudyPlan from '../views/AddNewStudyPlan.vue';
import ResourcesVideoOwn from '../views/ResourcesVideoOwn.vue';
import ResourcesDocumentOwn from '../views/ResourcesDocumentOwn.vue';
import AdventurePath from '../views/AdventurePath.vue';
import CrossWord from '../views/Crossword.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Login',
    component: Login
  },
  {
    path: '/home',
    name: 'Home',
    component: Home
  },
  {
    path: '/word_learning',
    name: 'WordLearning',
    component: WordLearning
  },
  {
    path: '/read_learning',
    name: 'ReadLearning',
    component: ReadLearning
  },
  {
    path: '/oral_practice',
    name: 'OralPractice',
    component: OralPractice
  },
  {
    path: '/listening_practice',
    name: 'ListeningPractice',
    component: ListeningPractice
  },
  {
    path: '/admin',
    name: 'Admin',
    component: Admin,
  },
  {
    path: '/register',
    name: 'Register',
    component: Register
  },
  {
    path: '/user_center',
    name: 'UserCenter',
    component: UserCenter
  },
  {
    path: '/editdocument',
    name: 'Editdocument',
    component: EditDocument
  },
  {
    path: '/forgetpassword',
    name: 'ForgetPassword',
    component: ForgetPassword
  },
   {
        path: '/leaderboard',
        name: 'Leaderboard',
        component: Leaderboard
   
  },
  {
    path: '/learningresources',
    name: 'LearningResources',
    component: LearningResources
    },
  {
      path: '/learningresources/video',
      name: 'ResourcesVideo',
      component: ResourcesVideo 
    },
    {
        path: '/learningresources/document',
        name: 'ResourcesDocument',
        component: ResourcesDocument
    },
    {
        path: '/user/resources/video',
        name: 'ResourcesVideoOwn',
        component: ResourcesVideoOwn
    },
    {
        path: '/user/resources/document',
        name: 'ResourcesDocumentOwn',
        component: ResourcesDocumentOwn
    },
  {
    path: '/study-plans',
    name: 'StudyPlansView',
    component: StudyPlansView
  },
  {
    path: '/study-plans/my',
    name: 'MyStudyPlans',
    component: MyStudyPlans
  },
  {
    path: '/study-plans/add',
    name: 'AddNewStudyPlan',
    component: AddNewStudyPlan
  },
  {
    path: '/user/friends',
    name: 'MyFriends',
    component: MyFriendsPage
  },
  {
    path: '/user/resources',
    name: 'MyResources',
    component: MyResources
  },
  {
    path: '/user/achievements',
    name: 'MyAchievements',
    component: MyAchievements
  },
  {
    path: '/adventure',
    name: 'AdventurePath',
    component: AdventurePath
    },
  {
        path: '/crossword',
        name: 'CrossWord',
      component: CrossWord
    }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫 - 设置页面标题
router.beforeEach((to, from, next) => {
  console.log('路由跳转:', from.path, '->', to.path)
  
    if (typeof to.meta?.title === 'string') {
        document.title = to.meta.title
    }
  
  next()
})

export default router