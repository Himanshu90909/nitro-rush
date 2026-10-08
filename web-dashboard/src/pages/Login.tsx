import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { AlertCircle, Lock, Mail, ArrowRight } from 'lucide-react';

export const Login: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await login(email, password);
      navigate('/');
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Login failed. Please check your credentials.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0B0F1A] flex items-center justify-center p-4 relative overflow-hidden font-sans">
      {/* Neon Decorative Background Effects */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-[#00E5FF]/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-[#FF2E92]/10 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-md w-full bg-[#121829] border border-[#00E5FF]/40 shadow-[0_0_30px_rgba(0,229,255,0.15)] rounded-2xl p-8 relative z-10 clip-corner">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="text-5xl mb-2">🏎️</div>
          <h1 className="text-3xl font-extrabold font-heading text-[#00E5FF] tracking-wider">
            NITRO<span className="text-[#FF2E92]">RUSH</span>
          </h1>
          <p className="text-xs text-slate-400 font-heading uppercase tracking-widest mt-1">
            Multiplayer Arcade Racing Hub
          </p>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="mb-6 bg-red-950/60 border border-red-500/50 rounded-lg p-3 flex items-start gap-2 text-red-300 text-xs">
            <AlertCircle className="w-4 h-4 text-red-400 shrink-0 mt-0.5" />
            <span>{error}</span>
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-xs font-heading font-semibold text-slate-300 uppercase tracking-wider mb-1.5">
              Email Address
            </label>
            <div className="relative">
              <Mail className="w-4 h-4 text-slate-400 absolute left-3 top-3" />
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="driver@nitrorush.com"
                className="w-full bg-[#0B0F1A] border border-[#1F293D] focus:border-[#00E5FF] focus:ring-1 focus:ring-[#00E5FF] text-white rounded-lg pl-10 pr-4 py-2.5 text-sm transition-all outline-none"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-heading font-semibold text-slate-300 uppercase tracking-wider mb-1.5">
              Password
            </label>
            <div className="relative">
              <Lock className="w-4 h-4 text-slate-400 absolute left-3 top-3" />
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full bg-[#0B0F1A] border border-[#1F293D] focus:border-[#00E5FF] focus:ring-1 focus:ring-[#00E5FF] text-white rounded-lg pl-10 pr-4 py-2.5 text-sm transition-all outline-none"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-gradient-to-r from-[#00E5FF] to-cyan-500 hover:from-[#FF2E92] hover:to-pink-600 text-black font-heading font-extrabold text-sm uppercase py-3 rounded-lg shadow-[0_0_20px_rgba(0,229,255,0.4)] transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 mt-6"
          >
            {isSubmitting ? 'Authenticating...' : 'Enter Paddock'}
            <ArrowRight className="w-4 h-4" />
          </button>
        </form>

        {/* Toggle to Register */}
        <div className="mt-8 text-center pt-5 border-t border-[#1F293D]">
          <p className="text-xs text-slate-400">
            Need a new driver account?{' '}
            <Link
              to="/register"
              className="text-[#00E5FF] hover:text-[#FF2E92] font-heading font-semibold transition-colors underline ml-1"
            >
              Create Account
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};
