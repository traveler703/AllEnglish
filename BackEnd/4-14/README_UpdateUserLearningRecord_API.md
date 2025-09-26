# �����û�ѧϰ��¼ API �ĵ�

## API �˵�

### PUT /api/UserLearningRecord

�����û�ѧϰ��¼������Ϊ���º��ѧϰ��¼����������ֵ�����Ϊ�Ƿ�ɹ����¡�

## �������

### ������ (JSON)
```json
{
    "userId": "string",              // �û�ID�����
    "articleCount": 0,               // ���Ķ�����������
    "wordCount": 0,                  // ��ѧϰ�ĵ�������  
    "oralTime": 0,                   // ��ѵ���Ŀ�����ʱ�������ӣ�
    "listeningTime": 0,              // ��ѵ����������ʱ�������ӣ�
    "articlePerDay": 0,              // �������Ķ�����������
    "wordPerDay": 0,                 // ������ѧϰ�ĵ�������
    "oralPerDay": 0,                 // ������ѵ���Ŀ���ʱ�������ӣ�
    "listeningPerDay": 0             // ������ѵ��������ʱ�������ӣ�
}
```

### ����˵��
- `userId`: �û�Ψһ��ʶ��������Ϊ��
- ������ֵ���͵��ֶζ�����Ϊ����
- ����û�ѧϰ��¼�����ڣ�ϵͳ���Զ������¼�¼
- ����û�ѧϰ��¼�Ѵ��ڣ�ϵͳ��������м�¼

## ��Ӧ��ʽ

### �ɹ���Ӧ (200 OK)
```json
{
    "success": true,
    "message": "�����û�ѧϰ��¼�ɹ�"
}
```

### ������Ӧ

#### 400 Bad Request - ��������Ϊ��
```json
{
    "success": false,
    "message": "�������ݲ���Ϊ��"
}
```

#### 400 Bad Request - �û�IDΪ��
```json
{
    "success": false,
    "message": "�û�ID����Ϊ��"
}
```

#### 400 Bad Request - ������֤ʧ��
```json
{
    "success": false,
    "message": "ѧϰ��¼���ݲ���Ϊ����"
}
```

#### 400 Bad Request - ����ʧ��
```json
{
    "success": false,
    "message": "�����û�ѧϰ��¼ʧ��"
}
```

#### 500 Internal Server Error - ����������
```json
{
    "success": false,
    "message": "�������ڲ�����",
    "error": "���������Ϣ"
}
```

## ʹ��ʾ��

### cURL ʾ��
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

### JavaScript ʾ��
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
            console.log('���³ɹ�:', result.message);
        } else {
            console.error('����ʧ��:', result.message);
        }
        
        return result;
    } catch (error) {
        console.error('�������:', error);
        throw error;
    }
};

// ʹ��ʾ��
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

## ע������

1. **������֤**: ������ֵ�ֶζ�����Ϊ�Ǹ���
2. **�û�ID**: �����ṩ��Ч���û�ID
3. **�Զ�����**: ����û�ѧϰ��¼�����ڣ�ϵͳ���Զ�����
4. **��������**: ��API�����ѧϰ��¼�������ֶΣ���ȷ���ṩ����������
5. **����ȫ**: ���²��������ݿ�������ִ�У�ȷ������һ����

## ���API

- `GET /api/UserLearningRecord/{userId}` - ��ȡ�û�ѧϰ��¼

## �汾��Ϣ

- �汾: 1.0
- ������: 2024
