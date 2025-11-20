'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, ApolloError } from '@apollo/client';
import { OBTER_PENDENTES, OBTER_STAFF_LIST } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import PendingCard, { PendingItem } from '@/components/PendingCard';
import PendenciasSearch from './PendenciasSearch';
import { StaffMember } from '@/components/StaffCard';
import { ArrowLeft, ArrowRight, ArrowLeftCircle } from 'lucide-react';
import toast from 'react-hot-toast';

interface PendentesQueryData {
    obterPendentes: PendingItem[];
}
interface StaffQueryData {
    obterStaffList: StaffMember[];
}
const PAGE_SIZE = 20;

export default function PendenciasClient() {
    const router = useRouter();
    const { user, logout } = useAuth();

    const [pageRecentes, setPageRecentes] = useState(0);
    const [pageResolvidas, setPageResolvidas] = useState(0);

    const { data: staffData, loading: loadingStaff, error: errorStaff } = useQuery<StaffQueryData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: 200 },
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

    const staffList = staffData?.obterStaffList?.filter(s => !!s) as StaffMember[] ?? [];
    const isAdmin = staffList.some(s => s.usuarioId === user?.id && (s.job === 'Administrador' || s.job === 'EditorChefe'));

    const { data: recentesData, loading: loadingRecentes, refetch: refetchRecentes } = useQuery<PendentesQueryData>(OBTER_PENDENTES, {
        variables: { pagina: pageRecentes, tamanho: PAGE_SIZE, status: 'AguardandoRevisao' },
        skip: !user || loadingStaff,
        fetchPolicy: 'network-only'
    });

    const { data: resolvidasData, loading: loadingResolvidas, refetch: refetchResolvidas } = useQuery<PendentesQueryData>(OBTER_PENDENTES, {
        variables: { pagina: pageResolvidas, tamanho: PAGE_SIZE, status: 'Aprovado' },
        skip: !user || loadingStaff,
        fetchPolicy: 'network-only'
    });

    const pendenciasRecentes = recentesData?.obterPendentes ?? [];
    const canGoNextRecentes = pendenciasRecentes.length === PAGE_SIZE;
    const canGoPrevRecentes = pageRecentes > 0;

    const pendenciasResolvidas = resolvidasData?.obterPendentes ?? [];
    const canGoNextResolvidas = pendenciasResolvidas.length === PAGE_SIZE;
    const canGoPrevResolvidas = pageResolvidas > 0;

    const handleUpdate = () => {
        refetchRecentes();
        refetchResolvidas();
    };

    if (loadingStaff) {
        return <Layout pageType="editorial"><div className="text-center mt-20">Verificando permissões...</div></Layout>;
    }

    if (errorStaff) {
        return <Layout pageType="editorial"><div className="text-center mt-20 text-red-600">Erro ao carregar dados da equipe.</div></Layout>;
    }

    return (
        <Layout pageType="editorial">
            <div className="w-full mx-auto mb-[5vh]">
                <button onClick={() => router.push('/editorial')} className="flex items-center gap-2 text-sm text-emerald-600 hover:text-emerald-800 font-medium mb-6">
                    <ArrowLeftCircle size={18} /> Voltar
                </button>

                <h1 className="text-3xl font-bold mb-10 text-center">Controle de Pendências</h1>

                <div className="mb-12">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">Buscar pendências</h2>
                    <PendenciasSearch staffList={staffList} onUpdate={handleUpdate} isAdmin={isAdmin} />
                </div>

                <div className="mb-12">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">Pendências recentes (Aguardando Revisão)</h2>
                    {loadingRecentes ? <p className="text-center">Carregando...</p> : pendenciasRecentes.length > 0 ? (
                        <>
                            <ul className="space-y-1">
                                {pendenciasRecentes.map(pending => <PendingCard key={pending.id} pending={pending} staffList={staffList} onUpdate={handleUpdate} isAdmin={isAdmin} />)}
                            </ul>
                            <div className="flex justify-center items-center gap-4 mt-8">
                                <button onClick={() => setPageRecentes(p => p - 1)} disabled={!canGoPrevRecentes} className="btn-secondary"><ArrowLeft size={20} /></button>
                                <span className="text-lg font-medium">{pageRecentes + 1}</span>
                                <button onClick={() => setPageRecentes(p => p + 1)} disabled={!canGoNextRecentes} className="btn-secondary"><ArrowRight size={20} /></button>
                            </div>
                        </>
                    ) : <p className="text-center text-gray-500 italic">Nenhuma pendência.</p>}
                </div>

                <div className="mt-8">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">Pendências resolvidas (Aprovadas)</h2>
                    {loadingResolvidas ? <p className="text-center">Carregando...</p> : pendenciasResolvidas.length > 0 ? (
                        <>
                            <ul className="space-y-1">
                                {pendenciasResolvidas.map(pending => <PendingCard key={pending.id} pending={pending} staffList={staffList} onUpdate={handleUpdate} isAdmin={isAdmin} />)}
                            </ul>
                            <div className="flex justify-center items-center gap-4 mt-8">
                                <button onClick={() => setPageResolvidas(p => p - 1)} disabled={!canGoPrevResolvidas} className="btn-secondary"><ArrowLeft size={20} /></button>
                                <span className="text-lg font-medium">{pageResolvidas + 1}</span>
                                <button onClick={() => setPageResolvidas(p => p + 1)} disabled={!canGoNextResolvidas} className="btn-secondary"><ArrowRight size={20} /></button>
                            </div>
                        </>
                    ) : <p className="text-center text-gray-500 italic">Nenhuma pendência resolvida.</p>}
                </div>
            </div>
        </Layout>
    );
}