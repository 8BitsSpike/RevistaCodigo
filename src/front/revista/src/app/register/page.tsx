'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';

type IconProps = React.SVGProps<SVGSVGElement>;

// Ícones SVG para o botão de mostrar/esconder senha
const EyeIcon = (props: IconProps) => (
    // Ícone de Olho Aberto (Mostrar Senha)
    <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-eye">
        <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
        <circle cx="12" cy="12" r="3" />
    </svg>
);

const EyeOffIcon = (props: IconProps) => (
    // Ícone de Olho Fechado (Esconder Senha)
    <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-eye-off">
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
        {isMet
            ? <path d="M22 11.08V12a10 10 0 1 1-5.93-8.83"></path>
            : <path d="M22 11.08V12a10 10 0 1 1-5.93-8.83"></path>
        }
        <path d="M22 4L12 14.01l-3-3"></path>
    </svg>
);


const REGISTER_API_URL = 'https://localhost:54868/api/Usuario/Register';

export default function RegisterPage() {

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [passwordConfirm, setPasswordConfirm] = useState('');

    // Estado para controlar a visibilidade das senhas
    const [showPassword, setShowPassword] = useState(false);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    //Verifica as regras de segurança da senha em tempo real
    const [passwordValidationChecks, setPasswordValidationChecks] = useState<Record<string, boolean>>({
        minLength: false,
        hasUppercase: false,
        hasNumber: false,
        hasSpecial: false,
    });

    // Efeito para validar a senha sempre que ela mudar
    useEffect(() => {
        const checks = {
            // 1. Mínimo 8 caracteres
            minLength: password.length >= 8,
            // 2. Pelo menos 1 letra maiúscula
            hasUppercase: /[A-Z]/.test(password),
            // 3. Pelo menos 1 número
            hasNumber: /[0-9]/.test(password),
            // 4. Pelo menos 1 caractere especial
            hasSpecial: /[^A-Za-z0-9]/.test(password),
        };
        setPasswordValidationChecks(checks);
        // Limpa o erro genérico se o usuário começar a digitar
        if (password) setError('');
    }, [password]);

    // Função para alternar a visibilidade da senha
    const toggleShowPassword = () => {
        setShowPassword(prev => !prev);
    };

    const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setLoading(true);

        // 1. Validação de Segurança
        const allValid = Object.values(passwordValidationChecks).every(v => v);

        if (!allValid) {
            setError('A senha não atende a todos os requisitos de segurança. Por favor, verifique a lista acima.');
            setLoading(false);
            return;
        }

        // 2. Validação de Confirmação
        if (password !== passwordConfirm) {
            setError('As senhas não coincidem.');
            setLoading(false);
            return;
        }

        const payload = {
            Name: firstName,
            Sobrenome: lastName,
            Email: email,
            Password: password,
            PasswordConfirm: passwordConfirm
        };


        try {
            const response = await fetch(REGISTER_API_URL, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(payload),
            });

            if (response.ok) {

                setSuccess('Cadastro realizado com sucesso! Redirecionando para o login...');

                // Limpa os campos após o sucesso
                setFirstName('');
                setLastName('');
                setEmail('');
                setPassword('');
                setPasswordConfirm('');

                // Redireciona para o Login
                setTimeout(() => {
                    window.location.href = '/login';
                }, 2000);

            } else {
                let errorMessage = 'Não foi possível completar o cadastro. Credenciais inválidas ou erro do servidor.';

                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.error || errorData.title || errorMessage;

                    if (errorData.errors && typeof errorData.errors === 'object') {
                        const validationMessages = Object.values(errorData.errors).flat().join('; ');
                        if (validationMessages) {
                            errorMessage = validationMessages;
                        }
                    }

                } catch (jsonError) {
                    // Se não for possível ler o JSON (ex: erro 500), usa uma mensagem genérica
                    errorMessage = `Falha no cadastro (Status: ${response.status}).`;
                }

                setError(errorMessage);
                setSuccess('');
            }
        } catch (err) {
            console.error('Erro de rede ou autenticação:', err);
            setError('Não foi possível conectar ao servidor. Verifique a URL e o backend.');
            setSuccess('');
        } finally {
            setLoading(false);
        }
    };

    const inputFocusStyle = "focus:outline-none focus:ring-2 focus:ring-emerald-500";
    const buttonBaseStyle = "w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-md text-base font-medium text-white transition duration-200 ease-in-out transform";
    const buttonLoadingStyle = "bg-emerald-400 cursor-not-allowed opacity-75";
    const buttonActiveStyle = "bg-emerald-600 hover:bg-emerald-700 focus:outline-none focus:ring-4 focus:ring-emerald-500 focus:ring-opacity-50 hover:scale-[1.01]";
    const linkStyle = "font-medium text-emerald-600 hover:text-emerald-700 transition duration-150 ease-in-out";

    // Mapeamento das regras para exibição visual
    const passwordRules = [
        { key: 'minLength', label: '8 caracteres no mínimo' },
        { key: 'hasUppercase', label: '1 letra maiúscula' },
        { key: 'hasNumber', label: '1 número' },
        { key: 'hasSpecial', label: '1 caractere especial (!@#$)' },
    ];

    return (
        <div className="flex min-h-screen items-center justify-center bg-gray-50 p-4 sm:p-6 font-sans">
            <div className="w-full max-w-md bg-white p-8 sm:p-10 rounded-xl shadow-2xl transition-all duration-300 transform hover:shadow-3xl">

                {/* LOGO RBEB */}
                <div className="flex justify-center mb-6">
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

                <div className="text-center mb-8">
                    <h1 className="text-3xl font-bold text-gray-800">Crie sua Conta</h1>
                    <p className="mt-2 text-gray-500">
                        Preencha os dados abaixo e registre-se na RBEB.
                    </p>
                </div>

                <form onSubmit={handleRegister} className="space-y-6">

                    {/* Campos Nome e Sobrenome lado a lado */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="firstName" className="block text-sm font-semibold text-gray-700 mb-1">Nome</label>
                            <input
                                id="firstName"
                                name="firstName"
                                type="text"
                                required
                                value={firstName}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFirstName(e.target.value)}
                                className={`w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm ${inputFocusStyle}`}
                                disabled={loading}
                            />
                        </div>
                        <div>
                            <label htmlFor="lastName" className="block text-sm font-semibold text-gray-700 mb-1">Sobrenome</label>
                            <input
                                id="lastName"
                                name="lastName"
                                type="text"
                                required
                                value={lastName}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setLastName(e.target.value)}
                                className={`w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm ${inputFocusStyle}`}
                                disabled={loading}
                            />
                        </div>
                    </div>

                    {/* Campo Email */}
                    <div>
                        <label htmlFor="email" className="block text-sm font-semibold text-gray-700 mb-1">Email</label>
                        <input
                            id="email"
                            name="email"
                            type="email"
                            autoComplete="email"
                            required
                            value={email}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
                            className={`w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm ${inputFocusStyle}`}
                            placeholder="seu.email@exemplo.com"
                            disabled={loading}
                        />
                    </div>

                    {/* Campo Senha */}
                    <div>
                        <label htmlFor="password" className="block text-sm font-semibold text-gray-700 mb-1">Senha</label>
                        <div className="relative">
                            <input
                                id="password"
                                name="password"
                                type={showPassword ? 'text' : 'password'}
                                autoComplete="new-password"
                                required
                                value={password}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
                                className={`w-full pr-10 px-4 py-2 border border-gray-300 rounded-lg shadow-sm ${inputFocusStyle}`}
                                placeholder="Mínimo 8 caracteres"
                                disabled={loading}
                            />
                            {/* Botão para mostrar/esconder senha */}
                            <button
                                type="button"
                                onClick={toggleShowPassword}
                                className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition duration-150"
                                aria-label={showPassword ? "Esconder senha" : "Mostrar senha"}
                            >
                                {showPassword ? (
                                    <EyeOffIcon className="h-5 w-5" />
                                ) : (
                                    <EyeIcon className="h-5 w-5" />
                                )}
                            </button>
                        </div>

                        {/* Indicadores de Requisitos de Senha */}
                        {password.length > 0 && (
                            <div className="mt-2 grid grid-cols-2 gap-2 text-xs font-medium">
                                {passwordRules.map(rule => (
                                    <div key={rule.key} className="flex items-center">
                                        <CheckCircleIcon isMet={passwordValidationChecks[rule.key]} />
                                        <span className={passwordValidationChecks[rule.key] ? 'text-emerald-600' : 'text-gray-500'}>
                                            {rule.label}
                                        </span>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Campo Confirmação de Senha */}
                    <div>
                        <label htmlFor="passwordConfirm" className="block text-sm font-semibold text-gray-700 mb-1">Confirmação de Senha</label>
                        <div className="relative">
                            <input
                                id="passwordConfirm"
                                name="passwordConfirm"
                                type={showPassword ? 'text' : 'password'}
                                required
                                value={passwordConfirm}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPasswordConfirm(e.target.value)}
                                className={`w-full pr-10 px-4 py-2 border border-gray-300 rounded-lg shadow-sm ${inputFocusStyle} ${passwordConfirm.length > 0 && password !== passwordConfirm ? 'border-red-500' : ''
                                    }`}
                                placeholder="********"
                                disabled={loading}
                            />
                            <button
                                type="button"
                                onClick={toggleShowPassword}
                                className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition duration-150"
                                aria-label={showPassword ? "Esconder senha" : "Mostrar senha"}
                            >
                                {showPassword ? (
                                    <EyeOffIcon className="h-5 w-5" />
                                ) : (
                                    <EyeIcon className="h-5 w-5" />
                                )}
                            </button>
                        </div>
                        {passwordConfirm.length > 0 && password !== passwordConfirm && (
                            <p className="mt-1 text-xs text-red-500 font-medium">As senhas não coincidem.</p>
                        )}
                    </div>

                    {/* Exibição de Erros e Sucesso */}
                    {(error || success) && (
                        <div className={`p-3 border rounded-lg text-sm font-medium transition-all duration-300 animate-fade-in ${error ? 'bg-red-100 border-red-400 text-red-700' : 'bg-green-100 border-green-400 text-green-700'
                            }`} role={error ? "alert" : "status"}>
                            {error || success}
                        </div>
                    )}

                    {/* Botão de Cadastro */}
                    <div>
                        <button
                            type="submit"
                            disabled={loading || !passwordValidationChecks.minLength || password !== passwordConfirm}
                            className={`${buttonBaseStyle} ${(loading || !passwordValidationChecks.minLength || password !== passwordConfirm)
                                ? buttonLoadingStyle
                                : buttonActiveStyle
                                }`}
                        >
                            {loading ? (
                                <div className="flex items-center space-x-2">
                                    <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                    <span>Cadastrando...</span>
                                </div>
                            ) : (
                                'Cadastrar'
                            )}
                        </button>
                    </div>
                </form>

                <div className="mt-6 text-center">
                    <p className="text-sm text-gray-600">
                        Já tem conta?{' '}
                        <a href="/login" className={linkStyle}>
                            Faça Login
                        </a>
                    </p>
                </div>
            </div>
        </div>
    );
}
