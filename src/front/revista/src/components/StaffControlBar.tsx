'use client';

import { useState, useEffect, ChangeEvent, useMemo } from 'react';
import { useMutation } from '@apollo/client';
import {
    ATUALIZAR_METADADOS_ARTIGO,
    ATUALIZAR_EQUIPE_EDITORIAL,
} from '@/graphql/queries';
import { USER_API_BASE, OPTION } from '@/lib/fetcher';
import CommentaryModal from './CommentaryModal';
import { StatusArtigo, PosicaoEditorial, TipoArtigo, FuncaoTrabalho } from '@/types/enums';
import { StaffMember } from '@/components/StaffCard';
import { Search, X } from 'lucide-react';
import Image from 'next/image';
import toast from 'react-hot-toast';

// --- Tipos ---
interface EditorialTeamData {
    initialAuthorId: string[];
    editorIds: string[];
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

type ListTeamRole = 'initialAuthorId' | 'reviewerIds' | 'correctorIds' | 'editorIds';

// --- Componente Interno (Caixa de Busca Múltipla) ---
interface TeamSearchBoxProps {
    title: string;
    role: ListTeamRole;
    currentIds: string[];
    renderList: StaffMember[]; 
    excludeIds: string[];
    restrictToStaff?: boolean; 
    onAdd: (role: ListTeamRole, user: UsuarioBusca) => void;
    onRemove: (role: ListTeamRole, userId: string) => void;
}

function TeamSearchBox({ title, role, currentIds, renderList, excludeIds, restrictToStaff, onAdd, onRemove }: TeamSearchBoxProps) {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState<UsuarioBusca[]>([]);

    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (query.length < 3) {
                setResults([]);
                return;
            }
            const token = localStorage.getItem('userToken');
            if (!token) return;

            try {
                const res = await fetch(`${USER_API_BASE}/UserSearch?nome=${query}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data: UsuarioBusca[] = await res.json();
                    
                    const filtered = data.filter(u => {
                        if (currentIds.includes(u.id)) return false;
                        if (excludeIds.includes(u.id)) return false;

                        if (restrictToStaff) {
                            const isStaff = renderList.some(s => s.usuarioId === u.id);
                            if (!isStaff) return false;
                        }

                        return true;
                    });
                    
                    setResults(filtered);
                }
            } catch (err) { console.error("Erro buscando usuários", err); }
        }, 500);

        return () => clearTimeout(delayDebounceFn);
    }, [query, currentIds, excludeIds, restrictToStaff, renderList]);

    const members = currentIds.map(id => renderList.find(s => s.usuarioId === id)).filter(Boolean) as StaffMember[];

    return (
        <div className="flex-1 min-w-[200px]">
            <p className="text-sm font-semibold text-gray-600 mb-2 text-right pr-2">{title}</p>
            <div className="relative">
                <input
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder={restrictToStaff ? "Buscar Staff..." : "Buscar Usuário..."}
                    className="w-full p-2 pr-10 border border-gray-300 rounded-md text-sm"
                />
                <Search size={16} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400" />
                {results.length > 0 && (
                    <ul className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
                        {results.map(u => (
                            <li
                                key={u.id}
                                onClick={() => { onAdd(role, u); setQuery(''); setResults([]); }}
                                className="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer"
                            >
                                <Image src={u.foto || '/faviccon.png'} alt={u.name} width={24} height={24} className="rounded-full" />
                                <span className="text-sm">{u.name} {u.sobrenome}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
            <div className="mt-2 h-[120px] overflow-y-auto border bg-gray-50 rounded-md p-2 space-y-2">
                {members.length === 0 && (
                    <p className="text-xs text-gray-400 text-center p-4">Vazio</p>
                )}
                {members.map(member => (
                    <div key={member.usuarioId} className="flex items-center justify-between p-1 bg-white rounded border">
                        <div className="flex items-center gap-2">
                            <div className="w-[24px] h-[24px] relative rounded-full overflow-hidden">
                                <Image src={member.url || '/faviccon.png'} alt={member.nome} fill className="object-cover" />
                            </div>
                            <span className="text-xs font-medium truncate max-w-[100px]">{member.nome}</span>
                        </div>
                        <button onClick={() => onRemove(role, member.usuarioId)} className="text-red-400 hover:text-red-600">
                            <X size={14} />
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
}

// --- Componente Principal ---

export default function StaffControlBar({ artigoId, editorialId, currentData, staffList, onUpdate }: StaffControlBarProps) {
    const [formData, setFormData] = useState({
        status: currentData.status,
        posicao: currentData.editorial.position,
        tipo: currentData.tipo,
        permitirComentario: currentData.permitirComentario
    });

    const [teamData, setTeamData] = useState(currentData.editorial.team);
    const [extraMembers, setExtraMembers] = useState<StaffMember[]>([]);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const [atualizarMetadados, { loading: loadingMeta }] = useMutation(ATUALIZAR_METADADOS_ARTIGO);
    const [atualizarEquipe, { loading: loadingTeam }] = useMutation(ATUALIZAR_EQUIPE_EDITORIAL);

    const loading = loadingMeta || loadingTeam;


    // === EFEITO DE HIDRATAÇÃO ===
    useEffect(() => {
        const fetchMissingMembers = async () => {
            const allTeamIds = [
                ...(currentData.editorial.team.initialAuthorId || []),
                ...(currentData.editorial.team.reviewerIds || []),
                ...(currentData.editorial.team.correctorIds || []),
                ...(currentData.editorial.team.editorIds || [])
            ].filter(Boolean);

            const missingIds = allTeamIds.filter(id => 
                !staffList.some(s => s.usuarioId === id) && 
                !extraMembers.some(e => e.usuarioId === id)
            );

            if (missingIds.length === 0) return;

            const token = localStorage.getItem('userToken');
            if (!token) return;

            const fetchedMembers: StaffMember[] = [];

            await Promise.all(missingIds.map(async (id) => {
                try {
                    const res = await fetch(`${USER_API_BASE}/GetUserLimited?id=${id}&token=${token}&option=${OPTION}`, {
                        headers: { Authorization: `Bearer ${token}` }
                    });

                    if (res.ok) {
                        const textData = await res.text();
                        if (textData && textData.length > 0) {
                            const u = JSON.parse(textData);
                            
                            // --- DEBUG CRUCIAL ---
                            // Abra o F12 e veja o que aparece aqui
                            console.log(`Dados RAW do usuário ${id}:`, u);
                            console.log(`Chaves disponíveis:`, Object.keys(u));

                            // TENTATIVA ABRANGENTE DE MAPEAMENTO
                            // 1. ID
                            const realId = u.id || u.Id || u.usuarioId || u.UsuarioId || u.UserId || id;
                            
                            // 2. NOME (Tenta Nome Completo ou Primeiro Nome)
                            const firstName = u.name || u.Name || u.nome || u.Nome || u.FirstName || u.given_name || '';
                            
                            // 3. SOBRENOME
                            const lastName = u.sobrenome || u.Sobrenome || u.Surname || u.LastName || u.family_name || '';
                            
                            // 4. FOTO
                            const photo = u.foto || u.Foto || u.url || u.Url || u.ImageUrl || u.ProfilePicture || '';

                            // Se tivermos apenas FirstName e LastName separados
                            let finalName = firstName;
                            if (lastName) finalName = `${firstName} ${lastName}`;
                            
                            // Se não achou nada acima, tenta procurar por "FullName"
                            if (!finalName.trim()) {
                                finalName = u.FullName || u.fullName || u.UserName || u.Email || 'Usuário Sem Nome';
                            }

                            fetchedMembers.push({
                                usuarioId: realId,
                                nome: finalName.trim(),
                                url: photo,
                                job: FuncaoTrabalho.Aposentado as any, 
                                isActive: true
                            } as StaffMember);
                        }
                    }
                } catch (err) {
                    console.error(`Erro ao carregar utilizador ${id}`, err);
                }
            }));

            if (fetchedMembers.length > 0) {
                setExtraMembers(prev => {
                    // Filtra duplicatas antes de adicionar
                    const uniqueNewMembers = fetchedMembers.filter(
                        fm => !prev.some(p => p.usuarioId === fm.usuarioId)
                    );
                    return [...prev, ...uniqueNewMembers];
                });
            }
        };

        fetchMissingMembers();
    }, [currentData.editorial.team, staffList]); // eslint-disable-line react-hooks/exhaustive-deps

    const allKnownMembers = useMemo(() => {
        const map = new Map<string, StaffMember>();
        // Staff tem prioridade
        staffList.forEach(s => map.set(s.usuarioId, s));
        // Extras preenchem os buracos
        extraMembers.forEach(s => {
            if (!map.has(s.usuarioId)) map.set(s.usuarioId, s);
        });
        return Array.from(map.values());
    }, [staffList, extraMembers]);

    const handleTeamAdd = (role: ListTeamRole, user: UsuarioBusca) => {
        setTeamData(prev => ({
            ...prev,
            [role]: [...prev[role], user.id]
        }));

        // Adiciona imediatamente ao extraMembers para aparecer na lista sem delay
        setExtraMembers(prev => {
            // Evita duplicatas
            if (prev.some(p => p.usuarioId === user.id)) return prev;
            
            return [...prev, {
                usuarioId: user.id,
                nome: `${user.name} ${user.sobrenome || ''}`.trim(),
                url: user.foto || '',
                job: FuncaoTrabalho.Aposentado as any,
                isActive: true,
            } as StaffMember];
        });
    };

    const handleTeamRemove = (role: ListTeamRole, userId: string) => {
        setTeamData(prev => ({
            ...prev,
            [role]: prev[role].filter((id: string) => id !== userId)
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
        // Não limpamos extraMembers aqui para não sumir com os dados já carregados
    };

    const handleSaveClick = () => {
        setIsModalOpen(true);
    };

    const handleConfirmSave = async (commentary: string) => {
        const toastId = toast.loading("Salvando alterações...");

        try {
            const mutationsToRun = [];

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

            if (JSON.stringify(teamData) !== JSON.stringify(currentData.editorial.team)) {
                const cleanTeamInput = {
                    initialAuthorId: teamData.initialAuthorId || [],
                    editorIds: teamData.editorIds || [],
                    reviewerIds: teamData.reviewerIds || [],
                    correctorIds: teamData.correctorIds || []
                };

                mutationsToRun.push(
                    atualizarEquipe({
                        variables: { 
                            artigoId: artigoId, 
                            teamInput: cleanTeamInput,
                            commentary 
                        }
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

            <div className="mb-8 p-4 border border-gray-200 shadow-sm bg-white rounded-lg">
                {/* ... Campos de Status, Posição, Tipo ... */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end mb-6">
                    <div>
                        <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Status</label>
                        <select name="status" value={formData.status} onChange={handleFormChange} className="input-std text-sm">
                            {Object.values(StatusArtigo).map(s => <option key={s} value={s}>{s}</option>)}
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Posição</label>
                        <select name="posicao" value={formData.posicao} onChange={handleFormChange} className="input-std text-sm">
                            {Object.values(PosicaoEditorial).map(p => <option key={p} value={p}>{p}</option>)}
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Tipo</label>
                        <select name="tipo" value={formData.tipo} onChange={handleFormChange} className="input-std text-sm">
                            {Object.values(TipoArtigo).map(t => <option key={t} value={t}>{t}</option>)}
                        </select>
                    </div>
                    <div className="flex items-center pb-2">
                        <input
                            type="checkbox"
                            id="permitirComentario"
                            name="permitirComentario"
                            checked={formData.permitirComentario}
                            onChange={handleFormChange}
                            className="h-4 w-4 text-emerald-600 border-gray-300 rounded focus:ring-emerald-500"
                        />
                        <label htmlFor="permitirComentario" className="ml-2 text-sm font-semibold text-gray-700">Comentários</label>
                    </div>
                </div>

                <div className="border-t border-gray-100 pt-6">
                    <h3 className="text-sm font-bold text-gray-500 uppercase mb-4">Equipe Editorial</h3>
                    <div className="flex flex-wrap gap-4">
                        {/* AUTORES */}
                        <TeamSearchBox
                            title="Autores:"
                            role="initialAuthorId"
                            currentIds={teamData.initialAuthorId}
                            renderList={allKnownMembers} 
                            excludeIds={[]}
                            restrictToStaff={false}
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />
                        
                        {/* REVISORES */}
                        <TeamSearchBox
                            title="Revisores:"
                            role="reviewerIds"
                            currentIds={teamData.reviewerIds}
                            renderList={allKnownMembers}
                            excludeIds={teamData.initialAuthorId} 
                            restrictToStaff={false}
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />

                        {/* CORRETORES */}
                        <TeamSearchBox
                            title="Corretores:"
                            role="correctorIds"
                            currentIds={teamData.correctorIds}
                            renderList={allKnownMembers}
                            excludeIds={teamData.initialAuthorId}
                            restrictToStaff={false} 
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />

                        {/* EDITORES CHEFES */}
                        <TeamSearchBox
                            title="Editores Chefes:"
                            role="editorIds"
                            currentIds={teamData.editorIds}
                            renderList={allKnownMembers}
                            excludeIds={teamData.initialAuthorId}
                            restrictToStaff={true} 
                            onAdd={handleTeamAdd}
                            onRemove={handleTeamRemove}
                        />
                    </div>
                </div>

                <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-gray-100">
                    <button
                        onClick={handleCancel}
                        disabled={loading}
                        className="px-4 py-2 rounded-md border border-gray-300 text-gray-600 text-sm font-medium hover:bg-gray-50 transition"
                    >
                        Reverter
                    </button>
                    <button
                        onClick={handleSaveClick}
                        disabled={loading}
                        className="px-4 py-2 rounded-md bg-emerald-600 text-white text-sm font-bold shadow hover:bg-emerald-700 transition disabled:opacity-50"
                    >
                        Salvar Tudo
                    </button>
                </div>
            </div>
        </>
    );
}