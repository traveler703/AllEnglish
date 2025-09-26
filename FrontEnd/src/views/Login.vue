<template>
    <div class="wrapped">
        <div class="login-box">
            <h2 class="login-title">AllEn登录</h2>

            <el-input v-model="editableNote"
                      type="textarea"
                      :autosize="{ minRows: 2, maxRows: 4 }"
                      placeholder="登录AllEn账号，即可获取您在任何登陆过的设备上的单词，阅读，听力学习记录及其他权益"
                      class="editable-text"></el-input>

            <el-form ref="loginFormRef"
                     :model="loginForm"
                     :rules="loginRules"
                     label-position="top"
                     class="login-form">
                <el-form-item label="用户邮箱" prop="email">
                    <el-input v-model="loginForm.email"
                              placeholder="请输入邮箱"
                              type="email"
                              spellcheck="false"></el-input>
                </el-form-item>

                <el-form-item label="用户密码" prop="password">
                    <el-input v-model="loginForm.password"
                              placeholder="请输入密码"
                              type="password"
                              show-password></el-input>
                </el-form-item>

                <div class="action-buttons">
                    <el-button type="text" @click="handleRegister">注册账号</el-button>
                    <el-button type="text" @click="handleForgotPassword">找回密码</el-button>
                </div>

                <el-button type="primary"
                           class="login-btn"
                           :loading="loading"
                           @click="handleLogin">
                    登录
                </el-button>
            </el-form>
        </div>
    </div>
</template>

<script setup>
    import { ref, reactive } from 'vue'
    import { ElMessage } from 'element-plus'
    import { useRouter } from 'vue-router'
    import api from '../utils/axios'
    const router = useRouter()

    const loginForm = reactive({
        email: '',
        password: ''
    })

    const loginRules = {
        email: [
            { required: true, message: '请输入邮箱地址', trigger: 'blur' },
            { type: 'email', message: '请输入正确的邮箱地址', trigger: ['blur', 'change'] }
        ],
        password: [
            { required: true, message: '请输入密码', trigger: 'blur' },
            { min: 6, max: 20, message: '长度在 6 到 20 个字符', trigger: 'blur' }
        ]
    }

    const loading = ref(false)
    const loginFormRef = ref(null)
    const editableNote = ref('')

    import axios from 'axios'

    const handleLogin = () => {
        loginFormRef.value.validate(async (valid) => {
            if (valid) {
                loading.value = true
                try {
                    // 1) 一次调用：后端应返回 { token, userName, id, ... }
                    const response = await api.post('/api/user/login', {
                        email: loginForm.email,
                        password: loginForm.password
                    })
                    const data = response.data
                    
                    // 2) 存 JWT
                    localStorage.setItem('jwtToken', data.token)

                    // 3) 保存用户信息
                    const userData = {
                        userName: data.userName,
                        Id: data.id,
                        Email: data.email,
                        DateOfBirth: data.birthday,
                        Gender: data.gender,
                        PhoneNumber: data.phoneNumber,
                        Category: data.category,
                        level: 8,
                        coins: 1500,
                        avatarUrl: data.avatarUrl
                    }

                    // 保存用户信息
                    localStorage.setItem('user', JSON.stringify(userData))
                    localStorage.setItem('isAuthenticated', 'true')
                    localStorage.setItem('role', data.category)
                    localStorage.setItem('email', data.email)
                    localStorage.setItem('username', data.userName)

                    ElMessage.success(data.message || '登录成功')

                    console.log(data.id);
                    const res = await api.get('/api/paying/user-reinstate', { params: { userId: data.id } });
                    data.category = res.data.coin;

                    if (data.category === 'admin') {
                        router.push('/admin')
                    } else {
                        router.push({
                            path: '/user_center',
                            query: userData
                        })
                    }
                } catch (error) {
                    if (error.response?.status === 401) {
                        ElMessage.error('用户名或密码错误')
                    } else {
                        ElMessage.error('服务器错误，请稍后再试')
                    }
                } finally {
                    loading.value = false
                }
            }
        })
    }


    const handleRegister = () => {
        router.push('/register')
    }

    const handleForgotPassword = () => {
        router.push('/forgetpassword')
    }
</script>

<style scoped>
    .wrapped {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 80vh;
        padding: 20px;
    }

    .login-box {
        width: 450px;
        padding: 30px 30px;
        background-color: #fff;
        border-radius: 16px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
        transition: all 0.3s ease;
    }

        .login-box:hover {
            box-shadow: 0 12px 30px rgba(0, 0, 0, 0.15);
            transform: translateY(-4px);
        }

    .login-title {
        margin-bottom: 20px;
        text-align: center;
        color: #ff69b4;
        font-size: 30px;
        font-weight: 600;
    }

    .editable-text {
        margin-bottom: 20px;
        font-size: 20px;
    }

    :deep(.editable-text .el-textarea__inner) {
        font-size: 16px;
        border: none;
        box-shadow: none;
        resize: none;
        background-color: transparent;
        color: #606266;
        text-align: center;
    }

    .login-form {
        margin-top: 20px;
    }

    :deep(.el-input__wrapper) {
        height: 70px;
        font-size: 18px;
        padding: 0 14px;
        border-radius: 12px;
        box-sizing: border-box;
    }

    .action-buttons {
        display: flex;
        justify-content: space-between;
        margin-bottom: 30px;
        font-size: 15px;
    }

    .login-btn {
        width: 100%;
        height: 52px;
        font-size: 18px;
        margin-top: 15px;
    }

    @media (max-width: 768px) {
        .login-box {
            width: 100%;
            padding: 40px 20px;
        }

        .login-title {
            font-size: 24px;
        }

        .login-btn {
            font-size: 16px;
            height: 48px;
        }
    }
</style>