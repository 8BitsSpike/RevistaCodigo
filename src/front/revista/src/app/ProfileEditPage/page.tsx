'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import useAuth from '@/hooks/useAuth';
import { USER_API_BASE } from '@/lib/fetcher';
import { ArrowLeft, Save, Plus, X, Upload, Edit, Trash2, Calendar } from 'lucide-react';
import toast from 'react-hot-toast';
import Image from 'next/image';
import { formatDate } from '@/lib/dateUtils';

interface InstituicaoInfo {
    instituicao: string;
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
        endereco: '',
        foto: '',
    });

    const [infoList, setInfoList] = useState<InstituicaoInfo[]>([]);
    const [atuacaoList, setAtuacaoList] = useState<InstituicaoInfo[]>([]);

    const [newEdu, setNewEdu] = useState<InstituicaoInfo>({
        instituicao: '', areaAtuacao: '', contribuicao: '', informacoesAdd: '', dataInicio: '', dataFim: ''
    });
    const [newAtuacao, setNewAtuacao] = useState<InstituicaoInfo>({
        instituicao: '', areaAtuacao: '', contribuicao: '', informacoesAdd: '', dataInicio: '', dataFim: ''
    });

    const [loadingData, setLoadingData] = useState(true);
    const [saving, setSaving] = useState(false);

    // Helper to convert ISO date (from API) to YYYY-MM-DD (for Input)
    const toInputDate = (isoString?: string) => {
        if (!isoString) return '';
        return isoString.split('T')[0];
    };

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
                    endereco: data.endereco || '',
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

    const addEducation = () => {
        if (newEdu.instituicao) {
            setInfoList([...infoList, newEdu]);
            setNewEdu({ instituicao: '', areaAtuacao: '', contribuicao: '', informacoesAdd: '', dataInicio: '', dataFim: '' });
        } else {
            toast.error("Nome da Instituição é obrigatório");
        }
    };

    const removeEducation = (index: number) => {
        if (confirm("Remover esta formação?")) {
            setInfoList(infoList.filter((_, i) => i !== index));
        }
    };

    const addAtuacao = () => {
        if (newAtuacao.instituicao) {
            setAtuacaoList([...atuacaoList, newAtuacao]);
            setNewAtuacao({ instituicao: '', areaAtuacao: '', contribuicao: '', informacoesAdd: '', dataInicio: '', dataFim: '' });
        } else {
            toast.error("Nome da Instituição/Empresa é obrigatório");
        }
    };

    const removeAtuacao = (index: number) => {
        if (confirm("Remover esta atuação?")) {
            setAtuacaoList(atuacaoList.filter((_, i) => i !== index));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!user) return;
        if (!formData.name.trim() || !formData.sobrenome.trim()) return toast.error('Nome e Sobrenome são obrigatórios.');

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
            if (!res.ok) throw new Error('Falha ao atualizar');

            toast.success('Perfil atualizado!');
            localStorage.setItem('userName', formData.name);
            localStorage.setItem('userFoto', formData.foto);
            router.push('/profile');
        } catch (err) {
            console.error(err);
            toast.error('Erro ao salvar.');
        } finally {
            setSaving(false);
        }
    };

    if (loadingData) return <div className="min-h-screen flex items-center justify-center">Carregando...</div>;

    return (
        <div className="min-h-screen bg-gray-50 py-10">
            <div className="max-w-3xl mx-auto bg-white rounded-lg shadow-md overflow-hidden">
                <div className="bg-emerald-600 px-6 py-4 flex justify-between items-center">
                    <h1 className="text-xl font-bold text-white flex items-center gap-2"><Edit size={20} /> Editar Perfil</h1>
                    <button onClick={() => { if (confirm("Descartar?")) router.back(); }} className="text-emerald-100 hover:text-white text-sm font-medium flex items-center gap-1"><ArrowLeft size={16} /> Voltar</button>
                </div>

                <form onSubmit={handleSubmit} className="p-8 space-y-8">
                    <div className="flex flex-col items-center">
                        <div className="relative w-32 h-32 rounded-full overflow-hidden bg-gray-200 border-4 border-white shadow-md mb-4">
                            {/* FIX: Image fallback */}
                            <Image src={formData.foto || '/faviccon.png'} alt="Preview" fill className="object-cover" />
                        </div>
                        <label className="cursor-pointer bg-emerald-50 text-emerald-700 px-4 py-2 rounded-md text-sm font-medium hover:bg-emerald-100 transition flex items-center gap-2">
                            <Upload size={16} /> Alterar Foto
                            <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                        </label>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div><label className="block text-sm font-medium text-gray-700 mb-1">Nome *</label><input type="text" name="name" value={formData.name} onChange={handleInputChange} className="input-std" required /></div>
                        <div><label className="block text-sm font-medium text-gray-700 mb-1">Sobrenome *</label><input type="text" name="sobrenome" value={formData.sobrenome} onChange={handleInputChange} className="input-std" required /></div>
                        <div className="md:col-span-2"><label className="block text-sm font-medium text-gray-700 mb-1">Email</label><input type="email" name="email" value={formData.email} className="input-std bg-gray-50" disabled /></div>
                        <div className="md:col-span-2"><label className="block text-sm font-medium text-gray-700 mb-1">Endereço</label><input type="text" name="endereco" value={formData.endereco} onChange={handleInputChange} className="input-std" /></div>
                        <div className="md:col-span-2"><label className="block text-sm font-medium text-gray-700 mb-1">Biografia</label><textarea name="biografia" value={formData.biografia} onChange={handleInputChange} rows={4} className="input-std resize-none" /></div>
                    </div>

                    <div className="space-y-6 border-t pt-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Informações Institucionais</label>
                            <div className="bg-gray-50 p-4 rounded-lg border mb-4 space-y-3">
                                <div className="grid grid-cols-2 gap-3">
                                    <input type="text" placeholder="Instituição" value={newEdu.instituicao} onChange={e => setNewEdu({ ...newEdu, instituicao: e.target.value })} className="input-std text-sm" />
                                    <input type="text" placeholder="Curso" value={newEdu.areaAtuacao || ''} onChange={e => setNewEdu({ ...newEdu, areaAtuacao: e.target.value })} className="input-std text-sm" />
                                </div>
                                <div className="grid grid-cols-2 gap-3">
                                    <input type="date" placeholder="Início" value={toInputDate(newEdu.dataInicio)} onChange={e => setNewEdu({ ...newEdu, dataInicio: e.target.value })} className="input-std text-sm" />
                                    <input type="date" placeholder="Fim" value={toInputDate(newEdu.dataFim)} onChange={e => setNewEdu({ ...newEdu, dataFim: e.target.value })} className="input-std text-sm" />
                                </div>
                                <input type="text" placeholder="Contribuição" value={newEdu.contribuicao || ''} onChange={e => setNewEdu({ ...newEdu, contribuicao: e.target.value })} className="input-std text-sm" />
                                <textarea placeholder="Informações Adicionais" value={newEdu.informacoesAdd || ''} onChange={e => setNewEdu({ ...newEdu, informacoesAdd: e.target.value })} className="input-std text-sm h-16 resize-none" />
                                <button type="button" onClick={addEducation} className="btn-primary text-sm w-full"><Plus size={16} /> Adicionar Formação</button>
                            </div>

                            <ul className="space-y-2">
                                {infoList.map((item, idx) => (
                                    <li key={idx} className="flex justify-between items-center bg-white p-3 rounded border shadow-sm">
                                        <div>
                                            <p className="font-semibold text-sm text-gray-800">{item.instituicao}</p>
                                            <p className="text-xs text-gray-600">{item.areaAtuacao}</p>
                                            {/* FIX: Show formatted date in list */}
                                            {(item.dataInicio || item.dataFim) && (
                                                <p className="text-xs text-emerald-600 flex items-center gap-1 mt-1">
                                                    <Calendar size={12} />
                                                    {formatDate(item.dataInicio || '')} - {item.dataFim ? formatDate(item.dataFim) : 'Atual'}
                                                </p>
                                            )}
                                        </div>
                                        <button type="button" onClick={() => removeEducation(idx)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                                    </li>
                                ))}
                            </ul>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Áreas de Atuação</label>
                            <div className="bg-gray-50 p-4 rounded-lg border mb-4 space-y-3">
                                <div className="grid grid-cols-2 gap-3">
                                    <input type="text" placeholder="Empresa/Instituição" value={newAtuacao.instituicao} onChange={e => setNewAtuacao({ ...newAtuacao, instituicao: e.target.value })} className="input-std text-sm" />
                                    <input type="text" placeholder="Cargo/Função" value={newAtuacao.areaAtuacao || ''} onChange={e => setNewAtuacao({ ...newAtuacao, areaAtuacao: e.target.value })} className="input-std text-sm" />
                                </div>
                                <div className="grid grid-cols-2 gap-3">
                                    <input type="date" placeholder="Início" value={toInputDate(newAtuacao.dataInicio)} onChange={e => setNewAtuacao({ ...newAtuacao, dataInicio: e.target.value })} className="input-std text-sm" />
                                    <input type="date" placeholder="Fim" value={toInputDate(newAtuacao.dataFim)} onChange={e => setNewAtuacao({ ...newAtuacao, dataFim: e.target.value })} className="input-std text-sm" />
                                </div>
                                <input type="text" placeholder="Contribuições e Descrição das Atividades" value={newAtuacao.contribuicao || ''} onChange={e => setNewAtuacao({ ...newAtuacao, contribuicao: e.target.value })} className="input-std text-sm" />
                                <textarea placeholder="Informações Adicionais" value={newAtuacao.informacoesAdd || ''} onChange={e => setNewAtuacao({ ...newAtuacao, informacoesAdd: e.target.value })} className="input-std text-sm h-16 resize-none" />
                                <button type="button" onClick={addAtuacao} className="btn-primary text-sm w-full"><Plus size={16} /> Adicionar Atuação</button>
                            </div>

                            <ul className="space-y-2">
                                {atuacaoList.map((item, idx) => (
                                    <li key={idx} className="flex justify-between items-center bg-white p-3 rounded border shadow-sm">
                                        <div>
                                            <p className="font-semibold text-sm text-gray-800">{item.instituicao}</p>
                                            <p className="text-xs text-gray-600">{item.areaAtuacao}</p>
                                            {(item.dataInicio || item.dataFim) && (
                                                <p className="text-xs text-emerald-600 flex items-center gap-1 mt-1">
                                                    <Calendar size={12} />
                                                    {formatDate(item.dataInicio || '')} - {item.dataFim ? formatDate(item.dataFim) : 'Atual'}
                                                </p>
                                            )}
                                        </div>
                                        <button type="button" onClick={() => removeAtuacao(idx)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="flex justify-end pt-6 border-t">
                        <button type="submit" disabled={saving} className="btn-primary px-6 py-3">{saving ? 'Salvando...' : <><Save size={18} /> Salvar Alterações</>}</button>
                    </div>
                </form>
            </div>
        </div>
    );
}