'use client';

import { useState, useEffect } from 'react';
import { useQuery, useLazyQuery } from '@apollo/client/react';
import { useParams, useRouter } from 'next/navigation';
import { GET_ARTIGO_VIEW, GET_COMENTARIOS_PUBLICOS, ATUALIZAR_INTERACAO, DELETAR_INTERACAO } from '@/graphql/queries';
import Layout from '@/components/Layout';
import AuthorCard from '@/components/AuthorCard';
import CommentCard, { Comment } from '@/components/CommentCard';
import CreateCommentCard from '@/components/CreateCommentCard';
import { Printer, MessageSquare, BookOpen, Layers } from 'lucide-react';
import Image from 'next/image';
import Link from 'next/link';
import useAuth from '@/hooks/useAuth';
import toast from 'react-hot-toast';

const COMENTARIOS_PAGE_SIZE = 10; // Carrega 10 por vez no "Carregar mais"

// --- Tipos de Dados ---
interface ArtigoView {
    id: string;
    titulo: string;
    tipo: string;
    permitirComentario: boolean;
    totalComentarios: number;
    midiaDestaque?: {
        url: string;
        textoAlternativo: string;
    };
    conteudoAtual: {
        content: string;
        midias: {
            url: string;
            textoAlternativo: string;
        }[];
    };
    autores: {
        usuarioId: string;
        nome: string;
        url: string;
    }[];
    volume?: {
        id: string;
        volumeTitulo: string;
        volumeResumo: string;
    };
    interacoes: { // Vem da GET_ARTIGO_VIEW
        comentariosEditoriais: Comment[];
    };
}

interface ArtigoViewQueryData {
    obterArtigoView: ArtigoView;
}

interface ComentariosPublicosQueryData {
    obterComentariosPublicos: Comment[];
}

// --- Componente Principal ---
export default function ArtigoClient() {
    const params = useParams();
    const artigoId = params.id as string;
    const { user } = useAuth();

    const [nomeUsuario, setNomeUsuario] = useState("Leitor");

    useEffect(() => {
        // (MODIFICADO) Busca o nome do usuário do localStorage (do hook useAuth)
        const storedName = localStorage.getItem('userName');
        if (storedName) {
            setNomeUsuario(storedName);
        }
    }, [user]);

    // --- Query 1: Dados Principais do Artigo ---
    const {
        data: artigoData,
        loading: loadingArtigo,
        error: errorArtigo,
        refetch: refetchArtigoView // (NOVO) Função para recarregar a query principal
    } = useQuery<ArtigoViewQueryData>(GET_ARTIGO_VIEW, {
        variables: { artigoId },
        skip: !artigoId,
        onError: (err) => {
            toast.error(`Erro ao carregar artigo: ${err.message}`);
        }
    });

    const artigo = artigoData?.obterArtigoView;

    // --- Query 2: Comentários Públicos Paginados ---
    const {
        data: comentariosData,
        loading: loadingComentarios,
        error: errorComentarios,
        fetchMore,
        refetch: refetchComentarios // (NOVO) Função para recarregar comentários
    } = useQuery<ComentariosPublicosQueryData>(GET_COMENTARIOS_PUBLICOS, {
        variables: { artigoId, page: 0, pageSize: 20 }, // Carga inicial de 20
        skip: !artigoId,
        onError: (err) => {
            toast.error(`Erro ao carregar comentários: ${err.message}`);
        }
    });

    const comentariosPublicos = comentariosData?.obterComentariosPublicos ?? [];
    const comentariosEditoriais = artigo?.interacoes.comentariosEditoriais ?? [];

    // Função para carregar mais comentários
    const handleLoadMoreComments = () => {
        if (!comentariosData) return;

        toast.loading('Carregando mais...', { id: 'load-comments' });
        fetchMore({
            variables: {
                // Calcula a próxima página
                page: Math.ceil(comentariosPublicos.length / COMENTARIOS_PAGE_SIZE),
                pageSize: COMENTARIOS_PAGE_SIZE,
            },
            updateQuery: (prev, { fetchMoreResult }) => {
                toast.dismiss('load-comments');
                if (!fetchMoreResult || fetchMoreResult.obterComentariosPublicos.length === 0) {
                    toast.success('Não há mais comentários.');
                    return prev;
                }
                return {
                    obterComentariosPublicos: [
                        ...prev.obterComentariosPublicos,
                        ...fetchMoreResult.obterComentariosPublicos,
                    ],
                };
            },
        }).catch(err => toast.error(`Erro ao carregar: ${err.message}`));
    };

    // --- Renderização ---

    if (loadingArtigo) {
        return <Layout><p className="text-center mt-20">Carregando artigo...</p></Layout>;
    }

    // (MODIFICADO) O erro agora é tratado pelo toast
    if (errorArtigo && !artigo) {
        return <Layout><p className="text-center mt-20 text-red-600">Artigo não pôde ser carregado.</p></Layout>;
    }

    if (!artigo) {
        return <Layout><p className="text-center mt-20">Artigo não encontrado.</p></Layout>;
    }

    if (artigo.tipo === 'Administrativo') {
        return (
            <Layout>
                <div className="w-[90%] mx-auto my-8">
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-8">{artigo.titulo}</h1>
                    <div
                        className="prose prose-lg max-w-none"
                        dangerouslySetInnerHTML={{ __html: artigo.conteudoAtual.content }}
                    />
                </div>
            </Layout>
        );
    }

    const showPrintButton = artigo.tipo === 'Artigo';
    const totalPublicComments = artigo.totalComentarios || 0;
    const hasMoreComments = comentariosPublicos.length < totalPublicComments;

    const handlePrint = () => {
        window.print();
    };

    // (NOVO) Função unificada para recarregar tudo
    const refetchAll = () => {
        refetchArtigoView();
        refetchComentarios();
    };

    return (
        <Layout>
            <div className="print-container">
                <div className="print-hide">
                    {artigo.midiaDestaque && (
                        <div className="w-[90%] mx-auto relative h-[400px]">
                            <Image
                                src={artigo.midiaDestaque.url}
                                alt={artigo.midiaDestaque.textoAlternativo || artigo.titulo}
                                fill
                                className="object-cover rounded-lg shadow-lg"
                                priority
                            />
                        </div>
                    )}
                </div>

                <article className="print-container-content w-[90%] mx-auto">
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mt-8 mb-8 print-title">
                        {artigo.titulo}
                    </h1>

                    <div className="print-authors">
                        {artigo.autores.map(autor => (
                            <AuthorCard
                                key={autor.usuarioId}
                                usuarioId={autor.usuarioId}
                                nome={autor.nome}
                                urlFoto={autor.url}
                            />
                        ))}
                    </div>

                    <div
                        className="prose prose-lg max-w-none mt-4 mx-auto"
                        style={{
                            margin: '1% auto 2% auto',
                            width: '96%'
                        }}
                    >
                        {artigo.midiaDestaque && (
                            <div className="print-hide w-full flex justify-center my-6">
                                <Image
                                    src={artigo.midiaDestaque.url}
                                    alt={artigo.midiaDestaque.textoAlternativo || artigo.titulo}
                                    width={800}
                                    height={600}
                                    className="object-contain rounded-md"
                                />
                            </div>
                        )}

                        <div
                            className="print-content"
                            dangerouslySetInnerHTML={{ __html: artigo.conteudoAtual.content }}
                        />
                    </div>

                    {artigo.volume && (
                        <div className="print-volume w-[90%] my-10 mx-auto p-6 bg-gray-50 rounded-lg shadow-sm border">
                            <h3 className="text-xl font-semibold text-gray-800 flex items-center gap-2">
                                <Layers className="text-emerald-600" />
                                Publicado em: {artigo.volume.volumeTitulo}
                            </h3>
                            <p className="text-gray-600 mt-2">{artigo.volume.volumeResumo}</p>
                            <Link
                                href={`/volume/${artigo.volume.id}`}
                                className="text-sm text-emerald-600 hover:text-emerald-800 hover:underline font-medium mt-3 inline-block print-hide"
                            >
                                Clique aqui para ver o volume completo
                            </Link>
                        </div>
                    )}

                    {showPrintButton && (
                        <div className="w-[90%] mx-auto text-center my-10 print-hide">
                            <button
                                onClick={handlePrint}
                                className="px-6 py-3 rounded-lg bg-emerald-600 text-white font-medium shadow hover:bg-emerald-700 transition flex items-center gap-2 mx-auto"
                            >
                                <Printer size={20} />
                                Imprimir artigo para PDF
                            </button>
                        </div>
                    )}
                </article>
            </div>

            <div className="w-[90%] mx-auto mt-12 print-hide">

                {/* Comentários Editoriais */}
                {comentariosEditoriais.length > 0 && (
                    <section className="mb-10">
                        <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
                            <MessageSquare className="text-emerald-600" />
                            Comentários da equipe editorial:
                        </h2>
                        <div className="space-y-4">
                            {comentariosEditoriais.map(comment => (
                                <CommentCard
                                    key={comment.id}
                                    comment={comment}
                                    artigoId={artigoId}
                                    isPublic={false}
                                    permitirRespostas={false}
                                    // Prop 'onCommentDeleted' alterada para 'onCommentAction'
                                    onCommentAction={refetchAll}
                                />
                            ))}
                        </div>
                    </section>
                )}

                {/* Criar Comentário Público (se permitido) */}
                {artigo.permitirComentario && (
                    <section className="mb-10">
                        <CreateCommentCard
                            artigoId={artigoId}
                            onCommentPosted={refetchAll} // Recarrega tudo
                        />
                    </section>
                )}

                {/* Comentários Públicos (se houver) */}
                {/* Adiciona verificação de erro */}
                {errorComentarios && (
                    <p className="text-center text-red-600">Não foi possível carregar os comentários públicos.</p>
                )}

                {comentariosPublicos.length > 0 && (
                    <section className="mb-10">
                        <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2">
                            <MessageSquare className="text-gray-700" />
                            Comentários dos leitores:
                        </h2>
                        <div className="space-y-4">
                            {comentariosPublicos.map(comment => (
                                <CommentCard
                                    key={comment.id}
                                    comment={comment}
                                    artigoId={artigoId}
                                    isPublic={true}
                                    permitirRespostas={artigo.permitirComentario}
                                    // Prop 'onCommentDeleted' alterada para 'onCommentAction'
                                    onCommentAction={refetchAll}
                                />
                            ))}
                        </div>

                        {/* Botão "Carregar Mais" */}
                        {hasMoreComments && (
                            <div className="text-center mt-8">
                                <button
                                    onClick={handleLoadMoreComments}
                                    disabled={loadingComentarios}
                                    className="px-5 py-2 rounded-lg bg-gray-100 text-gray-800 font-medium hover:bg-gray-200 transition"
                                >
                                    {loadingComentarios ? 'Carregando...' : 'Carregar mais comentários'}
                                </button>
                            </div>
                        )}
                    </section>
                )}

            </div>
        </Layout>
    );
}