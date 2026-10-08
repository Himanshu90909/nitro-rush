import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { AlertCircle, Lock, Mail, User as UserIcon, ArrowRight } from 'lucide-react';

export const Register: React.FC = () => {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await register(email, password, username);
      navigate('/');
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Registration failed. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0B0F1A] flex items-center justify-center p-4 relative overflow-hidden font-sans">
      {/* Neon Decorative Background Effects */}
      <div className="absolute -top-40 -right-40 w-96 h-96 bg-[#FF2E92]/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -left-40 w-96 h-96 bg-[#00E5FF]/10 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-md w-full bg-[#121829] border border-[#FF2E92]/40 shadow-[0_0_30px_rgba(255,46,146,0.15)] rounded-2xl p-8 relative z-10 clip-corner">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="text-5xl mb-2">🏁</div>
          <h1 className="text-3xl font-extrabold font-heading text-[#FF2E92] tracking-wider">
            JOIN <span className="text-[#00E5FF]">NITRO RUSH</span>
          </h1>
          <p className="text-xs text-slate-400 font-heading uppercase tracking-widest mt-1">
            Register Driver Profile
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
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs font-heading font-semibold text-slate-300 uppercase tracking-wider mb-1.5">
              Driver Call-Sign (Username)
            </label>
            <div className="relative">
              <UserIcon className="w-4 h-4 text-slate-400 absolute left-3 top-3" />
              <input
                type="text"
                required
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="ApexRacer"
                className="w-full bg-[#0B0F1A] border border-[#1F293D] focus:border-[#FF2E92] focus:ring-1 focus:ring-[#FF2E92] text-white rounded-lg pl-10 pr-4 py-2.5 text-sm transition-all outline-none"
              />
            </div>
          </div>

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
                className="w-full bg-[#0B0F1A] border border-[#1F293D] focus:border-[#FF2E92] focus:ring-1 focus:ring-[#FF2E92] text-white rounded-lg pl-10 pr-4 py-2.5 text-sm transition-all outline-none"
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
                minLength={6}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full bg-[#0B0F1A] border border-[#1F293D] focus:border-[#FF2E92] focus:ring-1 focus:ring-[#FF2E92] text-white rounded-lg pl-10 pr-4 py-2.5 text-sm transition-all outline-none"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-gradient-to-r from-[#FF2E92] to-pink-600 hover:from-[#00E5FF] hover:to-cyan-400 text-white hover:text-black font-heading font-extrabold text-sm uppercase py-3 rounded-lg shadow-[0_0_20px_rgba(255,46,146,0.4)] transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 mt-6"
          >
            {isSubmitting ? 'Registering Driver...' : 'Claim License'}
            <ArrowRight className="w-4 h-4" />
          </button>
        </form>

        {/* Toggle to Login */}
        <div className="mt-8 text-center pt-5 border-t border-[#1F293D]">
          <p className="text-xs text-slate-400">
            Already registered?{' '}
            <Link
              to="/login"
              className="text-[#FF2E92] hover:text-[#00E5FF] font-heading font-semibold transition-colors underline ml-1"
            >
              Sign In
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};
