'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, ApolloError } from '@apollo/client';
import { OBTER_STAFF_LIST, CRIAR_NOVO_STAFF } from '@/graphql/queries';
import { USER_API_BASE } from '@/lib/fetcher';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import StaffCard, { StaffMember } from '@/components/StaffCard';
import { User, UserPlus, X } from 'lucide-react';
import Image from 'next/image';
import CommentaryModal from '@/components/CommentaryModal';
import toast from 'react-hot-toast';
import { StatusArtigo, PosicaoEditorial, TipoArtigo, FuncaoTrabalho } from '@/types/enums';

interface StaffListData { obterStaffList: (StaffMember | null | undefined)[]; }
interface UsuarioBusca { id: string; name: string; sobrenome?: string; foto?: string; }
interface CriarStaffData { criarNovoStaff: StaffMember; }
const PAGE_SIZE = 50;

export default function EditorialClient() {
    const router = useRouter();
    const { logout } = useAuth();
    const [showHireForm, setShowHireForm] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [isHireModalOpen, setIsHireModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UsuarioBusca | null>(null);
    const [userSearchQuery, setUserSearchQuery] = useState('');
    const [userSearchResults, setUserSearchResults] = useState<UsuarioBusca[]>([]);
    const [selectedJob, setSelectedJob] = useState<FuncaoTrabalho>(FuncaoTrabalho.EditorBolsista);

    const { data, loading, error, refetch } = useQuery<StaffListData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: PAGE_SIZE },
        fetchPolicy: 'cache-and-network',
        onError: (err: ApolloError) => {
            if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) {
                toast.error("Acesso negado.");
                localStorage.removeItem('isStaff');
                logout();
                router.push('/');
            }
        }
    });

    const [criarStaff, { loading: loadingCreate }] = useMutation<CriarStaffData>(CRIAR_NOVO_STAFF, {
        onCompleted: () => {
            toast.success('Contratado!');
            setShowHireForm(false);
            setSelectedUser(null);
            setUserSearchQuery('');
            setIsHireModalOpen(false);
            refetch();
        },
        onError: (err) => {
            setErrorMsg(err.message);
            toast.error(err.message);
            setIsHireModalOpen(false);
        }
    });

    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (userSearchQuery.length < 3) {
                setUserSearchResults([]);
                return;
            }
            const token = localStorage.getItem('userToken');
            if (!token) return;
            try {
                const res = await fetch(`${USER_API_BASE}/UserSearch?nome=${userSearchQuery}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const dataApi = await res.json();
                    const staffIds = data?.obterStaffList ? data.obterStaffList.filter(s => !!s).map(s => s!.usuarioId) : [];
                    const filtered = dataApi.filter((u: any) => !staffIds.includes(u.id));
                    setUserSearchResults(filtered);
                }
            } catch (err) { console.error(err); }
        }, 500);
        return () => clearTimeout(delayDebounceFn);
    }, [userSearchQuery, data?.obterStaffList]);

    const handleSelectUser = (user: UsuarioBusca) => {
        setSelectedUser(user);
        setUserSearchQuery('');
        setUserSearchResults([]);
    };
    const handleHireSubmit = () => {
        if (!selectedUser) return setErrorMsg("Selecione um usuário.");
        setErrorMsg('');
        setIsHireModalOpen(true);
    };
    const handleConfirmHire = (commentary: string) => {
        if (!selectedUser) return;
        toast.loading('Contratando...', { id: 'hire-toast' });
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
        }).finally(() => toast.dismiss('hire-toast'));
    };

    if (loading) {
        return (
            <Layout pageType="editorial">
                {/* FIX: Changed p to div to allow Layout rendering */}
                <div className="text-center mt-20">Carregando equipe...</div>
            </Layout>
        );
    }

    if (error && !data) {
        return (
            <Layout pageType="editorial">
                {/* FIX: Changed p to div */}
                <div className="text-center mt-20 text-red-600">
                    Erro ao carregar a página. Verifique sua conexão ou permissões.
                </div>
            </Layout>
        );
    }
    const staffList = data?.obterStaffList?.filter((s): s is StaffMember => !!s) ?? [];

    return (
        <Layout pageType="editorial">
            <CommentaryModal isOpen={isHireModalOpen} title={`Contratar ${selectedUser?.name || ''}`} loading={loadingCreate} onClose={() => setIsHireModalOpen(false)} onSubmit={handleConfirmHire} />
            <div className="w-full mx-auto mb-[5vh]">
                <h1 className="text-3xl font-bold mb-10 text-center">Sala Editorial</h1>
                <div className="mb-12 p-6 bg-gray-50 rounded-lg shadow-sm">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">Equipe Editorial</h2>
                    <ul className="space-y-4">
                        {staffList.map(staff => <StaffCard key={staff!.usuarioId} staff={staff as StaffMember} onUpdate={refetch} />)}
                    </ul>
                    <div className="mt-8 flex flex-col items-end">
                        <button onClick={() => setShowHireForm(prev => !prev)} className="btn-primary"><UserPlus size={18} /> {showHireForm ? 'Cancelar' : 'Contratar novo membro'}</button>
                        {showHireForm && (
                            <div className="w-full lg:w-2/3 mt-6 p-6 bg-white rounded-lg shadow-inner border border-gray-200">
                                {errorMsg && <p className="text-red-600 text-sm mb-4">{errorMsg}</p>}
                                <div className="mb-4">
                                    <label className="block text-sm font-semibold text-gray-700 mb-2">Nome do usuário</label>
                                    {!selectedUser ? (
                                        <div className="relative">
                                            <input type="text" value={userSearchQuery} onChange={(e) => setUserSearchQuery(e.target.value)} className="input-std" placeholder="Buscar..." />
                                            {userSearchResults.length > 0 && (
                                                <ul className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto">
                                                    {userSearchResults.map(u => (
                                                        <li key={u.id} onClick={() => handleSelectUser(u)} className="flex items-center gap-3 p-3 hover:bg-gray-50 cursor-pointer transition">
                                                            <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                                                {/* FIX: Image fallback */}
                                                                <Image src={u.foto || '/faviccon.png'} alt={u.name} fill className="object-cover" />
                                                            </div>
                                                            <span className="font-medium text-gray-800">{u.name} {u.sobrenome}</span>
                                                        </li>
                                                    ))}
                                                </ul>
                                            )}
                                        </div>
                                    ) : (
                                        <div className="flex items-center justify-between bg-emerald-50 border border-emerald-200 px-3 py-2 rounded-lg">
                                            <div className="flex items-center gap-2">
                                                <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                                    {/* FIX: Image fallback */}
                                                    <Image src={selectedUser.foto || '/faviccon.png'} alt={selectedUser.name} fill className="object-cover" />
                                                </div>
                                                <span className="text-sm font-medium text-emerald-800">{selectedUser.name} {selectedUser.sobrenome}</span>
                                            </div>
                                            <button onClick={() => setSelectedUser(null)} className="text-red-500"><X size={16} /></button>
                                        </div>
                                    )}
                                </div>
                                <div className="mb-6">
                                    <label className="block text-sm font-semibold text-gray-700 mb-2">Função</label>
                                    <select 
                                        value={selectedJob} 
                                        onChange={(e) => setSelectedJob(e.target.value as FuncaoTrabalho)} 
                                        className="input-std"
                                    >
                                        {/* Use o Enum para garantir que o valor (value) seja MAIÚSCULO */}
                                        <option value={FuncaoTrabalho.EditorBolsista}>Editor Bolsista</option>
                                        <option value={FuncaoTrabalho.EditorChefe}>Editor Chefe</option>
                                        <option value={FuncaoTrabalho.Administrador}>Administrador</option>
                                    </select>
                                </div>
                                <button onClick={handleHireSubmit} disabled={!selectedUser || loadingCreate} className="btn-primary w-full">{loadingCreate ? 'Enviando...' : 'Enviar'}</button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </Layout>
    );
}