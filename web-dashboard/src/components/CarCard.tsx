import React from 'react';
import { Car, CarRarity, GarageCar, UpgradeType } from '../api/types';
import { ProgressBar } from './ProgressBar';
import { Zap, Shield, Flame, Gauge, Wrench, ShoppingCart } from 'lucide-react';

interface CarCardProps {
  car: Car;
  garageCar?: GarageCar;
  onPurchase?: (car: Car) => void;
  onUpgrade?: (carId: string, upgradeType: UpgradeType) => void;
  isPurchasing?: boolean;
  isUpgrading?: boolean;
}

export const CarCard: React.FC<CarCardProps> = ({
  car,
  garageCar,
  onPurchase,
  onUpgrade,
  isPurchasing = false,
  isUpgrading = false,
}) => {
  const isOwned = !!garageCar;

  const rarityColors: Record<CarRarity, { border: string; bg: string; text: string }> = {
    COMMON: { border: 'border-slate-600', bg: 'bg-slate-800/80', text: 'text-slate-300' },
    RARE: { border: 'border-blue-500', bg: 'bg-blue-950/80', text: 'text-blue-400' },
    EPIC: { border: 'border-[#A855F7]', bg: 'bg-purple-950/80', text: 'text-[#A855F7]' },
    LEGENDARY: { border: 'border-[#FFE600]', bg: 'bg-amber-950/80', text: 'text-[#FFE600]' },
  };

  const rarity = rarityColors[car.rarity] || rarityColors.COMMON;

  // Compute stats with upgrade bonuses (+5% per level above 1)
  const calcStat = (base: number, level: number = 1) => {
    return Math.min(100, Math.round(base * (1 + (level - 1) * 0.08)));
  };

  const currentTopSpeed = calcStat(car.topSpeed, garageCar?.engineLevel);
  const currentAccel = calcStat(car.acceleration, garageCar?.turboLevel);
  const currentHandling = calcStat(car.handling, garageCar?.handlingLevel ?? garageCar?.tiresLevel);
  const currentNitro = calcStat(car.nitro, garageCar?.nitroLevel);

  const upgradeCategories: { type: UpgradeType; label: string; level: number }[] = [
    { type: 'ENGINE', label: 'Engine', level: garageCar?.engineLevel || 1 },
    { type: 'TURBO', label: 'Turbo', level: garageCar?.turboLevel || 1 },
    { type: 'TIRES', label: 'Tires', level: garageCar?.tiresLevel || 1 },
    { type: 'BRAKES', label: 'Brakes', level: garageCar?.brakesLevel || 1 },
    { type: 'NITRO', label: 'Nitro', level: garageCar?.nitroLevel || 1 },
  ];

  return (
    <div className={`bg-[#121829] border ${isOwned ? 'border-[#00E5FF]/40' : 'border-[#1F293D]'} rounded-xl p-5 flex flex-col justify-between hover:border-[#00E5FF] transition-all duration-300 relative group overflow-hidden`}>
      {/* Top Banner & Rarity */}
      <div>
        <div className="flex justify-between items-start mb-3">
          <div>
            <h3 className="text-xl font-bold font-heading text-white tracking-wide group-hover:text-[#00E5FF] transition-colors">
              {car.name}
            </h3>
            <p className="text-xs text-slate-400 line-clamp-1 mt-0.5">{car.description || 'High performance racing machine.'}</p>
          </div>
          <span className={`text-[10px] font-heading font-bold px-2.5 py-1 rounded border uppercase tracking-widest ${rarity.border} ${rarity.bg} ${rarity.text}`}>
            {car.rarity}
          </span>
        </div>

        {/* Car Visual Preview */}
        <div className="w-full h-36 bg-[#0B0F1A] rounded-lg border border-[#1F293D] mb-4 flex items-center justify-center relative overflow-hidden group-hover:border-[#00E5FF]/50 transition-colors">
          <div className="absolute inset-0 bg-gradient-to-t from-[#0B0F1A] via-transparent to-transparent z-10" />
          {car.imageUrl ? (
            <img src={car.imageUrl} alt={car.name} className="object-contain h-28 z-0 group-hover:scale-105 transition-transform duration-300" />
          ) : (
            <div className="flex flex-col items-center justify-center text-slate-600">
              <span className="text-5xl">🏎️</span>
              <span className="text-xs font-mono uppercase tracking-widest mt-1 text-slate-500">Nitro Rush Spec</span>
            </div>
          )}
          {isOwned && (
            <span className="absolute top-2 right-2 z-20 bg-[#00E5FF]/10 text-[#00E5FF] border border-[#00E5FF]/40 px-2 py-0.5 rounded text-[10px] font-heading font-semibold">
              OWNED
            </span>
          )}
        </div>

        {/* Base Performance Stats */}
        <div className="space-y-2 mb-4">
          <ProgressBar current={currentTopSpeed} max={100} label="Top Speed" color="cyan" size="sm" />
          <ProgressBar current={currentAccel} max={100} label="Acceleration" color="magenta" size="sm" />
          <ProgressBar current={currentHandling} max={100} label="Handling" color="purple" size="sm" />
          <ProgressBar current={currentNitro} max={100} label="Nitro Boost" color="yellow" size="sm" />
        </div>
      </div>

      {/* Upgrades or Purchase Section */}
      <div className="pt-3 border-t border-[#1F293D]">
        {isOwned ? (
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-xs font-heading text-slate-300 font-semibold flex items-center gap-1">
                <Wrench className="w-3.5 h-3.5 text-[#00E5FF]" /> PERFORMANCE UPGRADES
              </span>
            </div>
            <div className="space-y-2">
              {upgradeCategories.map((upg) => (
                <div key={upg.type} className="flex items-center justify-between bg-[#0B0F1A] px-2.5 py-1.5 rounded border border-[#1F293D]">
                  <span className="text-xs text-slate-300 font-medium">{upg.label}</span>
                  
                  <div className="flex items-center gap-2">
                    {/* Level Pips 1 to 5 */}
                    <div className="flex gap-1">
                      {[1, 2, 3, 4, 5].map((pip) => (
                        <div
                          key={pip}
                          className={`w-2.5 h-2.5 rounded-sm ${
                            pip <= upg.level ? 'bg-[#00E5FF] shadow-[0_0_6px_rgba(0,229,255,0.8)]' : 'bg-slate-800 border border-slate-700'
                          }`}
                        />
                      ))}
                    </div>

                    {/* Upgrade Action */}
                    {upg.level < 5 ? (
                      <button
                        onClick={() => onUpgrade && onUpgrade(car.id, upg.type)}
                        disabled={isUpgrading}
                        className="text-[10px] bg-[#00E5FF]/10 hover:bg-[#00E5FF] text-[#00E5FF] hover:text-black font-heading font-bold px-2 py-0.5 rounded border border-[#00E5FF]/40 transition-colors disabled:opacity-50"
                      >
                        + LVL
                      </button>
                    ) : (
                      <span className="text-[10px] text-[#00FF66] font-heading font-bold">MAX</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <div className="flex items-center justify-between">
            <div>
              <span className="text-[10px] text-slate-400 uppercase font-heading block">Purchase Price</span>
              <span className="text-xl font-heading font-bold text-[#FFE600] flex items-center gap-1">
                💰 {car.price.toLocaleString()} <span className="text-xs text-slate-400 font-normal">CR</span>
              </span>
            </div>

            <button
              onClick={() => onPurchase && onPurchase(car)}
              disabled={isPurchasing}
              className="bg-gradient-to-r from-[#00E5FF] to-cyan-500 hover:from-[#FF2E92] hover:to-pink-600 text-black font-heading font-bold text-xs uppercase px-4 py-2.5 rounded-lg shadow-[0_0_15px_rgba(0,229,255,0.3)] transition-all duration-300 flex items-center gap-1.5 disabled:opacity-50"
            >
              <ShoppingCart className="w-4 h-4" /> Buy Car
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
