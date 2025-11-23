'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useLazyQuery, ApolloError } from '@apollo/client';
import { OBTER_STAFF_LIST, OBTER_VOLUMES, OBTER_VOLUMES_POR_ANO, OBTER_VOLUMES_POR_STATUS } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import VolumeSearch, { VolumeSearchVariables } from './VolumeSearch';
import VolumeEditorialCard, { VolumeCardData } from '@/components/VolumeEditorialCard';
import { StaffMember } from '@/components/StaffCard';
import { ArrowLeft, ArrowRight, ArrowLeftCircle, PlusCircle } from 'lucide-react';
import { StatusVolume } from '@/types/enums'; 
import toast from 'react-hot-toast';

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
    const { logout } = useAuth();

    const [page, setPage] = useState(0);
    const [currentSearchVars, setCurrentSearchVars] = useState<VolumeSearchVariables | null>(null);
    const [showCreateForm, setShowCreateForm] = useState(false);

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

    const [runSearchRecentes, { data: recentesData, loading: recentesLoading, error: recentesError }] = useLazyQuery<VolumeQueryData>(OBTER_VOLUMES, { fetchPolicy: 'network-only' });
    const [runSearchAno, { data: anoData, loading: anoLoading, error: anoError }] = useLazyQuery<VolumeQueryData>(OBTER_VOLUMES_POR_ANO, { fetchPolicy: 'network-only' });
    const [runSearchStatus, { data: statusData, loading: statusLoading, error: statusError }] = useLazyQuery<VolumeQueryData>(OBTER_VOLUMES_POR_STATUS, { fetchPolicy: 'network-only' });

    const handleSearch = (variables: VolumeSearchVariables, newPage: number) => {
        setPage(newPage);
        setCurrentSearchVars(variables);
        
        const tamanhoNumerico = variables.pageSize ? parseInt(String(variables.pageSize), 10) : 10;

        const queryVars = { 
            pagina: newPage, 
            tamanho: isNaN(tamanhoNumerico) ? 10 : tamanhoNumerico
        };

        if (variables.searchType === 'ano') {

            const termoString = String(variables.searchTerm || '');
            const anoNumerico = parseInt(termoString);

            if (!isNaN(anoNumerico) && anoNumerico > 1900 && anoNumerico < 2100) {
                runSearchAno({ 
                    variables: { 
                        ...queryVars, 
                        ano: anoNumerico 
                    } 
                });
            } else {
                toast.error("Por favor, insira um ano válido (ex: 2024).");
            }

        } else if (variables.searchType === 'status') {

            if (variables.searchStatus) {
                runSearchStatus({ 
                    variables: { 
                        ...queryVars, 
                        status: variables.searchStatus 
                    } 
                });
            } else {
                toast.error("Selecione um status.");
            }

        } else {

            runSearchRecentes({ variables: queryVars });
        }
    };

    const handlePageChange = (newPage: number) => {
        if (currentSearchVars && newPage >= 0) handleSearch(currentSearchVars, newPage);
    };

    const handleUpdate = () => {
        setShowCreateForm(false);
        if (currentSearchVars) handleSearch(currentSearchVars, page);
    };

    const volumes = recentesData?.obterVolumes || anoData?.obterVolumesPorAno || statusData?.obterVolumesPorStatus || [];
    const loading = loadingStaff || recentesLoading || anoLoading || statusLoading;
    const error = errorStaff || recentesError || anoError || statusError;
    const canGoPrevious = page > 0;
    const canGoNext = volumes.length === (currentSearchVars?.pageSize || 10);

    if (loadingStaff) return <Layout pageType="editorial"><div className="text-center mt-20">Carregando...</div></Layout>;

    return (
        <Layout pageType="editorial">
            <div className="w-full mx-auto mb-[5vh]">
                <div className="flex items-center mb-6">
                    <button onClick={() => router.push('/editorial')} className="flex items-center gap-2 text-sm text-emerald-600 hover:text-emerald-800 font-medium">
                        <ArrowLeftCircle size={18} /> Voltar
                    </button>
                    <h1 className="text-3xl font-bold text-center flex-1">Sala de Imprensa</h1>
                </div>

                <VolumeSearch onSearch={(vars) => handleSearch(vars, 0)} loading={loading} />

                <div className="mt-8">
                    {loading && !loadingStaff && <p className="text-center">Buscando...</p>}
                    {error && <p className="text-center text-red-600">Erro: {error.message}</p>}
                    {!loading && !currentSearchVars && <p className="text-center text-gray-400 italic mt-10">Filtre para ver edições</p>}
                    {!loading && currentSearchVars && volumes.length === 0 && <p className="text-center text-gray-500 italic mt-10">Nenhuma edição encontrada.</p>}

                    {volumes.length > 0 && (
                        <>
                            <ul className="space-y-1">
                                {volumes.map(volume => (
                                    <VolumeEditorialCard key={volume.id} mode="view" initialData={volume} onUpdate={handleUpdate} />
                                ))}
                            </ul>
                            <div className="flex justify-center items-center gap-4 mt-8">
                                <button onClick={() => handlePageChange(page - 1)} disabled={!canGoPrevious || loading} className="btn-secondary"><ArrowLeft size={20} /></button>
                                <span className="text-lg font-medium">{page + 1}</span>
                                <button onClick={() => handlePageChange(page + 1)} disabled={!canGoNext || loading} className="btn-secondary"><ArrowRight size={20} /></button>
                            </div>
                        </>
                    )}
                </div>

                <div className="flex justify-end mt-12">
                    {!showCreateForm ? (
                        <button onClick={() => setShowCreateForm(true)} className="btn-primary">
                            <PlusCircle size={20} /> Criar nova edição
                        </button>
                    ) : (
                        <div className="w-full">
                            <h2 className="text-2xl font-semibold mb-4 text-center">Criar Nova Edição</h2>
                            <VolumeEditorialCard mode="create" onUpdate={() => setShowCreateForm(false)} />
                        </div>
                    )}
                </div>
            </div>
        </Layout>
    );
}