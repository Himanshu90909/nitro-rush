import React from 'react';
import { LeaderboardEntry } from '../api/types';
import { Trophy, Award, Medal } from 'lucide-react';

interface LeaderboardRowProps {
  entry: LeaderboardEntry;
  isSelf?: boolean;
}

export const LeaderboardRow: React.FC<LeaderboardRowProps> = ({ entry, isSelf = false }) => {
  const getRankBadge = (rank: number) => {
    if (rank === 1) {
      return (
        <div className="w-8 h-8 rounded-full bg-amber-500/20 border border-amber-400 text-amber-300 flex items-center justify-center font-heading font-extrabold shadow-[0_0_10px_rgba(251,191,36,0.5)]">
          <Trophy className="w-4 h-4 text-amber-400" />
        </div>
      );
    }
    if (rank === 2) {
      return (
        <div className="w-8 h-8 rounded-full bg-slate-300/20 border border-slate-300 text-slate-200 flex items-center justify-center font-heading font-extrabold">
          <Award className="w-4 h-4 text-slate-300" />
        </div>
      );
    }
    if (rank === 3) {
      return (
        <div className="w-8 h-8 rounded-full bg-amber-700/20 border border-amber-600 text-amber-500 flex items-center justify-center font-heading font-extrabold">
          <Medal className="w-4 h-4 text-amber-600" />
        </div>
      );
    }
    return (
      <div className="w-8 h-8 text-slate-400 flex items-center justify-center font-heading font-bold text-sm">
        #{rank}
      </div>
    );
  };

  return (
    <div
      className={`flex items-center justify-between px-4 py-3 rounded-lg border transition-all duration-200 ${
        isSelf
          ? 'bg-[#00E5FF]/10 border-[#00E5FF] shadow-[0_0_15px_rgba(0,229,255,0.2)]'
          : 'bg-[#121829] border-[#1F293D] hover:border-slate-700'
      }`}
    >
      <div className="flex items-center gap-3">
        {getRankBadge(entry.rank)}
        <div>
          <div className="flex items-center gap-2">
            <span
              className={`font-heading font-bold ${
                isSelf ? 'text-[#00E5FF]' : 'text-slate-100'
              }`}
            >
              {entry.username}
            </span>
            {isSelf && (
              <span className="bg-[#00E5FF] text-black text-[10px] font-heading font-extrabold px-1.5 py-0.5 rounded uppercase">
                YOU
              </span>
            )}
          </div>
          {entry.wins !== undefined && (
            <span className="text-xs text-slate-400">{entry.wins} Victories</span>
          )}
        </div>
      </div>

      <div className="text-right">
        <span className="font-heading font-extrabold text-lg text-[#FFE600] tracking-wider">
          {entry.score.toLocaleString()}
        </span>
        <span className="block text-[10px] text-slate-400 uppercase font-heading">PTS</span>
      </div>
    </div>
  );
};
