'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useLazyQuery, ApolloError } from '@apollo/client';
import {
    OBTER_STAFF_LIST,
    OBTER_VOLUMES,
    OBTER_VOLUMES_POR_ANO,
    OBTER_VOLUMES_POR_STATUS
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import VolumeSearch, { VolumeSearchVariables } from './VolumeSearch';
import VolumeEditorialCard, { VolumeCardData } from '@/components/VolumeEditorialCard';
import { StaffMember } from '@/components/StaffCard';
import { ArrowLeft, ArrowRight, ArrowLeftCircle, PlusCircle } from 'lucide-react';
import { StatusVolume } from '@/types/enums';

// --- Tipos ---

interface VolumeQueryData {
    obterVolumes?: VolumeCardData[];
    obterVolumesPorAno?: VolumeCardData[];
    obterVolumesPorStatus?: VolumeCardData[];
}

interface StaffQueryData {
    obterStaffList: StaffMember[];
}

export default function ImprensaClient() {
    const router = useRouter();
    const { user, logout } = useAuth();

    // --- Estados ---
    const [page, setPage] = useState(0);
    const [currentSearchVars, setCurrentSearchVars] = useState<VolumeSearchVariables | null>(null);
    const [showCreateForm, setShowCreateForm] = useState(false); // Controla o formulário de criação

    // --- Query 1: Verificação de Staff ---
    const { data: staffData, loading: loadingStaff, error: errorStaff } = useQuery<StaffQueryData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: 200 },
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

    // --- Queries de Busca (Lazy) ---

    const [runSearchRecentes, { data: recentesData, loading: recentesLoading, error: recentesError }] = useLazyQuery<VolumeQueryData>(
        OBTER_VOLUMES, { fetchPolicy: 'network-only' }
    );

    const [runSearchAno, { data: anoData, loading: anoLoading, error: anoError }] = useLazyQuery<VolumeQueryData>(
        OBTER_VOLUMES_POR_ANO, { fetchPolicy: 'network-only' }
    );

    const [runSearchStatus, { data: statusData, loading: statusLoading, error: statusError }] = useLazyQuery<VolumeQueryData>(
        OBTER_VOLUMES_POR_STATUS, { fetchPolicy: 'network-only' }
    );

    // --- Handlers ---

    const handleSearch = (variables: VolumeSearchVariables, newPage: number) => {
        setPage(newPage);
        setCurrentSearchVars(variables);

        const queryVars = {
            pagina: newPage,
            tamanho: variables.pageSize,
        };

        if (variables.searchType === 'ano') {
            runSearchAno({ variables: { ...queryVars, ano: variables.searchTerm } });
        } else if (variables.searchType === 'status') {
            runSearchStatus({ variables: { ...queryVars, status: variables.searchStatus } });
        } else { // 'recentes'
            runSearchRecentes({ variables: queryVars });
        }
    };

    const handlePageChange = (newPage: number) => {
        if (currentSearchVars && newPage >= 0) {
            handleSearch(currentSearchVars, newPage);
        }
    };

    // Função de callback para os cards (recarrega a busca atual)
    const handleUpdate = () => {
        setShowCreateForm(false); // Fecha o formulário de criação
        if (currentSearchVars) {
            handleSearch(currentSearchVars, page); // Recarrega a busca atual
        }
    };

    // Consolida resultados, loading e erros
    const volumes =
        recentesData?.obterVolumes ||
        anoData?.obterVolumesPorAno ||
        statusData?.obterVolumesPorStatus ||
        [];

    const loading = loadingStaff || recentesLoading || anoLoading || statusLoading;
    const error = errorStaff || recentesError || anoError || statusError;

    const canGoPrevious = page > 0;
    const canGoNext = volumes.length === (currentSearchVars?.pageSize || 10);

    if (loadingStaff) {
        return <Layout><p className="text-center mt-20">Verificando permissões...</p></Layout>;
    }

    return (
        <Layout>
            <div className="w-full mx-auto mb-[5vh]">

                {/* Header da Página */}
                <div className="flex items-center mb-6">
                    <button
                        onClick={() => router.push('/editorial')}
                        className="flex items-center gap-2 text-sm text-emerald-600 hover:text-emerald-800 font-medium"
                    >
                        <ArrowLeftCircle size={18} />
                        Voltar para a Sala Editorial
                    </button>
                    <h1 className="text-3xl font-bold text-center flex-1">Sala de Imprensa</h1>
                </div>

                {/* Barra de Busca */}
                <VolumeSearch
                    onSearch={(vars) => handleSearch(vars, 0)}
                    loading={loading}
                />

                {/* --- Área de Resultados da Busca --- */}
                <div className="mt-8">
                    {loading && !loadingStaff && <p className="text-center">Buscando edições...</p>}
                    {error && <p className="text-center text-red-600">Erro: {error.message}</p>}

                    {!loading && !currentSearchVars && (
                        <p className="text-center text-gray-400 italic mt-10">
                            Filtre e busque para receber as edições
                        </p>
                    )}

                    {!loading && currentSearchVars && volumes.length === 0 && (
                        <p className="text-center text-gray-500 italic mt-10">
                            Nenhuma edição encontrada para esta busca.
                        </p>
                    )}

                    {volumes.length > 0 && (
                        <>
                            <ul className="space-y-1">
                                {volumes.map(volume => (
                                    <VolumeEditorialCard
                                        key={volume.id}
                                        mode="view"
                                        initialData={volume}
                                        onUpdate={handleUpdate}
                                    />
                                ))}
                            </ul>

                            {/* Paginação */}
                            <div className="flex justify-center items-center gap-4 mt-8">
                                <button
                                    onClick={() => handlePageChange(page - 1)}
                                    disabled={!canGoPrevious || loading}
                                    className="p-2 rounded-md border disabled:opacity-50"
                                >
                                    <ArrowLeft size={20} />
                                </button>
                                <span className="text-lg font-medium">{page + 1}</span>
                                <button
                                    onClick={() => handlePageChange(page + 1)}
                                    disabled={!canGoNext || loading}
                                    className="p-2 rounded-md border disabled:opacity-50"
                                >
                                    <ArrowRight size={20} />
                                </button>
                            </div>
                        </>
                    )}
                </div>

                {/* --- Área de Criação --- */}
                <div className="flex justify-end mt-12">
                    {!showCreateForm ? (
                        <button
                            onClick={() => setShowCreateForm(true)}
                            className="px-6 py-3 rounded-lg bg-emerald-600 text-white font-bold shadow-md hover:bg-emerald-700 transition flex items-center gap-2"
                        >
                            <PlusCircle size={20} />
                            Criar nova edição
                        </button>
                    ) : (
                        <div className="w-full">
                            <h2 className="text-2xl font-semibold mb-4 text-center">Criar Nova Edição</h2>
                            <VolumeEditorialCard
                                mode="create"
                                onUpdate={() => setShowCreateForm(false)} // Fecha o formulário
                            />
                        </div>
                    )}
                </div>

            </div>
        </Layout>
    );
}