'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useQuery } from '@apollo/client/react';
import { GET_MEUS_ARTIGOS } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { PenSquare, BookMarked } from 'lucide-react';
import { USER_API_BASE } from '@/lib/fetcher';

interface ArtigoCardData {
    id: string;
    titulo: string;
    resumo?: string;
    status: string;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
}

interface MeusArtigosQueryData {
    obterMeusArtigosCardList: ArtigoCardData[];
}

export default function SalaProfessoresClient() {
    const router = useRouter();
    const { user, loading: authLoading } = useAuth();

    const [biografia, setBiografia] = useState<string | null>(null);
    const [loadingBio, setLoadingBio] = useState(true);

    const {
        data: artigosData,
        loading: loadingArtigos,
        error: errorArtigos
    } = useQuery<MeusArtigosQueryData>(GET_MEUS_ARTIGOS, {
        skip: !user,
    });

    useEffect(() => {
        if (authLoading) return;

        if (!user) {
            router.push('/login');
            return;
        }

        const token = localStorage.getItem('userToken');
        if (!token) {
            router.push('/login');
            return;
        }

        const fetchProfileBio = async () => {
            try {
                setLoadingBio(true);
                // Use USER_API_BASE
                const res = await fetch(`${USER_API_BASE}/${user.id}?token=${token}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (!res.ok) throw new Error('Erro ao carregar dados do perfil');
                const data = await res.json();
                setBiografia(data.biografia || null);
            } catch (err) {
                console.error(err);
                setBiografia(null);
            } finally {
                setLoadingBio(false);
            }
        };

        fetchProfileBio();
    }, [user, authLoading, router]);

    const reviewArticles = artigosData?.obterMeusArtigosCardList.filter(
        art => art.status === 'EmRevisao'
    ) ?? [];

    const canSubmit = biografia !== null && biografia !== '';

    const isLoading = authLoading || loadingBio || loadingArtigos;

    if (isLoading) {
        return (
            <Layout>
                <p className="text-center mt-20 text-gray-600">Carregando...</p>
            </Layout>
        );
    }

    if (!user) {
        return null;
    }

    return (
        <Layout>
            <div className="w-[90%] mx-auto mb-[5vh]">
                <h1 className="text-3xl font-bold mb-10 text-center">Sala dos Professores</h1>

                <div className="mb-12 p-6 bg-gray-50 rounded-lg shadow-sm">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
                        <PenSquare className="text-emerald-600" />
                        Escrever novo artigo
                    </h2>

                    <div className="flex flex-col items-start">
                        <Link
                            href="/professores/criarartigo"
                            aria-disabled={!canSubmit}
                            className={`
                px-5 py-2 rounded-lg shadow-md text-white font-medium
                transition duration-300
                ${canSubmit
                                    ? 'bg-emerald-600 hover:bg-emerald-700'
                                    : 'bg-gray-400 cursor-not-allowed opacity-70'
                                }
              `}
                            onClick={(e) => {
                                if (!canSubmit) e.preventDefault();
                            }}
                        >
                            Iniciar Submissão
                        </Link>

                        {!canSubmit && (
                            <p className="text-sm text-gray-600 mt-4">
                                Para criar um novo artigo é necessário completar seu cadastro com maiores informações sobre você.
                                Você pode fazer isso na {' '}
                                <Link href="/profile" className="text-emerald-600 underline hover:text-emerald-800">
                                    Página de perfil.
                                </Link>
                            </p>
                        )}
                    </div>
                </div>

                <div className="mt-8">
                    <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
                        <BookMarked className="text-emerald-600" />
                        Meus artigos em revisão:
                    </h2>

                    {errorArtigos && (
                        <p className="text-center text-red-600">Erro ao carregar artigos: {errorArtigos.message}</p>
                    )}

                    {reviewArticles.length > 0 ? (
                        <ul className="w-full flex flex-col items-center">
                            {reviewArticles.map(art => (
                                <ArticleCard
                                    key={art.id}
                                    id={art.id}
                                    title={art.titulo}
                                    excerpt={art.resumo}
                                    href={`/editorial/artigoEdicao/${art.id}`}
                                    imagem={art.midiaDestaque ? {
                                        url: art.midiaDestaque.url,
                                        textoAlternativo: art.midiaDestaque.textoAlternativo
                                    } : null}
                                />
                            ))}
                        </ul>
                    ) : (
                        <p className="text-gray-600 text-center italic">
                            Sem artigos em revisão no momento.
                        </p>
                    )}
                </div>
            </div>
        </Layout>
    );
}