'use client';

import { Suspense } from 'react';
import { useQuery } from '@apollo/client/react';
import { useParams } from 'next/navigation';
import { GET_VOLUME_VIEW } from '@/graphql/queries';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { BookMarked } from 'lucide-react';


interface ArtigoCardData {
    id: string;
    titulo: string;
    resumo?: string;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
}

interface VolumeViewData {
    id: string;
    volumeTitulo: string;
    volumeResumo: string;
    imagemCapa?: {
        url: string;
        textoAlternativo: string;
    };
    artigos: ArtigoCardData[];
}

interface VolumeViewQueryData {
    obterVolumeView: VolumeViewData;
}


function VolumePageContent() {
    const params = useParams();
    const volumeId = params.id as string;

    const { data, loading, error } = useQuery<VolumeViewQueryData>(
        GET_VOLUME_VIEW,
        {
            variables: { volumeId },
            skip: !volumeId, // Pula a query se o ID não estiver presente
        }
    );

    const volume = data?.obterVolumeView;

    if (loading) {
        return (
            <Layout>
                <p className="text-center mt-20 text-gray-600">Carregando Volume...</p>
            </Layout>
        );
    }

    if (error) {
        return (
            <Layout>
                <p className="text-center mt-20 text-red-600">Erro ao carregar o volume: {error.message}</p>
            </Layout>
        );
    }

    if (!volume) {
        return (
            <Layout>
                <p className="text-center mt-20 text-gray-600">Volume não encontrado.</p>
            </Layout>
        );
    }

    return (
        <Layout>
            <div className="w-full mx-auto mb-[5vh]">

                <ul className="w-full flex flex-col items-center">
                    <ArticleCard
                        id={volume.id}
                        title={volume.volumeTitulo}
                        excerpt={volume.volumeResumo}
                        href={`/volume/${volume.id}`} // Link para ele mesmo
                        imagem={volume.imagemCapa ? {
                            url: volume.imagemCapa.url,
                            textoAlternativo: volume.imagemCapa.textoAlternativo
                        } : null}
                    />
                </ul>

                {/* Lista de Artigos no Volume */}
                <div className="mt-12 w-[90%] mx-auto">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
                        <BookMarked className="text-emerald-600" />
                        Artigos neste Volume:
                    </h2>

                    {volume.artigos && volume.artigos.length > 0 ? (
                        <ul className="w-full flex flex-col items-center">
                            {volume.artigos.map((art) => (
                                <ArticleCard
                                    key={art.id}
                                    id={art.id}
                                    title={art.titulo}
                                    excerpt={art.resumo}
                                    href={`/artigo/${art.id}`} // Link para a página do artigo
                                    imagem={art.midiaDestaque ? {
                                        url: art.midiaDestaque.url,
                                        textoAlternativo: art.midiaDestaque.textoAlternativo
                                    } : null}
                                />
                            ))}
                        </ul>
                    ) : (
                        <p className="text-center text-gray-600 italic">
                            Nenhum artigo publicado neste volume.
                        </p>
                    )}
                </div>
            </div>
        </Layout>
    );
}

export default function VolumePageWrapper() {
    return (
        <Suspense fallback={<Layout><p className="text-center mt-20 text-gray-600">Carregando...</p></Layout>}>
            <VolumePageContent />
        </Suspense>
    );
}