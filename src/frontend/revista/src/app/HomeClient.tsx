"use client";

import { useQuery } from "@apollo/client/react";
import { GET_HOME_PAGE_DATA } from "@/graphql/queries";
import HeroCarousel, { Slide } from "@/components/HeroCarousel";
import EditionList, { Edition } from "@/components/EditionList";
import Layout from "@/components/Layout";

interface ArticleCardData {
    id: string;
    titulo: string;
    resumo: string;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
}

interface VolumeCardData {
    id: string;
    volumeTitulo: string;
    volumeResumo: string;
    imagemCapa?: {
        url: string;
        textoAlternativo: string;
    };
}

interface HomePageQueryData {
    latestArticles: ArticleCardData[];
    latestVolumes: VolumeCardData[];
}

export default function HomePageClient() {
    const { data, loading, error } = useQuery<HomePageQueryData>(GET_HOME_PAGE_DATA);

    if (loading) {
        return (
            <Layout>
                <div className="text-center mt-20">
                    <p>Carregando...</p>
                </div>
            </Layout>
        );
    }

    if (error) {
        return (
            <Layout>
                <div className="p-4 text-red-700 bg-red-100 border border-red-300 rounded-md mt-10 mx-auto max-w-2xl">
                    <h2>Erro ao carregar os dados</h2>
                    <pre className="mt-2 whitespace-pre-wrap text-xs">{error.message}</pre>
                </div>
            </Layout>
        );
    }

    const latestArticles = data?.latestArticles ?? [];
    const latestVolumes = data?.latestVolumes ?? [];

    if (latestArticles.length === 0 && latestVolumes.length === 0) {
        return (
            <Layout>
                <div className="text-center mt-20">
                    <p>Não há conteúdo para ser carregado no momento.</p>
                </div>
            </Layout>
        );
    }

    const carouselSlides: Slide[] = latestArticles.map((art) => ({
        title: art.titulo,
        excerpt: art.resumo,
        href: `/artigo/${art.id}`,
        // FIX: Handle missing image gracefully
        image: art.midiaDestaque?.url || undefined,
    }));

    const volumeEditions: Edition[] = latestVolumes.map((vol) => ({
        id: vol.id,
        title: vol.volumeTitulo,
        resumo: vol.volumeResumo,
        imagem: vol.imagemCapa ? {
            url: vol.imagemCapa.url,
            textoAlternativo: vol.imagemCapa.textoAlternativo
        } : null
    }));

    return (
        <Layout hero={carouselSlides.length > 0 ? <HeroCarousel slides={carouselSlides} /> : undefined}>

            {latestVolumes.length > 0 && (
                <section className="mt-12 mb-20">
                    <h2 className="text-2xl font-semibold mb-6 text-center">Últimas Edições</h2>
                    <EditionList editions={volumeEditions} />
                </section>
            )}

        </Layout>
    );
}