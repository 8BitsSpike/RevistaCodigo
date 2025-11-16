'use client';

import { useState, useEffect, ChangeEvent } from 'react';
import { useMutation, useLazyQuery } from '@apollo/client';
import {
    ATUALIZAR_METADADOS_ARTIGO,
    ATUALIZAR_EQUIPE_EDITORIAL,
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import CommentaryModal from './CommentaryModal';
import { StatusArtigo, PosicaoEditorial, TipoArtigo } from '@/types/enums';
import { StaffMember } from '@/components/StaffCard';
import { X, Search, User } from 'lucide-react';
import Image from 'next/image';
import toast from 'react-hot-toast';

// --- Tipos ---

interface EditorialTeamData {
    initialAuthorId: string[];
    editorId: string;
    reviewerIds: string[];
    correctorIds: string[];
}

interface ArtigoData {
    id: string;
    status: StatusArtigo;
    tipo: TipoArtigo;
    permitirComentario: boolean;
    editorial: {
        position: PosicaoEditorial;
        team: EditorialTeamData;
    };
}

interface StaffControlBarProps {
    artigoId: string;
    editorialId: string;
    currentData: ArtigoData;
    staffList: StaffMember[];
    onUpdate: () => void;
}

interface UsuarioBusca {
    id: string;
    name: string;
    sobrenome?: string;
    foto?: string;
}

const API_USUARIO_BASE = 'https://localhost:44387/api/Usuario';

// Tipos de Papel separados
type ListTeamRole = 'initialAuthorId' | 'reviewerIds' | 'correctorIds';
type SingleTeamRole = 'editorId';

// --- Componente Interno 1 (Caixa de Busca Múltipla) ---
interface TeamSearchBoxProps {
    title: string;
    role: ListTeamRole;
    currentIds: string[];
    allStaff: StaffMember[];
    authorIds: string[];
    onAdd: (role: ListTeamRole, userId: string) => void;
    onRemove: (role: ListTeamRole, userId: string) => void;
}

function TeamSearchBox({ title, role, currentIds, allStaff, authorIds, onAdd, onRemove }: TeamSearchBoxProps) {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState<UsuarioBusca[]>([]);

    // Busca na UsuarioAPI
    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (query.length < 3) {
                setResults([]);
                return;
            }
            const token = localStorage.getItem('jwtToken');
            if (!token) return;

            try {
                const res = await fetch(`${API_USUARIO_BASE}/Search?name=${query}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data: UsuarioBusca[] = await res.json();
                    const filtered = data.filter(u =>
                        !currentIds.includes(u.id) &&
                        (role === 'initialAuthorId' ? true : !authorIds.includes(u.id))
                    );
                    setResults(filtered);
                }
            } catch (err) { console.error("Erro buscando usuários", err); }
        }, 500);

        return () => clearTimeout(delayDebounceFn);
    }, [query, currentIds, authorIds, role]);

    const members = currentIds.map(id => allStaff.find(s => s.usuarioId === id)).filter(Boolean) as StaffMember[];

    return (
        <div className="flex-1 min-w-[200px]">
            <p className="text-sm font-semibold text-gray-600 mb-2 text-right pr-2">{title}</p>
            <div className="relative">
                <input
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Buscar membro..."
                    className="w-full p-2 pr-10 border border-gray-300 rounded-md text-sm"
                />
                <Search size={16} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400" />
                {results.length > 0 && (
                    <ul className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
                        {results.map(u => (
                            <li
                                key={u.id}
                                onClick={() => { onAdd(role, u.id); setQuery(''); setResults([]); }}
                                className="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer"
                            >
                                <Image src={u.foto || '/default-avatar.png'} alt={u.name} width={24} height={24} className="rounded-full" />
                                <span className="text-sm">{u.name} {u.sobrenome}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
            <div className="mt-2 h-[120px] overflow-y-auto border bg-gray-50 rounded-md p-2 space-y-2">
                {members.length === 0 && (
                    <p className="text-xs text-gray-400 text-center p-4">Inclua um {title.slice(0, -1)}</p>
                )}
                {members.map(member => (
                    <div key={member.usuarioId} className="flex items-center justify-between p-1 bg-white rounded border">
                        <div className="flex items-center gap-2">
                            <Image src={member.url || '/default-avatar.png'} alt={member.nome} width={30} height={30} className="rounded-full" />
                            <span className="text-sm font-medium">{member.nome}</span>
                        </div>
                        <button onClick={() => onRemove(role, member.usuarioId)} className="text-red-400 hover:text-red-600">
                            <X size={16} />
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
}

// --- Componente Interno 2 (Caixa de Busca Única - para EditorId) ---
interface SingleUserSearchBoxProps {
    title: string;
    role: SingleTeamRole;
    currentId: string;
    allStaff: StaffMember[];
    authorIds: string[];
    onSet: (role: SingleTeamRole, userId: string) => void;
}

function SingleUserSearchBox({ title, role, currentId, allStaff, authorIds, onSet }: SingleUserSearchBoxProps) {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState<UsuarioBusca[]>([]);

    useEffect(() => {
        // Mesma lógica de busca do TeamSearchBox
        const delayDebounceFn = setTimeout(async () => {
            if (query.length < 3) { setResults([]); return; }
            const token = localStorage.getItem('jwtToken');
            if (!token) return;
            try {
                const res = await fetch(`${API_USUARIO_BASE}/Search?name=${query}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data: UsuarioBusca[] = await res.json();
                    // Filtra o usuário atual e autores
                    const filtered = data.filter(u => u.id !== currentId && !authorIds.includes(u.id));
                    setResults(filtered);
                }
            } catch (err) { console.error("Erro buscando usuários", err); }
        }, 500);
        return () => clearTimeout(delayDebounceFn);
    }, [query, currentId, authorIds]);

    const member = allStaff.find(s => s.usuarioId === currentId);

    return (
        <div className="flex-1 min-w-[200px]">
            <p className="text-sm font-semibold text-gray-600 mb-2 text-right pr-2">{title}</p>
            <div className="relative">
                <input
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Buscar editor..."
                    className="w-full p-2 pr-10 border border-gray-300 rounded-md text-sm"
                />
                <Search size={16} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400" />
                {results.length > 0 && (
                    <ul className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
                        {results.map(u => (
                            <li
                                key={u.id}
                                onClick={() => { onSet(role, u.id); setQuery(''); setResults([]); }}
                                className="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer"
                            >
                                <Image src={u.foto || '/default-avatar.png'} alt={u.name} width={24} height={24} className="rounded-full" />
                                <span className="text-sm">{u.name} {u.sobrenome}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
            {/* Mostra o usuário selecionado ou um placeholder */}
            <div className="mt-2 h-[120px] overflow-y-auto border bg-gray-50 rounded-md p-2 space-y-2">
                {!member ? (
                    <p className="text-xs text-gray-400 text-center p-4">Nenhum editor definido</p>
                ) : (
                    <div className="flex items-center justify-between p-1 bg-white rounded border">
                        <div className="flex items-center gap-2">
                            <Image src={member.url || '/default-avatar.png'} alt={member.nome} width={30} height={30} className="rounded-full" />
                            <span className="text-sm font-medium">{member.nome}</span>
                        </div>
                        <button onClick={() => onSet(role, '')} className="text-red-400 hover:text-red-600">
                            <X size={16} />
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}

// --- Componente Principal (StaffControlBar) ---

export default function StaffControlBar({ artigoId, editorialId, currentData, staffList, onUpdate }: StaffControlBarProps) {

    const [formData, setFormData] = useState({
        status: currentData.status,
        posicao: currentData.editorial.position,
        tipo: currentData.tipo,
        permitirComentario: currentData.permitirComentario
    });

    const [teamData, setTeamData] = useState(currentData.editorial.team);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const [atualizarMetadados, { loading: loadingMeta }] = useMutation(ATUALIZAR_METADADOS_ARTIGO);
    const [atualizarEquipe, { loading: loadingTeam }] = useMutation(ATUALIZAR_EQUIPE_EDITORIAL);

    const loading = loadingMeta || loadingTeam;

    // --- (HANDLERS CORRIGIDOS) ---

    // Para listas (Autores, Revisores, Corretores)
    const handleTeamAdd = (role: ListTeamRole, userId: string) => {
        setTeamData(prev => ({
            ...prev,
            [role]: [...prev[role], userId]
        }));
    };

    const handleTeamRemove = (role: ListTeamRole, userId: string) => {
        setTeamData(prev => ({
            ...prev,
            // O 'filter' agora só opera em arrays (ListTeamRole)
            [role]: prev[role].filter((id: string) => id !== userId)
        }));
    };

    // Handler para campos de string única (EditorId)
    const handleTeamSet = (role: SingleTeamRole, userId: string) => {
        setTeamData(prev => ({
            ...prev,
            [role]: userId // Define a string diretamente
        }));
    };

    const handleFormChange = (e: ChangeEvent<HTMLSelectElement | HTMLInputElement>) => {
        const { name, value, type } = e.target;
        const isCheckbox = type === 'checkbox';

        setFormData(prev => ({
            ...prev,
            [name]: isCheckbox ? (e.target as HTMLInputElement).checked : value
        }));
    };

    const handleCancel = () => {
        setFormData({
            status: currentData.status,
            posicao: currentData.editorial.position,
            tipo: currentData.tipo,
            permitirComentario: currentData.permitirComentario
        });
        setTeamData(currentData.editorial.team);
    };

    const handleSaveClick = () => {
        setIsModalOpen(true);
    };

    // Chamado pelo modal
    const handleConfirmSave = async (commentary: string) => {
        const toastId = toast.loading("Salvando alterações...");

        try {
            const mutationsToRun = [];

            // Mutação de Metadados
            const metaInput = {
                status: formData.status !== currentData.status ? formData.status : null,
                tipo: formData.tipo !== currentData.tipo ? formData.tipo : null,
                permitirComentario: formData.permitirComentario !== currentData.permitirComentario ? formData.permitirComentario : null,
                posicao: formData.posicao !== currentData.editorial.position ? formData.posicao : null,
                titulo: null,
                resumo: null,
                idsAutor: null,
                referenciasAutor: null,
            };
            if (Object.values(metaInput).some(v => v !== null)) {
                mutationsToRun.push(
                    atualizarMetadados({
                        variables: { id: artigoId, input: metaInput, commentary }
                    })
                );
            }

            // 2. Mutação de Equipe
            if (JSON.stringify(teamData) !== JSON.stringify(currentData.editorial.team)) {
                mutationsToRun.push(
                    atualizarEquipe({
                        variables: { artigoId: artigoId, teamInput: teamData, commentary }
                    })
                );
            }

            if (mutationsToRun.length === 0) {
                toast.error("Nenhuma alteração detectada.", { id: toastId });
                setIsModalOpen(false);
                return;
            }

            await Promise.all(mutationsToRun);

            toast.success("Alterações salvas com sucesso!", { id: toastId });
            onUpdate();
            setIsModalOpen(false);

        } catch (err: any) {
            console.error("Erro ao salvar:", err);
            toast.error(`Falha ao salvar: ${err.message}`, { id: toastId });
            setIsModalOpen(false);
        }
    };

    return (
        <>
            <CommentaryModal
                isOpen={isModalOpen}
                title="Justificar Alterações"
                loading={loading}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleConfirmSave}
            />

            <div
                className="mb-8 p-4 border-2 border-gray-400 shadow-md"
                style={{
                    border: '2px solid gray',
                    boxShadow: '0 6px 10px rgba(0,0,0,0.4)',
                }}
            >
                {/* --- Formulário Superior (Metadados) --- */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
                    <div>
                        <label className="block text-sm font-semibold">Status do Artigo</label>
                        <select name="status" value={formData.status} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1 bg-white">
                            {Object.values(StatusArtigo).map(s => <option key={s} value={s}>{s.replace(/([A-Z])/g, ' $1').trim()}</option>)}
                        </select>
                    </div>
                    <div>
                        <label className="block text-sm font-semibold">Posição Editorial</label>
                        <select name="posicao" value={formData.posicao} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1 bg-white">
                            {Object.values(PosicaoEditorial).map(p => <option key={p} value={p}>{p.replace(/([A-Z])/g, ' $1').trim()}</option>)}
                        </select>
                    </div>
                    <div>
                        <label className="block text-sm font-semibold">Tipo de Artigo</label>
                        <select name="tipo" value={formData.tipo} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1 bg-white">
                            {Object.values(TipoArtigo).map(t => <option key={t} value={t}>{t}</option>)}
                        </select>
                    </div>
                    <div className="flex items-center justify-center pb-2">
                        <input
                            type="checkbox"
                            id="permitirComentario"
                            name="permitirComentario"
                            checked={formData.permitirComentario}
                            onChange={handleFormChange}
                            className="h-4 w-4 text-emerald-600 border-gray-300 rounded"
                        />
                        <label htmlFor="permitirComentario" className="ml-2 text-sm font-semibold">Permitir Comentários</label>
                    </div>
                </div>

                {/* --- Divisor --- */}
                <div className="w-[70%] h-px bg-gray-300 my-6 mx-auto"></div>

                {/* --- Formulário Inferior (Equipe) --- */}
                <div>
                    <h3 className="text-lg font-semibold text-gray-800 text-center mb-4">Equipe Editorial</h3>
                    <div className="flex flex-wrap gap-4">

                        <TeamSearchBox
                            title="Autores:"
                            role="initialAuthorId"
                            currentIds={teamData.initialAuthorId}
                            allStaff={staffList}
                            authorIds={[]}
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />
                        <TeamSearchBox
                            title="Revisores:"
                            role="reviewerIds"
                            currentIds={teamData.reviewerIds}
                            allStaff={staffList}
                            authorIds={teamData.initialAuthorId}
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />
                        <TeamSearchBox
                            title="Corretores:"
                            role="correctorIds"
                            currentIds={teamData.correctorIds}
                            allStaff={staffList}
                            authorIds={teamData.initialAuthorId}
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />

                        {/* Renderiza o novo componente para EditorId */}
                        <SingleUserSearchBox
                            title="Editor Chefe:"
                            role="editorId"
                            currentId={teamData.editorId}
                            allStaff={staffList}
                            authorIds={teamData.initialAuthorId}
                            onSet={handleTeamSet}
                        />

                    </div>
                </div>

                {/* --- Botões de Ação (Salvar/Cancelar) --- */}
                <div className="flex justify-end gap-4 mt-8 pt-4 border-t border-gray-200">
                    <button
                        onClick={handleCancel}
                        disabled={loading}
                        className="px-6 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 font-medium hover:bg-gray-50 transition"
                    >
                        Cancelar
                    </button>
                    <button
                        onClick={handleSaveClick}
                        disabled={loading}
                        className="px-6 py-2 rounded-lg bg-emerald-600 text-white font-bold shadow hover:bg-emerald-700 transition"
                    >
                        Salvar Alterações
                    </button>
                </div>
            </div>
        </>
    );
}