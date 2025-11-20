'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Eye, EyeOff, Check, X, Lock, ArrowLeft } from 'lucide-react';
import { USER_API_BASE } from '@/lib/fetcher';
import toast from 'react-hot-toast';
import Image from 'next/image';

export default function ResetPasswordPage() {
  const router = useRouter();

  const [email, setEmail] = useState('');
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  // Validação de Senha
  const [passwordCriteria, setPasswordCriteria] = useState({
    length: false,
    uppercase: false,
    number: false,
    special: false
  });

  useEffect(() => {
    setPasswordCriteria({
      length: newPassword.length >= 8,
      uppercase: /[A-Z]/.test(newPassword),
      number: /[0-9]/.test(newPassword),
      special: /[^A-Za-z0-9]/.test(newPassword)
    });
  }, [newPassword]);

  const isPasswordValid = Object.values(passwordCriteria).every(Boolean);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!isPasswordValid) {
      toast.error('A nova senha não atende aos requisitos de segurança.');
      return;
    }

    setLoading(true);

    try {
      // Use USER_API_BASE
      const res = await fetch(`${USER_API_BASE}/ResetPassword`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email,
          password: oldPassword,
          newPassword
        }),
      });

      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.message || 'Erro ao alterar senha.');
      }

      toast.success('Senha alterada com sucesso! Faça login novamente.');
      router.push('/login');
    } catch (err: any) {
      console.error(err);
      toast.error(err.message || 'Falha ao alterar senha.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-white rounded-xl shadow-lg overflow-hidden">
        <div className="bg-emerald-600 p-6 text-center relative">
          <Link href="/login" className="absolute left-4 top-6 text-emerald-100 hover:text-white">
            <ArrowLeft size={24} />
          </Link>
          <div className="flex justify-center mb-2">
            <div className="bg-white p-2 rounded-full">
              <Lock className="text-emerald-600" size={24} />
            </div>
          </div>
          <h1 className="text-2xl font-bold text-white">Alterar Senha</h1>
          <p className="text-emerald-100 mt-1">Atualize suas credenciais de acesso</p>
        </div>

        <div className="p-8">
          <form onSubmit={handleSubmit} className="space-y-5">

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                placeholder="seu@email.com"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Senha Atual</label>
              <input
                type="password"
                value={oldPassword}
                onChange={(e) => setOldPassword(e.target.value)}
                className="w-full p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                placeholder="••••••••"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nova Senha</label>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full p-2 pr-10 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  placeholder="Nova senha forte"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>

              {/* Password Strength Meter */}
              <div className="mt-3 grid grid-cols-2 gap-2 text-xs text-gray-600 bg-gray-50 p-3 rounded-md">
                <span className={`flex items-center gap-1 ${passwordCriteria.length ? 'text-green-600 font-medium' : ''}`}>
                  {passwordCriteria.length ? <Check size={12} /> : <div className="w-3 h-3 rounded-full border border-gray-400"></div>}
                  Mínimo 8 caracteres
                </span>
                <span className={`flex items-center gap-1 ${passwordCriteria.uppercase ? 'text-green-600 font-medium' : ''}`}>
                  {passwordCriteria.uppercase ? <Check size={12} /> : <div className="w-3 h-3 rounded-full border border-gray-400"></div>}
                  Letra Maiúscula
                </span>
                <span className={`flex items-center gap-1 ${passwordCriteria.number ? 'text-green-600 font-medium' : ''}`}>
                  {passwordCriteria.number ? <Check size={12} /> : <div className="w-3 h-3 rounded-full border border-gray-400"></div>}
                  Número
                </span>
                <span className={`flex items-center gap-1 ${passwordCriteria.special ? 'text-green-600 font-medium' : ''}`}>
                  {passwordCriteria.special ? <Check size={12} /> : <div className="w-3 h-3 rounded-full border border-gray-400"></div>}
                  Caractere Especial
                </span>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading || !isPasswordValid}
              className="w-full py-3 px-4 rounded-lg shadow-sm text-sm font-bold text-white bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed transition mt-2"
            >
              {loading ? 'Processando...' : 'Atualizar Senha'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}