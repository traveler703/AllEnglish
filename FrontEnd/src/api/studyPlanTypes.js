/**
 * 学习计划查询参数类型
 * @typedef {Object} StudyPlanParams
 * @property {number} [pageNumber]
 * @property {number} [pageSize]
 * @property {string} [searchTerm]
 * @property {number} [difficulty]
 * @property {string} [category]
 */

/**
 * 学习计划基础类型
 * @typedef {Object} StudyPlan
 * @property {number} id
 * @property {string} title
 * @property {string} description
 * @property {number} difficulty
 * @property {string} difficultyLevel
 * @property {string} category
 * @property {number} duration
 * @property {string} [coverImage]
 * @property {string} [createdAt]
 * @property {string} [updatedAt]
 */

/**
 * 学习计划详情类型
 * @typedef {StudyPlan & Object} StudyPlanDetail
 * @property {string} content
 * @property {string[]} objectives
 * @property {Array<{title: string, url: string, type: string}>} resources
 * @property {string[]} [tags]
 */

/**
 * 学习计划列表响应类型
 * @typedef {Object} StudyPlanResponse
 * @property {StudyPlan[]} items
 * @property {number} totalItems
 * @property {number} totalPages
 * @property {number} currentPage
 * @property {number} pageSize
 * @property {boolean} hasPreviousPage
 * @property {boolean} hasNextPage
 */

export default {}