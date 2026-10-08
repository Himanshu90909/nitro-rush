import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { ApiResponse } from './types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

// Request Interceptor: attach JWT
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: refresh token on 401
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiResponse<unknown>>) => {
    const originalRequest = error.config as CustomAxiosRequestConfig;

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;
      const refreshToken = localStorage.getItem('refreshToken');

      if (refreshToken) {
        try {
          const refreshResponse = await axios.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
            `${API_BASE_URL}/api/v1/auth/refresh`,
            { refreshToken }
          );

          if (refreshResponse.data.success && refreshResponse.data.data) {
            const { accessToken, refreshToken: newRefreshToken } = refreshResponse.data.data;
            localStorage.setItem('accessToken', accessToken);
            if (newRefreshToken) {
              localStorage.setItem('refreshToken', newRefreshToken);
            }
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
            }
            return apiClient(originalRequest);
          }
        } catch (refreshErr) {
          // Refresh failed
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
          if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
            window.location.href = '/login';
          }
          return Promise.reject(refreshErr);
        }
      }

      // No refresh token or retry failed
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

// Helper function to unwrap ApiResponse data
export async function unwrap<T>(promise: Promise<{ data: ApiResponse<T> }>): Promise<T> {
  try {
    const res = await promise;
    if (!res.data.success) {
      throw new Error(res.data.error?.message || 'API request failed');
    }
    return res.data.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.data) {
      const apiErr = err.response.data as ApiResponse<unknown>;
      if (apiErr.error?.message) {
        throw new Error(apiErr.error.message);
      }
    }
    if (err instanceof Error) {
      throw err;
    }
    throw new Error('An unexpected error occurred');
  }
}
