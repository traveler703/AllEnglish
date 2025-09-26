<!-- Register.vue -->
<template>
    <div class="register-container">
        <div class="register-box">
            <h2 class="register-title">注册账号</h2>

            <!-- 第一步：邮箱验证 -->
            <el-form v-if="step === 1"
                     ref="emailFormRef"
                     :model="emailForm"
                     :rules="emailRules"
                     label-position="top"
                     class="register-form">
                <el-form-item label="邮箱地址" prop="email">
                    <el-input v-model="emailForm.email" placeholder="请输入邮箱" />
                </el-form-item>

                <el-form-item label="验证码" prop="code">
                    <el-input v-model="emailForm.code"
                              placeholder="请输入验证码"
                              style="width: 70%; margin-right: 10px" />
                    <el-button @click="sendCode" :disabled="codeSent">
                        {{ codeSent ? '已发送(' + countdown + 's)' : '发送验证码' }}
                    </el-button>
                </el-form-item>

                <div class="form-actions">
                    <el-button type="primary" @click="verifyCode">下一步</el-button>
                    <el-button type="text" @click="goBackToLogin">返回登录</el-button>
                </div>
            </el-form>

            <!-- 第二步：填写信息 -->
            <el-form v-else
                     ref="infoFormRef"
                     :model="infoForm"
                     :rules="infoRules"
                     label-position="top"
                     class="register-form">
                <el-form-item label="头像">
                    <el-upload class="avatar-uploader"
                               action="/api/User/upload-avatar"
                               :show-file-list="false"
                               :on-success="handleAvatarSuccess"
                               :before-upload="beforeAvatarUpload"
                               name="file">
                        <template #default>
                            <el-tooltip content="点击修改头像" placement="top">
                                <img v-if="infoForm.avatarUrl" :src="infoForm.avatarUrl" class="avatar" />
                                <i v-else class="el-icon-plus avatar-uploader-icon"></i>
                            </el-tooltip>
                        </template>
                    </el-upload>
                </el-form-item>

                <el-form-item label="用户名" prop="username">
                    <el-input v-model="infoForm.username" placeholder="请输入用户名" />
                </el-form-item>

                <el-form-item label="密码" prop="password">
                    <el-input v-model="infoForm.password" type="password" placeholder="请输入密码" show-password />
                </el-form-item>

                <el-form-item label="确认密码" prop="confirmPassword">
                    <el-input v-model="infoForm.confirmPassword" type="password" placeholder="请再次输入密码" show-password />
                </el-form-item>

                <el-form-item label="性别" prop="gender">
                    <el-select v-model="infoForm.gender" placeholder="请选择性别">
                        <el-option label="男" value="male" />
                        <el-option label="女" value="female" />
                        <el-option label="不愿透露" value="other" />
                    </el-select>
                </el-form-item>

                <el-form-item label="生日" prop="birthday">
                    <el-date-picker v-model="infoForm.birthday"
                                    type="date"
                                    placeholder="选择日期"
                                    style="width: 100%;"
                                    value-format="YYYY-MM-DD" />
                </el-form-item>

                <el-form-item label="电话号码" prop="phone">
                    <el-input v-model="infoForm.phone" placeholder="请输入电话号码" />
                </el-form-item>

                <div class="form-actions">
                    <el-button type="primary" @click="submitRegister" :disabled="!isFormComplete">注册</el-button>
                    <el-button type="text" @click="step = 1">上一步</el-button>
                </div>
            </el-form>
        </div>
    </div>
</template>

<script setup>
    import { ref, reactive } from 'vue'
    import { ElMessage } from 'element-plus'
    import { useRouter } from 'vue-router'
    import axios from 'axios'
    import { computed } from 'vue'

    const router = useRouter()
    const step = ref(1)
    const codeSent = ref(false)
    const countdown = ref(60)
    const emailForRegister = ref('')

    const emailFormRef = ref(null)
    const infoFormRef = ref(null)

    const emailForm = reactive({
        email: '',
        code: ''
    })

    const infoForm = reactive({
        username: '',
        password: '',
        confirmPassword: '',
        gender: '',
        birthday: '',
        phone: '',
        avatarUrl: ''
    })

    const emailRules = {
        email: [
            { required: true, message: '请输入邮箱地址', trigger: 'blur' },
            { type: 'email', message: '邮箱格式不正确', trigger: 'blur' }
        ],
        code: [{ required: true, message: '请输入验证码', trigger: 'blur' }]
    }

    const infoRules = {
        username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
        password: [
            { required: true, message: '请输入密码', trigger: 'blur' },
            { min: 6, max: 20, message: '长度在 6 到 20 个字符', trigger: 'blur' }
        ],
        confirmPassword: [
            { required: true, message: '请确认密码', trigger: 'blur' },
            {
                validator: (rule, value, callback) => {
                    if (value !== infoForm.password) {
                        callback(new Error('两次密码输入不一致'))
                    } else {
                        callback()
                    }
                },
                trigger: 'blur'
            }
        ],
        gender: [{ required: true, message: '请选择性别', trigger: 'change' }],
        birthday: [{ required: true, message: '请选择生日', trigger: 'change' }],
        phone: [
            { required: true, message: '请输入电话', trigger: 'blur' },
            { pattern: /^1[3-9]\d{9}$/, message: '请输入有效的手机号码', trigger: 'blur' }
        ]
    }

    const sendCode = async () => {
        if (!emailForm.email) {
            ElMessage.warning('请先输入邮箱地址')
            return
        }

        try {
            await axios.post('https://localhost:7071/api/user/send-verification-code', {
                email: emailForm.email,
            })
            ElMessage.success('验证码已发送')
            codeSent.value = true

            const timer = setInterval(() => {
                countdown.value--
                if (countdown.value <= 0) {
                    clearInterval(timer)
                    codeSent.value = false
                    countdown.value = 60
                }
            }, 1000)
        } catch (error) {
            console.error(error)
            ElMessage.error(error.response?.data || '发送验证码失败')
        }
    }

    const verifyCode = async () => {
        if (!emailForm.email || !emailForm.code) {
            ElMessage.warning('请填写邮箱和验证码')
            return
        }

        try {
            const response = await axios.post('https://localhost:7071/api/user/verify-code', {
                email: emailForm.email,
                code: emailForm.code
            })

            if (response.status === 200) {
                ElMessage.success('验证成功')
                emailForRegister.value = emailForm.email
                step.value = 2
            }
        } catch (error) {
            console.error(error)
            ElMessage.error(error.response?.data || '验证码验证失败')
        }
    }

    const isFormComplete = computed(() => {
        return (
            infoForm.username &&
            infoForm.password &&
            infoForm.confirmPassword &&
            infoForm.gender &&
            infoForm.birthday &&
            infoForm.phone &&
            infoForm.avatarUrl 
    )
    })

    const submitRegister = async () => {
        try {
            await infoFormRef.value.validate()

            const registerData = {
                UserName: infoForm.username,
                Password: infoForm.password,
                Email: emailForRegister.value,
                Gender: infoForm.gender,
                DateOfBirth: infoForm.birthday,
                PhoneNumber: infoForm.phone,
                AvatarUrl: infoForm.avatarUrl
            }
            console.log(registerData)
            await axios.post('https://localhost:7071/api/user/register', registerData)
            ElMessage.success('注册成功')
            router.push('/')
        } catch (error) {
            console.error(error)
            ElMessage.error(error.response?.data || '注册失败')
        }
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
            infoForm.avatarUrl = response.path
            console.log('上传成功响应：', infoForm.avatarUrl)
            ElMessage.success('头像上传成功')
        } else {
            ElMessage.error(response?.message || '上传失败')
        }
    }



    const goBackToLogin = () => {
        router.push('/')
    }
</script>

<style scoped>
    .register-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 100vh;
        padding: 20px;
    }

    .register-box {
        width: 600px;
        padding: 50px 40px;
        background-color: #fff;
        border-radius: 16px;
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
    }

    .register-title {
        text-align: center;
        font-size: 28px;
        color: #303133;
        margin-bottom: 30px;
    }

    .form-actions {
        display: flex;
        justify-content: space-between;
        margin-top: 20px;
    }

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

</style>
