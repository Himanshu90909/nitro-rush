import React, { useEffect, useState } from 'react';
import { leaderboardService } from '../api/services';
import { LeaderboardEntry } from '../api/types';
import { useAuth } from '../context/AuthContext';
import { LeaderboardRow } from '../components/LeaderboardRow';
import { Trophy, Globe, Users, Flame, Search } from 'lucide-react';

export const Leaderboard: React.FC = () => {
  const { user } = useAuth();
  const [globalEntries, setGlobalEntries] = useState<LeaderboardEntry[]>([]);
  const [nearbyEntries, setNearbyEntries] = useState<LeaderboardEntry[]>([]);
  const [selfEntry, setSelfEntry] = useState<LeaderboardEntry | null>(null);

  const [activeTab, setActiveTab] = useState<'GLOBAL' | 'NEARBY'>('GLOBAL');
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const fetchLeaderboards = async () => {
      setLoading(true);
      try {
        const [globalRes, nearbyRes] = await Promise.allSettled([
          leaderboardService.getGlobalLeaderboard(50),
          leaderboardService.getNearbyLeaderboard(),
        ]);

        if (globalRes.status === 'fulfilled') {
          setGlobalEntries(globalRes.value);
        }
        if (nearbyRes.status === 'fulfilled') {
          setNearbyEntries(nearbyRes.value);
        }

        if (user?.id) {
          try {
            const selfRes = await leaderboardService.getPlayerLeaderboard(user.id);
            setSelfEntry(selfRes);
          } catch {
            // If individual fetch fails, look in nearby or global
            const foundInNearby =
              nearbyRes.status === 'fulfilled'
                ? nearbyRes.value.find((e) => e.playerId === user.id)
                : null;
            if (foundInNearby) setSelfEntry(foundInNearby);
          }
        }
      } catch (err) {
        console.error('Leaderboard error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchLeaderboards();
  }, [user?.id]);

  const displayedList = activeTab === 'GLOBAL' ? globalEntries : nearbyEntries;

  const filteredEntries = displayedList.filter((entry) =>
    entry.username.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Title Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-[#121829] border border-[#1F293D] p-6 rounded-2xl">
        <div>
          <h1 className="text-3xl font-extrabold font-heading text-white flex items-center gap-3">
            <Trophy className="w-8 h-8 text-[#FFE600]" /> HALL OF SPEED LEADERBOARD
          </h1>
          <p className="text-xs text-slate-400 mt-1">
            Global rank standings of top live drivers across all multiplayer divisions.
          </p>
        </div>

        {/* Tab & Search controls */}
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-3" />
            <input
              type="text"
              placeholder="Search driver call-sign..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="bg-[#0B0F1A] border border-[#1F293D] focus:border-[#00E5FF] text-white text-xs rounded-xl pl-9 pr-4 py-2.5 outline-none font-sans w-full sm:w-56"
            />
          </div>

          <div className="flex bg-[#0B0F1A] p-1 rounded-xl border border-[#1F293D]">
            <button
              onClick={() => setActiveTab('GLOBAL')}
              className={`flex items-center gap-1.5 px-4 py-2 rounded-lg font-heading font-bold text-xs transition-all ${
                activeTab === 'GLOBAL'
                  ? 'bg-[#FFE600] text-black shadow-[0_0_12px_rgba(255,230,0,0.4)]'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              <Globe className="w-3.5 h-3.5" /> Top 50
            </button>
            <button
              onClick={() => setActiveTab('NEARBY')}
              className={`flex items-center gap-1.5 px-4 py-2 rounded-lg font-heading font-bold text-xs transition-all ${
                activeTab === 'NEARBY'
                  ? 'bg-[#00E5FF] text-black shadow-[0_0_12px_rgba(0,229,255,0.4)]'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              <Users className="w-3.5 h-3.5" /> Nearby
            </button>
          </div>
        </div>
      </div>

      {/* Pinned Self Rank Card */}
      {selfEntry && (
        <div className="bg-[#0B0F1A] border-2 border-[#00E5FF] rounded-2xl p-4 shadow-[0_0_20px_rgba(0,229,255,0.2)]">
          <span className="text-[10px] font-heading uppercase tracking-widest text-[#00E5FF] block mb-2 font-extrabold">
            Your Pinned Standing
          </span>
          <LeaderboardRow entry={selfEntry} isSelf={true} />
        </div>
      )}

      {/* Leaderboard Table */}
      <div className="bg-[#121829] border border-[#1F293D] rounded-2xl p-6">
        {loading ? (
          <div className="text-center py-16 text-slate-500 font-mono text-sm">
            Fetching global driver standings...
          </div>
        ) : filteredEntries.length > 0 ? (
          <div className="space-y-2.5">
            {filteredEntries.map((entry) => (
              <LeaderboardRow
                key={entry.playerId}
                entry={entry}
                isSelf={entry.playerId === user?.id}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-16 text-slate-400 font-heading">
            No driver match found for "{searchQuery}".
          </div>
        )}
      </div>
    </div>
  );
};
