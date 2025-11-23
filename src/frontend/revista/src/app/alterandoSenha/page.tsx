'use client';

import { useState, useEffect } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { Eye, EyeOff, Check } from 'lucide-react';
import toast from 'react-hot-toast';
import { USER_API_BASE } from '@/lib/fetcher';

export default function ResetPasswordPage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const userId = searchParams.get('id') || searchParams.get('userId');
  const token = searchParams.get('token');

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  const [passwordCriteria, setPasswordCriteria] = useState({
    length: false,
    uppercase: false,
    number: false,
    special: false
  });

  useEffect(() => {
    setPasswordCriteria({
      length: password.length >= 8,
      uppercase: /[A-Z]/.test(password),
      number: /[0-9]/.test(password),
      special: /[^A-Za-z0-9]/.test(password),
    });
  }, [password]);

  const isPasswordValid = Object.values(passwordCriteria).every(Boolean);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!userId || !token) {
      toast.error('Link inválido ou expirado.');
      return;
    }

    if (!isPasswordValid) {
      toast.error('A nova senha não atende aos requisitos.');
      return;
    }

    if (password !== confirmPassword) {
      toast.error('As senhas não coincidem.');
      return;
    }

    setLoading(true);

    try {
      const res = await fetch(`${USER_API_BASE}/ResetPassword`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId, token, newPassword: password }),
      });

      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}));
        throw new Error(errorData.message || errorData.error || 'Erro ao redefinir senha.');
      }

      toast.success('Senha redefinida com sucesso!');
      router.push('/login');

    } catch (err: any) {
      toast.error(err.message || 'Erro ao conectar ao servidor.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 font-sans">
      <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-2xl shadow-2xl">

        {/* Logo */}
        <div className="flex justify-center mb-4">
          <img
            src="/faviccon.png"
            alt="Logo"
            className="h-[100px] w-[100px] object-contain rounded-full shadow-lg"
          />
        </div>

        <h2 className="text-center text-2xl font-semibold text-gray-800">Redefinir Senha</h2>
        <p className="text-center text-gray-600 text-sm">Digite sua nova senha abaixo.</p>

        <form onSubmit={handleSubmit} className="space-y-6">

          {/* Nova senha */}
          <div>
            <label className="block text-sm font-semibold text-gray-700">Nova Senha</label>
            <div className="relative mt-2">
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="block w-full pr-10 px-4 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                placeholder="********"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>

            {/* Critérios */}
            {password.length > 0 && (
              <div className="mt-3 grid grid-cols-2 gap-2 text-xs font-medium">
                <PasswordRule label="Mínimo de 8 caracteres" met={passwordCriteria.length} />
                <PasswordRule label="1 letra maiúscula" met={passwordCriteria.uppercase} />
                <PasswordRule label="1 número" met={passwordCriteria.number} />
                <PasswordRule label="1 caractere especial (!@#$)" met={passwordCriteria.special} />
              </div>
            )}
          </div>

          {/* Confirmar Senha */}
          <div>
            <label className="block text-sm font-semibold text-gray-700">Confirmar Senha</label>
            <input
              type={showPassword ? 'text' : 'password'}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              className={`block w-full px-4 py-2 border rounded-lg shadow-sm focus:ring-2 
                ${confirmPassword && password !== confirmPassword ? 'border-red-500' : 'border-gray-300'}
                focus:ring-emerald-500 transition`}
              placeholder="********"
            />
            {confirmPassword && password !== confirmPassword && (
              <p className="text-xs text-red-500 mt-1 font-medium">As senhas não coincidem.</p>
            )}
          </div>

          {/* Botão */}
          <button
            type="submit"
            disabled={loading || !isPasswordValid || password !== confirmPassword}
            className={`w-full flex justify-center py-3 px-4 rounded-lg text-white font-medium transition
              ${loading || !isPasswordValid || password !== confirmPassword
                ? 'bg-emerald-400 cursor-not-allowed'
                : 'bg-emerald-600 hover:bg-emerald-700'}`}
          >
            {loading ? 'Processando...' : 'Redefinir Senha'}
          </button>

        </form>

        <div className="text-sm text-center pt-2">
          <Link href="/login" className="font-medium text-emerald-600 hover:text-emerald-500 transition">
            Voltar ao login
          </Link>
        </div>
      </div>
    </div>
  );
}

/* Componente auxiliar para regra de senha */
function PasswordRule({ label, met }: { label: string; met: boolean }) {
  return (
    <div className="flex items-center">
      <Check className={`h-4 w-4 mr-1 transition-colors ${met ? 'text-emerald-500' : 'text-gray-400'}`} />
      <span className={met ? 'text-emerald-600' : 'text-gray-500'}>{label}</span>
    </div>
  );
}
