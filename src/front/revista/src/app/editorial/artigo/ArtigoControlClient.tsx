'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useLazyQuery, ApolloError } from '@apollo/client';
import {
    OBTER_STAFF_LIST,
    OBTER_ARTIGOS_POR_STATUS,
    OBTER_ARTIGOS_EDITORIAL_POR_TIPO,
    SEARCH_ARTIGOS_EDITORIAL_BY_TITLE,
    SEARCH_ARTIGOS_EDITORIAL_BY_AUTOR_IDS
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ArtigoSearch, { SearchVariables } from './ArtigoSearch';
import ArticleEditorialCard, { ArtigoEditorial } from '@/components/ArticleEditorialCard';
import { StaffMember } from '@/components/StaffCard';
import { ArrowLeft, ArrowRight, ArrowLeftCircle } from 'lucide-react';
import { StatusArtigo, TipoArtigo } from '@/types/enums';
import toast from 'react-hot-toast';

interface ArtigoCardListQueryData {
    obterArtigosPorStatus?: ArtigoEditorial[];
    obterArtigosEditorialPorTipo?: ArtigoEditorial[];
    searchArtigosEditorialByTitle?: ArtigoEditorial[];
    searchArtigosEditorialByAutorIds?: ArtigoEditorial[];
}

interface StaffQueryData {
    obterStaffList: StaffMember[];
}

export default function ArtigoControlClient() {
    const router = useRouter();
    const { user, logout } = useAuth();

    const [page, setPage] = useState(0);
    const [currentSearchVars, setCurrentSearchVars] = useState<SearchVariables | null>(null);

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

    const [runStatusSearch, { data: statusData, loading: statusLoading, error: statusError }] = useLazyQuery<ArtigoCardListQueryData>(
        OBTER_ARTIGOS_POR_STATUS, { fetchPolicy: 'network-only' }
    );
    const [runTipoSearch, { data: tipoData, loading: tipoLoading, error: tipoError }] = useLazyQuery<ArtigoCardListQueryData>(
        OBTER_ARTIGOS_EDITORIAL_POR_TIPO, { fetchPolicy: 'network-only' }
    );
    const [runTitleSearch, { data: titleData, loading: titleLoading, error: titleError }] = useLazyQuery<ArtigoCardListQueryData>(
        SEARCH_ARTIGOS_EDITORIAL_BY_TITLE, { fetchPolicy: 'network-only' }
    );
    const [runAuthorSearch, { data: authorData, loading: authorLoading, error: authorError }] = useLazyQuery<ArtigoCardListQueryData>(
        SEARCH_ARTIGOS_EDITORIAL_BY_AUTOR_IDS, { fetchPolicy: 'network-only' }
    );

    const handleSearch = (variables: SearchVariables, newPage: number) => {
        setPage(newPage);
        setCurrentSearchVars(variables);
        const queryVars = { pagina: newPage, tamanho: variables.pageSize };

        if (variables.searchType === 'titulo') {
            runTitleSearch({ variables: { ...queryVars, searchTerm: variables.searchTerm || '' } });
        } else if (variables.searchType === 'autor') {
            runAuthorSearch({ variables: { ...queryVars, idsAutor: [variables.searchTerm || ''] } });
        } else if (variables.searchType === 'status') {
            runStatusSearch({ variables: { ...queryVars, status: variables.searchStatus || StatusArtigo.EmRevisao } });
        } else if (variables.searchType === 'tipo') {
            runTipoSearch({ variables: { ...queryVars, tipo: variables.searchTipo || TipoArtigo.Artigo } });
        } else {
            runStatusSearch({ variables: { ...queryVars, status: StatusArtigo.EmRevisao } });
        }
    };

    const handlePageChange = (newPage: number) => {
        if (currentSearchVars && newPage >= 0) {
            handleSearch(currentSearchVars, newPage);
        }
    };

    const articles: ArtigoEditorial[] =
        statusData?.obterArtigosPorStatus ||
        tipoData?.obterArtigosEditorialPorTipo ||
        titleData?.searchArtigosEditorialByTitle ||
        authorData?.searchArtigosEditorialByAutorIds ||
        [];

    const loading = loadingStaff || statusLoading || tipoLoading || titleLoading || authorLoading;
    const error = errorStaff || statusError || tipoError || titleError || authorError;
    const canGoPrevious = page > 0;
    const canGoNext = articles.length === (currentSearchVars?.pageSize || 15);

    if (loadingStaff) return <Layout pageType="editorial"><p className="text-center mt-20">Carregando...</p></Layout>;

    return (
        <Layout pageType="editorial">
            <div className="w-full mx-auto mb-[5vh]">
                <div className="flex items-center mb-6">
                    <button onClick={() => router.push('/editorial')} className="flex items-center gap-2 text-sm text-emerald-600 hover:text-emerald-800 font-medium">
                        <ArrowLeftCircle size={18} /> Voltar
                    </button>
                    <h1 className="text-3xl font-bold text-center flex-1">Controle de Artigos</h1>
                </div>

                <ArtigoSearch
                    staffList={staffList}
                    onSearch={(vars) => handleSearch(vars, 0)}
                    loading={loading}
                />

                <div className="mt-8">
                    {loading && !loadingStaff && <p className="text-center">Buscando...</p>}
                    {error && <p className="text-center text-red-600">Erro: {error.message}</p>}

                    {!loading && !currentSearchVars && <p className="text-center text-gray-400 italic mt-10">Filtre e busque para ver os artigos</p>}
                    {!loading && currentSearchVars && articles.length === 0 && <p className="text-center text-gray-500 italic mt-10">Nenhum artigo encontrado.</p>}

                    {articles.length > 0 && (
                        <>
                            <ul className="space-y-1">
                                {articles.map(artigo => (
                                    <ArticleEditorialCard
                                        key={artigo.id}
                                        artigo={artigo}
                                        onUpdate={() => { if (currentSearchVars) handleSearch(currentSearchVars, page); }}
                                    />
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
            </div>
        </Layout>
    );
}