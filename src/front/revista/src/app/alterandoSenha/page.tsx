'use client';

import { useState, useEffect } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';

type IconProps = React.SVGProps<SVGSVGElement>;

// Ícones
const EyeIcon = (props: IconProps) => (
  <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
    <circle cx="12" cy="12" r="3" />
  </svg>
);

const EyeOffIcon = (props: IconProps) => (
  <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24" />
    <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68" />
    <path d="M6.61 6.61A13.16 13.16 0 0 0 2 12s3 7 10 7a9.7 9.7 0 0 0 5.46-1.39" />
    <line x1="2" x2="22" y1="2" y2="22" />
  </svg>
);

const CheckCircleIcon = ({ isMet }: { isMet: boolean }) => (
  <svg
    className={`h-4 w-4 mr-1 transition-colors duration-200 ${isMet ? 'text-emerald-500' : 'text-gray-400'}`}
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M22 11.08V12a10 10 0 1 1-5.93-8.83" />
    <path d="M22 4L12 14.01l-3-3" />
  </svg>
);

const RESET_API_URL = 'https://localhost:54868/api/Usuario/ResetPassword';

export default function ResetPasswordPage() {
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const [passwordValidationChecks, setPasswordValidationChecks] = useState({
    minLength: false,
    hasUppercase: false,
    hasNumber: false,
    hasSpecial: false,
  });

  const searchParams = useSearchParams();
  const router = useRouter();
  const userId = searchParams.get('id') || searchParams.get('userId');
  const token = searchParams.get('token');

  // Validações da senha
  useEffect(() => {
    setPasswordValidationChecks({
      minLength: password.length >= 8,
      hasUppercase: /[A-Z]/.test(password),
      hasNumber: /[0-9]/.test(password),
      hasSpecial: /[^A-Za-z0-9]/.test(password),
    });
  }, [password]);

  const passwordRules = [
    { key: 'minLength', label: 'Mínimo de 8 caracteres' },
    { key: 'hasUppercase', label: '1 letra maiúscula' },
    { key: 'hasNumber', label: '1 número' },
    { key: 'hasSpecial', label: '1 caractere especial (!@#$)' },
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');

    if (!userId || !token) {
      setError('Link inválido ou expirado.');
      return;
    }

    const allValid = Object.values(passwordValidationChecks).every(Boolean);
    if (!allValid) {
      setError('A senha não atende aos requisitos de segurança.');
      return;
    }

    if (password !== confirmPassword) {
      setError('As senhas não coincidem.');
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(RESET_API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId, token, newPassword: password }),
      });

      if (response.ok) {
        const text = await response.text();
        setMessage(text || 'Senha redefinida com sucesso! 🎉');
        setTimeout(() => router.push('/login'), 3000);
      } else {
        const errorData = await response.json().catch(() => ({}));
        setError(errorData.message || errorData.error || 'Erro ao redefinir senha.');
      }
    } catch {
      setError('Erro ao conectar ao servidor.');
    } finally {
      setLoading(false);
    }
  };

  const toggleShowPassword = () => setShowPassword((prev) => !prev);

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 font-sans">
      <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-2xl shadow-2xl">
        {/* Logo */}
        <div className="flex justify-center mb-4">
          <img
            src="/faviccon.png"
            alt="Logo RBEB"
            className="h-[100px] w-[100px] object-contain rounded-full shadow-lg"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = "https://via.placeholder.com/100x100/ffffff/2c3e50?text=RBEB";
              target.onerror = null;
            }}
          />
        </div>

        <h2 className="text-center text-2xl font-semibold text-gray-800">Redefinir Senha</h2>
        <p className="text-center text-gray-600 text-sm">Digite sua nova senha abaixo.</p>

        <form className="space-y-6" onSubmit={handleSubmit}>
          {/* Nova senha */}
          <div>
            <label htmlFor="password" className="block text-sm font-semibold text-gray-700">
              Nova Senha
            </label>
            <div className="relative mt-2">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="block w-full pr-10 px-4 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition duration-150 ease-in-out"
                placeholder="********"
                disabled={loading}
              />
              <button
                type="button"
                onClick={toggleShowPassword}
                className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                aria-label={showPassword ? 'Esconder senha' : 'Mostrar senha'}
              >
                {showPassword ? <EyeOffIcon className="h-5 w-5" /> : <EyeIcon className="h-5 w-5" />}
              </button>
            </div>

            {password.length > 0 && (
              <div className="mt-2 grid grid-cols-2 gap-2 text-xs font-medium">
                {passwordRules.map((rule) => (
                  <div key={rule.key} className="flex items-center">
                    <CheckCircleIcon
                      isMet={passwordValidationChecks[rule.key as keyof typeof passwordValidationChecks]}
                    />
                    <span
                      className={
                        passwordValidationChecks[rule.key as keyof typeof passwordValidationChecks]
                          ? 'text-emerald-600'
                          : 'text-gray-500'
                      }
                    >
                      {rule.label}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Confirmar senha */}
          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-semibold text-gray-700">
              Confirmar Nova Senha
            </label>
            <div className="relative mt-2">
              <input
                id="confirmPassword"
                type={showPassword ? 'text' : 'password'}
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className={`block w-full pr-10 px-4 py-2 border rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 transition duration-150 ${confirmPassword.length > 0 && password !== confirmPassword
                    ? 'border-red-500'
                    : 'border-gray-300'
                  }`}
                placeholder="********"
                disabled={loading}
              />
            </div>
            {confirmPassword.length > 0 && password !== confirmPassword && (
              <p className="mt-1 text-xs text-red-500 font-medium">As senhas não coincidem.</p>
            )}
          </div>

          {/* Mensagens */}
          {message && (
            <div className="p-3 bg-green-100 border border-green-400 text-green-700 text-sm rounded-lg text-center">
              {message}
            </div>
          )}
          {error && (
            <div className="p-3 bg-red-100 border border-red-400 text-red-700 text-sm rounded-lg text-center">
              {error}
            </div>
          )}

          {/* Botão */}
          <button
            type="submit"
            disabled={loading || password !== confirmPassword}
            className={`w-full flex justify-center py-3 px-4 rounded-lg text-white font-medium transition ${loading ? 'bg-emerald-400 cursor-not-allowed' : 'bg-emerald-600 hover:bg-emerald-700'
              }`}
          >
            {loading ? 'Redefinindo...' : 'Redefinir Senha'}
          </button>
        </form>

        <div className="text-sm text-center pt-2">
          <a href="/login" className="font-medium text-emerald-600 hover:text-emerald-500 transition">
            Voltar ao login
          </a>
        </div>
      </div>
    </div>
  );
}
