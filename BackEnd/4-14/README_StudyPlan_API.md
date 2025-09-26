# 学习计划API使用说明

本文档描述了基于Oracle数据库USER_STUDY_PLAN表的学习计划API接口。这些API支持完整的学习计划CRUD操作，并提供智能个性化单词选择功能。

创建学习计划时，系统会根据用户的学习历史自动推荐并选择最合适的单词，提供个性化的学习体验。

## 数据库表结构

USER_STUDY_PLAN表包含以下字段：

| 字段名 | 类型 | 说明 | 默认值 |
|--------|------|------|--------|
| ID | NUMBER | 唯一标识ID，是主码 | 自动生成 |
| TITLE | VARCHAR2(100 BYTE) | 学习计划名称，非空 | - |
| USER_ID | VARCHAR2(50 BYTE) | 创建者用户ID，非空，外码，引用自USER表的ID属性 | - |
| PLAN_DATE | DATE | 创建日期，非空 | - |
| DURATION | NUMBER | 持续天数，非负 | - |
| IS_PUBLIC | NUMBER | 是否公开，非空 | - |
| PLAN_TYPE | VARCHAR2(30 BYTE) | 类型(AUTO/MANUAL) | AUTO |
| STATUS | VARCHAR2(30 BYTE) | 状态(PROCEEDING/FINISHED/CANCELLED) | PROCEEDING |
| WORD_COUNT | NUMBER | 单词数量，非负 | 20 |
| WORD_IDS | CLOB | 单词ID集合，非空 | - |
| ARTICLE_COUNT | NUMBER | 短文数量，非负 | 5 |
| ORAL_TIME | NUMBER | 口语时长，非负 | 10 |

## 数据库相关配置

在数据库中增加了一个名为USER_STUDY_PLAN_TRG的触发器，当向USER_STUDY_PLAN中增加学习计划时，系统能够自动为该学习计划分配一个ID，该ID唯一标识一条学习计划

```sql
create or replace TRIGGER USER_STUDY_PLAN_TRG
BEFORE INSERT ON USER_STUDY_PLAN
FOR EACH ROW
BEGIN
  IF :NEW.ID IS NULL THEN
    :NEW.ID := USER_STUDY_PLAN_SEQ.NEXTVAL;
  END IF;
END;
```


## API接口

### API 1: 创建学习计划

**接口地址：** `POST /api/StudyPlan/create-new`

**功能特性：**
- **自动分配学习计划ID**：系统使用Oracle序列(USER_STUDY_PLAN_SEQ)自动生成唯一标识ID
- **智能个性化单词选择**：系统会根据用户的学习历史，按优先级自动选择合适的单词

**单词选择优先级算法：**
1. **薄弱单词**（最高优先级）：正确率低于60%的已学单词
2. **收藏单词**：用户收藏的重点单词
3. **复习单词**：已学但学习次数少于3次的单词
4. **未学习单词**：用户尚未学习的新单词
5. **其他单词**：如果上述类别不够，从其他已学单词中选择

**请求参数：**
```json
{
  "title": "我的学习计划",
  "userId": "114514",
  "planDate": "2024-01-15T00:00:00",
  "duration": 7,
  "isPublic": 0,
  "planType": "AUTO",
  "wordCount": 20,
  "articleCount": 5,
  "oralTime": 10
}
```

**响应示例：**
```json
{
  "success": true,
  "message": "学习计划创建成功",
  "data": {
    "id": 1,
    "title": "我的学习计划",
    "userId": "114514",
    "planDate": "2024-01-15T00:00:00",
    "duration": 7,
    "isPublic": 0,
    "planType": "AUTO",
    "status": "PROCEEDING",
    "wordCount": 20,
    "wordIds": "1,5,12,23,45,67,89,101,134,156,178,201,234,267,289,301,334,356,378,401",
    "articleCount": 5,
    "oralTime": 10
  }
}
```

**注意事项：**
- 由于USER_WORD表中有大量数据，导致本API运行比较慢，须耐心等待

### API 2: 根据学习计划ID查找学习计划

**接口地址：** `GET /api/StudyPlan/get-by-id/{id}`

**请求参数：**
- id: 学习计划ID (路径参数)

**响应示例：**返回该学习计划ID的学习计划的所有属性的值

```json
{
  "success": true,
  "message": "获取成功",
  "data": {
    "id": 1,
    "title": "我的学习计划",
    "userId": "114514",
    "planDate": "2024-01-15T00:00:00",
    "duration": 7,
    "isPublic": 0,
    "planType": "AUTO",
    "status": "PROCEEDING",
    "wordCount": 20,
    "wordIds": "1,5,12,23,45,67,89,101,134,156,178,201,234,267,289,301,334,356,378,401",
    "articleCount": 5,
    "oralTime": 10
  }
}
```

### API 3: 根据用户ID查找学习计划

**接口地址：** `GET /api/StudyPlan/get-by-user/{userId}`

**请求参数：**
- userId: 用户ID (路径参数)

**响应示例：**返回满足条件的学习计划的ID

```json
{
  "success": true,
  "message": "获取成功",
  "data": {
    "studyPlanIds": [
      3,
      2
    ]
  }
}
```

### API 4: 根据用户ID和日期查找学习计划

**接口地址：** `GET /api/StudyPlan/get-by-user-and-date/{userId}?planDate={planDate}`

**请求参数：**
- userId: 用户ID (路径参数)
- planDate: 创建日期 (查询参数，格式：2024-01-15)

**响应示例：**返回满足条件的学习计划的ID

```json
{
  "success": true,
  "message": "获取成功",
  "data": {
    "studyPlanIds": [
      3,
      2
    ]
  }
}
```

### API 5: 删除学习计划

**接口地址：** `DELETE /api/StudyPlan/delete/{id}`

**请求参数：**
- id: 学习计划ID (路径参数)

**响应示例：**
```json
{
  "success": true,
  "message": "学习计划删除成功",
  "data": null
}
```

### API 6: 更新学习计划

**接口地址：** `PUT /api/StudyPlan/update-full`

**请求参数：**
```json
{
  "id": 1,
  "title": "更新的学习计划",
  "userId": "114514",
  "planDate": "2024-01-15T00:00:00",
  "duration": 10,
  "isPublic": 1,
  "planType": "MANUAL",
  "status": "PROCEEDING",
  "wordCount": 25,
  "wordIds": "1,2,3,4,5,6,7,8",
  "articleCount": 8,
  "oralTime": 15
}
```

**响应示例：**
```json
{
  "success": true,
  "message": "学习计划更新成功",
  "data": null
}
```

## 错误处理

所有API在出现错误时都会返回以下格式：

```json
{
  "success": false,
  "message": "错误信息描述",
  "data": null
}
```

常见HTTP状态码：
- 200: 成功
- 404: 资源不存在  
- 500: 服务器内部错误

## 数据类型说明

### PLAN_TYPE (计划类型)
- `AUTO`: 自动计划
- `MANUAL`: 手动计划

### STATUS (状态)
- `PROCEEDING`: 进行中
- `FINISHED`: 已完成
- `CANCELLED`: 已取消

### IS_PUBLIC (是否公开)
- `0`: 私有
- `1`: 公开

## API总结

系统提供以下API接口：

| API | 端点 | 方法 | 功能描述 |
|-----|------|------|----------|
| **API 1** | `/api/StudyPlan/create-new` | POST | 创建学习计划（自动个性化单词选择） |
| **API 2** | `/api/StudyPlan/get-by-id/{id}` | GET | 根据学习计划ID查找计划详情 |
| **API 3** | `/api/StudyPlan/get-by-user/{userId}` | GET | 根据用户ID获取所有学习计划ID |
| **API 4** | `/api/StudyPlan/get-by-user-and-date/{userId}` | GET | 根据用户ID和日期查找计划ID |
| **API 5** | `/api/StudyPlan/delete/{id}` | DELETE | 删除指定的学习计划 |
| **API 6** | `/api/StudyPlan/update-full` | PUT | 完整更新学习计划所有属性 |
