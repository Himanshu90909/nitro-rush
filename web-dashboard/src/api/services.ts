import { apiClient, unwrap } from './client';
import {
  AuthResponse,
  Car,
  ClaimRewardResponse,
  EventProgress,
  GameEvent,
  GarageCar,
  LeaderboardEntry,
  PlayerAnalytics,
  RaceResult,
  RewardSourceType,
  UpgradeType,
  User,
} from './types';

export const authService = {
  register: (data: { email: string; password: string; username: string }) =>
    unwrap<AuthResponse>(apiClient.post('/api/v1/auth/register', data)),

  login: (data: { email: string; password: string }) =>
    unwrap<AuthResponse>(apiClient.post('/api/v1/auth/login', data)),

  refresh: (refreshToken: string) =>
    unwrap<{ accessToken: string; refreshToken: string }>(
      apiClient.post('/api/v1/auth/refresh', { refreshToken })
    ),
};

export const profileService = {
  getProfile: () => unwrap<User>(apiClient.get('/api/v1/player/profile')),

  updateProfile: (data: { username?: string; headline?: string }) =>
    unwrap<User>(apiClient.put('/api/v1/player/profile', data)),
};

export const garageService = {
  getCars: () => unwrap<Car[]>(apiClient.get('/api/v1/cars')),

  getGarage: () => unwrap<GarageCar[]>(apiClient.get('/api/v1/garage')),

  purchaseCar: (carId: string) =>
    unwrap<GarageCar>(apiClient.post(`/api/v1/garage/cars/${carId}/purchase`)),

  upgradeCar: (carId: string, upgradeType: UpgradeType) =>
    unwrap<GarageCar>(
      apiClient.post(`/api/v1/garage/cars/${carId}/upgrade`, { upgradeType })
    ),
};

export const eventsService = {
  getEvents: () => unwrap<GameEvent[]>(apiClient.get('/api/v1/events')),

  getEvent: (id: string) => unwrap<GameEvent>(apiClient.get(`/api/v1/events/${id}`)),

  getEventProgress: (id: string) =>
    unwrap<EventProgress>(apiClient.get(`/api/v1/events/${id}/progress`)),

  joinEvent: (id: string) =>
    unwrap<EventProgress>(apiClient.post(`/api/v1/events/${id}/join`)),
};

export const rewardsService = {
  claimReward: (sourceType: RewardSourceType, sourceId: string) =>
    unwrap<ClaimRewardResponse>(
      apiClient.post('/api/v1/rewards/claim', { sourceType, sourceId })
    ),
};

export const leaderboardService = {
  getGlobalLeaderboard: (limit: number = 50) =>
    unwrap<LeaderboardEntry[]>(
      apiClient.get('/api/v1/leaderboards/global', { params: { limit } })
    ),

  getPlayerLeaderboard: (playerId: string) =>
    unwrap<LeaderboardEntry>(apiClient.get(`/api/v1/leaderboards/player/${playerId}`)),

  getNearbyLeaderboard: () =>
    unwrap<LeaderboardEntry[]>(apiClient.get('/api/v1/leaderboards/nearby')),
};

export const racesService = {
  getRaceResults: (raceId: string) =>
    unwrap<RaceResult>(apiClient.get(`/api/v1/races/${raceId}/results`)),
};

export const analyticsService = {
  getPlayerAnalytics: () =>
    unwrap<PlayerAnalytics>(apiClient.get('/api/v1/analytics/player')),
};
