'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { USER_API_BASE } from '@/lib/fetcher';


export default function ForgotPasswordPage() {
    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const router = useRouter();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setMessage('');
        setLoading(true);

        try {
            const response = await fetch(`${USER_API_BASE}/RequestPasswordReset`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email }),
            });

            if (response.ok) {
                const text = await response.text();
                setMessage(text || 'Se o e-mail estiver cadastrado, um link de recuperação foi enviado.');
            } else {
                const errorData = await response.json().catch(() => ({}));
                setError(errorData.message || errorData.error || 'Não foi possível enviar o link de recuperação.');
            }
        } catch (err) {
            console.error('Erro ao solicitar recuperação:', err);
            setError('Não foi possível conectar ao servidor. Tente novamente mais tarde.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="flex items-center justify-center min-h-screen bg-gray-50 font-sans">
            <div className="w-full max-w-md p-8 space-y-6 bg-white rounded-xl shadow-2xl transition-all duration-300 transform hover:shadow-3xl">
                <div className="flex justify-center mb-">
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

                <h2 className="text-center text-2xl font-semibold text-gray-800">
                    Recuperar Senha
                </h2>
                <p className="text-center text-gray-600 text-sm">
                    Insira o seu e-mail para receber um link de redefinição de senha.
                </p>

                <form className="space-y-6" onSubmit={handleSubmit}>
                    <div>
                        <label
                            htmlFor="email"
                            className="block text-sm font-semibold text-gray-700"
                        >
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

                    {message && (
                        <div className="p-4 text-sm font-medium text-green-700 bg-green-100 border border-green-300 rounded-lg">
                            {message}
                        </div>
                    )}

                    {error && (
                        <div className="p-4 text-sm font-medium text-red-700 bg-red-100 border border-red-300 rounded-lg">
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
                                    <span>Enviando...</span>
                                </div>
                            ) : (
                                'Enviar link de recuperação'
                            )}
                        </button>
                    </div>
                </form>

                <div className="text-sm text-center pt-2">
                    <a
                        href="/login"
                        className="font-medium text-emerald-600 hover:text-emerald-500 transition duration-150 ease-in-out"
                    >
                        Voltar ao login
                    </a>
                </div>
            </div>
        </div>
    );
}