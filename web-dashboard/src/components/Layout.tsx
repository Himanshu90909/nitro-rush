import React, { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react'
import { useAuth } from '../context/AuthContext';
import {
  LayoutDashboard,
  Car,
  Trophy,
  Calendar,
  User as UserIcon,
  LogOut,
  Menu,
  X,
  Zap,
  Coins,
  ShieldCheck,
  Sparkles,
} from 'lucide-react';

export const Layout: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navItems = [
    { label: 'Dashboard', path: '/', icon: LayoutDashboard },
    { label: 'Garage', path: '/garage', icon: Car },
    { label: 'Leaderboard', path: '/leaderboard', icon: Trophy },
    { label: 'Events', path: '/events', icon: Calendar },
    { label: 'Profile', path: '/profile', icon: UserIcon },
  ];

  return (
    <div className="min-h-screen bg-[#0B0F1A] text-slate-100 flex flex-col md:flex-row font-sans">
      {/* Mobile Top Header */}
      <header className="md:hidden bg-[#121829] border-b border-[#1F293D] px-4 py-3 flex justify-between items-center z-50">
        <div className="flex items-center gap-2">
          <span className="text-2xl">🏎️</span>
          <span className="font-heading font-extrabold text-lg tracking-wider text-[#00E5FF]">
            NITRO<span className="text-[#FF2E92]">RUSH</span>
          </span>
        </div>
        <button
          onClick={() => setMobileOpen(!mobileOpen)}
          className="p-2 text-slate-300 hover:text-white"
        >
          {mobileOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
        </button>
      </header>

      {/* Sidebar Navigation */}
      <aside
        className={`fixed md:static inset-y-0 left-0 z-40 w-64 bg-[#121829] border-r border-[#1F293D] p-5 flex flex-col justify-between transform transition-transform duration-300 ease-in-out ${
          mobileOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
        }`}
      >
        <div>
          {/* Logo */}
          <div className="hidden md:flex items-center gap-2 mb-8 px-2">
            <span className="text-3xl">🏎️</span>
            <div>
              <h1 className="font-heading font-extrabold text-xl tracking-wider text-[#00E5FF]">
                NITRO<span className="text-[#FF2E92]">RUSH</span>
              </h1>
              <span className="text-[10px] text-slate-400 font-heading uppercase tracking-widest block -mt-1">
                Player Web Hub
              </span>
            </div>
          </div>

          {/* Navigation Links */}
          <nav className="space-y-1.5">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink
                  key={item.path}
                  to={item.path}
                  end={item.path === '/'}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) =>
                    `flex items-center gap-3 px-4 py-3 rounded-lg font-heading font-semibold text-sm transition-all duration-200 ${
                      isActive
                        ? 'bg-[#00E5FF]/10 text-[#00E5FF] border border-[#00E5FF]/40 shadow-[0_0_12px_rgba(0,229,255,0.2)]'
                        : 'text-slate-400 hover:text-slate-200 hover:bg-[#1F293D]/50'
                    }`
                  }
                >
                  <Icon className="w-5 h-5" />
                  {item.label}
                </NavLink>
              );
            })}
          </nav>
        </div>

        {/* User Mini Profile & Logout */}
        <div className="pt-4 border-t border-[#1F293D]">
          <div className="flex items-center gap-3 mb-4 px-2">
            <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-[#00E5FF] to-[#FF2E92] p-0.5">
              <div className="w-full h-full bg-[#0B0F1A] rounded-full flex items-center justify-center font-heading font-bold text-sm text-[#00E5FF]">
                {user?.username?.substring(0, 2).toUpperCase() || 'NR'}
              </div>
            </div>
            <div className="overflow-hidden">
              <p className="font-heading font-bold text-sm text-white truncate">
                {user?.username || 'Player'}
              </p>
              <span className="text-xs text-slate-400 font-mono">
                LVL {user?.level || 1} Driver
              </span>
            </div>
          </div>

          <button
            onClick={handleLogout}
            className="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg border border-red-500/30 text-red-400 hover:bg-red-500/10 font-heading font-semibold text-xs transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Sign Out
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 overflow-y-auto">
        {/* Top Header Bar */}
        <header className="bg-[#121829]/80 backdrop-blur-md border-b border-[#1F293D] px-6 py-3 sticky top-0 z-30 flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-2 text-xs font-mono text-slate-400">
            <span className="w-2 h-2 rounded-full bg-[#00FF66] animate-pulse" />
            <span>MULTIPLAYER LIVE SERVERS ONLINE</span>
          </div>

          {/* Top Bar Player Currencies & Level */}
          <div className="flex items-center gap-3 flex-wrap">
            {/* Level Badge */}
            <div className="bg-[#0B0F1A] border border-[#00E5FF]/40 px-3 py-1 rounded-full flex items-center gap-1.5 shadow-[0_0_8px_rgba(0,229,255,0.2)]">
              <Sparkles className="w-3.5 h-3.5 text-[#00E5FF]" />
              <span className="text-xs font-heading font-extrabold text-[#00E5FF]">
                LVL {user?.level || 1}
              </span>
            </div>

            {/* Credits */}
            <div className="bg-[#0B0F1A] border border-[#FFE600]/40 px-3 py-1 rounded-full flex items-center gap-1.5 shadow-[0_0_8px_rgba(255,230,0,0.2)]">
              <Coins className="w-3.5 h-3.5 text-[#FFE600]" />
              <span className="text-xs font-heading font-extrabold text-[#FFE600]">
                {user?.credits?.toLocaleString() ?? 0} <span className="text-[10px] text-slate-400 font-mono">CR</span>
              </span>
            </div>

            {/* Tokens */}
            <div className="bg-[#0B0F1A] border border-[#FF2E92]/40 px-3 py-1 rounded-full flex items-center gap-1.5 shadow-[0_0_8px_rgba(255,46,146,0.2)]">
              <Zap className="w-3.5 h-3.5 text-[#FF2E92]" />
              <span className="text-xs font-heading font-extrabold text-[#FF2E92]">
                {user?.tokens?.toLocaleString() ?? 0} <span className="text-[10px] text-slate-400 font-mono">TK</span>
              </span>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="p-6 flex-1 max-w-7xl w-full mx-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
