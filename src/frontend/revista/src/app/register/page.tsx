'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Eye, EyeOff, Check, X, ArrowLeft, UserPlus, AlertCircle } from 'lucide-react';
import { USER_API_BASE } from '@/lib/fetcher';
import toast from 'react-hot-toast';

export default function RegisterPage() {
    const router = useRouter();

    const [formData, setFormData] = useState({
        name: '',
        sobrenome: '',
        email: '',
        password: '',
        confirmPassword: ''
    });

    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false); // Added state
    const [loading, setLoading] = useState(false);

    const [emailError, setEmailError] = useState('');

    // Password Strength
    const [passwordCriteria, setPasswordCriteria] = useState({
        length: false,
        uppercase: false,
        number: false,
        special: false
    });

    useEffect(() => {
        const pwd = formData.password;
        setPasswordCriteria({
            length: pwd.length >= 8,
            uppercase: /[A-Z]/.test(pwd),
            number: /[0-9]/.test(pwd),
            special: /[^A-Za-z0-9]/.test(pwd)
        });
    }, [formData.password]);

    const isPasswordValid = Object.values(passwordCriteria).every(Boolean);
    const passwordsMatch = formData.password === formData.confirmPassword && formData.password !== '';

    const validateEmail = (val: string) => {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!regex.test(val)) {
            setEmailError('Endereço de e-mail inválido.');
            return false;
        }
        setEmailError('');
        return true;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
        if (e.target.name === 'email' && emailError) {
            validateEmail(e.target.value);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateEmail(formData.email)) {
            toast.error('Corrija o e-mail antes de continuar.');
            return;
        }

        if (!isPasswordValid) {
            toast.error('A senha não atende aos requisitos de segurança.');
            return;
        }

        if (!passwordsMatch) {
            toast.error('As senhas não coincidem.');
            return;
        }

        setLoading(true);

        try {
            const res = await fetch(`${USER_API_BASE}/Register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    name: formData.name,
                    sobrenome: formData.sobrenome,
                    email: formData.email,
                    password: formData.password,
                    passwordConfirm: formData.confirmPassword
                }),
            });

            if (!res.ok) {
                let errMsg = 'Erro ao criar conta.';
                try {
                    const errorData = await res.json();
                    if (errorData.message) errMsg = errorData.message;
                    // Sometimes .NET returns "errors" object validation
                    if (errorData.errors) {
                        errMsg = Object.values(errorData.errors).flat().join(', ');
                    }
                } catch { /* text response fallback */ }
                throw new Error(errMsg);
            }

            toast.success('Conta criada com sucesso! Redirecionando para login...');

            // Explicit routing to login page
            setTimeout(() => {
                router.push('/login');
            }, 1500);

        } catch (err: any) {
            console.error(err);
            toast.error(err.message || 'Falha no cadastro.');
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
                    <h1 className="text-2xl font-bold text-white flex items-center justify-center gap-2">
                        <UserPlus size={24} />
                        Criar Conta
                    </h1>
                    <p className="text-emerald-100 mt-2">Junte-se à nossa comunidade acadêmica</p>
                </div>

                <div className="p-8">
                    <form onSubmit={handleSubmit} className="space-y-4">

                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
                                <input
                                    type="text"
                                    name="name"
                                    value={formData.name}
                                    onChange={handleChange}
                                    className="input-std"
                                    required
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Sobrenome</label>
                                <input
                                    type="text"
                                    name="sobrenome"
                                    value={formData.sobrenome}
                                    onChange={handleChange}
                                    className="input-std"
                                    required
                                />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                            <input
                                type="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
                                onBlur={(e) => validateEmail(e.target.value)}
                                className={`input-std ${emailError ? 'border-red-500 focus:ring-red-500' : ''}`}
                                required
                            />
                            {emailError && <p className="text-xs text-red-600 mt-1">{emailError}</p>}
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
                            <div className="relative">
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    name="password"
                                    value={formData.password}
                                    onChange={handleChange}
                                    className="input-std pr-10"
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

                            <div className="mt-3 grid grid-cols-2 gap-2 text-xs text-gray-600">
                                <span className={`flex items-center gap-1 ${passwordCriteria.length ? 'text-green-600' : ''}`}>
                                    {passwordCriteria.length ? <Check size={12} /> : <X size={12} />} Mínimo 8 caracteres
                                </span>
                                <span className={`flex items-center gap-1 ${passwordCriteria.uppercase ? 'text-green-600' : ''}`}>
                                    {passwordCriteria.uppercase ? <Check size={12} /> : <X size={12} />} Letra Maiúscula
                                </span>
                                <span className={`flex items-center gap-1 ${passwordCriteria.number ? 'text-green-600' : ''}`}>
                                    {passwordCriteria.number ? <Check size={12} /> : <X size={12} />} Número
                                </span>
                                <span className={`flex items-center gap-1 ${passwordCriteria.special ? 'text-green-600' : ''}`}>
                                    {passwordCriteria.special ? <Check size={12} /> : <X size={12} />} Caractere Especial
                                </span>
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Confirmar Senha</label>
                            <div className="relative">
                                <input
                                    type={showConfirmPassword ? 'text' : 'password'}
                                    name="confirmPassword"
                                    value={formData.confirmPassword}
                                    onChange={handleChange}
                                    className={`input-std pr-10 ${!passwordsMatch && formData.confirmPassword ? 'border-red-300' : ''}`}
                                    required
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                                >
                                    {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                </button>
                            </div>
                            {!passwordsMatch && formData.confirmPassword && (
                                <p className="text-xs text-red-500 mt-1">As senhas não coincidem.</p>
                            )}
                        </div>

                        <button
                            type="submit"
                            disabled={loading || !isPasswordValid || !passwordsMatch}
                            className="btn-primary w-full justify-center mt-4"
                        >
                            {loading ? 'Criando conta...' : 'Registrar'}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}