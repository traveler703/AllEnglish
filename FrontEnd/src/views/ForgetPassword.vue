<template>
  <div class="password-recovery-container">
    <div class="recovery-box">
      <h2 class="title">找回密码</h2>

      <!-- 第一步：验证邮箱 -->
      <div v-if="currentStep === 1" class="step-1">
        <el-form ref="emailForm" :model="form" :rules="rules" label-position="top">
          <el-form-item label="邮箱" prop="email">
            <el-input
              v-model="form.email"
              placeholder="请输入注册邮箱"
              clearable
              spellcheck="false"
              class="enlarged-input"
            />
          </el-form-item>

          <el-form-item label="验证码" prop="verificationCode">
            <div class="verification-code-container">
              <el-input
                v-model="form.verificationCode"
                placeholder="请输入验证码"
                clearable
                spellcheck="false"
                class="enlarged-input"
              />
              <el-button
                type="primary"
                :disabled="countdown > 0"
                @click="sendVerificationCode"
                class="send-code-btn enlarged-button"
              >
                {{ countdown > 0 ? `${countdown}秒后重试` : '获取验证码' }}
              </el-button>
            </div>
          </el-form-item>

          <div class="form-actions">
            <el-button type="primary" @click="handleStep1Submit" class="submit-btn enlarged-button">
              验证
            </el-button>
            <el-button type="text" @click="goBackToLogin" class="back-btn">返回登录</el-button>
          </div>
        </el-form>
      </div>

      <!-- 第二步：重置密码 -->
      <div v-if="currentStep === 2" class="step-2">
        <el-form
          key="step2-form"
          ref="passwordFormRef"
          :model="passwordForm"
          :rules="passwordRules"
          label-position="top"
        >
          <el-form-item label="新密码" prop="newPassword">
            <el-input
              v-model="passwordForm.newPassword"
              placeholder="请输入新密码"
              type="password"
              show-password
              clearable
              spellcheck="false"
              autocomplete="new-password"
              class="enlarged-input"
            />
          </el-form-item>

          <el-form-item label="确认密码" prop="confirmPassword">
            <el-input
              v-model="passwordForm.confirmPassword"
              placeholder="请再次输入新密码"
              type="password"
              show-password
              clearable
              spellcheck="false"
              autocomplete="new-password"
              class="enlarged-input"
            />
          </el-form-item>

          <div class="form-actions">
            <el-button type="primary" @click="handleStep2Submit" class="submit-btn enlarged-button">
              提交
            </el-button>
            <el-button type="text" @click="goBackToLogin" class="back-btn">返回登录</el-button>
          </div>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'
import { useRouter } from 'vue-router'

export default {
  name: 'PasswordRecovery',
  setup() {
    const router = useRouter()
    const currentStep = ref(1)
    const countdown = ref(0)

    const form = reactive({
      email: '',
      verificationCode: ''
    })

    const emailForgot = ref('')

    const codeSent = ref(false)

    const passwordForm = reactive({
      newPassword: '',
      confirmPassword: ''
    })

    const passwordFormRef = ref(null)

    const allowPasswordInput = () => true

    const rules = {
      email: [
        { required: true, message: '请输入邮箱地址', trigger: 'blur' },
        { type: 'email', message: '请输入正确的邮箱地址', trigger: ['blur', 'change'] }
      ],
      verificationCode: [
        { required: true, message: '请输入验证码', trigger: 'blur' }
      ]
    }

    const passwordRules = {
      newPassword: [
        { required: true, message: '请输入新密码', trigger: 'blur' },
        { min: 6, max: 20, message: '密码长度在6到20个字符', trigger: 'blur' }
      ],
      confirmPassword: [
        { required: true, message: '请再次输入新密码', trigger: 'blur' },
        { validator: validatePassword, trigger: 'blur' }
      ]
    }

    function validatePassword(rule, value, callback) {
      if (value !== passwordForm.newPassword) {
        callback(new Error('两次输入的密码不一致'))
      } else {
        callback()
      }
    }

    const goBackToLogin = () => {
      router.push('/')
    }

    const sendVerificationCode = async() => {
      if (!form.email) {
        ElMessage.warning('请先输入邮箱')
        return
      }
      try {
            await axios.post('https://localhost:7071/api/user/send-verification-code', {
                email: form.email,
                purpose:'reset'
            })
            ElMessage.success('验证码已发送')
            codeSent.value = true
            countdown.value = 60;

              if (window.verificationTimer) {
              clearInterval(window.verificationTimer);
              }
            window.verificationTimer = setInterval(() => {
              countdown.value--;
              
              if (countdown.value <= 0) {
                clearInterval(window.verificationTimer);
                codeSent.value = false;
                countdown.value = 0; 
              }
            }, 1000);


        } catch (error) {
            console.error(error)
            ElMessage.error(error.response?.data || '发送验证码失败')
        }
    }

    const handleStep1Submit = async() => {
      if (!form.email || !form.verificationCode) {
            ElMessage.warning('请填写邮箱和验证码')
            return
        }

        try {
            const response = await axios.post('https://localhost:7071/api/user/verify-code', {
                email: form.email,
                code: form.verificationCode
            })

            if (response.status === 200) {
                ElMessage.success('验证成功，请设置新密码')
                emailForgot.value = form.email
                currentStep.value = 2
            }
        } catch (error) {
            console.error(error)
            ElMessage.error(error.response?.data || '验证码验证失败')
        }
    }

    const handleStep2Submit = async () => {
      if (!passwordForm.newPassword || !passwordForm.confirmPassword) {
        ElMessage.warning('请填写完整的新密码')
        return
      }

      if (passwordForm.newPassword !== passwordForm.confirmPassword) {
        ElMessage.error('两次密码输入不一致')
        return
      }

      try {
        const response = await axios.put('https://localhost:7071/api/user/changePassword', {
          email: emailForgot.value, // 前面已验证的邮箱
          newPassword: passwordForm.newPassword
        })

        if (response.status === 200) {
          ElMessage.success('密码重置成功')
          currentStep.value = 1
          form.email = ''
          form.verificationCode = ''
          passwordForm.newPassword = ''
          passwordForm.confirmPassword = ''
          router.push('/')
        }
      } catch (error) {
        console.error(error)
        ElMessage.error(error.response?.data || '密码修改失败')
      }
    }

    return {
      currentStep,
      countdown,
      form,
      passwordForm,
      passwordFormRef,
      rules,
      passwordRules,
      allowPasswordInput,
      sendVerificationCode,
      handleStep1Submit,
      handleStep2Submit,
      goBackToLogin
    }
  }
}
</script>

<style scoped>
.password-recovery-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 40px;
}

.recovery-box {
  background-color: white;
  padding: 60px 100px;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 900px;
}

.title {
  text-align: center;
  margin-bottom: 40px;
  color: #333;
  font-size: 28px;
  font-weight: 600;
}

.verification-code-container {
  display: flex;
  gap: 15px;
}

.send-code-btn {
  width: 180px;
}

.submit-btn {
  width: 100%;
  margin-top: 20px;
}

.el-form-item {
  margin-bottom: 30px;
}

.el-form-item__label {
  font-size: 18px;
  padding-bottom: 10px !important;
}

.enlarged-input {
  font-size: 18px;
  height: 50px;
}

.enlarged-button {
  font-size: 18px;
  height: 50px;
  padding: 0 20px;
}

.el-input__inner {
  height: 100%;
}

.form-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
}

.back-btn {
  margin-left: 20px;
  font-size: 16px;
}
</style>