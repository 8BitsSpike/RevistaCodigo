'use client';

import { useState, Suspense } from 'react';
import { useQuery } from '@apollo/client/react';
import { useSearchParams } from 'next/navigation';
import { SEARCH_ARTICLES } from '@/graphql/queries';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { ArrowLeft, ArrowRight } from 'lucide-react';

// Tipos para os dados da query
interface ArtigoSearchResult {
    id: string;
    titulo: string;
    resumo?: string;
    status: string;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
}

interface SearchQueryData {
    titleResults?: ArtigoSearchResult[];
    authorResults?: ArtigoSearchResult[];
}

const PAGE_SIZE = 15;

// O conteúdo da página foi movido para este componente
function SearchClientContent() {
    const [page, setPage] = useState(0);
    const searchParams = useSearchParams();

    // Lê os parâmetros da URL
    const searchTerm = searchParams.get('q') || '';
    const searchType = searchParams.get('tipo') || 'titulo';

    const { data, loading, error } = useQuery<SearchQueryData>(SEARCH_ARTICLES, {
        variables: {
            searchTerm: searchTerm,
            page,
            pageSize: PAGE_SIZE,
            searchByTitle: searchType === 'titulo',
            searchByAuthor: searchType === 'autor',
        },
        skip: !searchTerm,
    });

    const articles = data?.titleResults || data?.authorResults || [];

    const canGoPrevious = page > 0;
    const canGoNext = articles.length === PAGE_SIZE;

    const handlePageChange = (newPage: number) => {

        setPage(newPage);
    };

    return (
        <Layout>
            <div className="w-[90%] mx-auto mb-[5vh]">

                {/* Título dinâmico para os resultados */}
                {searchTerm && (
                    <h1 className="text-3xl font-bold mb-8 text-center">
                        Resultados da busca por: "{searchTerm}"
                    </h1>
                )}
                {!searchTerm && (
                    <h1 className="text-3xl font-bold mb-8 text-center">
                        Página de Busca
                    </h1>
                )}

                {/* Área de Resultados */}
                <div>
                    {loading && <p className="text-center">Buscando...</p>}
                    {error && <p className="text-center text-red-600">Erro: {error.message}</p>}

                    {!loading && searchTerm && articles.length === 0 && (
                        <p className="text-center text-gray-600">Nenhum resultado encontrado para "{searchTerm}".</p>
                    )}

                    {!searchTerm && !loading && (
                        <p className="text-center text-gray-600">Use a barra de busca no topo da página para encontrar artigos.</p>
                    )}

                    {articles.length > 0 && (
                        <>
                            <ul className="w-full flex flex-col items-center">
                                {articles.map((art) => (
                                    <ArticleCard
                                        key={art.id}
                                        id={art.id}
                                        title={art.titulo}
                                        excerpt={art.resumo}
                                        href={`/artigo/${art.id}`}
                                        imagem={art.midiaDestaque ? {
                                            url: art.midiaDestaque.url,
                                            textoAlternativo: art.midiaDestaque.textoAlternativo
                                        } : null}
                                    />
                                ))}
                            </ul>

                            {/* Controles de Paginação */}
                            <div className="flex justify-center items-center gap-4 mt-8">
                                <button
                                    onClick={() => handlePageChange(page - 1)}
                                    disabled={!canGoPrevious}
                                    className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed"
                                    aria-label="Página anterior"
                                >
                                    <ArrowLeft size={20} />
                                </button>

                                {/* O número da página só é exibido se houver mais de uma página */}
                                {(canGoNext || canGoPrevious) && (
                                    <span className="text-lg font-medium">{page + 1}</span>
                                )}

                                <button
                                    onClick={() => handlePageChange(page + 1)}
                                    disabled={!canGoNext}
                                    className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed"
                                    aria-label="Próxima página"
                                >
                                    <ArrowRight size={20} />
                                </button>
                            </div>
                        </>
                    )}
                </div>
            </div>
        </Layout>
    );
}

export default function SearchClientWrapper() {
    return (
        <Suspense fallback={<Layout><p className="text-center mt-20 text-gray-600">Carregando...</p></Layout>}>
            <SearchClientContent />
        </Suspense>
    );
}