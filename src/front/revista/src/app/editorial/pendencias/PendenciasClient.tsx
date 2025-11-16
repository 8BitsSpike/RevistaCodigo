'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, ApolloError } from '@apollo/client';
import { OBTER_PENDENTES, OBTER_STAFF_LIST } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import PendingCard, { PendingItem } from '@/components/PendingCard';
import PendenciasSearch from './PendenciasSearch';
import { StaffMember } from '@/components/StaffCard';
import { ArrowLeft, ArrowRight, ArrowLeftCircle } from 'lucide-react';

// --- Tipos ---

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

    // Estados de paginação
    const [pageRecentes, setPageRecentes] = useState(0);
    const [pageResolvidas, setPageResolvidas] = useState(0);

    // --- Query Lista de Staff ---
    // Usada para autorização e para passar aos cards
    const { data: staffData, loading: loadingStaff, error: errorStaff } = useQuery<StaffQueryData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: 200 }, // Busca até 200 membros de staff
        fetchPolicy: 'cache-and-network',
        onError: (err: ApolloError) => {
            if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) {
                alert("Acesso negado. Você não tem permissão para ver esta página.");
                localStorage.removeItem('isStaff');
                logout();
                router.push('/');
            }
        }
    });

    const staffList = staffData?.obterStaffList?.filter(s => !!s) as StaffMember[] ?? [];
    const isAdmin = staffList.some(s =>
        s.usuarioId === user?.id &&
        (s.job === 'Administrador' || s.job === 'EditorChefe')
    );

    // --- Query Pendências Recentes (AguardandoRevisao) ---
    const {
        data: recentesData,
        loading: loadingRecentes,
        refetch: refetchRecentes
    } = useQuery<PendentesQueryData>(OBTER_PENDENTES, {
        variables: {
            pagina: pageRecentes,
            tamanho: PAGE_SIZE,
            status: 'AguardandoRevisao'
        },
        skip: !user || loadingStaff, // Não busca até sabermos que somos staff
        fetchPolicy: 'network-only'
    });

    const pendenciasRecentes = recentesData?.obterPendentes ?? [];
    const canGoNextRecentes = pendenciasRecentes.length === PAGE_SIZE;
    const canGoPrevRecentes = pageRecentes > 0;

    // --- Query Pendências Resolvidas (Aprovado) ---
    const {
        data: resolvidasData,
        loading: loadingResolvidas,
        refetch: refetchResolvidas
    } = useQuery<PendentesQueryData>(OBTER_PENDENTES, {
        variables: {
            pagina: pageResolvidas,
            tamanho: PAGE_SIZE,
            status: 'Aprovado' // Começamos mostrando Aprovados
        },
        skip: !user || loadingStaff,
        fetchPolicy: 'network-only'
    });

    const pendenciasResolvidas = resolvidasData?.obterPendentes ?? [];
    const canGoNextResolvidas = pendenciasResolvidas.length === PAGE_SIZE;
    const canGoPrevResolvidas = pageResolvidas > 0;

    // Função para recarregar todas as listas
    const handleUpdate = () => {
        refetchRecentes();
        refetchResolvidas();
        // A busca (lazy query) será recarregada se for executada novamente
    };

    if (loadingStaff) {
        return <Layout><p className="text-center mt-20">Verificando permissões...</p></Layout>;
    }

    // Erro de autorização é tratado no onError
    if (errorStaff) {
        return <Layout><p className="text-center mt-20 text-red-600">Erro ao carregar dados da equipe.</p></Layout>;
    }

    return (
        <Layout>
            <div className="w-full mx-auto mb-[5vh]">
                {/* Botão Voltar */}
                <button
                    onClick={() => router.push('/editorial')}
                    className="flex items-center gap-2 text-sm text-emerald-600 hover:text-emerald-800 font-medium mb-6"
                >
                    <ArrowLeftCircle size={18} />
                    Voltar para a Sala Editorial
                </button>

                <h1 className="text-3xl font-bold mb-10 text-center">Controle de Pendências</h1>

                {/* --- Área 'Buscar pendências' --- */}
                <div className="mb-12">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">
                        Buscar pendências
                    </h2>
                    <PendenciasSearch
                        staffList={staffList}
                        onUpdate={handleUpdate}
                        isAdmin={isAdmin}
                    />
                </div>

                {/* --- Área 'Pendências recentes' --- */}
                <div className="mb-12">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">
                        Pendências recentes (Aguardando Revisão)
                    </h2>
                    {loadingRecentes ? (
                        <p className="text-center">Carregando...</p>
                    ) : pendenciasRecentes.length > 0 ? (
                        <>
                            <ul className="space-y-1">
                                {pendenciasRecentes.map(pending => (
                                    <PendingCard
                                        key={pending.id}
                                        pending={pending}
                                        staffList={staffList}
                                        onUpdate={handleUpdate}
                                        isAdmin={isAdmin}
                                    />
                                ))}
                            </ul>
                            {/* Paginação para Recentes */}
                            {(canGoNextRecentes || canGoPrevRecentes) && (
                                <div className="flex justify-center items-center gap-4 mt-8">
                                    <button
                                        onClick={() => setPageRecentes(p => p - 1)}
                                        disabled={!canGoPrevRecentes}
                                        className="p-2 rounded-md border disabled:opacity-50"
                                    >
                                        <ArrowLeft size={20} />
                                    </button>
                                    <span className="text-lg font-medium">{pageRecentes + 1}</span>
                                    <button
                                        onClick={() => setPageRecentes(p => p + 1)}
                                        disabled={!canGoNextRecentes}
                                        className="p-2 rounded-md border disabled:opacity-50"
                                    >
                                        <ArrowRight size={20} />
                                    </button>
                                </div>
                            )}
                        </>
                    ) : (
                        <p className="text-center text-gray-500 italic">Nenhuma pendência aguardando revisão.</p>
                    )}
                </div>

                {/* --- Área 'Pendências resolvidas' --- */}
                <div className="mt-8">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 border-b border-gray-200 pb-2">
                        Pendências resolvidas (Aprovadas)
                    </h2>
                    {loadingResolvidas ? (
                        <p className="text-center">Carregando...</p>
                    ) : pendenciasResolvidas.length > 0 ? (
                        <>
                            <ul className="space-y-1">
                                {pendenciasResolvidas.map(pending => (
                                    <PendingCard
                                        key={pending.id}
                                        pending={pending}
                                        staffList={staffList}
                                        onUpdate={handleUpdate}
                                        isAdmin={isAdmin}
                                    />
                                ))}
                            </ul>
                            {/* Paginação para Resolvidas */}
                            {(canGoNextResolvidas || canGoPrevResolvidas) && (
                                <div className="flex justify-center items-center gap-4 mt-8">
                                    <button
                                        onClick={() => setPageResolvidas(p => p - 1)}
                                        disabled={!canGoPrevResolvidas}
                                        className="p-2 rounded-md border disabled:opacity-50"
                                    >
                                        <ArrowLeft size={20} />
                                    </button>
                                    <span className="text-lg font-medium">{pageResolvidas + 1}</span>
                                    <button
                                        onClick={() => setPageResolvidas(p => p + 1)}
                                        disabled={!canGoNextResolvidas}
                                        className="p-2 rounded-md border disabled:opacity-50"
                                    >
                                        <ArrowRight size={20} />
                                    </button>
                                </div>
                            )}
                        </>
                    ) : (
                        <p className="text-center text-gray-500 italic">Nenhuma pendência resolvida encontrada.</p>
                    )}
                </div>

            </div>
        </Layout>
    );
}