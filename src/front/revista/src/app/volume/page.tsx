'use client';

import { Suspense } from 'react';
import { useQuery } from '@apollo/client/react';
import { useSearchParams } from 'next/navigation'; // FIX: useSearchParams
import { GET_VOLUME_VIEW } from '@/graphql/queries';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { BookMarked, Loader2 } from 'lucide-react';

interface ArtigoCardData {
    id: string;
    titulo: string;
    resumo?: string;
    midiaDestaque?: { url: string; textoAlternativo: string; };
}

interface VolumeViewData {
    id: string;
    volumeTitulo: string;
    volumeResumo: string;
    imagemCapa?: { url: string; textoAlternativo: string; };
    artigos: ArtigoCardData[];
}

interface VolumeViewQueryData {
    obterVolumeView: VolumeViewData;
}

function VolumePageContent() {
    const searchParams = useSearchParams();
    const volumeId = searchParams.get('id'); // FIX: Get ID from query param

    const { data, loading, error } = useQuery<VolumeViewQueryData>(
        GET_VOLUME_VIEW,
        {
            variables: { volumeId },
            skip: !volumeId,
        }
    );

    const volume = data?.obterVolumeView;

    if (loading) return <Layout><div className="flex justify-center mt-20"><Loader2 className="animate-spin" /></div></Layout>;
    if (error) return <Layout><div className="text-center mt-20 text-red-600">Erro ao carregar o volume.</div></Layout>;
    if (!volumeId) return <Layout><div className="text-center mt-20 text-gray-600">Nenhum volume selecionado.</div></Layout>;
    if (!volume) return <Layout><div className="text-center mt-20 text-gray-600">Volume não encontrado.</div></Layout>;

    return (
        <Layout>
            <div className="w-full mx-auto mb-[5vh]">
                <ul className="w-full flex flex-col items-center">
                    <ArticleCard
                        id={volume.id}
                        title={volume.volumeTitulo}
                        excerpt={volume.volumeResumo}
                        href={`/volume?id=${volume.id}`} // Keep consistent
                        imagem={volume.imagemCapa ? {
                            url: volume.imagemCapa.url,
                            textoAlternativo: volume.imagemCapa.textoAlternativo
                        } : null}
                    />
                </ul>

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
                                    href={`/artigo/${art.id}`}
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
        <Suspense fallback={<Layout><div className="flex justify-center mt-20"><Loader2 className="animate-spin" /></div></Layout>}>
            <VolumePageContent />
        </Suspense>
    );
}