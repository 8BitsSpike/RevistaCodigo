'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import useAuth from '@/hooks/useAuth';
import { AuthResponseSuccess, UserCredentials } from '@/types';
import client from '@/lib/apolloClient';
import { VERIFICAR_STAFF } from '@/graphql/queries';

type IconProps = React.SVGProps<SVGSVGElement>;

const EyeIcon = (props: IconProps) => (
  <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-eye">
    <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
    <circle cx="12" cy="12" r="3" />
  </svg>
);

const EyeOffIcon = (props: IconProps) => (
  <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-eye-off">
    <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24" />
    <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68" />
    <path d="M6.61 6.61A13.16 13.16 0 0 0 2 12s3 7 10 7a9.7 9.7 0 0 0 5.46-1.39" />
    <line x1="2" x2="22" y1="2" y2="22" />
  </svg>
);

const AUTH_API_URL = 'https://localhost:44387/api/Usuario/Authenticate';
// Base URL para buscar o perfil do usuário
const USER_API_URL = 'https://localhost:44387/api/Usuario';

interface VerificarStaffData {
  verificarStaff: boolean;
}

// Interface para os dados do perfil (para 'foto')
interface UserProfileData {
  foto?: string;
}

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const redirectToHome = () => router.push('/');

  const toggleShowPassword = () => setShowPassword(prev => !prev);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const credentials: UserCredentials = { email, password };

    try {
      // --- Autenticar na UsuarioAPI (/Authenticate) ---
      const response = await fetch(AUTH_API_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (response.ok) {
        const data: any = await response.json();

        const userData = {
          ...data,
          id: data._id || data.id,
          jwtToken: data.jwtToken,
          name: data.name,
          sobrenome: data.sobrenome,
        };

        // Salva o token temporariamente para a próxima requisição
        localStorage.setItem('userToken', userData.jwtToken);
        localStorage.setItem('userId', userData.id);

        // --- Buscar a foto do perfil ---
        let userFoto: string | null = null;
        try {
          const profileResponse = await fetch(`${USER_API_URL}/${userData.id}`, {
            headers: {
              'Authorization': `Bearer ${userData.jwtToken}`,
            },
          });
          if (profileResponse.ok) {
            const profileData: UserProfileData = await profileResponse.json();
            userFoto = profileData.foto || null;
          }
        } catch (profileError) {
          console.error('Falha ao buscar foto do perfil:', profileError);
        }

        // Verificar o status de Staff na ArtigoAPI ---
        let isStaff = false;
        try {
          const { data: staffData } = await client.query<VerificarStaffData>({
            query: VERIFICAR_STAFF,
          });
          isStaff = staffData?.verificarStaff || false;
        } catch (staffError) {
          console.error('Falha ao verificar status de staff:', staffError);
        }

        localStorage.setItem('isStaff', isStaff.toString());

        login({
          id: userData.id,
          jwtToken: userData.jwtToken,
          name: userData.name,
          sobrenome: userData.sobrenome,
          foto: userFoto,
        });

        redirectToHome();

      } else {
        let errorMessage = '';
        try {
          const errorData = await response.json();
          const msg = (errorData.message || errorData.error || errorData.title || '').toLowerCase();

          if (msg.includes('não encontrado') || msg.includes('inexistente') || msg.includes('usuario nao')) {
            errorMessage = 'Usuário não encontrado';
          } else if (msg.includes('senha') || msg.includes('password')) {
            errorMessage = 'Senha inválida';
          } else if (response.status === 404) {
            errorMessage = 'Usuário não encontrado';
          } else if (response.status === 401) {
            errorMessage = 'Senha inválida';
          } else {
            errorMessage = errorData.message || 'Erro ao fazer login.';
          }
        } catch {
          if (response.status === 404) {
            errorMessage = 'Usuário não encontrado';
          } else if (response.status === 401) {
            errorMessage = 'Senha inválida';
          } else {
            errorMessage = `Falha no login (Status: ${response.status}).`;
          }
        }
        setError(errorMessage);
      }
    } catch (err) {
      console.error('Erro de rede ou autenticação:', err);
      setError('Não foi possível conectar ao servidor. Verifique a URL e o backend.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 font-sans">
      <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-xl shadow-2xl transition-all duration-300 transform hover:shadow-3xl">
        <div className="flex justify-center">
          <img
            src="/faviccon.png"
            alt="Logo RBEB"
            className="h-[100px] w-[100px] object-contain rounded-full shadow-lg"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = "https://faviccon.png/100x100/ffffff/2c3e50?text=RBEB";
              target.onerror = null;
            }}
          />
        </div>

        <p className="text-center text-gray-600">Inicie sessão na RBEB</p>

        <form className="space-y-6" onSubmit={handleLogin}>
          <div>
            <label htmlFor="email" className="block text-sm font-semibold text-gray-700">
              Endereço de Email
            </label>
            <input
              id="email"
              name="email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="mt-2 block w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition duration-150 ease-in-out"
              placeholder="seu.email@exemplo.com"
              disabled={loading}
            />
          </div>

          <div>
            <div className="flex items-center justify-between">
              <label htmlFor="password" className="block text-sm font-semibold text-gray-700">
                Senha
              </label>
              <a href="/forgot-password" className="text-sm font-medium text-emerald-600 hover:text-emerald-500 transition duration-150 ease-in-out">
                Esqueceu a senha?
              </a>
            </div>

            <div className="relative mt-2">
              <input
                id="password"
                name="password"
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
                className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition duration-150"
                aria-label={showPassword ? "Esconder senha" : "Mostrar senha"}
              >
                {showPassword ? <EyeOffIcon className="h-5 w-5" /> : <EyeIcon className="h-5 w-5" />}
              </button>
            </div>
          </div>

          {error && (
            <div className="p-4 text-sm font-medium text-red-700 bg-red-100 border border-red-300 rounded-lg animate-fade-in" role="alert">
              {error}
            </div>
          )}

          <div>
            <button
              type="submit"
              disabled={loading}
              className={`w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-md text-base font-medium transition duration-200 ease-in-out transform ${loading
                  ? 'bg-emerald-400 cursor-not-allowed'
                  : 'bg-emerald-600 hover:bg-emerald-700 focus:outline-none focus:ring-4 focus:ring-emerald-500 focus:ring-opacity-50 hover:scale-[1.01]'
                } text-white`}
            >
              {loading ? (
                <div className="flex items-center space-x-2">
                  <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  <span>A Entrar...</span>
                </div>
              ) : (
                'Iniciar Sessão'
              )}
            </button>
          </div>
        </form>

        <div className="text-sm text-center pt-2">
          <a href="/register" className="font-medium text-emerald-600 hover:text-emerald-500 transition duration-150 ease-in-out">
            Ainda não tem conta? Registre-se
          </a>
        </div>
      </div>
    </div>
  );
}