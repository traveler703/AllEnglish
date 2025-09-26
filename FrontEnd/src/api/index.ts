
import axios, { type AxiosInstance } from 'axios';
import {
    getArticles,
    getArticleById,
    submitArticleAnswers,
    type ArticleParams,
    type Article,
    type ArticleDetail,
    type ArticleResponse,
    type Question,
    type SubmitAnswersRequest,
    getUserCompletedArticlesCount,
    getUserArticleHighestScore
} from './article';

// 创建 Axios 实例，设置基础 URL
const apiClient: AxiosInstance = axios.create({
    baseURL: '/api', 
    headers: {
        'Content-Type': 'application/json'
    }
});
// 导出API函数和类型定义，使其可在其他组件中使用
export {
    getArticles,
    getArticleById,
    submitArticleAnswers,
    getUserCompletedArticlesCount,
    getUserArticleHighestScore,
    type Article,
    type ArticleDetail,
    type ArticleParams,
    type Question,
    type ArticleResponse,
    type SubmitAnswersRequest
};

export function getCoinBalance(userId: string) {
    return apiClient.get('/paying/coin-balance', { params: { userId } });
}

export function dailySignIn(userId: string) {
    return apiClient.post('/paying/daily-signin', { userId });
}

// 资源购买相关API
export function purchaseMaterial(userId: string, materialId: string) {
    return apiClient.post('/paying/purchase-material', { userId, materialId });
}

export function getUserInventory(userId: string) {
    return apiClient.get('/paying/user-inventory', { params: { userId } });
}

export function checkMaterialInInventory(userId: string, materialId: string) {
    return apiClient.get('/paying/check-inventory', { params: { userId, materialId } });
}

export function getAvailableMaterials() {
    return apiClient.get('/paying/available-materials');
}

export default apiClient;

