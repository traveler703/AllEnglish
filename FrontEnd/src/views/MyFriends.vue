<template>
  <div class="shell">
    <!-- 左侧：好友列表 -->
    <section class="sidebar card">
      <header class="side-header">
        <div>
          <h2>我的好友</h2>
          <p class="muted">管理好友、处理请求、快速添加。</p>
        </div>
        <el-button type="primary" round @click="showAddDialog = true">＋ 添加好友</el-button>
      </header>

      <!-- 搜索 -->
      <div class="toolbar">
        <el-input
          v-model="searchQuery"
          placeholder="搜索好友（昵称或ID）"
          class="round-input"
          clearable
        >
          <template #prefix>🔎</template>
        </el-input>
      </div>

      <!-- 好友 / 请求 -->
      <el-tabs v-model="activeTab" class="round-tabs">
        <el-tab-pane label="好友列表" name="friends">
          <el-empty v-if="!filteredFriends.length" description="暂无好友，去添加一个吧～" />
          <div v-else class="friend-list">
            <div
              v-for="f in filteredFriends"
              :key="f.friendsId"
              class="friend-row"
              :class="{ active: selectedFriend && selectedFriend.friendsId === f.friendsId }"
              @click="selectFriend(f)"
            >
              <el-avatar :size="48" :src="f.friendAvatarUrl" />
              <div class="meta">
                <div class="name">{{ f.friendUserName || f.friendsUserId }}</div>
                <div class="sub">添加时间：{{ formatDate(f.createdAt) }}</div>
              </div>

              <el-popconfirm
                title="确定删除这个好友吗？"
                confirm-button-text="删除"
                cancel-button-text="取消"
                confirm-button-type="danger"
                @confirm.stop="removeFriend(f.friendsId)"
              >
                <template #reference>
                  <el-button class="pill danger" @click.stop>删</el-button>
                </template>
              </el-popconfirm>
            </div>
          </div>
        </el-tab-pane>

        <el-tab-pane label="好友请求" name="requests">
          <el-empty v-if="!friendRequests.length" description="暂无好友请求" />
          <div v-else class="request-list">
            <div v-for="r in friendRequests" :key="r.friendsId" class="request-row">
              <div class="who">
                <el-avatar :size="44" :src="r.friendAvatarUrl" /> <!-- 已添加 CSS 强制圆形 -->
                <div class="who-meta">
                  <div class="name">{{ r.friendUserName || '未知用户' }}</div> <!-- 修改：如果无昵称，显示“未知用户”而非 ID，提升美观 -->
                  <div class="sub">请求时间：{{ formatDate(r.createdAt) }}</div> <!-- 时间已缩小 -->
                </div>
              </div>
              <div class="ops">
                <el-button type="primary" round @click="respondRequest(r.userId, true)">接受</el-button>
                <el-button type="danger" round @click="respondRequest(r.userId, false)">拒绝</el-button>
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
    </section>

    <!-- 右侧：聊天面板占位 -->
    <section class="chat card">
      <header class="chat-header">
        <template v-if="selectedFriend">
          <div class="chat-peer">
            <el-avatar :size="44" :src="selectedFriend.friendAvatarUrl" />
            <div class="peer-meta">
              <div class="name">{{ selectedFriend.friendUserName || selectedFriend.friendsUserId }}</div>
              <div class="sub">已连接的好友</div>
            </div>
          </div>
          <div class="chat-actions">
            <el-button class="pill" disabled>更多</el-button>
          </div>
        </template>
        <template v-else>
          <div class="chat-empty-title">聊天面板</div>
        </template>
      </header>

      <main class="chat-body">
        <el-empty
          v-if="!selectedFriend"
          description="从左侧选择一个好友，聊天面板会显示在这里（功能暂未实现）"
        />
        <div v-else class="chat-placeholder">
          <el-empty description="聊天功能开发中，敬请期待～" />
        </div>
      </main>
    </section>

    <!-- 添加好友弹窗 -->
    <el-dialog v-model="showAddDialog" width="640px" class="round-dialog" :close-on-click-modal="false">
      <template #header><div class="dialog-title">添加好友</div></template>

      <div class="add-box">
        <div class="search-row">
          <el-input
            v-model="addKeyword"
            placeholder="按昵称或ID搜索用户"
            class="round-input"
            @keyup.enter="searchUsers"
          >
            <template #prefix>🔎</template>
          </el-input>
          <el-button type="primary" round @click="searchUsers">搜索</el-button>
          <el-button text @click="resetSearch">重置</el-button>
        </div>

        <div class="results inner-card" v-loading="searchLoading">
          <el-empty v-if="!searchLoading && !searchResults.length" description="输入关键词后搜索用户" />
          <div v-else class="result-list">
            <div v-for="u in searchResults" :key="u.userId" class="result-item">
              <el-avatar :size="44" :src="u.avatar" />
              <div class="result-meta">
                <div class="name">{{ u.name || u.userId }}</div>
                <div class="sub">{{ u.userId }}</div>
              </div>

              <template v-if="alreadyFriends.has(u.userId)">
                <el-tag type="success" round>已是好友</el-tag>
              </template>
              <template v-else-if="sentTo.has(u.userId)">
                <el-tag type="info" round>已发送</el-tag>
              </template>
              <template v-else>
                <el-button type="primary" round @click="sendFriendRequest(u.userId)">添加</el-button>
              </template>
            </div>
          </div>
        </div>
      </div>

      <template #footer><el-button round @click="showAddDialog = false">关闭</el-button></template>
    </el-dialog>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue';
import { FriendsAPI, type FriendRecord, type RequestItem } from '@/api/friend';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'MyFriends',
  setup() {
    const activeTab = ref<'friends'|'requests'>('friends');
    const searchQuery = ref('');

    const friends = ref<FriendRecord[]>([]);
    const friendRequests = ref<FriendRecord[]>([]);
    const selectedFriend = ref<FriendRecord | null>(null);

    const showAddDialog = ref(false);
    const addKeyword = ref('');
    const searchResults = ref<{ userId: string; name: string; avatar?: string }[]>([]);
    const searchLoading = ref(false);

    const alreadyFriends = ref<Set<string>>(new Set());
    const sentTo = ref<Set<string>>(new Set());

    const loadFriends = async () => {
      const res = await FriendsAPI.list();
      friends.value = res.data;
      alreadyFriends.value = new Set(res.data.map(x => x.friendsUserId));
      if (selectedFriend.value && !friends.value.find(f => f.friendsId === selectedFriend.value?.friendsId)) {
        selectedFriend.value = null;
      }
    };
    const loadRequests = async () => {
      const res = await FriendsAPI.incoming();
      friendRequests.value = res.data;
    };
    const refreshSent = async () => {
      try {
        const res = await FriendsAPI.sent();
        sentTo.value = new Set(res.data.map(x => x.toUserId));
      } catch {}
    };
    onMounted(async () => { await Promise.all([loadFriends(), loadRequests(), refreshSent()]); });

    const filteredFriends = computed(() =>
      friends.value.filter(f => {
        const name = (f.friendsUserId || f.friendUserName || '').toLowerCase();
        return name.includes(searchQuery.value.toLowerCase());
      })
    );

    const selectFriend = (f: FriendRecord) => { selectedFriend.value = f; };

    const searchUsers = async () => {
      const q = addKeyword.value.trim();
      if (!q) return;
      searchLoading.value = true;
      try { searchResults.value = (await FriendsAPI.search(q)).data; }
      finally { searchLoading.value = false; }
      console.log('raw search:', (await FriendsAPI.search(q)));
    };
    const resetSearch = () => { addKeyword.value = ''; searchResults.value = []; };
    const sendFriendRequest = async (toUserId: string) => {
      await FriendsAPI.request(toUserId);
      ElMessage.success('请求已发送'); sentTo.value.add(toUserId);
    };
    const respondRequest = async (friendsId: string, accept: boolean) => {
      await FriendsAPI.respond(friendsId, accept ? 1 : 2);
      await loadRequests();
      if (accept) await loadFriends();
      ElMessage.success(accept ? '已接受' : '已拒绝');
    };
    const removeFriend = async (friendsId: number) => {
      await FriendsAPI.remove(friendsId); await loadFriends(); ElMessage.success('已删除好友');
    };

    const formatDate = (s?: string) => {
      if (!s) return '未知';
      const d = new Date(s), p = (n:number)=> (n<10?`0${n}`:n);
      return `${d.getFullYear()}-${p(d.getMonth()+1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`;
    };

    watch(showAddDialog, v => { if (v) { addKeyword.value=''; searchResults.value=[]; } });

    return {
      activeTab, searchQuery, friends, friendRequests, selectedFriend,
      showAddDialog, addKeyword, searchResults, searchLoading,
      alreadyFriends, sentTo,
      filteredFriends, selectFriend,
      searchUsers, resetSearch, sendFriendRequest, respondRequest, removeFriend,
      formatDate
    };
  }
});
</script>

<style scoped>
.shell {
  --radius: 24px;
  width: 100vw;
  margin-left: calc(50% - 50vw);
  margin-right: calc(50% - 50vw);

  height: 100vh;
  min-height: 100vh;
  box-sizing: border-box;

  padding: 24px;
  background: linear-gradient(135deg, #eef5ff 0%, #f9fbff 100%);
  display: grid;
  grid-template-columns: 440px 1fr; /* 左更宽一点 */
  gap: 24px;
}

/* 统一卡片风格，圆角+阴影；overflow: hidden 让内部也显圆角 */
.card {
  background: #fff;
  border-radius: var(--radius);
  box-shadow: 0 10px 28px rgba(0,0,0,0.07);
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
  border: 1px solid #eef1f5;
}

/* 左侧 */
.sidebar { padding: 18px; }
.side-header {
  display: flex; justify-content: space-between; align-items: center;
  padding-bottom: 12px; margin-bottom: 12px; border-bottom: 1px solid #f0f2f6;
}
.side-header h2 { margin: 0; font-size: 22px; font-weight: 800; }
.muted { margin: 4px 0 0; color: #6b7280; font-size: 13px; }

.toolbar { display: flex; gap: 12px; margin-bottom: 10px; }
.round-input :deep(.el-input__wrapper) { border-radius: 999px !important; }

/* Tabs 圆角 */
.round-tabs { --el-tabs-header-height: 46px; }
.round-tabs :deep(.el-tabs__header) { padding: 0 6px; }
.round-tabs :deep(.el-tabs__nav-wrap) { border-radius: 999px; background:#f6f8ff; }
.round-tabs :deep(.el-tabs__active-bar) { border-radius: 6px; }
.round-tabs :deep(.el-tabs__item) {  
  padding-left: 24px !important;
  padding-right: 24px !important;
}

/* 列表 */
.friend-list { overflow: auto; padding: 6px 2px 12px; display: grid; gap: 12px; }
.friend-row {
  display: grid; grid-template-columns: 48px 1fr auto; align-items: center; gap: 12px;
  padding: 12px; border: 1px solid #edf0f5; border-radius: 18px;
  transition: background .2s, box-shadow .2s; cursor: pointer;
}
.friend-row:hover { background: #f8fbff; box-shadow: 0 6px 18px rgba(0,0,0,0.05); }
.friend-row.active { background: #eef5ff; border-color: #d8e6ff; }
.meta .name { font-weight: 700; font-size: 15px; }
.meta .sub { color: #8a93a6; font-size: 12px; }
.pill { border-radius: 999px; padding: 6px 14px; height: 32px; }
.pill.danger { color: #f56c6c; border-color: #f56c6c; }

/* 请求 */
.request-list { overflow: auto; padding: 6px 2px 12px; display: grid; gap: 12px; }
.request-row {
  display: grid; grid-template-columns: 1fr auto; align-items: center; gap: 16px; /* 增加间距，避免拥挤 */
  padding: 14px; /* 稍大 padding 提升美观 */
  border: 1px solid #edf0f5; border-radius: 20px; /* 更圆润 */
  transition: background 0.2s, box-shadow 0.2s;
  cursor: default; /* 非点击区域不需指针 */
}
.request-row:hover { background: #f9fcff; box-shadow: 0 4px 12px rgba(0,0,0,0.04); } /* 添加 hover 提升交互感 */
.who { display: flex; align-items: center; gap: 12px; }
.who-meta .name { font-weight: 700; font-size: 15px; } /* 昵称稍大突出 */
.who-meta .sub { color: #9ca3af; font-size: 11px; } /* 时间字体缩小到 11px，颜色更淡 */
.ops { display: flex; gap: 8px; }
.ops .el-button { padding: 0 16px; height: 32px; font-size: 13px; } /* 按钮更紧凑 */

/* 强制头像圆形 */
:deep(.el-avatar) {
  border-radius: 50% !important; /* 确保正圆 */
  object-fit: cover; /* 如果图片比例不对，强制覆盖填充 */
}

/* 右侧聊天占位 */
.chat { padding: 18px; }
.chat-header {
  display: flex; align-items: center; justify-content: space-between;
  padding-bottom: 12px; margin-bottom: 12px; border-bottom: 1px solid #f0f2f6;
}
.chat-peer { display: flex; align-items: center; gap: 12px; }
.peer-meta .name { font-weight: 800; font-size: 16px; }
.peer-meta .sub { color: #8a93a6; font-size: 12px; }
.chat-body { flex: 1; overflow: auto; display: grid; place-items: center; }

/* Dialog 圆角覆盖 */
.round-dialog :deep(.el-dialog__header),
.round-dialog :deep(.el-dialog__body),
.round-dialog :deep(.el-dialog) { border-radius: var(--radius); overflow: hidden; }

/* 弹窗内部 */
.add-box { display: grid; gap: 14px; }
.search-row { display: flex; gap: 12px; align-items: center; }
.inner-card {
  background: #fff; border-radius: 18px; padding: 12px; border: 1px solid #edf0f5;
}
.result-list { display: grid; gap: 12px; }
.result-item {
  display: grid; grid-template-columns: 44px 1fr auto; align-items: center; gap: 12px;
  padding: 12px; border: 1px dashed #e8edf5; border-radius: 18px;
}
.result-meta .name { font-weight: 700; }

/* Popconfirm 圆角（深度覆盖） */
:deep(.el-popconfirm) { border-radius: 16px !important; }

/* 窄屏回退：上下布局 */
@media (max-width: 1024px) {
  .shell { grid-template-columns: 1fr; height: auto; min-height: 100vh; }
  .chat { min-height: 50vh; }
}
</style>