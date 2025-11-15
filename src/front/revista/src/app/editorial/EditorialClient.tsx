'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, ApolloError } from '@apollo/client';
import { OBTER_STAFF_LIST, CRIAR_NOVO_STAFF } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import StaffCard, { StaffMember } from '@/components/StaffCard';
import { User, UserPlus, Check, X } from 'lucide-react';
import Image from 'next/image';
import CommentaryModal from '@/components/CommentaryModal';
import toast from 'react-hot-toast';

// --- Interfaces ---

interface StaffListData {
    obterStaffList: (StaffMember | null | undefined)[];
}

interface UsuarioBusca {
    id: string;
    name: string;
    sobrenome?: string;
    foto?: string;
}

interface CriarStaffData {
    criarNovoStaff: StaffMember;
}

const API_USUARIO_BASE = 'https://localhost:44387/api/Usuario';
const PAGE_SIZE = 50;

export default function EditorialClient() {
    const router = useRouter();
    const { user, logout } = useAuth();

    // --- Estados da Página ---
    const [showHireForm, setShowHireForm] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [isHireModalOpen, setIsHireModalOpen] = useState(false);

    // --- Estados do Formulário "Contratar" ---
    const [selectedUser, setSelectedUser] = useState<UsuarioBusca | null>(null);
    const [userSearchQuery, setUserSearchQuery] = useState('');
    const [userSearchResults, setUserSearchResults] = useState<UsuarioBusca[]>([]);
    const [selectedJob, setSelectedJob] = useState<'EditorBolsista' | 'EditorChefe' | 'Administrador'>('EditorBolsista');

    // --- Query: Buscar Lista de Staff ---
    const { data, loading, error, refetch } = useQuery<StaffListData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: PAGE_SIZE },
        fetchPolicy: 'cache-and-network',
        onError: (err: ApolloError) => {
            if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) {
                toast.error("Acesso negado. Você não tem permissão para ver esta página.");
                localStorage.removeItem('isStaff');
                logout();
                router.push('/');
            } else {
                console.error("Erro desconhecido ao buscar staff:", err);
            }
        }
    });

    // --- Mutações ---
    const [criarStaff, { loading: loadingCreate }] = useMutation<CriarStaffData>(CRIAR_NOVO_STAFF, {
        onCompleted: () => {
            toast.success('Novo membro da equipe contratado!');
            setShowHireForm(false);
            setSelectedUser(null);
            setUserSearchQuery('');
            setIsHireModalOpen(false);
            refetch();
        },
        onError: (err) => {
            setErrorMsg(`Erro ao contratar: ${err.message}`);
            toast.error(`Erro ao contratar: ${err.message}`);
            setIsHireModalOpen(false);
        }
    });

    // --- Lógica de Busca de Usuário (para o formulário) ---
    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (userSearchQuery.length < 3) {
                setUserSearchResults([]);
                return;
            }
            const token = localStorage.getItem('jwtToken');
            if (!token) return;

            try {
                const res = await fetch(`${API_USUARIO_BASE}/Search?name=${userSearchQuery}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const dataApi = await res.json();

                    const staffIds = data?.obterStaffList
                        ? data.obterStaffList
                            .filter(s => !!s)
                            .map((s) => s!.usuarioId)
                        : [];

                    const filtered = dataApi.filter((u: any) => !staffIds.includes(u.id));
                    setUserSearchResults(filtered);
                }
            } catch (err) {
                console.error("Erro buscando usuários", err);
            }
        }, 500);

        return () => clearTimeout(delayDebounceFn);
    }, [userSearchQuery, data?.obterStaffList]);

    // --- Handlers ---

    const handleSelectUser = (user: UsuarioBusca) => {
        setSelectedUser(user);
        setUserSearchQuery('');
        setUserSearchResults([]);
    };

    const handleHireSubmit = () => {
        if (!selectedUser) {
            const msg = "Nenhum usuário selecionado.";
            setErrorMsg(msg);
            toast.error(msg); // (NOVO) Toast
            return;
        }
        setErrorMsg('');
        setIsHireModalOpen(true);
    };

    const handleConfirmHire = (commentary: string) => {
        if (!selectedUser) return;

        toast.loading('Contratando novo membro...', { id: 'hire-toast' }); // (NOVO)

        criarStaff({
            variables: {
                input: {
                    usuarioId: selectedUser.id,
                    nome: `${selectedUser.name} ${selectedUser.sobrenome || ''}`.trim(),
                    url: selectedUser.foto || '',
                    job: selectedJob,
                },
                commentary: commentary
            }
        }).finally(() => {
            toast.dismiss('hire-toast'); // Limpa o toast de loading
        });
    };

    if (loading) {
        return <Layout><p className="text-center mt-20">Carregando equipe...</p></Layout>;
    }

    if (error && !data) {
        return (
            <Layout>
                <p className="text-center mt-20 text-red-600">
                    Erro ao carregar a página. Verifique sua conexão ou permissões.
                </p>
            </Layout>
        );
    }

    const staffList = data?.obterStaffList?.filter((s): s is StaffMember => !!s) ?? [];

    return (
        <Layout>
            <CommentaryModal
                isOpen={isHireModalOpen}
                title={`Contratar ${selectedUser?.name || ''}`}
                loading={loadingCreate}
                onClose={() => setIsHireModalOpen(false)}
                onSubmit={handleConfirmHire}
            />

            <div className="w-full mx-auto mb-[5vh]">
                <h1 className="text-3xl font-bold mb-10 text-center">Sala Editorial</h1>

                {/* --- 1. Área 'Equipe Editorial' --- */}
                <div className="mb-12 p-6 bg-gray-50 rounded-lg shadow-sm">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">
                        Equipe Editorial
                    </h2>

                    <ul className="space-y-4">
                        {staffList.map(staff => (
                            <StaffCard
                                key={staff!.usuarioId}
                                staff={staff as StaffMember}
                                onUpdate={refetch}
                            />
                        ))}
                    </ul>

                    {/* --- 2. Área 'Contratar Novo Membro' --- */}
                    <div className="mt-8 flex flex-col items-end">
                        <button
                            onClick={() => setShowHireForm(prev => !prev)}
                            className="px-5 py-2 rounded-lg bg-emerald-600 text-white font-medium shadow hover:bg-emerald-700 transition flex items-center gap-2"
                        >
                            <UserPlus size={18} />
                            {showHireForm ? 'Cancelar' : 'Contratar novo membro'}
                        </button>

                        {showHireForm && (
                            <div className="w-full lg:w-2/3 mt-6 p-6 bg-white rounded-lg shadow-inner border border-gray-200">
                                {errorMsg && <p className="text-red-600 text-sm mb-4">{errorMsg}</p>}

                                {/* Campo 1: Nome do Usuário (Busca) */}
                                <div className="mb-4">
                                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                                        Nome do usuário
                                    </label>
                                    {!selectedUser ? (
                                        <div className="relative">
                                            <input
                                                type="text"
                                                value={userSearchQuery}
                                                onChange={(e) => setUserSearchQuery(e.target.value)}
                                                className="w-full p-3 border border-gray-300 rounded-lg"
                                                placeholder="Buscar usuário por nome..."
                                            />
                                            {userSearchResults.length > 0 && (
                                                <ul className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto">
                                                    {userSearchResults.map(u => (
                                                        <li
                                                            key={u.id}
                                                            onClick={() => handleSelectUser(u)}
                                                            className="flex items-center gap-3 p-3 hover:bg-gray-50 cursor-pointer transition"
                                                        >
                                                            <div className="w-10 h-10 relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                                                                {u.foto ? <Image src={u.foto} alt={u.name} fill className="object-cover" /> : <User size={20} className="text-gray-400 m-auto" />}
                                                            </div>
                                                            <span className="font-medium text-gray-800">{u.name} {u.sobrenome}</span>
                                                        </li>
                                                    ))}
                                                </ul>
                                            )}
                                        </div>
                                    ) : (
                                        <div
                                            className="group relative flex items-center justify-between bg-emerald-50 border border-emerald-200 px-3 py-2 rounded-lg"
                                        >
                                            <div className="flex items-center gap-2">
                                                <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                                    {selectedUser.foto && <Image src={selectedUser.foto} alt="Eu" fill className="object-cover" />}
                                                </div>
                                                <span className="text-sm font-medium text-emerald-800">{selectedUser.name} {selectedUser.sobrenome}</span>
                                            </div>
                                            <button
                                                onClick={() => setSelectedUser(null)}
                                                className="text-red-500 hover:text-red-700 p-1 rounded-full hover:bg-red-100"
                                                title="Click para remover"
                                            >
                                                <X size={16} />
                                            </button>
                                        </div>
                                    )}
                                </div>

                                {/* Campo 2: Função (Dropdown) */}
                                <div className="mb-6">
                                    <label htmlFor="job" className="block text-sm font-semibold text-gray-700 mb-2">Função</label>
                                    <select
                                        id="job"
                                        value={selectedJob}
                                        onChange={(e) => setSelectedJob(e.target.value as any)}
                                        className="w-full p-3 border border-gray-300 rounded-lg bg-white focus:ring-2 focus:ring-emerald-500 outline-none"
                                    >
                                        <option value="EditorBolsista">Editor Bolsista</option>
                                        <option value="EditorChefe">Editor Chefe</option>
                                        <option value="Administrador">Administrador</option>
                                    </select>
                                </div>

                                {/* Botão Enviar */}
                                <button
                                    onClick={handleHireSubmit}
                                    disabled={!selectedUser || loadingCreate}
                                    className="w-full px-6 py-3 rounded-lg bg-emerald-600 text-white font-bold shadow-md hover:bg-emerald-700 transition disabled:opacity-70 disabled:cursor-not-allowed"
                                >
                                    {loadingCreate ? 'Enviando...' : 'Enviar'}
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </Layout>
    );
}