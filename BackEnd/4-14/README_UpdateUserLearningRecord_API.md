# 更新用户学习记录 API 文档

## API 端点

### PUT /api/UserLearningRecord

更新用户学习记录，输入为更新后的学习记录的所有属性值，输出为是否成功更新。

## 请求参数

### 请求体 (JSON)
```json
{
    "userId": "string",              // 用户ID（必填）
    "articleCount": 0,               // 已阅读的文章总量
    "wordCount": 0,                  // 已学习的单词总量  
    "oralTime": 0,                   // 已训练的口语总时长（分钟）
    "listeningTime": 0,              // 已训练的听力总时长（分钟）
    "articlePerDay": 0,              // 单日已阅读的文章数量
    "wordPerDay": 0,                 // 单日已学习的单词数量
    "oralPerDay": 0,                 // 单日已训练的口语时长（分钟）
    "listeningPerDay": 0             // 单日已训练的听力时长（分钟）
}
```

### 参数说明
- `userId`: 用户唯一标识符，不能为空
- 所有数值类型的字段都不能为负数
- 如果用户学习记录不存在，系统会自动创建新记录
- 如果用户学习记录已存在，系统会更新现有记录

## 响应格式

### 成功响应 (200 OK)
```json
{
    "success": true,
    "message": "更新用户学习记录成功"
}
```

### 错误响应

#### 400 Bad Request - 请求数据为空
```json
{
    "success": false,
    "message": "请求数据不能为空"
}
```

#### 400 Bad Request - 用户ID为空
```json
{
    "success": false,
    "message": "用户ID不能为空"
}
```

#### 400 Bad Request - 数据验证失败
```json
{
    "success": false,
    "message": "学习记录数据不能为负数"
}
```

#### 400 Bad Request - 更新失败
```json
{
    "success": false,
    "message": "更新用户学习记录失败"
}
```

#### 500 Internal Server Error - 服务器错误
```json
{
    "success": false,
    "message": "服务器内部错误",
    "error": "具体错误信息"
}
```

## 使用示例

### cURL 示例
```bash
curl -X PUT "https://localhost:7071/api/UserLearningRecord" \
     -H "Content-Type: application/json" \
     -d '{
         "userId": "user123",
         "articleCount": 50,
         "wordCount": 1000,
         "oralTime": 120,
         "listeningTime": 180,
         "articlePerDay": 5,
         "wordPerDay": 20,
         "oralPerDay": 15,
         "listeningPerDay": 30
     }'
```

### JavaScript 示例
```javascript
const updateUserLearningRecord = async (learningRecord) => {
    try {
        const response = await fetch('https://localhost:7071/api/UserLearningRecord', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(learningRecord)
        });
        
        const result = await response.json();
        
        if (result.success) {
            console.log('更新成功:', result.message);
        } else {
            console.error('更新失败:', result.message);
        }
        
        return result;
    } catch (error) {
        console.error('请求错误:', error);
        throw error;
    }
};

// 使用示例
const learningRecord = {
    userId: "user123",
    articleCount: 50,
    wordCount: 1000,
    oralTime: 120,
    listeningTime: 180,
    articlePerDay: 5,
    wordPerDay: 20,
    oralPerDay: 15,
    listeningPerDay: 30
};

updateUserLearningRecord(learningRecord);
```

## 注意事项

1. **数据验证**: 所有数值字段都必须为非负数
2. **用户ID**: 必须提供有效的用户ID
3. **自动创建**: 如果用户学习记录不存在，系统会自动创建
4. **完整更新**: 此API会更新学习记录的所有字段，请确保提供完整的数据
5. **事务安全**: 更新操作在数据库事务中执行，确保数据一致性

## 相关API

- `GET /api/UserLearningRecord/{userId}` - 获取用户学习记录

## 版本信息

- 版本: 1.0
- 最后更新: 2024
