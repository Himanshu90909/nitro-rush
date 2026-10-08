import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import {
  analyticsService,
  garageService,
  leaderboardService,
} from '../api/services';
import { GarageCar, LeaderboardEntry, PlayerAnalytics } from '../api/types';
import { StatCard } from '../components/StatCard';
import { ProgressBar } from '../components/ProgressBar';
import { LeaderboardRow } from '../components/LeaderboardRow';
import { CarCard } from '../components/CarCard';
import {
  Trophy,
  Award,
  Zap,
  Car as CarIcon,
  Flame,
  Activity,
  Users,
  ChevronRight,
} from 'lucide-react';
import { Link } from 'react-router-dom';

export const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [analytics, setAnalytics] = useState<PlayerAnalytics | null>(null);
  const [nearbyPlayers, setNearbyPlayers] = useState<LeaderboardEntry[]>([]);
  const [garageCars, setGarageCars] = useState<GarageCar[]>([]);
  const [selfLeaderboard, setSelfLeaderboard] = useState<LeaderboardEntry | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const loadDashboardData = async () => {
      setLoading(true);
      try {
        const [analyticsData, nearbyData, garageData] = await Promise.allSettled([
          analyticsService.getPlayerAnalytics(),
          leaderboardService.getNearbyLeaderboard(),
          garageService.getGarage(),
        ]);

        if (analyticsData.status === 'fulfilled') {
          setAnalytics(analyticsData.value);
        }
        if (nearbyData.status === 'fulfilled') {
          setNearbyPlayers(nearbyData.value);
          if (user?.id) {
            const selfEntry = nearbyData.value.find((p) => p.playerId === user.id);
            if (selfEntry) setSelfLeaderboard(selfEntry);
          }
        }
        if (garageData.status === 'fulfilled') {
          setGarageCars(garageData.value);
        }
      } catch (err) {
        console.error('Failed loading dashboard data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadDashboardData();
  }, [user?.id]);

  // Calculate XP threshold for current level (e.g., Level * 1000 XP)
  const currentXp = user?.xp || 0;
  const xpForNextLevel = ((user?.level || 1) + 1) * 1000;

  return (
    <div className="space-[#0B0F1A] space-y-8">
      {/* Welcome Banner */}
      <div className="bg-gradient-to-r from-[#121829] via-[#161F33] to-[#121829] border border-[#00E5FF]/30 rounded-2xl p-6 relative overflow-hidden clip-corner shadow-[0_0_25px_rgba(0,229,255,0.1)]">
        <div className="absolute -right-10 -bottom-10 opacity-10 text-9xl pointer-events-none select-none">
          🏎️
        </div>

        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <span className="bg-[#00E5FF] text-black text-[10px] font-heading font-extrabold px-2 py-0.5 rounded tracking-wider">
                DRIVER STATUS ACTIVE
              </span>
              <span className="text-xs text-slate-400 font-mono">
                {user?.headline || 'Ready to dominate the track'}
              </span>
            </div>
            <h1 className="text-3xl md:text-4xl font-extrabold font-heading text-white">
              WELCOME BACK, <span className="text-[#00E5FF]">{user?.username || 'DRIVER'}</span>
            </h1>
            <p className="text-sm text-slate-300 mt-1 max-w-xl">
              Track telemetry initialized. View nearby competitors, check garage performance upgrades, and join active seasonal events.
            </p>
          </div>

          <div className="bg-[#0B0F1A]/80 border border-[#1F293D] p-4 rounded-xl min-w-[260px]">
            <div className="flex justify-between items-center mb-2">
              <span className="text-xs font-heading text-slate-300 font-bold flex items-center gap-1.5">
                <Zap className="w-4 h-4 text-[#00E5FF]" /> LEVEL PROGRESS
              </span>
              <span className="text-xs font-heading font-extrabold text-[#00E5FF]">
                LVL {user?.level || 1}
              </span>
            </div>
            <ProgressBar
              current={currentXp}
              max={xpForNextLevel}
              color="cyan"
              size="md"
            />
          </div>
        </div>
      </div>

      {/* Key Telemetry Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Global Rank"
          value={selfLeaderboard ? `#${selfLeaderboard.rank}` : '#--'}
          subtext="Based on competitive leaderboard points"
          accent="yellow"
          icon={<Trophy className="w-6 h-6" />}
        />
        <StatCard
          label="Total Races"
          value={analytics?.totalRaces ?? (loading ? '...' : 0)}
          subtext={`Win Rate: ${analytics?.winRate ? Math.round(analytics.winRate * 100) : 0}%`}
          accent="cyan"
          icon={<Activity className="w-6 h-6" />}
        />
        <StatCard
          label="Victories"
          value={analytics?.wins ?? (loading ? '...' : 0)}
          subtext={`Podiums: ${analytics?.podiums ?? 0}`}
          accent="green"
          icon={<Award className="w-6 h-6" />}
        />
        <StatCard
          label="Cars Owned"
          value={garageCars.length}
          subtext={analytics?.favoriteCar ? `Favorite: ${analytics.favoriteCar}` : 'Paddock Garage'}
          accent="magenta"
          icon={<CarIcon className="w-6 h-6" />}
        />
      </div>

      {/* Main Grid: Nearby Leaderboard Competitors & Garage Quick View */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Nearby Competitors (Live Leaderboard) */}
        <div className="lg:col-span-2 bg-[#121829] border border-[#1F293D] rounded-xl p-6">
          <div className="flex items-center justify-between mb-6 pb-4 border-b border-[#1F293D]">
            <div>
              <h2 className="text-xl font-extrabold font-heading text-white flex items-center gap-2">
                <Users className="w-5 h-5 text-[#00E5FF]" /> NEARBY COMPETITORS
              </h2>
              <p className="text-xs text-slate-400 mt-0.5">
                Live rankings of drivers close to your current score
              </p>
            </div>
            <Link
              to="/leaderboard"
              className="text-xs font-heading font-bold text-[#00E5FF] hover:text-[#FF2E92] flex items-center gap-1 transition-colors"
            >
              Full Leaderboard <ChevronRight className="w-4 h-4" />
            </Link>
          </div>

          {loading ? (
            <div className="text-center py-12 text-slate-500 font-mono text-sm">
              Loading driver telemetry...
            </div>
          ) : nearbyPlayers.length > 0 ? (
            <div className="space-y-3">
              {nearbyPlayers.map((player) => (
                <LeaderboardRow
                  key={player.playerId}
                  entry={player}
                  isSelf={player.playerId === user?.id}
                />
              ))}
            </div>
          ) : (
            <div className="bg-[#0B0F1A] border border-[#1F293D] rounded-lg p-8 text-center text-slate-400">
              <p className="font-heading text-sm">No nearby drivers found in current rank division.</p>
            </div>
          )}
        </div>

        {/* Garage Quick Overview */}
        <div className="bg-[#121829] border border-[#1F293D] rounded-xl p-6 flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between mb-6 pb-4 border-b border-[#1F293D]">
              <div>
                <h2 className="text-xl font-extrabold font-heading text-white flex items-center gap-2">
                  <CarIcon className="w-5 h-5 text-[#FF2E92]" /> TOP GARAGE VEHICLE
                </h2>
                <p className="text-xs text-slate-400 mt-0.5">
                  Your primary active car in paddock
                </p>
              </div>
              <Link
                to="/garage"
                className="text-xs font-heading font-bold text-[#FF2E92] hover:text-[#00E5FF] flex items-center gap-1 transition-colors"
              >
                View Garage <ChevronRight className="w-4 h-4" />
              </Link>
            </div>

            {loading ? (
              <div className="text-center py-12 text-slate-500 font-mono text-sm">
                Fetching garage data...
              </div>
            ) : garageCars.length > 0 ? (
              <CarCard car={garageCars[0].car} garageCar={garageCars[0]} />
            ) : (
              <div className="bg-[#0B0F1A] border border-[#1F293D] rounded-lg p-6 text-center text-slate-400">
                <CarIcon className="w-10 h-10 text-slate-600 mx-auto mb-2" />
                <p className="font-heading text-sm text-white">No cars in garage yet.</p>
                <p className="text-xs text-slate-400 mt-1">Visit the showroom to buy your first racer!</p>
                <Link
                  to="/garage"
                  className="mt-4 inline-block bg-[#00E5FF] text-black font-heading font-bold text-xs uppercase px-4 py-2 rounded shadow-[0_0_10px_rgba(0,229,255,0.3)]"
                >
                  Go To Showroom
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
