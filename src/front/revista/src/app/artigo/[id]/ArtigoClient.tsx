'use client';

import { useState, useEffect } from 'react';
import { useQuery } from '@apollo/client/react';
import { useParams, useRouter } from 'next/navigation';
import {
    GET_ARTIGO_VIEW,
    GET_COMENTARIOS_PUBLICOS,
    ATUALIZAR_INTERACAO,
    DELETAR_INTERACAO
} from '@/graphql/queries';
import Layout from '@/components/Layout';
import AuthorCard from '@/components/AuthorCard';
import CommentCard, { Comment } from '@/components/CommentCard';
import CreateCommentCard from '@/components/CreateCommentCard';
import { Printer, MessageSquare, BookOpen, Layers } from 'lucide-react';
import Image from 'next/image';
import Link from 'next/link';
import useAuth from '@/hooks/useAuth';

const COMENTARIOS_PAGE_SIZE = 10;

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
    interacoes: {
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
    const { user } = useAuth(); // Para o nome do usuário nos comentários

    const [nomeUsuario, setNomeUsuario] = useState("Leitor"); // Nome do usuário logado

    useEffect(() => {
        // TODO: Substituir por uma busca real do nome na UsuarioAPI
        const storedName = localStorage.getItem('userName'); // Assumindo que você salve isso no login
        if (storedName) {
            setNomeUsuario(storedName);
        }
    }, [user]);

    const {
        data: artigoData,
        loading: loadingArtigo,
        error: errorArtigo
    } = useQuery<ArtigoViewQueryData>(GET_ARTIGO_VIEW, {
        variables: { artigoId },
        skip: !artigoId,
    });

    const artigo = artigoData?.obterArtigoView;

    const {
        data: comentariosData,
        loading: loadingComentarios,
        error: errorComentarios,
        fetchMore
    } = useQuery<ComentariosPublicosQueryData>(GET_COMENTARIOS_PUBLICOS, {
        variables: { artigoId, page: 0, pageSize: 20 },
        skip: !artigoId,
    });

    const comentariosPublicos = comentariosData?.obterComentariosPublicos ?? [];
    const comentariosEditoriais = artigo?.interacoes.comentariosEditoriais ?? [];

    const handleLoadMoreComments = () => {
        if (!comentariosData) return;

        fetchMore({
            variables: {
                page: Math.ceil(comentariosPublicos.length / COMENTARIOS_PAGE_SIZE),
                pageSize: COMENTARIOS_PAGE_SIZE,
            },
            updateQuery: (prev, { fetchMoreResult }) => {
                if (!fetchMoreResult) return prev;
                return {
                    obterComentariosPublicos: [
                        ...prev.obterComentariosPublicos,
                        ...fetchMoreResult.obterComentariosPublicos,
                    ],
                };
            },
        });
    };

    // --- Renderização ---

    if (loadingArtigo) {
        return <Layout><p className="text-center mt-20">Carregando artigo...</p></Layout>;
    }

    if (errorArtigo) {
        return <Layout><p className="text-center mt-20 text-red-600">Erro: {errorArtigo.message}</p></Layout>;
    }

    if (!artigo) {
        return <Layout><p className="text-center mt-20">Artigo não encontrado.</p></Layout>;
    }

    // 'Administrativo' (Apenas Título e Conteúdo)
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

    // 'Artigo' ou outros tipos (Layout completo)
    const showPrintButton = artigo.tipo === 'Artigo';
    const totalPublicComments = artigo.totalComentarios || 0;
    const hasMoreComments = comentariosPublicos.length < totalPublicComments;

    // Função de Impressão
    const handlePrint = () => {
        window.print();
    };

    // Função para recarregar comentários (após deletar ou postar)
    const refetchComments = () => {
        fetchMore({
            variables: {
                artigoId,
                page: 0,
                pageSize: Math.max(20, comentariosPublicos.length) // Recarrega tudo
            },
            updateQuery: (prev, { fetchMoreResult }) => {
                if (!fetchMoreResult) return prev;
                return { obterComentariosPublicos: fetchMoreResult.obterComentariosPublicos };
            },
        });
    };

    return (
        <Layout>
            {/* A folha de estilo print-container aplica visibility: hidden no @media print,
        e o print-container-content força a visibilidade dos filhos.
      */}
            <div className="print-container">
                {/* --- ÁREA NÃO IMPRIMÍVEL (Wrapper para esconder no print) --- */}
                <div className="print-hide">
                    {/* Imagem Destaque Topo */}
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

                {/* --- ÁREA IMPRIMÍVEL E VISÍVEL --- */}
                <article className="print-container-content w-[90%] mx-auto">
                    {/* Título */}
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mt-8 mb-8 print-title">
                        {artigo.titulo}
                    </h1>

                    {/* Autores */}
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

                    {/* Imagem Destaque e Conteúdo */}
                    <div
                        className="prose prose-lg max-w-none mt-4 mx-auto"
                        style={{
                            margin: '1% auto 2% auto',
                            width: '96%' // 100% - 2% margin left - 2% margin right
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

                        {/* Conteúdo (perigoso, mas necessário para HTML) */}
                        <div
                            className="print-content"
                            dangerouslySetInnerHTML={{ __html: artigo.conteudoAtual.content }}
                        />
                    </div>

                    {/* Card do Volume */}
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

                    {/* Botão de Imprimir */}
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

            {/* --- FIM DA ÁREA DE IMPRESSÃO --- */}

            {/* --- SEÇÃO DE COMENTÁRIOS (Sempre oculta no print) --- */}
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
                                    isPublic={false} // Não é público
                                    permitirRespostas={false} // Não pode responder
                                    onCommentDeleted={refetchComments} // Recarrega os dados do artigo
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
                            onCommentPosted={refetchComments} // Recarrega os comentários públicos
                        />
                    </section>
                )}

                {/* Comentários Públicos (se houver) */}
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
                                    onCommentDeleted={refetchComments} // Recarrega os comentários públicos
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