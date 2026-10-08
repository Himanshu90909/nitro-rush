import React from 'react';

interface ProgressBarProps {
  current: number;
  max: number;
  label?: string;
  color?: 'cyan' | 'magenta' | 'yellow' | 'green' | 'purple';
  showPercent?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  current,
  max,
  label,
  color = 'cyan',
  showPercent = true,
  size = 'md',
}) => {
  const percent = Math.min(100, Math.max(0, Math.round((current / (max || 1)) * 100)));

  const gradients = {
    cyan: 'from-[#00E5FF] to-cyan-400 shadow-[0_0_10px_rgba(0,229,255,0.5)]',
    magenta: 'from-[#FF2E92] to-pink-500 shadow-[0_0_10px_rgba(255,46,146,0.5)]',
    yellow: 'from-[#FFE600] to-amber-400 shadow-[0_0_10px_rgba(255,230,0,0.5)]',
    green: 'from-[#00FF66] to-emerald-400 shadow-[0_0_10px_rgba(0,255,102,0.5)]',
    purple: 'from-[#A855F7] to-indigo-500 shadow-[0_0_10px_rgba(168,85,247,0.5)]',
  };

  const heights = {
    sm: 'h-1.5',
    md: 'h-3',
    lg: 'h-5',
  };

  return (
    <div className="w-full">
      {(label || showPercent) && (
        <div className="flex justify-between items-center mb-1 text-xs">
          {label && <span className="text-slate-300 font-medium font-heading tracking-wider">{label}</span>}
          <span className="text-slate-400 font-mono">
            {current} / {max} {showPercent && `(${percent}%)`}
          </span>
        </div>
      )}
      <div className={`w-full bg-[#1F293D] rounded-full overflow-hidden p-0.5 border border-slate-800 ${heights[size]}`}>
        <div
          className={`h-full rounded-full bg-gradient-to-r ${gradients[color]} transition-all duration-500 ease-out`}
          style={{ width: `${percent}%` }}
        />
      </div>
    </div>
  );
};
