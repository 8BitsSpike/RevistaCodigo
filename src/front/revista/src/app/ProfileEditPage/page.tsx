'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout';
import { User, ArrowLeft, Building2, Trash2, Briefcase } from 'lucide-react';

const API_BASE = 'https://localhost:44387/api/Usuario';

interface InfoInstitucional {
    instituicao?: string;
    curso?: string;
    dataInicio?: string;
    dataFim?: string;
    descricaoCurso?: string;
    informacoesAdd?: string;
}

// Estrutura para Atuação Profissional (Experiência)
interface Atuacao {
    instituicao?: string;
    areaAtuacao?: string;
    dataInicio?: string;
    dataFim?: string;
    contribuicao?: string;
    informacoesAdd?: string;
}

interface Usuario {
    _id?: string;
    id?: string;
    tipo?: string;
    name?: string;
    sobrenome?: string;
    email?: string;
    foto?: string;
    password?: string;
    biografia?: string;
    infoInstitucionais?: InfoInstitucional[];
    atuacoes?: Atuacao[];
}

export default function ProfileEditPage() {
    const [usuario, setUsuario] = useState<Usuario>({});
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [showToast, setShowToast] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [showCancelModal, setShowCancelModal] = useState(false);

    // Estados para remoção de Info Institucional
    const [removeInfoIndex, setRemoveInfoIndex] = useState<number | null>(null);
    const [showRemoveInfoModal, setShowRemoveInfoModal] = useState(false);

    // Estados para remoção de Atuação Profissional
    const [removeAtuacaoIndex, setRemoveAtuacaoIndex] = useState<number | null>(null);
    const [showRemoveAtuacaoModal, setShowRemoveAtuacaoModal] = useState(false);

    const [fieldErrors, setFieldErrors] = useState<{ name?: string; sobrenome?: string; email?: string }>({});

    const router = useRouter();

    // Carregar perfil
    useEffect(() => {
        const id = localStorage.getItem('userId');
        const token = localStorage.getItem('jwtToken');

        if (!id || !token) {
            router.push('/login');
            return;
        }

        const fetchProfile = async () => {
            try {
                const res = await fetch(`${API_BASE}/${id}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (!res.ok) throw new Error('Erro ao carregar o perfil');
                const data = await res.json();
                setUsuario({
                    ...data,
                    infoInstitucionais: data.infoInstitucionais || [],
                    atuacoes: data.atuacoes || [],
                });
            } catch {
                setError('Não foi possível carregar seu perfil.');
            } finally {
                setLoading(false);
            }
        };

        fetchProfile();
    }, [router]);

    // Alteração dos campos
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setUsuario((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    };

    //HANDLERS INFO INSTITUCIONAL
    const handleInfoChange = (index: number, field: string, value: string) => {
        const infos = [...(usuario.infoInstitucionais || [])];
        infos[index] = { ...infos[index], [field]: value };
        setUsuario((prev) => ({ ...prev, infoInstitucionais: infos }));
    };

    const addInfo = () => {
        setUsuario((prev) => ({
            ...prev,
            infoInstitucionais: [...(prev.infoInstitucionais || []), {}],
        }));
    };

    const confirmRemoveInfo = (index: number) => {
        setRemoveInfoIndex(index);
        setShowRemoveInfoModal(true);
    };

    const removeInfo = () => {
        if (removeInfoIndex !== null) {
            const infos = [...(usuario.infoInstitucionais || [])];
            infos.splice(removeInfoIndex, 1);
            setUsuario((prev) => ({ ...prev, infoInstitucionais: infos }));
            setRemoveInfoIndex(null);
            setShowRemoveInfoModal(false);
        }
    };

    // HANDLERS ATUAÇÃO PROFISSIONAL
    const handleAtuacaoChange = (index: number, field: string, value: string) => {
        const atuacoes = [...(usuario.atuacoes || [])];
        atuacoes[index] = { ...atuacoes[index], [field]: value };
        setUsuario((prev) => ({ ...prev, atuacoes: atuacoes }));
    };

    const addAtuacao = () => {
        setUsuario((prev) => ({
            ...prev,
            atuacoes: [...(prev.atuacoes || []), {}],
        }));
    };

    const confirmRemoveAtuacao = (index: number) => {
        setRemoveAtuacaoIndex(index);
        setShowRemoveAtuacaoModal(true);
    };

    const removeAtuacao = () => {
        if (removeAtuacaoIndex !== null) {
            const atuacoes = [...(usuario.atuacoes || [])];
            atuacoes.splice(removeAtuacaoIndex, 1);
            setUsuario((prev) => ({ ...prev, atuacoes: atuacoes }));
            setRemoveAtuacaoIndex(null);
            setShowRemoveAtuacaoModal(false);
        }
    };

    // Validação e salvamento
    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setFieldErrors({});

        const errors: typeof fieldErrors = {};
        if (!usuario.name?.trim()) errors.name = 'O campo Nome é obrigatório.';
        if (!usuario.sobrenome?.trim()) errors.sobrenome = 'O campo Sobrenome é obrigatório.';
        if (!usuario.email?.trim()) errors.email = 'O campo Email é obrigatório.';
        else {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(usuario.email)) errors.email = 'Digite um email válido.';
        }

        if (Object.keys(errors).length > 0) {
            setFieldErrors(errors);
            return;
        }

        setSaving(true);

        try {
            const token = localStorage.getItem('jwtToken');
            const id = usuario._id || usuario.id;
            const res = await fetch(`${API_BASE}/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify(usuario),
            });

            if (!res.ok) throw new Error('Erro ao salvar');
            setShowToast(true);
            setTimeout(() => router.push('/profile'), 2000);
        } catch {
            setError('Não foi possível salvar as alterações.');
        } finally {
            setSaving(false);
        }
    };

    const handleCancel = () => setShowCancelModal(true);

    const handleBack = () => {
        if (window.confirm('Tem certeza que deseja voltar? As alterações não salvas serão perdidas.')) {
            router.push('/profile');
        }
    };

    if (loading)
        return (
            <Layout>
                <p className="text-center mt-20 text-gray-600">Carregando...</p>
            </Layout>
        );

    return (
        <Layout>
            <div className="max-w-5xl mx-auto mt-10 bg-white rounded-2xl shadow-lg p-10 relative">
                {/* BOTÃO DE VOLTAR */}
                <button
                    onClick={handleBack}
                    className="absolute top-6 left-6 flex items-center gap-2 text-gray-600 hover:text-emerald-700 transition"
                >
                    <ArrowLeft className="w-5 h-5" /> Voltar
                </button>

                <h1 className="text-3xl font-bold text-gray-800 mb-10 flex items-center justify-center gap-2">
                    <User className="text-emerald-600 w-8 h-8" /> Editar Perfil
                </h1>

                {error && <p className="text-red-600 mb-4 text-center">{error}</p>}

                <form onSubmit={handleSave} className="space-y-10">
                    {/* FOTO */}
                    <div className="flex flex-col items-center gap-6">
                        <img
                            src={usuario.foto || '/default-avatar.png'}
                            alt="Foto de perfil"
                            className="w-36 h-36 rounded-full border-4 border-emerald-600 shadow-md object-cover"
                        />
                        <div className="text-center">
                            <input
                                placeholder='Escolha sua imagem de perfil'
                                type="file"
                                accept="image/*"
                                onChange={(e) => {
                                    const file = e.target.files?.[0];
                                    if (file) {
                                        const reader = new FileReader();
                                        reader.onloadend = () => {
                                            setUsuario((prev) => ({ ...prev, foto: reader.result as string }));
                                        };
                                        reader.readAsDataURL(file);
                                    }
                                }}
                                className="w-full border rounded-lg p-2"
                            />
                            <p className="text-sm text-gray-500 mt-1">Escolha uma nova imagem para seu perfil</p>
                        </div>
                    </div>

                    {/* DADOS PESSOAIS */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Nome */}
                        <div>
                            <label className="block text-sm font-semibold mb-1 text-gray-700">Nome</label>
                            <input
                                placeholder='Primeiro nome'
                                name="name"
                                value={usuario.name || ''}
                                onChange={handleChange}
                                className={`w-full border rounded-lg p-2 focus:ring-2 focus:ring-emerald-500 outline-none ${fieldErrors.name ? 'border-red-600' : ''
                                    }`}
                            />
                            {fieldErrors.name && <p className="text-red-600 text-sm mt-1">{fieldErrors.name}</p>}
                        </div>

                        {/* Sobrenome */}
                        <div>
                            <label className="block text-sm font-semibold mb-1 text-gray-700">Sobrenome</label>
                            <input
                                placeholder='Sobre nome'
                                name="sobrenome"
                                value={usuario.sobrenome || ''}
                                onChange={handleChange}
                                className={`w-full border rounded-lg p-2 focus:ring-2 focus:ring-emerald-500 outline-none ${fieldErrors.sobrenome ? 'border-red-600' : ''
                                    }`}
                            />
                            {fieldErrors.sobrenome && <p className="text-red-600 text-sm mt-1">{fieldErrors.sobrenome}</p>}
                        </div>
                    </div>

                    {/* Email */}
                    <div>
                        <label className="block text-sm font-semibold mb-1 text-gray-700">Email</label>
                        <input
                            placeholder='E-mail'
                            type="email"
                            name="email"
                            value={usuario.email || ''}
                            onChange={handleChange}
                            className={`w-full border rounded-lg p-2 focus:ring-2 focus:ring-emerald-500 outline-none ${fieldErrors.email ? 'border-red-600' : ''
                                }`}
                        />
                        {fieldErrors.email && <p className="text-red-600 text-sm mt-1">{fieldErrors.email}</p>}
                    </div>

                    {/*CAMPO BIOGRAFIA */}
                    <div className="mb-12">
                        <label className="block text-sm font-semibold mb-1 text-gray-700">Biografia</label>
                        <textarea
                            name="biografia"
                            value={usuario.biografia || ''}
                            onChange={handleChange}
                            rows={4}
                            maxLength={2300}
                            placeholder="Escreva uma breve biografia sobre você..."
                            className="w-full border rounded-lg p-2 focus:ring-2 focus:ring-emerald-500 outline-none resize-none"
                        />
                        <p className="text-sm text-gray-500 text-right mt-1">
                            {usuario.biografia?.length || 0}/2300 caracteres
                        </p>
                    </div>

                    {/*ATUAÇÃO PROFISSIONAL (Experiência)*/}
                    <div>
                        <h2 className="text-xl font-semibold mb-4 flex items-center gap-2 text-gray-800 border-b border-gray-200 pb-2">
                            <Briefcase className="text-emerald-600" /> Atuação Profissional
                        </h2>

                        {(usuario.atuacoes ?? []).map((atuacao, i) => (
                            <div
                                key={`atuacao-${i}`}
                                className="border border-gray-200 p-6 rounded-xl mb-4 bg-gray-50 shadow-sm space-y-3"
                            >
                                <div className="flex justify-between items-center mb-2">
                                    <h3 className="font-bold text-gray-700">Atuação {i + 1}</h3>
                                    <button
                                        type="button"
                                        onClick={() => confirmRemoveAtuacao(i)}
                                        className="text-red-600 hover:text-red-800 flex items-center gap-1 text-sm"
                                    >
                                        <Trash2 className="w-4 h-4" /> Remover
                                    </button>
                                </div>

                                <input
                                    type="text"
                                    placeholder="Instituição/Empresa"
                                    value={atuacao.instituicao || ''}
                                    onChange={(e) => handleAtuacaoChange(i, 'instituicao', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <input
                                    type="text"
                                    placeholder="Área de Atuação/Cargo"
                                    value={atuacao.areaAtuacao || ''}
                                    onChange={(e) => handleAtuacaoChange(i, 'areaAtuacao', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <div className="grid grid-cols-2 gap-4">
                                    <input
                                        type="date"
                                        placeholder="Data Início"
                                        value={atuacao.dataInicio || ''}
                                        onChange={(e) => handleAtuacaoChange(i, 'dataInicio', e.target.value)}
                                        className="w-full border rounded p-2"
                                    />
                                    {/* Campo Data Fim*/}
                                    <input
                                        type="date"
                                        placeholder="Data Fim (Atual se vazio)"
                                        value={atuacao.dataFim || ''}
                                        onChange={(e) => handleAtuacaoChange(i, 'dataFim', e.target.value)}
                                        className="w-full border rounded p-2"
                                    />
                                </div>
                                <textarea
                                    placeholder="Contribuições e Descrição das Atividades"
                                    value={atuacao.contribuicao || ''}
                                    onChange={(e) => handleAtuacaoChange(i, 'contribuicao', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <textarea
                                    placeholder="Informações adicionais"
                                    value={atuacao.informacoesAdd || ''}
                                    onChange={(e) => handleAtuacaoChange(i, 'informacoesAdd', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                            </div>
                        ))}

                        <button
                            type="button"
                            onClick={addAtuacao}
                            className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700 transition"
                        >
                            + Adicionar Atuação
                        </button>
                    </div>

                    {/*INFO INSTITUCIONAIS*/}
                    <div>
                        <h2 className="text-xl font-semibold mb-4 flex items-center gap-2 text-gray-800 border-b border-gray-200 pb-2">
                            <Building2 className="text-emerald-600" /> Informações Institucionais
                        </h2>

                        {(usuario.infoInstitucionais ?? []).map((info, i) => (
                            <div
                                key={`institucional-${i}`}
                                className="border border-gray-200 p-6 rounded-xl mb-4 bg-gray-50 shadow-sm space-y-3"
                            >
                                <div className="flex justify-between items-center mb-2">
                                    <h3 className="font-bold text-gray-700">Instituição {i + 1}</h3>
                                    <button
                                        type="button"
                                        onClick={() => confirmRemoveInfo(i)}
                                        className="text-red-600 hover:text-red-800 flex items-center gap-1 text-sm"
                                    >
                                        <Trash2 className="w-4 h-4" /> Remover
                                    </button>
                                </div>

                                <input
                                    type="text"
                                    placeholder="Instituição"
                                    value={info.instituicao || ''}
                                    onChange={(e) => handleInfoChange(i, 'instituicao', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <input
                                    type="text"
                                    placeholder="Curso"
                                    value={info.curso || ''}
                                    onChange={(e) => handleInfoChange(i, 'curso', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <div className="grid grid-cols-2 gap-4">
                                    <input
                                        type="date"
                                        placeholder="Data Início"
                                        value={info.dataInicio || ''}
                                        onChange={(e) => handleInfoChange(i, 'dataInicio', e.target.value)}
                                        className="w-full border rounded p-2"
                                    />
                                    {/* Campo Data Fim*/}
                                    <input
                                        type="date"
                                        placeholder="Data Fim (Atual se vazio)"
                                        value={info.dataFim || ''}
                                        onChange={(e) => handleInfoChange(i, 'dataFim', e.target.value)}
                                        className="w-full border rounded p-2"
                                    />
                                </div>
                                <textarea
                                    placeholder="Descrição do curso"
                                    value={info.descricaoCurso || ''}
                                    onChange={(e) => handleInfoChange(i, 'descricaoCurso', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                                <textarea
                                    placeholder="Informações adicionais"
                                    value={info.informacoesAdd || ''}
                                    onChange={(e) => handleInfoChange(i, 'informacoesAdd', e.target.value)}
                                    className="w-full border rounded p-2"
                                />
                            </div>
                        ))}

                        <button
                            type="button"
                            onClick={addInfo}
                            className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700 transition"
                        >
                            + Adicionar informação
                        </button>
                    </div>

                    {/* BOTÕES SALVAR E CANCELAR */}
                    <div className="flex justify-center gap-6 pt-4">
                        <button
                            type="submit"
                            disabled={saving}
                            className="bg-emerald-600 text-white px-8 py-3 rounded-lg hover:bg-emerald-700 transition font-semibold"
                        >
                            {saving ? 'Salvando...' : 'Salvar Alterações'}
                        </button>

                        <button
                            type="button"
                            onClick={handleCancel}
                            className="bg-gray-300 text-gray-700 px-8 py-3 rounded-lg hover:bg-gray-400 transition font-semibold"
                        >
                            Cancelar
                        </button>
                    </div>
                </form>
            </div>

            {/* TOAST */}
            {showToast && (
                <div className="fixed bottom-6 right-6 bg-emerald-600 text-white px-6 py-3 rounded-lg shadow-lg animate-fadeIn">
                    ✅ Perfil atualizado com sucesso!
                </div>
            )}

            {/* MODAL CANCELAR */}
            {showCancelModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full text-center shadow-lg">
                        <h2 className="text-xl font-semibold mb-4">Cancelar Atualização?</h2>
                        <p className="mb-6">As alterações não salvas serão perdidas.</p>
                        <div className="flex justify-center gap-4">
                            <button
                                onClick={() => setShowCancelModal(false)}
                                className="px-6 py-2 bg-gray-300 rounded-lg hover:bg-gray-400"
                            >
                                Voltar
                            </button>
                            <button
                                onClick={() => router.push('/profile')}
                                className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                            >
                                Confirmar
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* MODAL REMOVER INFO INSTITUCIONAL */}
            {showRemoveInfoModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full text-center shadow-lg">
                        <h2 className="text-xl font-semibold mb-4">Remover Informação Institucional?</h2>
                        <p className="mb-6">Tem certeza? Isso será removido permanentemente.</p>
                        <div className="flex justify-center gap-4">
                            <button
                                onClick={() => setShowRemoveInfoModal(false)}
                                className="px-6 py-2 bg-gray-300 rounded-lg hover:bg-gray-400"
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={removeInfo}
                                className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                            >
                                Remover
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* MODAL REMOVER ATUAÇÃO PROFISSIONAL */}
            {showRemoveAtuacaoModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full text-center shadow-lg">
                        <h2 className="text-xl font-semibold mb-4">Remover Atuação Profissional?</h2>
                        <p className="mb-6">Tem certeza? Isso será removido permanentemente.</p>
                        <div className="flex justify-center gap-4">
                            <button
                                onClick={() => setShowRemoveAtuacaoModal(false)}
                                className="px-6 py-2 bg-gray-300 rounded-lg hover:bg-gray-400"
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={removeAtuacao}
                                className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                            >
                                Remover
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </Layout>
    );
}