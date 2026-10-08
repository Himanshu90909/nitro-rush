import React, { useEffect, useState } from 'react';
import { garageService } from '../api/services';
import { Car, GarageCar, UpgradeType } from '../api/types';
import { useAuth } from '../context/AuthContext';
import { CarCard } from '../components/CarCard';
import { Car as CarIcon, AlertCircle, CheckCircle2, ShoppingBag, Wrench, ShieldAlert } from 'lucide-react';

export const Garage: React.FC = () => {
  const { user, refreshUserProfile } = useAuth();
  const [allCars, setAllCars] = useState<Car[]>([]);
  const [garageCars, setGarageCars] = useState<GarageCar[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<'MY_GARAGE' | 'SHOWROOM'>('MY_GARAGE');

  // Confirmation modal state for purchasing
  const [selectedCarForPurchase, setSelectedCarForPurchase] = useState<Car | null>(null);

  // Toast / Error banner state
  const [toast, setToast] = useState<{ type: 'error' | 'success'; message: string } | null>(null);
  const [actionLoading, setActionLoading] = useState<boolean>(false);

  const loadData = async () => {
    setLoading(true);
    try {
      const [cars, garage] = await Promise.all([
        garageService.getCars(),
        garageService.getGarage(),
      ]);
      setAllCars(cars);
      setGarageCars(garage);
    } catch (err: unknown) {
      console.error('Failed loading garage:', err);
      showToast('error', 'Failed to load garage telemetry.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const showToast = (type: 'error' | 'success', message: string) => {
    setToast({ type, message });
    setTimeout(() => {
      setToast(null);
    }, 4000);
  };

  const handleConfirmPurchase = async () => {
    if (!selectedCarForPurchase) return;

    if ((user?.credits || 0) < selectedCarForPurchase.price) {
      showToast(
        'error',
        `Insufficient Credits! You need 💰 ${selectedCarForPurchase.price.toLocaleString()} CR but only have 💰 ${(user?.credits || 0).toLocaleString()} CR.`
      );
      setSelectedCarForPurchase(null);
      return;
    }

    setActionLoading(true);
    try {
      await garageService.purchaseCar(selectedCarForPurchase.id);
      showToast('success', `Successfully purchased ${selectedCarForPurchase.name}! Added to garage.`);
      setSelectedCarForPurchase(null);
      await refreshUserProfile();
      await loadData();
    } catch (err: unknown) {
      if (err instanceof Error) {
        showToast('error', err.message);
      } else {
        showToast('error', 'Failed to purchase car.');
      }
    } finally {
      setActionLoading(false);
    }
  };

  const handleUpgrade = async (carId: string, upgradeType: UpgradeType) => {
    setActionLoading(true);
    try {
      await garageService.upgradeCar(carId, upgradeType);
      showToast('success', `${upgradeType} upgraded successfully!`);
      await refreshUserProfile();
      await loadData();
    } catch (err: unknown) {
      if (err instanceof Error) {
        showToast('error', err.message);
      } else {
        showToast('error', 'Upgrade failed. Insufficient funds or max level reached.');
      }
    } finally {
      setActionLoading(false);
    }
  };

  // Map garage cars by carId for fast lookup
  const garageCarMap = new Map<string, GarageCar>(
    garageCars.map((gc) => [gc.carId || gc.car.id, gc])
  );

  const ownedCarCards = garageCars;
  const showroomCars = allCars.filter((car) => !garageCarMap.has(car.id));

  return (
    <div className="space-y-6">
      {/* Header Banner */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-[#121829] border border-[#1F293D] p-6 rounded-2xl">
        <div>
          <h1 className="text-3xl font-extrabold font-heading text-white flex items-center gap-3">
            <CarIcon className="w-8 h-8 text-[#00E5FF]" /> PADDOCK & GARAGE
          </h1>
          <p className="text-xs text-slate-400 mt-1">
            Manage owned fleet, install high-performance upgrades, or acquire new vehicles in the showroom.
          </p>
        </div>

        {/* Tab Selector Buttons */}
        <div className="flex bg-[#0B0F1A] p-1 rounded-xl border border-[#1F293D]">
          <button
            onClick={() => setActiveTab('MY_GARAGE')}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-lg font-heading font-bold text-xs transition-all ${
              activeTab === 'MY_GARAGE'
                ? 'bg-[#00E5FF] text-black shadow-[0_0_12px_rgba(0,229,255,0.4)]'
                : 'text-slate-400 hover:text-white'
            }`}
          >
            <Wrench className="w-4 h-4" /> My Garage ({garageCars.length})
          </button>
          <button
            onClick={() => setActiveTab('SHOWROOM')}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-lg font-heading font-bold text-xs transition-all ${
              activeTab === 'SHOWROOM'
                ? 'bg-[#FF2E92] text-white shadow-[0_0_12px_rgba(255,46,146,0.4)]'
                : 'text-slate-400 hover:text-white'
            }`}
          >
            <ShoppingBag className="w-4 h-4" /> Showroom ({showroomCars.length})
          </button>
        </div>
      </div>

      {/* Floating Toast Notification */}
      {toast && (
        <div
          className={`fixed top-20 right-6 z-50 p-4 rounded-xl shadow-2xl border flex items-center gap-3 max-w-md animate-bounce ${
            toast.type === 'error'
              ? 'bg-red-950/90 border-red-500 text-red-200'
              : 'bg-emerald-950/90 border-[#00FF66] text-[#00FF66]'
          }`}
        >
          {toast.type === 'error' ? (
            <ShieldAlert className="w-6 h-6 shrink-0 text-red-400" />
          ) : (
            <CheckCircle2 className="w-6 h-6 shrink-0 text-[#00FF66]" />
          )}
          <span className="text-xs font-heading font-semibold">{toast.message}</span>
        </div>
      )}

      {/* Content Area */}
      {loading ? (
        <div className="text-center py-20 text-slate-500 font-mono text-sm">
          Accessing paddock telemetry...
        </div>
      ) : activeTab === 'MY_GARAGE' ? (
        <div>
          {ownedCarCards.length === 0 ? (
            <div className="bg-[#121829] border border-[#1F293D] rounded-2xl p-12 text-center">
              <CarIcon className="w-12 h-12 text-slate-600 mx-auto mb-3" />
              <h2 className="text-xl font-heading font-bold text-white">Your Garage is Empty</h2>
              <p className="text-xs text-slate-400 mt-1 max-w-sm mx-auto mb-6">
                Head over to the Showroom tab to purchase your first high-performance racer!
              </p>
              <button
                onClick={() => setActiveTab('SHOWROOM')}
                className="bg-[#00E5FF] text-black font-heading font-extrabold text-xs uppercase px-6 py-3 rounded-lg shadow-[0_0_15px_rgba(0,229,255,0.4)]"
              >
                Open Showroom
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {ownedCarCards.map((gc) => (
                <CarCard
                  key={gc.id}
                  car={gc.car}
                  garageCar={gc}
                  onUpgrade={handleUpgrade}
                  isUpgrading={actionLoading}
                />
              ))}
            </div>
          )}
        </div>
      ) : (
        <div>
          {allCars.length === 0 ? (
            <div className="text-center py-12 text-slate-400 font-heading">
              No vehicles available in showroom catalog.
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {allCars.map((car) => {
                const garageCar = garageCarMap.get(car.id);
                return (
                  <CarCard
                    key={car.id}
                    car={car}
                    garageCar={garageCar}
                    onPurchase={(selected) => setSelectedCarForPurchase(selected)}
                    onUpgrade={handleUpgrade}
                    isUpgrading={actionLoading}
                  />
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* Purchase Confirmation Modal */}
      {selectedCarForPurchase && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-[#121829] border border-[#00E5FF] rounded-2xl p-6 max-w-md w-full shadow-[0_0_30px_rgba(0,229,255,0.3)] clip-corner">
            <h3 className="text-2xl font-extrabold font-heading text-white mb-2">
              CONFIRM PURCHASE
            </h3>
            <p className="text-xs text-slate-300 mb-4">
              Are you sure you want to purchase <span className="text-[#00E5FF] font-bold">{selectedCarForPurchase.name}</span>?
            </p>

            <div className="bg-[#0B0F1A] border border-[#1F293D] p-4 rounded-xl mb-6 space-y-2">
              <div className="flex justify-between text-xs">
                <span className="text-slate-400">Vehicle Cost:</span>
                <span className="font-heading font-bold text-[#FFE600]">
                  💰 {selectedCarForPurchase.price.toLocaleString()} CR
                </span>
              </div>
              <div className="flex justify-between text-xs">
                <span className="text-slate-400">Your Current Balance:</span>
                <span className="font-heading font-bold text-white">
                  💰 {(user?.credits || 0).toLocaleString()} CR
                </span>
              </div>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setSelectedCarForPurchase(null)}
                className="flex-1 bg-[#1F293D] hover:bg-slate-700 text-slate-300 font-heading font-bold text-xs uppercase py-3 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmPurchase}
                disabled={actionLoading}
                className="flex-1 bg-gradient-to-r from-[#00E5FF] to-cyan-500 hover:from-[#FF2E92] hover:to-pink-600 text-black font-heading font-extrabold text-xs uppercase py-3 rounded-lg shadow-[0_0_15px_rgba(0,229,255,0.4)] transition-all disabled:opacity-50"
              >
                {actionLoading ? 'Purchasing...' : 'Confirm Buy'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
