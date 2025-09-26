<template>
    <div class="edit-container">
        <h1>编辑资料</h1>

        <el-form :model="form" label-width="100px" class="form-box">
            <!-- 头像上传 -->
            <el-form-item label="头像">
                <el-upload class="avatar-uploader"
                           action="/api/User/upload-avatar"
                           :show-file-list="false"
                           :on-success="handleAvatarSuccess"
                           :before-upload="beforeAvatarUpload"
                           name="file">
                    <template #default>
                        <el-tooltip content="点击修改头像" placement="top">
                            <img v-if="form.avatarUrl" :src="form.avatarUrl" class="avatar" />
                            <i v-else class="el-icon-plus avatar-uploader-icon"></i>
                        </el-tooltip>
                    </template>
                </el-upload>
            </el-form-item>

            <!-- 可编辑字段 -->
            <el-form-item label="用户名">
                <el-input v-model="form.username" placeholder="请输入用户名" />
            </el-form-item>

            <el-form-item label="性别">
                <el-select v-model="form.gender" placeholder="请选择性别">
                    <el-option label="男" value="male" />
                    <el-option label="女" value="female" />
                    <el-option label="不愿透露" value="secret" />
                </el-select>
            </el-form-item>

            <el-form-item label="手机号">
                <el-input v-model="form.phone" placeholder="请输入手机号" />
            </el-form-item>

            <!-- 只读字段 -->
            <el-form-item label="邮箱">
                <el-input v-model="form.email" disabled class="readonly-input" />
            </el-form-item>

            <el-form-item label="用户类型">
                <el-input v-model="form.category" disabled class="readonly-input" />
            </el-form-item>

            <!-- 按钮 -->
            <el-form-item>
                <div class="button-row">
                    <div class="left-buttons">
                        <el-button type="primary" @click="onSubmit">保存修改</el-button>
                        <el-button @click="onCancel">取消</el-button>
                    </div>
                    <el-button type="danger" @click="onLogout">注销</el-button>
                </div>
            </el-form-item>
        </el-form>
    </div>
</template>

<script setup>
    import { useRoute, useRouter } from 'vue-router'
    import { ref } from 'vue'
    import { ElMessageBox, ElMessage } from 'element-plus'
    import axios from 'axios'

    const route = useRoute()
    const router = useRouter()

    const form = ref({
        userId: route.query.id,
        username: route.query.username,
        gender: route.query.gender || '',
        phone: route.query.phone || '',
        email: route.query.email || '',
        category: route.query.category || '',
        avatarUrl: route.query.avatarUrl || ''
    })

    async function onSubmit() {
        const updates = []

        if (form.value.username !== route.query.username) {
            updates.push({ type: 'UserName', content: form.value.username })
        }

        if (form.value.gender !== route.query.gender) {
            const genderValue = form.value.gender || 'unknown'
            updates.push({ type: 'Gender', content: genderValue })
        }

        if (form.value.phone !== route.query.phone) {
            updates.push({ type: 'PhoneNumber', content: form.value.phone })
        }

        if (form.value.avatarUrl !== route.query.avatarUrl) {
            updates.push({ type: 'AvatarUrl', content: form.value.avatarUrl })
        }

        try {
            for (const update of updates) {
                const res = await fetch(`/api/User/updateprofile`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        id: form.value.userId,
                        typeOfContent: update.type,
                        content: update.content
                    })
                })

                const result = await res.json()
                if (!res.ok) {
                    throw new Error(result.message || '更新失败')
                }
            }

            ElMessage.success('资料已保存')

            // 更新 localStorage 中的用户信息
            const localUser = JSON.parse(localStorage.getItem('user') || '{}')
            localUser.userName = form.value.username
            localUser.Gender = form.value.gender
            localUser.PhoneNumber = form.value.phone
            localUser.avatarUrl = form.value.avatarUrl
            localStorage.setItem('user', JSON.stringify(localUser))


            router.push({ name: 'UserCenter' })
        } catch (err) {
            console.error(err)
            ElMessage.error('资料保存失败')
        }
    }

    function onCancel() {
        router.push({ name: 'UserCenter' })
    }

    // 注销逻辑：弹出确认框，确认后调用后端
    const onLogout = () => {
        ElMessageBox.confirm(
            '确定要注销并删除账号吗？此操作不可撤销。',
            '注销确认',
            {
                confirmButtonText: '确认注销',
                cancelButtonText: '取消',
                type: 'warning',
            }
        )
            .then(async () => {
                try {
                    const email = form.value.email
                    const res = await axios.delete(`/api/User/delete?email=${encodeURIComponent(email)}`)
                    ElMessage.success(res.data || '账号已注销')

                    // 清除本地存储并跳转登录页
                    localStorage.removeItem('user')
                    router.push('/')
                } catch (err) {
                    console.error(err)
                    ElMessage.error(err.response?.data || '注销失败')
                }
            })
            .catch(() => {
                // 用户取消了
            })
    }

    function beforeAvatarUpload(file) {
        const isImage = file.type === 'image/jpeg' || file.type === 'image/png'
        const isLt2M = file.size / 1024 / 1024 < 2

        if (!isImage) {
            ElMessage.error('头像必须为 JPG 或 PNG 格式')
            return false
        }
        if (!isLt2M) {
            ElMessage.error('头像大小不能超过 2MB')
            return false
        }
        return true
    }

    function handleAvatarSuccess(response, file) {
        if (response && response.path) {
            form.value.avatarUrl = response.path
            ElMessage.success('头像上传成功')
        } else {
            ElMessage.error(response?.message || '上传失败')
        }
    }
</script>

<style scoped>
    .edit-container {
        width: 70%;
        height: 60%;
        margin: 40px auto;
        padding: 30px;
        background-color: #ffffff;
        border-radius: 16px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
        color: #000;
    }

    .form-box {
        margin-top: 20px;
    }

    .el-input {
        height: 48px;
    }

        .el-input :deep(.el-input__inner) {
            height: 48px;
            font-size: 16px;
            line-height: 48px;
        }

    .el-select {
        display: inline-block;
        width: 100%;
    }

        .el-select :deep(.el-input) {
            height: 48px;
        }

        .el-select :deep(.el-input__wrapper) {
            height: 48px;
            font-size: 16px;
            line-height: 48px;
            padding: 0 11px;
            box-sizing: border-box;
        }

    /* 头像上传区域 */
    .avatar-uploader {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 100px;
        height: 100px;
        border: 1px dashed #d9d9d9;
        border-radius: 50%;
        cursor: pointer;
        overflow: hidden;
        position: relative;
        background-color: #fafafa;
        text-align: center;
        line-height: 100px;
        font-size: 28px;
        color: #999;
    }

        .avatar-uploader :deep(.el-upload) {
            width: 100%;
            height: 100%;
            cursor: pointer;
        }

    .avatar {
        width: 100%;
        height: 100%;
        object-fit: cover;
        display: block;
    }

    .avatar-uploader-icon {
        font-size: 28px;
        color: #999;
    }

    /* 只读样式 */
    .readonly-input :deep(.el-input__inner) {
        color: #999;
        background-color: #f5f5f5;
        cursor: not-allowed;
    }

    .button-row {
        display: flex;
        justify-content: space-between;
        width: 100%;
    }

    .left-buttons {
        display: flex;
        gap: 10px;
    }
</style>
