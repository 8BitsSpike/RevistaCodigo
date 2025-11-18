'use client';

import { useEffect, useState, Suspense } from 'react';
import Layout from '@/components/Layout';

// useSearchParams para ler o ID da URL
import { useRouter, useSearchParams } from 'next/navigation';
import { GraduationCap, Mail, Calendar, Building2, FileText, Briefcase, BookMarked, PencilLine, User as UserIcon } from 'lucide-react';

import { useQuery } from "@apollo/client/react";
import { GET_MEUS_ARTIGOS, GET_AUTOR_VIEW, GET_ARTIGOS_BY_IDS } from "@/graphql/queries";
import ArticleCard from '@/components/ArticleCard';
import { Console } from 'console';

const API_BASE = 'https://localhost:54868/api/Usuario';


interface InfoInstitucional {
  instituicao?: string;
  curso?: string;
  dataInicio?: string;
  dataFim?: string;
  descricaoCurso?: string;
  informacoesAdd?: string;
}
interface Atuacao {
  instituicao?: string;
  areaAtuacao?: string;
  dataInicio?: string;
  dataFim?: string;
  contribuicao?: string;
  informacoesAdd?: string;
}
// Esta interface é usada para AMBOS os perfis
interface PerfilUsuario {
  _id?: string;
  name?: string;
  sobrenome?: string;
  email?: string;
  foto?: string;
  biografia?: string;
  infoInstitucionais?: InfoInstitucional[];
  atuacoes?: Atuacao[];
}

// --- Interfaces para ArtigoAPI ---
interface ArtigoCard {
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
  obterMeusArtigosCardList: ArtigoCard[];
}

interface AutorViewQueryData {
  obterAutorView: {
    usuarioId: string;
    nome: string;
    url: string;
    artigoWorkIds: string[];
  };
}

interface ArtigosByIdsQueryData {
  obterArtigoCardListPorLista: ArtigoCard[];
}


const formatYearRange = (dataInicio?: string, dataFim?: string) => {
  const endText = !dataFim || dataFim.toLowerCase() === 'atualmente'
    ? 'ATUALMENTE'
    : new Date(dataFim).getFullYear();

  const startYear = dataInicio ? new Date(dataInicio).getFullYear() : 'Data não informada';

  if (startYear === 'Data não informada') return 'Datas não informadas';
  return `${startYear} - ${endText}`;
};

// Componente principal que contém a lógica
function ProfilePageContent() {
  const [perfil, setPerfil] = useState<PerfilUsuario>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();
  const searchParams = useSearchParams();

  // --- Lógica de Roteamento ---
  const urlId = searchParams.get('id');
  const [myId, setMyId] = useState<string | null>(null);
  const [isMyProfile, setIsMyProfile] = useState(false);
  const [authChecked, setAuthChecked] = useState(false);

  useEffect(() => {
    // Pega o ID do usuário logado do localStorage
    const loggedInUserId = localStorage.getItem('userId');
    setMyId(loggedInUserId);
    // Define se esta é a página "Minha Conta" ou a página pública "Autor"
    const viewingOwnProfile = !urlId || urlId === loggedInUserId;
    setIsMyProfile(viewingOwnProfile);
    console.log(viewingOwnProfile, "eu?");
    setAuthChecked(true);
  }, [urlId]);


  // Busca "Meus Artigos" (só roda se for isMyProfile)
  const {
    data: meusArtigosData,
    loading: loadingMeusArtigos
  } = useQuery<MeusArtigosQueryData>(GET_MEUS_ARTIGOS, {
    skip: !isMyProfile || !authChecked, // Pula se não for "meu perfil"
  });

  // Busca "Autor View" (só roda se NÃO for isMyProfile)
  const {
    data: autorViewData,
    loading: loadingAutorView
  } = useQuery<AutorViewQueryData>(GET_AUTOR_VIEW, {
    variables: { autorId: urlId },
    skip: isMyProfile || !authChecked || !urlId, // Pula se FOR "meu perfil"
  });

  const autorArtigoWorkIds = autorViewData?.obterAutorView?.artigoWorkIds;

  const {
    data: artigosPorIdsData,
    loading: loadingArtigosPorIds
  } = useQuery<ArtigosByIdsQueryData>(GET_ARTIGOS_BY_IDS, {
    variables: { ids: autorArtigoWorkIds || [] },
    skip: isMyProfile || !autorArtigoWorkIds, // Pula se FOR "meu perfil"
  });


  useEffect(() => {
    // Só roda depois que a verificação de auth foi feita
    console.log("Estou autenticado:", authChecked);

    if (!authChecked) return;

    let targetId: string | null = null;
    let token: string | null = null;

    if (isMyProfile) {
      targetId = localStorage.userId;
      token = localStorage.userToken;

      console.log(targetId, token);

      if (!targetId || !token) {
        router.push('/login');
        return;
      }
    } else {
      targetId = autorViewData?.obterAutorView?.usuarioId || null;
      // Usamos o token do usuário logado para autenticar a requisição
      token = localStorage.getItem('jwtToken');
      if (!targetId) {
        // Se a query do autor ainda não rodou ou falhou, não faz nada
        if (!loadingAutorView) {
          setError('Autor não encontrado.');
          setLoading(false);
        }
        return;
      }
    }

    const fetchProfile = async () => {
      setLoading(true);
      try {
        const res = await fetch(`${API_BASE}/${targetId}?token=${token}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error('Erro ao carregar o perfil');
        const data = await res.json();
        setPerfil(data);
      } catch (err) {
        console.error(err);
        setError('Não foi possível carregar o perfil.');
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();

    // Depende de authChecked, isMyProfile, e dos dados do autor
  }, [router, authChecked, isMyProfile, autorViewData, loadingAutorView]);

  // --- Lógica de Renderização ---
  let publishedArticles: ArtigoCard[] = [];
  let reviewArticles: ArtigoCard[] = [];

  if (isMyProfile) {
    const allMyArticles = meusArtigosData?.obterMeusArtigosCardList ?? [];
    publishedArticles = allMyArticles.filter(art => art.status === 'Publicado');
    reviewArticles = allMyArticles.filter(art => art.status !== 'Publicado');
  } else {
    // Filtra para mostrar APENAS artigos publicados
    publishedArticles = (artigosPorIdsData?.obterArtigoCardListPorLista ?? []).filter(
      art => art.status === 'Publicado'
    );
  }


  const isLoading = loading || loadingMeusArtigos || loadingAutorView || loadingArtigosPorIds;

  if (isLoading)
    return (
      <Layout>
        <p className="text-center mt-20 text-gray-600">Carregando...</p>
      </Layout>
    );

  if (error)
    return (
      <Layout>
        <p className="text-center mt-20 text-red-600">{error}</p>
      </Layout>
    );

  return (
    <Layout>
      <div className="max-w-5xl mx-auto mt-16 bg-white rounded-2xl shadow-lg overflow-hidden p-8">

        {/* Título condicional */}
        {!isMyProfile && (
          <h1 className="text-3xl font-bold text-gray-800 mb-8 text-center">
            Página do Autor
          </h1>
        )}

        {/* Header do perfil */}
        <div className="flex flex-col md:flex-row items-center md:items-start gap-8 mb-8 border-b border-gray-200 pb-8">
          <div className="relative group">
            <img
              src={perfil.foto || '/default-avatar.png'}
              alt="Foto de perfil"
              className="w-40 h-40 rounded-full object-cover border-4 border-emerald-600 shadow-md group-hover:scale-105 transition-transform duration-300"
            />
          </div>
          <div className="flex-1 w-full">
            <h1 className="text-3xl font-bold text-gray-800">
              {perfil.name} {perfil.sobrenome}
            </h1>

            {/* Email só aparece para o próprio usuário */}
            {isMyProfile && (
              <p className="text-gray-600 flex items-center gap-2 mt-2">
                <Mail className="w-5 h-5 text-gray-500" />
                {perfil.email}
              </p>
            )}

            {/* Botão Editar Perfil só aparece para o próprio usuário */}
            {isMyProfile && (
              <button
                onClick={() => router.push('/ProfileEditPage')}
                className="mt-4 md:mt-4 bg-emerald-600 hover:bg-emerald-700 text-white px-5 py-2 rounded-lg shadow-md transition duration-300"
              >
                Editar Perfil
              </button>
            )}
          </div>
        </div>

        {/* Biografia */}
        <div className="mt-8 mb-10">
          <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
            <FileText className="text-emerald-600" />
            Biografia
          </h2>
          {perfil.biografia ? (
            <p className="whitespace-pre-line text-gray-700 leading-relaxed">{perfil.biografia}</p>
          ) : (
            <p className="text-gray-500 italic">Nenhuma biografia adicionada ainda.</p>
          )}
        </div>

        {/* Experiência Profissional */}
        <div className="mt-8 mb-10">
          <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
            <Briefcase className="text-emerald-600" />
            Experiência Profissional e Atuação
          </h2>
          {perfil.atuacoes && perfil.atuacoes.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {perfil.atuacoes.map((atuacao, i) => (
                <div
                  key={i}
                  className="border border-gray-200 rounded-xl p-6 shadow-sm hover:shadow-md transition duration-300 bg-gray-50"
                >
                  <h3 className="text-lg font-semibold text-gray-800 flex items-center gap-2 mb-3">
                    <Building2 className="text-emerald-600" />
                    {atuacao.instituicao || 'Instituição não informada'}
                  </h3>
                  <p className="text-gray-700 mb-2 flex items-center gap-2">
                    <Briefcase className="text-emerald-600" />
                    {atuacao.areaAtuacao || 'Área de Atuação não informada'}
                  </p>
                  <p className="text-gray-600 text-sm flex items-center gap-2 mb-3">
                    <Calendar className="text-emerald-600" />
                    {formatYearRange(atuacao.dataInicio, atuacao.dataFim)}
                  </p>
                  {atuacao.contribuicao && (
                    <p className="text-gray-700 mb-3 leading-relaxed border-t border-gray-200 pt-3">
                      {atuacao.contribuicao}
                    </p>
                  )}
                  {atuacao.informacoesAdd && (
                    <p className="text-gray-700 mb-3 italic bg-white rounded-lg p-3 shadow-sm border border-gray-100">
                      📘 {atuacao.informacoesAdd}
                    </p>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-600 text-center italic">
              Nenhuma experiência profissional adicionada ainda.
            </p>
          )}
        </div>

        {/* Informações Institucionais */}
        <div className="mt-8">
          <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
            <GraduationCap className="text-emerald-600" />
            Informações Institucionais
          </h2>
          {perfil.infoInstitucionais && perfil.infoInstitucionais.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {perfil.infoInstitucionais.map((info, i) => (
                <div
                  key={i}
                  className="border border-gray-200 rounded-xl p-6 shadow-sm hover:shadow-md transition duration-300 bg-gray-50"
                >
                  <h3 className="text-lg font-semibold text-gray-800 flex items-center gap-2 mb-3">
                    <Building2 className="text-emerald-600" />
                    {info.instituicao || 'Instituição não informada'}
                  </h3>
                  <p className="text-gray-700 mb-2 flex items-center gap-2">
                    <GraduationCap className="text-emerald-600" />
                    {info.curso || 'Curso não informado'}
                  </p>
                  <p className="text-gray-600 text-sm flex items-center gap-2 mb-3">
                    <Calendar className="text-emerald-600" />
                    {formatYearRange(info.dataInicio, info.dataFim)}
                  </p>
                  {info.descricaoCurso && (
                    <p className="text-gray-700 mb-3 leading-relaxed border-t border-gray-200 pt-3">
                      {info.descricaoCurso}
                    </p>
                  )}
                  {info.informacoesAdd && (
                    <p className="text-gray-700 mb-3 italic bg-white rounded-lg p-3 shadow-sm border border-gray-100">
                      📘 {info.informacoesAdd}
                    </p>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-600 text-center italic">
              Nenhuma informação institucional adicionada ainda.
            </p>
          )}
        </div>

        {/* Artigos que publiquei */}
        {publishedArticles.length > 0 && (
          <div className="mt-8 mb-10">
            <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
              <BookMarked className="text-emerald-600" />
              Artigos que publiquei:
            </h2>
            <ul className="w-full flex flex-col items-center">
              {publishedArticles.map(art => (
                <ArticleCard
                  key={art.id}
                  id={art.id}
                  title={art.titulo}
                  excerpt={art.resumo}
                  href={`/artigo/${art.id}`} // Link para a página de visualização pública
                  imagem={art.midiaDestaque ? {
                    url: art.midiaDestaque.url,
                    textoAlternativo: art.midiaDestaque.textoAlternativo
                  } : null}
                />
              ))}
            </ul>
          </div>
        )}

        {/* Meus artigos em ciclo editorial (só aparece para o próprio usuário) */}
        {isMyProfile && reviewArticles.length > 0 && (
          <div className="mt-8 mb-10">
            <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
              <PencilLine className="text-emerald-600" />
              Meus artigos em ciclo editorial:
            </h2>
            <ul className="w-full flex flex-col items-center">
              {reviewArticles.map(art => (
                <ArticleCard
                  key={art.id}
                  id={art.id}
                  title={art.titulo}
                  excerpt={art.resumo}
                  href={`/artigo/edicao/${art.id}`} // Link para a página de edição
                  imagem={art.midiaDestaque ? {
                    url: art.midiaDestaque.url,
                    textoAlternativo: art.midiaDestaque.textoAlternativo
                  } : null}
                />
              ))}
            </ul>
          </div>
        )}
      </div>
    </Layout>
  );
}


export default function ProfilePageWrapper() {
  return (
    <Suspense fallback={<Layout><p className="text-center mt-20 text-gray-600">Carregando...</p></Layout>}>
      <ProfilePageContent />
    </Suspense>
  );
}