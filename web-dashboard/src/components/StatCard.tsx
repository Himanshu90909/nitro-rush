import React from 'react';

interface StatCardProps {
  label: string;
  value: string | number;
  subtext?: string;
  accent?: 'cyan' | 'magenta' | 'yellow' | 'green' | 'purple';
  icon?: React.ReactNode;
}

export const StatCard: React.FC<StatCardProps> = ({
  label,
  value,
  subtext,
  accent = 'cyan',
  icon,
}) => {
  const accentStyles = {
    cyan: {
      border: 'border-[#00E5FF]/30 hover:border-[#00E5FF]',
      glow: 'hover:shadow-[0_0_20px_rgba(0,229,255,0.25)]',
      text: 'text-[#00E5FF]',
      bar: 'bg-[#00E5FF]',
    },
    magenta: {
      border: 'border-[#FF2E92]/30 hover:border-[#FF2E92]',
      glow: 'hover:shadow-[0_0_20px_rgba(255,46,146,0.25)]',
      text: 'text-[#FF2E92]',
      bar: 'bg-[#FF2E92]',
    },
    yellow: {
      border: 'border-[#FFE600]/30 hover:border-[#FFE600]',
      glow: 'hover:shadow-[0_0_20px_rgba(255,230,0,0.25)]',
      text: 'text-[#FFE600]',
      bar: 'bg-[#FFE600]',
    },
    green: {
      border: 'border-[#00FF66]/30 hover:border-[#00FF66]',
      glow: 'hover:shadow-[0_0_20px_rgba(0,255,102,0.25)]',
      text: 'text-[#00FF66]',
      bar: 'bg-[#00FF66]',
    },
    purple: {
      border: 'border-[#A855F7]/30 hover:border-[#A855F7]',
      glow: 'hover:shadow-[0_0_20px_rgba(168,85,247,0.25)]',
      text: 'text-[#A855F7]',
      bar: 'bg-[#A855F7]',
    },
  };

  const style = accentStyles[accent];

  return (
    <div
      className={`bg-[#121829] border ${style.border} ${style.glow} transition-all duration-300 rounded-xl p-5 relative overflow-hidden group clip-corner`}
    >
      <div className={`absolute top-0 left-0 w-1 h-full ${style.bar}`} />
      <div className="flex items-center justify-between">
        <span className="text-xs uppercase tracking-widest font-heading font-semibold text-slate-400">
          {label}
        </span>
        {icon && <div className={`${style.text} opacity-80 group-hover:scale-110 transition-transform`}>{icon}</div>}
      </div>

      <div className="mt-3 flex items-baseline justify-between">
        <span className={`text-3xl font-extrabold font-heading tracking-tight ${style.text}`}>
          {value}
        </span>
      </div>

      {subtext && <p className="mt-1 text-xs text-slate-400 font-sans">{subtext}</p>}
    </div>
  );
};
