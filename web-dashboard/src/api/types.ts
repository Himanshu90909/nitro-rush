export interface ApiError {
  code: string;
  message: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  error: ApiError | null;
  timestamp?: string;
}

export interface User {
  id: string;
  email: string;
  username: string;
  headline?: string;
  role: 'PLAYER' | 'ADMIN';
  level: number;
  xp: number;
  credits: number;
  tokens: number;
  createdAt?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export type CarRarity = 'COMMON' | 'RARE' | 'EPIC' | 'LEGENDARY';

export interface Car {
  id: string;
  name: string;
  rarity: CarRarity;
  price: number;
  topSpeed: number;
  acceleration: number;
  handling: number;
  nitro: number;
  imageUrl?: string;
  description?: string;
}

export type UpgradeType = 'ENGINE' | 'TURBO' | 'TIRES' | 'BRAKES' | 'NITRO';

export interface GarageCar {
  id: string;
  carId: string;
  car: Car;
  engineLevel: number;
  turboLevel: number;
  tiresLevel: number;
  brakesLevel: number;
  nitroLevel: number;
  purchasedAt?: string;
}

export type EventType = 'DAILY' | 'WEEKLY' | 'SPECIAL' | 'COMMUNITY';

export type ObjectiveType = 'WIN_RACES' | 'USE_NITRO' | 'FINISH_RACES' | 'TOP_3_FINISH';

export interface EventObjective {
  id: string;
  type: ObjectiveType;
  targetCount: number;
  description: string;
}

export interface EventRewards {
  credits: number;
  xp: number;
  tokens: number;
}

export interface GameEvent {
  id: string;
  name: string;
  description: string;
  type?: EventType;
  startTime: string;
  endTime: string;
  active: boolean;
  objectives: EventObjective[];
  rewards: EventRewards;
}

export interface EventProgress {
  eventId: string;
  joined: boolean;
  objectiveProgress: Record<string, number>;
  completed: boolean;
  claimed: boolean;
}

export interface LeaderboardEntry {
  rank: number;
  playerId: string;
  username: string;
  score: number;
  wins?: number;
  avatarUrl?: string;
  isCurrentPlayer?: boolean;
}

export interface RaceResult {
  id: string;
  trackName: string;
  position: number;
  finishTime: string;
  topSpeedAchieved: number;
  rewardsEarned: {
    credits: number;
    xp: number;
  };
  timestamp: string;
}

export interface PlayerAnalytics {
  totalRaces: number;
  wins: number;
  podiums: number;
  totalDistanceKm: number;
  nitroUsedCount: number;
  favoriteCar?: string;
  winRate: number;
}

export type RewardSourceType = 'EVENT' | 'RACE' | 'ACHIEVEMENT';

export interface ClaimRewardRequest {
  sourceType: RewardSourceType;
  sourceId: string;
}

export interface ClaimRewardResponse {
  claimed: boolean;
  creditsEarned: number;
  xpEarned: number;
  tokensEarned: number;
}
