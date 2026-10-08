import React, { createContext, useContext, useEffect, useState } from 'react';
import { User } from '../api/types';
import { authService, profileService } from '../api/services';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<User>;
  register: (email: string, password: string, username: string) => Promise<User>;
  logout: () => void;
  refreshUserProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('user');
    return saved ? JSON.parse(saved) : null;
  });
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem('accessToken')
  );
  const [isLoading, setIsLoading] = useState<boolean>(true);

  const refreshUserProfile = async () => {
    try {
      const updatedUser = await profileService.getProfile();
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
    } catch (err) {
      console.error('Failed to refresh profile:', err);
    }
  };

  useEffect(() => {
    const initAuth = async () => {
      const storedToken = localStorage.getItem('accessToken');
      if (storedToken) {
        try {
          const profile = await profileService.getProfile();
          setUser(profile);
          localStorage.setItem('user', JSON.stringify(profile));
        } catch (err) {
          console.error('Auth check failed:', err);
          // Token invalid
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
          setUser(null);
          setToken(null);
        }
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string): Promise<User> => {
    const res = await authService.login({ email, password });
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    localStorage.setItem('user', JSON.stringify(res.user));
    setToken(res.accessToken);
    setUser(res.user);
    return res.user;
  };

  const register = async (
    email: string,
    password: string,
    username: string
  ): Promise<User> => {
    const res = await authService.register({ email, password, username });
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    localStorage.setItem('user', JSON.stringify(res.user));
    setToken(res.accessToken);
    setUser(res.user);
    return res.user;
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        login,
        register,
        logout,
        refreshUserProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
