<template>
  <!-- 整个聊天组件的根容器 -->
  <div class="chat-wrapper">
    <el-card class="chat-container">
      <el-scrollbar class="chat-window">
        <div
          v-for="(msg, idx) in messages"
          :key="idx"
          class="message-item"
          :class="msg.role"
        >
          <div class="avatar-wrapper" v-if="msg.role === 'assistant' || msg.role === 'evaluation'">
            <img src="/avatars/assistant.png" alt="AI" class="avatar" />
          </div>
          <div class="bubble-wrapper">
            <div class="bubble" :class="msg.role">
              {{ msg.text }}
            </div>
            <audio
              v-if="msg.audioUrl"
              :src="msg.audioUrl"
              controls
              class="audio-player"
              @loadedmetadata="logAudioDuration"
            />
          </div>
          <!-- 气泡和音频播放器容器 -->
          <div class="avatar-wrapper" v-if="msg.role === 'user'">
            <img src="/avatars/user.png" alt="User" class="avatar" />
          </div>
        </div>
      </el-scrollbar>
    </el-card>
    <div class="chat-input-area">
      <el-button
        class="chat-btn"
        type="primary"
        @click="startRecording"
        :disabled="recording"
      >
        开始录音
      </el-button>
      <el-button
        class="chat-btn"
        type="warning"
        @click="stopRecording"
        :disabled="!recording"
      >
        停止录音
      </el-button>
      <el-button
        class="chat-btn danger"
        @click="endConversation"
        :disabled="messages.length <= 2 || summaryRequested"
      >
        结束对话
      </el-button>
      <el-button class="chat-btn restart" @click="restartConversation">
        重新开始
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { completeLevel as completeAdventureLevel } from '../utils/localProgress';
import { ElMessage } from 'element-plus';
import 'element-plus/es/components/message/style/css';

// 存储用户ID
const userId = ref(localStorage.getItem('userId') || '');

// 存储后端返回的 sessionId，用于区分不同会话
const sessionId = ref('');
// 是否正在录音
const recording = ref(false);
// MediaRecorder 实例引用
const recorder = ref<MediaRecorder | null>(null);
// 临时存储录音数据片段
const audioChunks = ref<Blob[]>([]);
// 聊天消息列表，包含用户、AI 对话和点评
const messages = reactive<Array<{ role: 'user' | 'assistant' | 'evaluation'; text: string; audioUrl?: string }>>([
  { role: 'user', text: "Hi I'm Bingbing." },
  { role: 'assistant', text: "Now let's start practice oral English!" }
]);
// 标记是否已请求过点评，防止重复触发
const summaryRequested = ref(false);

// 新增：记录总对话时长
const totalOralTime = ref(0);

// 新增：处理AI回复音频加载，并打印时长
function logAudioDuration(event: Event) {
  const audioElement = event.target as HTMLAudioElement;
  const duration = parseFloat(audioElement.duration.toFixed(2));
  console.log(`AI回复音频时长: ${duration} 秒`);
  totalOralTime.value += duration;
}

// 生命周期钩子
onMounted(async () => {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    recorder.value = new MediaRecorder(stream);
    recorder.value.ondataavailable = (e: BlobEvent) => {
      if (e.data.size) audioChunks.value.push(e.data);
    };
    recorder.value.onstop = handleAudioStop;
  } catch {
    ElMessage.error('无法访问麦克风，请检查权限');
  }
  // 启动会话，取 sessionId
  try {
    const resp = await fetch('/api/OralPractice/start', { method: 'POST' });
    if (!resp.ok) throw new Error(`Status ${resp.status}`);
    const json = await resp.json();
    sessionId.value = json.sessionId;
  } catch (err) {
    console.error(err);
    ElMessage.error('启动会话失败，请稍后重试');
  }
});

// 重新启动会话
async function startSession() {
  try {
    const resp = await fetch('/api/OralPractice/start', { method: 'POST' })
    if (!resp.ok) throw new Error(`启动会话失败 ${resp.status}`)
    const body = await resp.json()
    sessionId.value = body.sessionId
  } catch (e) {
    console.error(e)
    ElMessage.error('启动会话失败，请稍后重试')
  }
}

// 开始录音
function startRecording() {
  if (!recorder.value) return;
  audioChunks.value = [];
  recorder.value.start(); 
  recording.value = true;
}

// 停止录音
function stopRecording() {
  if (!recorder.value || !recording.value) return;
  recorder.value.stop();
  recording.value = false;
}

// 录音停止后处理：上传并识别、对话、播放回复
async function handleAudioStop() {
  const chunks = audioChunks.value;
  if (!chunks.length) return;
  const blob = new Blob(chunks, { type: 'audio/webm' });

  // 获取用户录音时长
  const tempAudio = new Audio(URL.createObjectURL(blob));
  tempAudio.onloadedmetadata = () => {
    const duration = parseFloat(tempAudio.duration.toFixed(2));
    console.log(`用户录音时长: ${duration} 秒`);
    ElMessage.info(`您的录音时长为: ${duration} 秒`);
    totalOralTime.value += duration;
    URL.revokeObjectURL(tempAudio.src); // 释放URL对象，避免内存泄漏
  };

  const form = new FormData();
  form.append('audio', blob, 'voice.webm');
  const url = `/api/OralPractice/oralpractice?sessionId=${encodeURIComponent(sessionId.value)}`;
  try {
    const res = await fetch(url, { method: 'POST', body: form });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    messages.push({ role: 'user',      text: data.userText });
    messages.push({ role: 'assistant', text: data.replyText, audioUrl: data.audioUrl });
    await nextTick();
    const audioEls = document.querySelectorAll<HTMLAudioElement>('.audio-player');
    audioEls[audioEls.length - 1]?.play();
  } catch (err: unknown) {
    console.error(err);
    const msg = err instanceof Error ? err.message : String(err);
    ElMessage.error('通信出错：' + msg);
  }
}

// 结束对话：请求整轮点评
async function endConversation() {
  if (summaryRequested.value) return;  // 如果已经请求过，直接 return
  summaryRequested.value = true; 
  // 1. 不清空消息，先去拿“整轮点评”
  try {
    const url = `/api/OralPractice/summary?sessionId=${encodeURIComponent(sessionId.value)}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    // 2. 把点评推到消息列表尾部
    messages.push({
      role: 'evaluation',
      text: data.evaluationText,
      audioUrl: data.evaluationUrl
    });

    // 更新学习计划
    await updateOralStudyPlans();

    // 3. 播放点评音频
    await nextTick();
    const els = document.querySelectorAll<HTMLAudioElement>('.audio-player');
    els[els.length - 1]?.play();
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    ElMessage.error('获取点评失败：' + msg);
    summaryRequested.value = false;   // 失败的话允许重试
  }
}

// 重新开始：清空消息并重置会话
async function restartConversation() {
  // 清除所有消息，重置回最初两句
  messages.splice(0)
  messages.push({ role: 'user',      text: "Hi I'm Bingbing." })
  messages.push({ role: 'assistant', text: "Now let's start practice oral English!" })
  // 重置 summary flag
  summaryRequested.value = false
  // 重置口语练习时长
  totalOralTime.value = 0;
  // 重新申请 sessionId
  await startSession()
}

// 更新学习计划
const updateOralStudyPlans = async () => {
      try {
        // 1. 获取用户所有学习计划
        const response = await fetch(
          `/api/UserStudyPlan/DetailsByUser/${userId.value}`
        );
        
        if (!response.ok) throw new Error('获取学习计划失败');
        const plans = await response.json();

        // 2. 筛选当前未完成的学习计划
        const activePlans = plans.filter((plan: any) => {
          return plan.completed == 0;
        });

        // 3. 更新每个活跃计划
        for (const plan of activePlans) {
          const updatedPlan = {
            ...plan,
            learnedOralTime: (plan.learnedOralTime || 0) + Math.round(totalOralTime.value)
          };
          
          const updateResponse = await fetch(
            `/api/UserStudyPlan/${userId.value}/${plan.planId}`,
            {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                userId: updatedPlan.userId,
                planId: updatedPlan.planId,
                learnedWordCount: updatedPlan.learnedWordCount,
                learnedArticleCount: updatedPlan.learnedArticleCount,
                listeningTime: updatedPlan.listeningTime,
                learnedOralTime: updatedPlan.learnedOralTime
              })
            }
          );

          if (!updateResponse.ok) throw new Error('更新学习计划失败');
        }
        ElMessage.success('口语练习时长已更新到学习计划！');
      } catch (error) {
        console.error('更新学习计划出错:', error);
        ElMessage.error('更新学习计划失败，请稍后重试');
      }
    };

</script>

<style scoped>
  .chat-wrapper {
    display: flex;
    flex-direction: column;
    width: 100%;
    height: 100vh;
    padding-right: 16px;
  }
  .chat-container {
    display: flex;
    flex-direction: column;
    width: 100%;
    flex: 1;              /* 占满剩余空间 */
    min-height: 0; 
    padding: 0;
    border-bottom: 0;
    border-radius: 24px;
    border-bottom-left-radius: 0px;
    border-bottom-right-radius: 0px;
    box-shadow: none;
  }
  .chat-window {
    flex: 1;
    min-height: 0;
    height: 1000px;
    max-height: 60%;
    padding: 16px;
    border-color:#e5e5ea;
    background: #f5f7fa;
    overflow-y: auto;
  }
  .message-item {
    display: flex;
    align-items: flex-start;
    margin-bottom: 12px;
  }
  .message-item.assistant {
    justify-content: flex-start;
  }
  .message-item.user {
    justify-content: flex-end;
  }
  .avatar-wrapper {
    width: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  .avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
  }
  .bubble-wrapper {
    max-width: 70%;
    display: flex;
    flex-direction: column;
  }
  .bubble {
    padding: 8px 12px;
    border-radius: 18px;
    line-height: 1.4;
    word-wrap: break-word;
    text-align: left;
  }
  .bubble.assistant {
    background: #e5e5ea;
    color: #000;
    margin-left: 8px;
  }
  .bubble.user {
    background: #0066ff;
    color: #fff;
    margin-right: 8px;
  }
  .bubble.evaluation {
    background: #e6f7ff;
    color: #0050b3;
    margin-left: 8px;
    font-size: 18px;
    font-weight: 600;
  }
  .audio-player {
    height: 38px;
    margin-top: 4px;
    margin-left: 8px;
  }
  .chat-input-area {
    display: flex;
    justify-content: space-around;
    padding: 16px;
    background: #fff;
    height: 20%;
    border-radius: 0 0 16px 16px;
    margin-top: 0;
  }
  .chat-btn {
    width: 100px; 
    height: 48px;
    margin: 0 8px;
    border-radius: 16px; 
  }
  .chat-btn:first-child {
    margin-left: 0;
  }
  .chat-btn:last-child {
    margin-right: 0;
  }
  .chat-btn.danger {
    background: #ff4d4f;
    color: #fff;
  }
  .chat-btn.restart{
    background: purple;
    color: #fff;
  }
</style>