import axios from 'axios';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:3000';

export const apiClient = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

export async function fetcher<T = any>(url: string, config = {}) {
  const res = await apiClient.get<T>(url, config as any);
  return res.data;
}
