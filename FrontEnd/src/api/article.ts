import axios from 'axios';

/**
 * 文章列表查询参数接口
 * 用于向后端API传递筛选和分页参数
 * 
 * 与后端接口参数对应关系:
 * - pageNumber: 页码，默认1
 * - pageSize: 每页条数，默认8
 * - courseId: 课程类别ID (1=基础课程, 2=进阶课程, 3=高级课程)
 * - difficulty: 难度等级ID (1=初级, 2=中级, 3=高级)
 * - category: 文章分类名称
 * - searchTerm: 搜索关键词，用于全文搜索
 */
export interface ArticleParams {
  pageNumber?: number;  // 当前页码，从1开始
  pageSize?: number;    // 每页显示数量
  courseId?: number;    // 课程类别ID：1=基础课程，2=进阶课程，3=高级课程
  difficulty?: number;  // 难度等级：1=初级，2=中级，3=高级
  category?: string;    // 文章分类
  searchTerm?: string;  // 搜索关键词
}

/**
 * 文章列表项数据接口
 * 用于列表页展示文章的基本信息
 * 
 * 与后端返回的文章列表项字段映射:
 * - articleId: 文章唯一标识符 (注意：不是id而是articleId)
 * - title: 文章标题
 * - content: 文章内容 (列表中通常不包含)
 * - difficulty: 难度级别编号
 * - difficultyLevel: 难度级别文本 (Beginner/Intermediate/Advanced)
 * - category: 文章分类
 * - description: 文章描述/摘要
 */
export interface Article {
  articleId: number;      // 文章ID，唯一标识 (注意：后端返回的是articleId而不是id)
  title: string;          // 文章标题
  content?: string;       // 文章内容(列表中可能不包含)
  difficulty: number;     // 难度等级ID
  difficultyLevel: string; // 难度等级文本描述
  category: string;       // 文章分类
  courseId?: string;      // 课程ID
  coverImage?: string;    // 封面图片URL
  readingTime?: number;   // 阅读时长(分钟)
  wordCount?: number;     // 文章字数
  description?: string;   // 文章简介/描述
}

/**
 * 问题数据接口
 * 文章中包含的练习题信息
 * 
 * 与后端练习题字段映射:
 * - id: 练习题唯一标识符
 * - articleId: 关联的文章ID
 * - seqo: 题目序号，用于显示顺序
 * - kind: 题目类型 (1=单选题, 2=填空题, 其他=待扩展)
 * - stem: 题干文本
 * - options: 选项内容 (单选题为选项列表，格式为A. xxx\nB. yyy\nC. zzz)
 * - answerKey: 正确答案 (单选题为选项字母，填空题为填空内容)
 */
export interface Question {
  id: number;           // 问题ID
  articleId: number;    // 所属文章ID
  seqo: string;         // 序号
  kind: number;         // 题目类型 (1=单选题, 2=填空题)
  stem: string;         // 题干内容
  options: string;      // 选项JSON
  answerKey: string;    // 答案
  score: number;        // 分数
  createdAt: string;    // 创建时间
}

/**
 * 文章详情数据接口
 * 继承自Article，包含完整的文章内容和额外信息
 * 
 * 与后端文章详情API返回的额外字段:
 * - content: 完整的文章内容HTML或文本
 * - createdAt: 文章创建时间
 * - updatedAt: 文章更新时间
 * - tags: 文章标签列表
 * - questions: 相关练习题列表
 */
export interface ArticleDetail extends Article {
  content: string;       // 详情中内容字段(必定存在)
  createdAt?: string;    // 创建时间
  updatedAt?: string;    // 更新时间
  tags?: string[];       // 文章标签
  questions?: Question[]; // 文章相关的练习题
  highestScore?: number;  // 文章最高分
}

/**
 * 文章列表响应数据接口
 * 包含列表数据和分页信息
 * 
 * 与后端返回的分页数据结构对应:
 * - items: 当前页的文章列表
 * - totalItems: 符合筛选条件的总文章数
 * - totalPages: 总页数
 * - currentPage: 当前页码
 * - pageSize: 每页大小
 * - hasPreviousPage/hasNextPage: 是否有上一页/下一页
 */
export interface ArticleResponse {
  items: Article[];          // 文章列表数据
  totalItems: number;        // 总文章数 (注意: 后端返回的是totalItems而不是totalCount)
  totalPages: number;        // 总页数
  currentPage: number;       // 当前页码
  pageSize: number;          // 每页数量
  hasPreviousPage: boolean;  // 是否有上一页
  hasNextPage: boolean;      // 是否有下一页
}

/**
 * 获取文章列表
 * 调用GET /api/Articles接口，支持分页和多条件筛选
 * 
 * @param params 查询参数，包含分页和筛选条件
 * @returns 包含文章列表和分页信息的响应对象
 */
export async function getArticles(params: ArticleParams): Promise<ArticleResponse> {
  try {
    console.log('准备请求文章列表，参数:', params);
    
    // 手动构建URL查询参数，避免编码问题
    const url = new URL('/api/Articles', window.location.origin);
    
    // 只添加有值的参数，避免发送undefined或空值
    // 注意将数字参数转换为字符串，因为URL参数总是字符串
    if (params.pageNumber !== undefined) url.searchParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize !== undefined) url.searchParams.append('pageSize', params.pageSize.toString());
    if (params.courseId) url.searchParams.append('courseId', params.courseId.toString());
    if (params.category) url.searchParams.append('category', params.category);
    if (params.difficulty) url.searchParams.append('difficulty', params.difficulty.toString());
    if (params.searchTerm) url.searchParams.append('searchTerm', params.searchTerm);
    
    console.log('请求URL:', url.toString());
    
    // 发送GET请求并设置正确的请求头
    const response = await axios.get(url.toString(), {
      headers: {
        'Content-Type': 'application/json; charset=UTF-8',
        'Accept': 'application/json'
      },
      // 添加超时设置
      timeout: 10000,
      // 允许跨域请求携带凭证
      withCredentials: true
    });
    
    console.log('API响应状态:', response.status);
    console.log('API响应头:', response.headers);
    console.log('API响应数据:', response.data);
    
    // 如果数据结构不匹配，尝试修复
    const data = response.data;
    if (data) {
      // 确保返回值符合ArticleResponse接口要求
      const result: ArticleResponse = {
        items: Array.isArray(data.items) ? data.items : [],
        totalItems: data.totalItems || 0,
        totalPages: data.totalPages || 1,
        currentPage: data.currentPage || 1,
        pageSize: data.pageSize || 8,
        hasPreviousPage: data.hasPreviousPage || false,
        hasNextPage: data.hasNextPage || false
      };
      
      // 清理每个文章对象中可能的问题数据
      result.items = result.items.map(item => ({
        ...item,
        // 确保category是字符串并去除多余的空白字符
        category: item.category?.trim() || '未分类',
        // 确保coverImage是有效的URL
        coverImage: item.coverImage || ''
      }));
      
      return result;
    }
    
    return response.data;
  } catch (error) {
    console.error('文章列表API请求错误:', error);
    // 返回空数据而不是抛出错误，这样UI不会崩溃
    return {
      items: [],
      totalItems: 0,
      totalPages: 0,
      currentPage: 1,
      pageSize: 8,
      hasPreviousPage: false,
      hasNextPage: false
    };
  }
}

/**
 * 根据ID获取文章详情
 * 调用GET /api/Articles/{id}/detail接口获取文章完整信息
 * 
 * @param id 文章ID
 * @returns 文章详情数据，包含完整内容、练习题等
 */
export async function getArticleById(id: number): Promise<ArticleDetail> {
  try {
    // 使用新的详情API地址格式
    // URL格式: /api/Articles/{id}/detail
    const response = await axios.get(`/api/Articles/${id}/detail`, {
      headers: {
        'Content-Type': 'application/json; charset=UTF-8',
        'Accept': 'application/json'
      }
    });
    return response.data;
  } catch (error) {
    console.error(`获取文章(ID:${id})详情失败:`, error);
    throw error;
  }
}

/**
 * 提交练习答案的请求参数接口
 */
export interface SubmitAnswersRequest {
  userId: number;
  articleID: number;
  startTime: string;
  endTime: string;
  answers: {
    questionId: number;
    userResponse: string;
  }[];
}

/**
 * 提交练习答案
 * 调用POST /api/Articles/submit接口提交用户答题记录
 * 
 * @param data 提交的答题数据
 * @returns 提交结果
 */
export async function submitArticleAnswers(data: SubmitAnswersRequest): Promise<any> {
  try {
    const response = await axios.post('/api/Articles/submit', data, {
      headers: {
        'Content-Type': 'application/json; charset=UTF-8'
      }
    });
    return response.data;
  } catch (error) {
    console.error('提交答案失败:', error);
    throw error;
  }
}

/**
 * 获取用户完成的文章数量
 * @param userId 用户ID，默认为0
 * @returns 包含completedCount字段的对象
 */
export async function getUserCompletedArticlesCount(userId: number = 0): Promise<{ completedCount: number }> {
  try {
    const response = await axios.get(`/api/Articles/user/${userId}/completed-articles/count`);
    return response.data;
  } catch (error) {
    console.error('获取用户完成文章数量失败:', error);
    return { completedCount: 0 };
  }
}

/**
 * 获取用户在特定文章的最高分
 * @param userId 用户ID，默认为0
 * @param articleId 文章ID
 * @returns 包含highestScore字段的对象
 */
export async function getUserArticleHighestScore(userId: number = 0, articleId: number): Promise<{ highestScore: number }> {
  try {
    const response = await axios.get(`/api/Articles/user/${userId}/article/${articleId}/highest-score`);
    return response.data;
  } catch (error) {
    console.error(`获取用户文章(${articleId})最高分失败:`, error);
    return { highestScore: 0 };
  }
}