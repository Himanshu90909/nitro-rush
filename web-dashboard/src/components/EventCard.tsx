import React, { useEffect, useState } from 'react';
import { EventProgress, GameEvent } from '../api/types';
import { ProgressBar } from './ProgressBar';
import { Clock, Flag, Gift, CheckCircle2, Play } from 'lucide-react';

interface EventCardProps {
  event: GameEvent;
  progress?: EventProgress;
  onJoin?: (eventId: string) => void;
  onClaim?: (eventId: string) => void;
  isJoining?: boolean;
  isClaiming?: boolean;
}

export const EventCard: React.FC<EventCardProps> = ({
  event,
  progress,
  onJoin,
  onClaim,
  isJoining = false,
  isClaiming = false,
}) => {
  const [timeLeft, setTimeLeft] = useState<string>('');

  useEffect(() => {
    const calculateTimeLeft = () => {
      const end = new Date(event.endTime).getTime();
      const now = new Date().getTime();
      const diff = end - now;

      if (diff <= 0) {
        setTimeLeft('EVENT ENDED');
        return;
      }

      const hours = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

      setTimeLeft(
        `${hours.toString().padStart(2, '0')}:${minutes
          .toString()
          .padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
      );
    };

    calculateTimeLeft();
    const interval = setInterval(calculateTimeLeft, 1000);
    return () => clearInterval(interval);
  }, [event.endTime]);

  const joined = progress?.joined ?? false;
  const claimed = progress?.claimed ?? false;

  // Calculate overall event completion
  const allCompleted =
    event.objectives.length > 0 &&
    event.objectives.every((obj) => {
      const current = progress?.objectiveProgress?.[obj.id] || 0;
      return current >= obj.targetCount;
    });

  return (
    <div className="bg-[#121829] border border-[#1F293D] hover:border-[#FF2E92]/50 rounded-xl p-5 flex flex-col justify-between transition-all duration-300 relative group overflow-hidden clip-corner">
      {/* Top Banner */}
      <div>
        <div className="flex justify-between items-start mb-2">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-[10px] bg-[#FF2E92]/20 text-[#FF2E92] border border-[#FF2E92]/40 font-heading font-bold px-2 py-0.5 rounded uppercase tracking-wider">
                {event.type || 'LIVE EVENT'}
              </span>
              <span className="text-xs text-slate-400 font-mono flex items-center gap-1">
                <Clock className="w-3.5 h-3.5 text-[#00E5FF]" /> {timeLeft}
              </span>
            </div>
            <h3 className="text-xl font-bold font-heading text-white mt-1 group-hover:text-[#FF2E92] transition-colors">
              {event.name}
            </h3>
          </div>
        </div>

        <p className="text-xs text-slate-300 mb-4">{event.description}</p>

        {/* Objectives Progress */}
        <div className="space-y-3 mb-5 bg-[#0B0F1A] p-3 rounded-lg border border-[#1F293D]">
          <span className="text-[11px] font-heading text-slate-400 uppercase tracking-wider block mb-2">
            Event Objectives
          </span>
          {event.objectives.map((obj) => {
            const current = progress?.objectiveProgress?.[obj.id] || 0;
            return (
              <ProgressBar
                key={obj.id}
                current={current}
                max={obj.targetCount}
                label={obj.description}
                color={current >= obj.targetCount ? 'green' : 'cyan'}
                size="sm"
              />
            );
          })}
        </div>

        {/* Rewards Preview */}
        {event.rewards && (
          <div className="mb-4">
            <span className="text-[11px] font-heading text-slate-400 uppercase tracking-wider block mb-1">
              Event Rewards
            </span>
            <div className="flex gap-2">
              {event.rewards.credits > 0 && (
                <span className="bg-[#1F293D] text-[#FFE600] text-xs font-heading font-bold px-2.5 py-1 rounded border border-[#FFE600]/30">
                  +{event.rewards.credits.toLocaleString()} CR
                </span>
              )}
              {event.rewards.xp > 0 && (
                <span className="bg-[#1F293D] text-[#00E5FF] text-xs font-heading font-bold px-2.5 py-1 rounded border border-[#00E5FF]/30">
                  +{event.rewards.xp.toLocaleString()} XP
                </span>
              )}
              {event.rewards.tokens > 0 && (
                <span className="bg-[#1F293D] text-[#FF2E92] text-xs font-heading font-bold px-2.5 py-1 rounded border border-[#FF2E92]/30">
                  +{event.rewards.tokens.toLocaleString()} TOKENS
                </span>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Action Footer */}
      <div className="pt-3 border-t border-[#1F293D] flex justify-end">
        {!joined ? (
          <button
            onClick={() => onJoin && onJoin(event.id)}
            disabled={isJoining}
            className="w-full bg-gradient-to-r from-[#FF2E92] to-pink-600 hover:from-[#00E5FF] hover:to-cyan-400 text-white hover:text-black font-heading font-bold text-xs uppercase py-2.5 px-4 rounded-lg shadow-[0_0_15px_rgba(255,46,146,0.3)] transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50"
          >
            <Play className="w-4 h-4" /> Join Event
          </button>
        ) : claimed ? (
          <button
            disabled
            className="w-full bg-[#1F293D] text-[#00FF66] font-heading font-bold text-xs uppercase py-2.5 px-4 rounded-lg flex items-center justify-center gap-2 cursor-default border border-[#00FF66]/30"
          >
            <CheckCircle2 className="w-4 h-4 text-[#00FF66]" /> Reward Claimed
          </button>
        ) : allCompleted ? (
          <button
            onClick={() => onClaim && onClaim(event.id)}
            disabled={isClaiming}
            className="w-full bg-gradient-to-r from-[#00FF66] to-emerald-500 text-black font-heading font-extrabold text-xs uppercase py-2.5 px-4 rounded-lg shadow-[0_0_15px_rgba(0,255,102,0.4)] transition-all duration-300 flex items-center justify-center gap-2 animate-pulse disabled:opacity-50"
          >
            <Gift className="w-4 h-4" /> Claim Event Reward
          </button>
        ) : (
          <button
            disabled
            className="w-full bg-[#1F293D] text-slate-300 font-heading font-bold text-xs uppercase py-2.5 px-4 rounded-lg flex items-center justify-center gap-2 border border-slate-700"
          >
            <Flag className="w-4 h-4 text-[#00E5FF]" /> In Progress
          </button>
        )}
      </div>
    </div>
  );
};
