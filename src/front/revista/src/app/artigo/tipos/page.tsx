'use client';

import { useState, Suspense } from 'react';
import { useQuery } from '@apollo/client/react';
import { useSearchParams } from 'next/navigation';
import { GET_ARTIGOS_POR_TIPO } from '@/graphql/queries';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { ArrowLeft, ArrowRight } from 'lucide-react';

const PAGE_SIZE = 15; // Quantidade de artigos por página

interface ArtigoCardData {
    id: string;
    titulo: string;
    resumo?: string;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
}

interface ArtigosPorTipoQueryData {
    obterArtigosCardListPorTipo: ArtigoCardData[];
}

function ArtigosPageComponent() {
    const [page, setPage] = useState(0);
    const searchParams = useSearchParams();
    const tipoParam = searchParams.get('tipo') || 'Artigo';

    const { data, loading, error } = useQuery<ArtigosPorTipoQueryData>(
        GET_ARTIGOS_POR_TIPO,
        {
            variables: {
                tipo: tipoParam,
                page,
                pageSize: PAGE_SIZE,
            },
            fetchPolicy: 'cache-and-network',
        }
    );

    const articles = data?.obterArtigosCardListPorTipo || [];
    const canGoPrevious = page > 0;
    const canGoNext = articles.length === PAGE_SIZE;

    const renderContent = () => {
        if (loading) {
            return <p className="text-center">Buscando artigos...</p>;
        }

        if (error) {
            return <p className="text-center text-red-600">Erro: {error.message}</p>;
        }

        if (articles.length === 0) {
            return <p className="text-center text-gray-600">Nenhum artigo encontrado em {tipoParam.toLowerCase()}.</p>;
        }

        return (
            <>
                {/* Lista de Artigos */}
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

                {/* Controles de Paginação Corrigidos */}
                {(canGoNext || canGoPrevious) && (
                    <div className="flex justify-center items-center gap-4 mt-8">
                        <button
                            onClick={() => setPage(p => p - 1)}
                            disabled={!canGoPrevious}
                            className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed"
                            aria-label="Página anterior"
                        >
                            <ArrowLeft size={20} />
                        </button>

                        {/* Exibe o número da página (começando em 1) */}
                        <span className="text-lg font-medium">{page + 1}</span>

                        <button
                            onClick={() => setPage(p => p + 1)}
                            disabled={!canGoNext}
                            className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed"
                            aria-label="Próxima página"
                        >
                            <ArrowRight size={20} />
                        </button>
                    </div>
                )}
            </>
        );
    };

    return (
        <Layout>
            <div className="w-[90%] mx-auto mb-[5vh]">
                {renderContent()}
            </div>
        </Layout>
    );
}


export default function ArtigosPageWrapper() {
    return (
        <Suspense fallback={<Layout><p className="text-center mt-20 text-gray-600">Carregando...</p></Layout>}>
            <ArtigosPageComponent />
        </Suspense>
    );
}