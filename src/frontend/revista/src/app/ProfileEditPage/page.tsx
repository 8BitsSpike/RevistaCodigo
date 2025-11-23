'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import useAuth from '@/hooks/useAuth';
import { USER_API_BASE } from '@/lib/fetcher';
import { ArrowLeft, Save, Plus, Upload, Edit, Trash2, Calendar, Building2, Briefcase } from 'lucide-react';
import toast from 'react-hot-toast';
import Image from 'next/image';
import { formatDate } from '@/lib/dateUtils';

interface InfoInstitucional {
    instituicao?: string;
    curso?: string;
    dataInicio?: string;
    dataFim?: string;
    descricaoCurso?: string;
    informacoesAdd?: string;
}

interface AtuacaoProfissional {
    instituicao?: string;
    areaAtuacao?: string;
    dataInicio?: string;
    dataFim?: string;
    contribuicao?: string;
    informacoesAdd?: string;
}

export default function ProfileEditPage() {
    const router = useRouter();
    const { user, loading: authLoading } = useAuth();

    const [formData, setFormData] = useState({
        name: '',
        sobrenome: '',
        email: '',
        biografia: '',
        foto: '',
    });

    const [infoList, setInfoList] = useState<InfoInstitucional[]>([]);
    const [atuacaoList, setAtuacaoList] = useState<AtuacaoProfissional[]>([]);

    const [loadingData, setLoadingData] = useState(true);
    const [saving, setSaving] = useState(false);

    const toInputDate = (isoString?: string) => {
        if (!isoString) return '';
        return isoString.split('T')[0];
    };

    // 1. CARREGAMENTO DE DADOS
    useEffect(() => {
        if (authLoading) return;
        if (!user) {
            router.push('/login');
            return;
        }

        const fetchProfile = async () => {
            try {
                const token = localStorage.getItem('userToken');
                const res = await fetch(`${USER_API_BASE}/${user.id}?token=${token}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (!res.ok) throw new Error('Erro ao carregar perfil');
                const data = await res.json();

                setFormData({
                    name: data.name || '',
                    sobrenome: data.sobrenome || '',
                    email: data.email || '',
                    biografia: data.biografia || '',
                    foto: data.foto || '',
                });
                setInfoList(data.infoInstitucionais || []);
                setAtuacaoList(data.atuacoes || []);
            } catch (err) {
                toast.error('Falha ao carregar dados.');
            } finally {
                setLoadingData(false);
            }
        };

        fetchProfile();
    }, [user, authLoading, router]);

    // 2. HANDLERS DE INPUTS GERAIS
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                setFormData(prev => ({ ...prev, foto: reader.result as string }));
            };
            reader.readAsDataURL(file);
        }
    };

    // 3. HANDLERS DE INFORMAÇÕES INSTITUCIONAIS (Formação)
    const handleInfoChange = (index: number, field: keyof InfoInstitucional, value: string) => {
        const infos = [...infoList];
        infos[index] = { ...infos[index], [field]: value };
        setInfoList(infos);
    };

    const addInfo = () => {
        setInfoList(prev => [...prev, {}]);
    };

    const removeInfo = (index: number) => {
        if (confirm("Remover esta Formação Acadêmica permanentemente?")) {
            setInfoList(infoList.filter((_, i) => i !== index));
        }
    };

    const handleAtuacaoChange = (index: number, field: keyof AtuacaoProfissional, value: string) => {
        const atuacoes = [...atuacaoList];
        atuacoes[index] = { ...atuacoes[index], [field]: value };
        setAtuacaoList(atuacoes);
    };

    const addAtuacao = () => {
        setAtuacaoList(prev => [...prev, {}]);
    };

    const removeAtuacao = (index: number) => {
        if (confirm("Remover esta Atuação Profissional permanentemente?")) {
            setAtuacaoList(atuacaoList.filter((_, i) => i !== index));
        }
    };

    // 5. SUBMISSÃO DO FORMULÁRIO 
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!user) return;

        // Validação básica
        if (!formData.name.trim() || !formData.sobrenome.trim()) {
            return toast.error('Nome e Sobrenome são obrigatórios.');
        }

        setSaving(true);
        const token = localStorage.getItem('userToken');

        const payload = {
            id: user.id,
            ...formData,
            infoInstitucionais: infoList,
            atuacoes: atuacaoList
        };

        try {
            const res = await fetch(`${USER_API_BASE}/${user.id}?token=${token}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                let errorMsg = 'Falha ao atualizar o perfil. Tente novamente.';
                try {
                    const errorData = await res.json();
                    errorMsg = errorData.message || errorMsg;
                } catch { }

                throw new Error(errorMsg);
            }

            toast.success('Perfil atualizado com sucesso! ✅');
            localStorage.setItem('userName', formData.name);
            localStorage.setItem('userFoto', formData.foto);

            router.push('/profile');
        } catch (err: any) {
            console.error(err);
            toast.error(err.message || 'Erro de conexão ao salvar.');
        } finally {
            setSaving(false);
        }
    };

    if (loadingData) return <div className="min-h-screen flex items-center justify-center text-lg text-emerald-600">Carregando...</div>;

    return (
        <div className="min-h-screen bg-gray-50 py-10">
            <div className="max-w-3xl mx-auto bg-white rounded-lg shadow-xl overflow-hidden">
                <div className="bg-emerald-600 px-6 py-4 flex justify-between items-center">
                    <h1 className="text-xl font-bold text-white flex items-center gap-2"><Edit size={20} /> Editar Perfil</h1>
                    <button onClick={() => { if (confirm("Tem certeza que deseja voltar? As alterações não salvas serão perdidas.")) router.back(); }} className="text-emerald-100 hover:text-white text-sm font-medium flex items-center gap-1"><ArrowLeft size={16} /> Voltar</button>
                </div>

                <form onSubmit={handleSubmit} className="p-8 space-y-10">

                    {/* --- DADOS PESSOAIS E FOTO --- */}
                    <div className="space-y-6">
                        <div className="flex flex-col items-center">
                            <div className="relative w-32 h-32 rounded-full overflow-hidden bg-gray-200 border-4 border-emerald-600 shadow-lg mb-4">
                                <Image src={formData.foto || '/faviccon.png'} alt="Preview" fill className="object-cover" priority={false} />
                            </div>
                            <label className="cursor-pointer bg-emerald-50 text-emerald-700 px-4 py-2 rounded-md text-sm font-medium hover:bg-emerald-100 transition flex items-center gap-2 border border-emerald-200">
                                <Upload size={16} /> Alterar Foto
                                <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                            </label>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div><label className="block text-sm font-medium text-gray-700 mb-1">Nome *</label><input type="text" name="name" value={formData.name} onChange={handleInputChange} className="input-std" required /></div>
                            <div><label className="block text-sm font-medium text-gray-700 mb-1">Sobrenome *</label><input type="text" name="sobrenome" value={formData.sobrenome} onChange={handleInputChange} className="input-std" required /></div>
                            <div className="md:col-span-2"><label className="block text-sm font-medium text-gray-700 mb-1">Email</label><input type="email" name="email" value={formData.email} className="input-std bg-gray-100 cursor-not-allowed" disabled /></div>
                            <div className="md:col-span-2"><label className="block text-sm font-medium text-gray-700 mb-1">Biografia</label><textarea name="biografia" value={formData.biografia} onChange={handleInputChange} rows={4} className="input-std resize-none" maxLength={2300} placeholder="Escreva uma breve biografia sobre você..." />
                                <p className="text-xs text-gray-500 text-right mt-1">{formData.biografia.length || 0}/2300 caracteres</p>
                            </div>
                        </div>
                    </div>

                    {/* --- INFORMAÇÕES INSTITUCIONAIS (Formação Acadêmica) --- */}
                    <div className="space-y-6 border-t pt-8">
                        <h2 className="text-lg font-semibold text-gray-800 flex items-center gap-2"><Building2 size={20} className="text-emerald-600" /> Formação Acadêmica</h2>

                        <div className="space-y-4">
                            {infoList.map((info, idx) => (
                                <div
                                    key={`institucional-${idx}`}
                                    className="border border-gray-200 p-6 rounded-xl bg-gray-50 shadow-sm space-y-3"
                                >
                                    <div className="flex justify-between items-center mb-2">
                                        <h3 className="font-bold text-gray-700">Formação {idx + 1}</h3>
                                        <button
                                            type="button"
                                            onClick={() => removeInfo(idx)}
                                            className="text-red-600 hover:text-red-800 flex items-center gap-1 text-sm"
                                        >
                                            <Trash2 className="w-4 h-4" /> Remover
                                        </button>
                                    </div>

                                    <input
                                        type="text"
                                        placeholder="Instituição"
                                        value={info.instituicao || ''}
                                        onChange={(e) => handleInfoChange(idx, 'instituicao', e.target.value)}
                                        className="input-std text-sm"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Curso"
                                        value={info.curso || ''}
                                        onChange={(e) => handleInfoChange(idx, 'curso', e.target.value)}
                                        className="input-std text-sm"
                                    />
                                    <div className="grid grid-cols-2 gap-3">
                                        <input
                                            type="date"
                                            placeholder="Início"
                                            value={toInputDate(info.dataInicio)}
                                            onChange={(e) => handleInfoChange(idx, 'dataInicio', e.target.value)}
                                            className="input-std text-sm"
                                        />
                                        <input
                                            type="date"
                                            placeholder="Fim (Atual se vazio)"
                                            value={toInputDate(info.dataFim)}
                                            onChange={(e) => handleInfoChange(idx, 'dataFim', e.target.value)}
                                            className="input-std text-sm"
                                        />
                                    </div>
                                    <textarea
                                        placeholder="Descrição do Curso"
                                        value={info.descricaoCurso || ''}
                                        onChange={(e) => handleInfoChange(idx, 'descricaoCurso', e.target.value)}
                                        className="input-std text-sm h-16 resize-none"
                                    />
                                    <textarea
                                        placeholder="Informações Adicionais"
                                        value={info.informacoesAdd || ''}
                                        onChange={(e) => handleInfoChange(idx, 'informacoesAdd', e.target.value)}
                                        className="input-std text-sm h-16 resize-none"
                                    />
                                </div>
                            ))}
                        </div>

                        <button
                            type="button"
                            onClick={addInfo}
                            className="btn-primary flex items-center gap-1 px-4 py-2 bg-emerald-600 text-white hover:bg-emerald-700 transition font-semibold"
                        >
                            <Plus size={16} /> Adicionar Formação
                        </button>
                    </div>

                    {/* --- ATUAÇÃO PROFISSIONAL (Experiência) --- */}
                    <div className="space-y-6 border-t pt-8">
                        <h2 className="text-lg font-semibold text-gray-800 flex items-center gap-2"><Briefcase size={20} className="text-emerald-600" /> Atuação Profissional</h2>

                        <div className="space-y-4">
                            {atuacaoList.map((atuacao, idx) => (
                                <div
                                    key={`atuacao-${idx}`}
                                    className="border border-gray-200 p-6 rounded-xl bg-gray-50 shadow-sm space-y-3"
                                >
                                    <div className="flex justify-between items-center mb-2">
                                        <h3 className="font-bold text-gray-700">Atuação {idx + 1}</h3>
                                        <button
                                            type="button"
                                            onClick={() => removeAtuacao(idx)}
                                            className="text-red-600 hover:text-red-800 flex items-center gap-1 text-sm"
                                        >
                                            <Trash2 className="w-4 h-4" /> Remover
                                        </button>
                                    </div>

                                    <input
                                        type="text"
                                        placeholder="Instituição/Empresa"
                                        value={atuacao.instituicao || ''}
                                        onChange={(e) => handleAtuacaoChange(idx, 'instituicao', e.target.value)}
                                        className="input-std text-sm"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Cargo/Função"
                                        value={atuacao.areaAtuacao || ''}
                                        onChange={(e) => handleAtuacaoChange(idx, 'areaAtuacao', e.target.value)}
                                        className="input-std text-sm"
                                    />
                                    <div className="grid grid-cols-2 gap-3">
                                        <input
                                            type="date"
                                            placeholder="Início"
                                            value={toInputDate(atuacao.dataInicio)}
                                            onChange={(e) => handleAtuacaoChange(idx, 'dataInicio', e.target.value)}
                                            className="input-std text-sm"
                                        />
                                        <input
                                            type="date"
                                            placeholder="Fim (Atual se vazio)"
                                            value={toInputDate(atuacao.dataFim)}
                                            onChange={(e) => handleAtuacaoChange(idx, 'dataFim', e.target.value)}
                                            className="input-std text-sm"
                                        />
                                    </div>
                                    <textarea
                                        placeholder="Contribuições e Descrição das Atividades"
                                        value={atuacao.contribuicao || ''}
                                        onChange={(e) => handleAtuacaoChange(idx, 'contribuicao', e.target.value)}
                                        className="input-std text-sm h-16 resize-none"
                                    />
                                    <textarea
                                        placeholder="Informações Adicionais"
                                        value={atuacao.informacoesAdd || ''}
                                        onChange={(e) => handleAtuacaoChange(idx, 'informacoesAdd', e.target.value)}
                                        className="input-std text-sm h-16 resize-none"
                                    />
                                </div>
                            ))}
                        </div>

                        <button
                            type="button"
                            onClick={addAtuacao}
                            className="btn-primary flex items-center gap-1 px-4 py-2 bg-emerald-600 text-white hover:bg-emerald-700 transition font-semibold"
                        >
                            <Plus size={16} /> Adicionar Atuação
                        </button>
                    </div>

                    {/* --- BOTÃO SALVAR --- */}
                    <div className="flex justify-end pt-6 border-t">
                        <button type="submit" disabled={saving} className="btn-primary flex items-center gap-2 px-6 py-3 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 font-semibold transition">
                            {saving ? 'Salvando...' : <><Save size={18} /> Salvar Alterações</>}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}