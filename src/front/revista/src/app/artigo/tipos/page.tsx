'use client';

import { useState, Suspense } from 'react';
import { useQuery } from '@apollo/client/react';
import { useSearchParams } from 'next/navigation';
import { GET_ARTIGOS_POR_TIPO } from '@/graphql/queries';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { TipoArtigo } from '@/types/enums';

const PAGE_SIZE = 15;

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

// --- CORREÇÃO AQUI ---
// Função atualizada para lidar com Plurais e Mapeamento correto
const getTipoEnum = (param: string | null): TipoArtigo => {
    if (!param) return TipoArtigo.Artigo;

    // Normaliza para maiúsculo e remove espaços
    const upperParam = param.trim().toUpperCase();

    switch (upperParam) {
        case 'ENTREVISTA':
        case 'ENTREVISTAS': // Lida com o plural vindo da URL
            return TipoArtigo.Entrevista;

        case 'BLOG':
        case 'BLOGS':
            return TipoArtigo.Blog;

        case 'INDICACAO':
        case 'INDICACOES':
        case 'INDICACÃO': // Lida com acentuação se escapar na URL
        case 'INDICAÇÃO':
            return TipoArtigo.Indicacao;

        case 'OPINIAO':
        case 'OPINIOES':
        case 'OPNIAO': // Erro de digitação conhecido
        case 'OPNIÃO':
            return TipoArtigo.Opniao; // Retorna "OPNIAO" (que é o que o backend espera)

        case 'VIDEO':
        case 'VIDEOS':
            return TipoArtigo.Video;

        case 'ADMINISTRATIVO':
            return TipoArtigo.Administrativo;

        case 'ARTIGO':
        case 'ARTIGOS':
        default:
            // Se não encontrar ou for "Artigos", retorna o padrão
            return TipoArtigo.Artigo;
    }
};

function ArtigosPageComponent() {
    const [page, setPage] = useState(0);
    const searchParams = useSearchParams();
    
    // 1. Pega o parâmetro da URL (ex: "Entrevistas")
    const rawTipo = searchParams.get('tipo');
    
    // 2. Converte para o Enum Singular Correto (ex: "ENTREVISTA")
    const tipoEnum = getTipoEnum(rawTipo);

    const { data, loading, error } = useQuery<ArtigosPorTipoQueryData>(
        GET_ARTIGOS_POR_TIPO,
        {
            variables: {
                tipo: tipoEnum, // Agora envia "ENTREVISTA" corretamente
                pagina: page,
                tamanho: PAGE_SIZE,
            },
            fetchPolicy: 'cache-and-network',
        }
    );

    const articles = data?.obterArtigosCardListPorTipo || [];
    const canGoPrevious = page > 0;
    const canGoNext = articles.length === PAGE_SIZE;

    const renderContent = () => {
        if (loading) {
            return <p className="text-center mt-10">Buscando artigos...</p>;
        }

        if (error) {
            return <p className="text-center text-red-600 mt-10">Erro: {error.message}</p>;
        }

        if (articles.length === 0) {
            return <p className="text-center text-gray-600 mt-10">Nenhum conteúdo encontrado em {rawTipo || 'Artigos'}.</p>;
        }

        return (
            <>
                <ul className="w-full flex flex-col items-center space-y-4">
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

                <div className="flex justify-center items-center gap-4 mt-8">
                    <button
                        onClick={() => setPage(p => Math.max(0, p - 1))}
                        disabled={!canGoPrevious}
                        className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                    >
                        <ArrowLeft size={20} />
                    </button>

                    <span className="text-lg font-medium">{page + 1}</span>

                    <button
                        onClick={() => setPage(p => p + 1)}
                        disabled={!canGoNext}
                        className="p-2 rounded-md border disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                    >
                        <ArrowRight size={20} />
                    </button>
                </div>
            </>
        );
    };

    return (
        <Layout>
            <div className="w-[90%] mx-auto mb-[5vh]">
                 <div className="flex items-center mb-6 mt-6">
                    <h1 className="text-3xl font-bold text-gray-800 capitalize">
                        {rawTipo || 'Artigos'}
                    </h1>
                </div>
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