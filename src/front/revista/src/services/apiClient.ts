import { apiClient, fetcher } from '@/lib/fetcher';

export const api = {
  
  async getHome() {
    return fetcher('/api/home');
  },
  async getPosts(params?: any) {
    const res = await apiClient.get('/api/posts', { params });
    return res.data;
  },
};
