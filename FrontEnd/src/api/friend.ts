import api from "../utils/axios";

export interface RawSearchItem {
  friendsId: string;        
  friendName: string;
  friendAvatarUrl?: string;
}

// 好友记录
export interface FriendRecord {
  friendsId: number;
  userId: string;
  friendsUserId: string;
  friendUserName?: string;
  friendAvatarUrl?: string;
  status: number;
  statusText?: string;
  createdAt: string;
  updateAt?: string;
}

// 好友请求项
export interface RequestItem{
    toUserId: string;
}

// 搜索好友
export interface SearchResult {
    userId: string;
    name: string;
    avatar?: string;
}

export const FriendsAPI = {
    list:                   () => api.get<FriendRecord[]>(`api/Friends`),

    incoming:               () => api.get<FriendRecord[]>(`api/Friends/requests`),

    sent:                   () => api.get<RequestItem[]>(`api/Friends/sent-requests`),

    request:                (friendsUserId: string) => api.post('api/Friends/request', { friendsUserId}),

    respond:                (friendsId:string, status:number) => api.put('api/Friends/status',
                                                                    { friendsId, status },
                                                                    { headers: { 'Content-Type': 'application/json' } }
                                                                    ),

    remove:                 (friendsId:number) => api.delete(`api/Friends/${friendsId}`),

    check:                  (friendUserId: string) => api.get<{status: number}>(`api/Friends/check/${friendUserId}`),

    count:                  () => api.get<{count: number}>(`api/Friends/count`),

    searchRaw: (q: string) =>
    api.get<RawSearchItem[]>('api/Friends/search', { params: { q } }),

    // 映射
    search: async (nickname: string) => {
    const { data } = await api.get<RawSearchItem[]>('api/Friends/search-users', { params: { nickname } });
    const normalized: SearchResult[] = data.map(it => ({
      userId: it.friendsId,
      name: it.friendName,
      avatar: it.friendAvatarUrl,
    }));
    return { data: normalized };
  },
};
