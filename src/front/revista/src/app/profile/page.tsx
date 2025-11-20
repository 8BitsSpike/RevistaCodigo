'use client';

import { useState, useEffect, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { useQuery } from '@apollo/client/react';
import { GET_MEUS_ARTIGOS, GET_ARTIGOS_BY_IDS } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ArticleCard from '@/components/ArticleCard';
import { User, MapPin, BookOpen, Edit, Mail, Building2, GraduationCap, Loader2, Calendar } from 'lucide-react';
import Image from 'next/image';
import Link from 'next/link';
import { USER_API_BASE } from '@/lib/fetcher';
import { formatDate } from '@/lib/dateUtils';

interface InstituicaoInfo {
    instituicao: string;
    areaAtuacao?: string;
    dataInicio?: string;
    dataFim?: string;
    contribuicao?: string;
    informacoesAdd?: string;
}

interface UserProfile {
    id: string;
    name: string;
    sobrenome?: string;
    email?: string;
    foto?: string;
    biografia?: string;
    endereco?: string;
    infoInstitucionais?: InstituicaoInfo[];
    atuacoes?: InstituicaoInfo[]; // FIX: Changed from string[] to Object Array
}

interface ArtigoCardData {
    id: string;
    titulo: string;
    resumo?: string;
    status: string;
    midiaDestaque?: { url: string; textoAlternativo: string; };
}

function ProfileContent() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const queryId = searchParams.get('id');
    const { user, loading: authLoading } = useAuth();
    const targetId = queryId || user?.id;
    const isOwnProfile = !!user && user.id === targetId;
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loadingProfile, setLoadingProfile] = useState(true);

    useEffect(() => {
        if (authLoading) return;
        if (!targetId) {
            if (!queryId) router.push('/login');
            return;
        }
        const fetchProfile = async () => {
            setLoadingProfile(true);
            try {
                const token = localStorage.getItem('userToken');
                const headers: any = {};
                if (token) headers['Authorization'] = `Bearer ${token}`;
                const res = await fetch(`${USER_API_BASE}/${targetId}?token=${token || ''}`, { headers });
                if (!res.ok) throw new Error('Perfil não encontrado');
                const data = await res.json();
                setProfile(data);
            } catch (error) { console.error(error); } finally { setLoadingProfile(false); }
        };
        fetchProfile();
    }, [targetId, authLoading, queryId, router]);

    const { data: myArticlesData } = useQuery(GET_MEUS_ARTIGOS, { skip: !isOwnProfile || authLoading });
    const articleIds: string[] = [];
    const { data: publicArticlesData } = useQuery(GET_ARTIGOS_BY_IDS, { variables: { ids: articleIds }, skip: isOwnProfile || articleIds.length === 0 });

    if (authLoading || loadingProfile) return <Layout><div className="flex justify-center mt-20"><Loader2 className="animate-spin" /></div></Layout>;
    if (!profile) return <Layout><div className="text-center mt-20"><h2 className="text-xl font-bold text-gray-800">Perfil não encontrado</h2><Link href="/" className="text-emerald-600 hover:underline mt-4 block">Voltar para a Home</Link></div></Layout>;

    const articlesToDisplay: ArtigoCardData[] = isOwnProfile ? (myArticlesData?.obterMeusArtigosCardList || []) : (publicArticlesData?.obterArtigoCardListPorLista || []);

    return (
        <Layout>
            <div className="w-[90%] mx-auto mb-20">
                <div className="bg-white shadow-lg rounded-lg overflow-hidden mb-8 border border-gray-100">
                    <div className="h-32 bg-emerald-600"></div>
                    <div className="px-8 pb-8">
                        <div className="relative flex justify-between items-end -mt-12 mb-6">
                            <div className="relative w-32 h-32 rounded-full border-4 border-white overflow-hidden bg-gray-200 shadow-md">
                                <Image src={profile.foto || '/faviccon.png'} alt={profile.name} fill className="object-cover" />
                            </div>
                            {isOwnProfile && <Link href="/ProfileEditPage" className="flex items-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md transition text-sm font-medium"><Edit size={16} /> Editar Perfil</Link>}
                        </div>

                        <h1 className="text-3xl font-bold text-gray-900">{profile.name} {profile.sobrenome}</h1>
                        <div className="flex items-center gap-2 text-gray-500 mt-1"><Mail size={16} /><span>{profile.email || 'Email não informado'}</span></div>
                        {profile.endereco && <div className="flex items-center gap-2 text-gray-500 mt-1"><MapPin size={16} /> <span>{profile.endereco}</span></div>}

                        <div className="mt-8 flex flex-col gap-6">

                            {profile.biografia && (
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-800 mb-2 flex items-center gap-2"><BookOpen size={18} className="text-emerald-600" /> Biografia</h3>
                                    <div className="text-gray-600 text-sm leading-relaxed whitespace-pre-line">{profile.biografia}</div>
                                </div>
                            )}

                            {profile.infoInstitucionais && profile.infoInstitucionais.length > 0 && (
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2"><Building2 size={18} className="text-emerald-600" /> Formação Acadêmica</h3>
                                    <div className="space-y-3">
                                        {profile.infoInstitucionais.map((info, idx) => (
                                            <div key={idx} className="p-3 bg-gray-50 rounded-lg border border-gray-100">
                                                <div className="font-medium text-gray-800">{info.instituicao}</div>
                                                {info.areaAtuacao && <div className="text-sm text-emerald-700 font-medium">{info.areaAtuacao}</div>}

                                                {(info.dataInicio || info.dataFim) && (
                                                    <div className="flex items-center gap-2 text-xs text-gray-500 mt-1">
                                                        <Calendar size={12} />
                                                        <span>{formatDate(info.dataInicio || '')} - {info.dataFim ? formatDate(info.dataFim) : 'Atual'}</span>
                                                    </div>
                                                )}

                                                {info.contribuicao && <div className="text-xs text-gray-600 mt-2"><strong>Contribuição:</strong> {info.contribuicao}</div>}
                                                {info.informacoesAdd && <div className="text-xs text-gray-500 mt-1 italic">{info.informacoesAdd}</div>}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* FIX: Rendering Atuações as structured cards */}
                            {profile.atuacoes && profile.atuacoes.length > 0 && (
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2"><GraduationCap size={18} className="text-emerald-600" /> Atuações Profissionais</h3>
                                    <div className="space-y-3">
                                        {profile.atuacoes.map((at, idx) => {
                                            // Handle case where it might still be a string legacy data
                                            if (typeof at === 'string') {
                                                return <span key={idx} className="px-3 py-1 bg-emerald-50 text-emerald-700 rounded-full text-xs">{at}</span>;
                                            }
                                            return (
                                                <div key={idx} className="p-3 bg-gray-50 rounded-lg border border-gray-100">
                                                    <div className="font-medium text-gray-800">{at.instituicao}</div>
                                                    {at.areaAtuacao && <div className="text-sm text-emerald-700 font-medium">{at.areaAtuacao}</div>}

                                                    {(at.dataInicio || at.dataFim) && (
                                                        <div className="flex items-center gap-2 text-xs text-gray-500 mt-1">
                                                            <Calendar size={12} />
                                                            <span>{formatDate(at.dataInicio || '')} - {at.dataFim ? formatDate(at.dataFim) : 'Atual'}</span>
                                                        </div>
                                                    )}

                                                    {at.contribuicao && <div className="text-xs text-gray-600 mt-2"><strong>Contribuição:</strong> {at.contribuicao}</div>}
                                                    {at.informacoesAdd && <div className="text-xs text-gray-500 mt-1 italic">{at.informacoesAdd}</div>}
                                                </div>
                                            );
                                        })}
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {articlesToDisplay.length > 0 && (
                    <div className="mt-12">
                        <h2 className="text-2xl font-bold text-gray-800 mb-6 border-b pb-2">{isOwnProfile ? 'Meus Artigos' : `Artigos de ${profile.name}`}</h2>
                        <div className="flex flex-col items-center">
                            {articlesToDisplay.map(art => (
                                <ArticleCard
                                    key={art.id}
                                    id={art.id}
                                    title={art.titulo}
                                    excerpt={art.resumo}
                                    href={isOwnProfile ? `/editorial/artigoEdicao/${art.id}` : `/artigo/${art.id}`}
                                    imagem={art.midiaDestaque ? { url: art.midiaDestaque.url, textoAlternativo: art.midiaDestaque.textoAlternativo } : null}
                                />
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </Layout>
    );
}

export default function ProfilePage() {
    return <Suspense fallback={<Layout><div className="flex justify-center mt-20"><Loader2 className="animate-spin" /></div></Layout>}><ProfileContent /></Suspense>;
}