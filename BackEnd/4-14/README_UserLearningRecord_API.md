# 用户学习记录 API 文档

## 概述

用户学习记录API提供了管理用户学习进度和统计数据的功能，包括文章阅读量、单词学习量、口语训练时长、听力训练时长等数据的记录和查询。

## 数据库设计

### USER_LEARNING_RECORD 表

| 字段名 | 类型 | 说明 | 默认值 |
|--------|------|------|--------|
| USER_ID | VARCHAR2(50BYTE) | 用户ID（主键，外键引用USERS.ID） | - |
| ARTICLE_COUNT | NUMBER | 已阅读的文章的总量 | 0 |
| WORD_COUNT | NUMBER | 已学习的单词的总量 | 0 |
| ORAL_TIME | NUMBER | 已训练的口语的总时长（分钟） | 0 |
| LISTENING_TIME | NUMBER | 已训练的听力的总时长（分钟） | 0 |
| ARTICLE_PER_DAY | NUMBER | 单日已阅读的文章的数量 | 0 |
| WORD_PER_DAY | NUMBER | 单日已学习的单词的数量 | 0 |
| ORAL_PER_DAY | NUMBER | 单日已训练的口语的时长（分钟） | 0 |
| LISTENING_PER_DAY | NUMBER | 单日已训练的听力的时长（分钟） | 0 |

### 相关触发器

创建了两个触发器：TRG_CHECK_ACHIEVEMENTS_COIN和TRG_CHECK_ACHIEVEMENTS_LEARNING，这两个触发器会在用户的金币数量和学习记录发生变化时调用PROC_CHECK_ACHIEVEMENTS这个procedure。

### PROC_CHECK_ACHIEVEMENTS过程

这个过程会在用户的金币数量和学习状况达到某项成就的要求时，自动更新该用户已取得的成就的状态。

## API

### 获取用户学习记录

**GET** `/api/userlearningrecord/{userId}`

根据用户ID获取该用户的学习记录状况。

#### 请求参数

| 参数名 | 类型 | 位置 | 必需 | 说明 |
|--------|------|------|------|------|
| userId | string | Path | 是 | 用户ID |

#### 请求示例

```bash
GET /api/userlearningrecord/user123
```

#### 响应示例

**成功响应 (200 OK)**
```json
{
  "success": true,
  "message": "获取用户学习记录成功",
  "data": {
    "userId": "user123",
    "articleCount": 25,
    "wordCount": 150,
    "oralTime": 120,
    "listeningTime": 200,
    "articlePerDay": 3,
    "wordPerDay": 20,
    "oralPerDay": 15,
    "listeningPerDay": 30
  }
}
```

**用户不存在 (404 Not Found)**
```json
{
  "success": false,
  "message": "未找到该用户的学习记录"
}
```

**参数错误 (400 Bad Request)**
```json
{
  "success": false,
  "message": "用户ID不能为空"
}
```

