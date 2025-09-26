# 成就系统 API 文档

## 概述
成就系统提供了四个主要API接口，用于查询成就信息和用户成就状态。

## API 接口列表

### 1. 根据成就ID查询成就信息
**接口地址：** `GET /api/Achievement/{achievementId}`

**功能描述：** 根据成就的ID查询成就的信息，输出为该成就的所有属性的值。

**请求参数：**
- `achievementId` (int): 成就ID，路径参数

**响应示例：**
```json
{
  "id": 1,
  "title": "学习达人",
  "description": "完成100次学习任务",
  "requirement": 100,
  "icon": "/images/achievements/study_master.png"
}
```

**错误响应：**
```json
{
  "message": "未找到ID为 999 的成就"
}
```

### 2. 根据用户ID查询该用户所有已取得的成就
**接口地址：** `GET /api/Achievement/UserGained/{userId}`

**功能描述：** 根据用户ID查询该用户所有已取得的成就，输出为该用户所有已取得的成就的成就ID。

**请求参数：**
- `userId` (string): 用户ID，路径参数

**响应示例：**
```json
{
  "achievementIds": [1, 3, 5, 8]
}
```

### 3. 根据用户ID和成就ID查询该用户是否已取得该成就
**接口地址：** `GET /api/Achievement/UserStatus/{userId}/{achievementId}`

**功能描述：** 根据用户ID和成就ID查询该用户是否已取得该成就，输出为对应的HAS_GAINED的值。

**请求参数：**
- `userId` (string): 用户ID，路径参数
- `achievementId` (int): 成就ID，路径参数

**响应示例：**
```json
{
  "hasGained": 1
}
```

**说明：**
- `hasGained = 0`: 未取得该成就
- `hasGained = 1`: 已取得该成就

### 4. 根据用户ID查询所有成就的取得情况
**接口地址：** `GET /api/Achievement/AllUserAchievements/{userId}`

**功能描述：** 根据用户ID查询所有成就的取得情况，输出为该用户的所有成就的取得情况，即USER_ACHIEVEMENTS表中符合该用户ID的记录中的ACHIEVEMENT_ID、HAS_GAINED、GAIN_DATE属性的值。

**请求参数：**
- `userId` (string): 用户ID，路径参数

**响应示例：**
```json
[
  {
    "achievementId": 1,
    "hasGained": 1,
    "gainDate": "2024-01-15T10:30:00"
  },
  {
    "achievementId": 2,
    "hasGained": 0,
    "gainDate": null
  },
  {
    "achievementId": 3,
    "hasGained": 1,
    "gainDate": "2024-01-20T14:15:00"
  }
]
```

## 数据库表结构

### ACHIEVEMENT 表
- `ID` (NUMBER): 成就ID，主键
- `TITLE` (VARCHAR2(100)): 成就名称，非空
- `DESCRIPTION` (VARCHAR2(500)): 成就简介
- `REQUIREMENT` (NUMBER): 成就所需条件，非空
- `ICON` (VARCHAR2(300)): 成就图标路径

### USER_ACHIEVEMENTS 表
- `USER_ID` (VARCHAR2(50)): 用户ID，外键引用USER表的ID
- `ACHIEVEMENT_ID` (NUMBER): 成就ID，外键引用ACHIEVEMENT表的ID
- `HAS_GAINED` (NUMBER): 是否取得成就，默认0（未取得）
- `GAIN_DATE` (DATE): 取得成就的日期，未取得时为null

## 使用示例

### 查询成就信息
```bash
curl -X GET "https://localhost:7071/api/Achievement/1"
```

### 查询用户已取得的成就
```bash
curl -X GET "https://localhost:7071/api/Achievement/UserGained/user123"
```

### 查询用户特定成就状态
```bash
curl -X GET "https://localhost:7071/api/Achievement/UserStatus/user123/1"
```

### 查询用户所有成就状态
```bash
curl -X GET "https://localhost:7071/api/Achievement/AllUserAchievements/user123"
```

## 注意事项

1. 所有接口都返回JSON格式的数据
2. 用户ID和成就ID必须存在于数据库中
3. 如果查询的成就不存在，会返回404错误
4. 用户成就状态查询时，如果用户没有该成就的记录，会返回未取得状态（hasGained = 0）
5. 日期格式采用ISO 8601标准 