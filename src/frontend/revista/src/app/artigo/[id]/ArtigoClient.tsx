'use client';

import { useState, useEffect } from 'react';
import { useQuery } from '@apollo/client/react';
import { useParams } from 'next/navigation';
import { GET_ARTIGO_VIEW, GET_COMENTARIOS_PUBLICOS } from '@/graphql/queries';
import Layout from '@/components/Layout';
import AuthorCard from '@/components/AuthorCard';
import CommentCard, { Comment } from '@/components/CommentCard';
import CreateCommentCard from '@/components/CreateCommentCard';
import { Printer, MessageSquare, Layers } from 'lucide-react';
import Image from 'next/image';
import Link from 'next/link';
import useAuth from '@/hooks/useAuth';
import toast from 'react-hot-toast';

// --- Utility Function to clean route parameters ---
const sanitizeId = (id: string) => {
    if (!id) return '';
    // 1. Decode URL encoding
    let cleaned = decodeURIComponent(id);
    // 2. Remove URL parameter prefixes (e.g., "id=")
    if (cleaned.toLowerCase().startsWith('id=')) {
        cleaned = cleaned.substring(cleaned.indexOf('=') + 1);
    }
    // 3. Remove leading/trailing quotes/whitespace
    return cleaned.trim().replace(/^['"]|['"]$/g, '');
};



const COMENTARIOS_PAGE_SIZE = 10;

// DTO Interface updated after removing totalComentarios and midiaDestaque
interface ArtigoView { id: string; titulo: string; tipo: string; permitirComentario: boolean; totalComentarios: number; conteudoAtual: { content: string; midias: { url: string; textoAlternativo: string; }[]; }; autores: { usuarioId: string; nome: string; url: string; }[]; volume?: { id: string; volumeTitulo: string; volumeResumo: string; }; interacoes: { comentariosEditoriais: Comment[]; totalComentariosPublicos: number; }; }
interface ArtigoViewQueryData { obterArtigoView: ArtigoView; }
interface ComentariosPublicosQueryData { obterComentariosPublicos: Comment[]; }

export default function ArtigoClient() {
    const params = useParams();
    const rawArtigoId = params.id as string; const artigoId = sanitizeId(rawArtigoId);
    const { user } = useAuth();
    const { data: artigoData, loading: loadingArtigo, error: errorArtigo, refetch: refetchArtigoView } = useQuery<ArtigoViewQueryData>(GET_ARTIGO_VIEW, { variables: { artigoId }, skip: !artigoId, onError: (err) => toast.error(`Erro: ${err.message}`) });
    const artigo = artigoData?.obterArtigoView;
    const { data: comentariosData, loading: loadingComentarios, error: errorComentarios, fetchMore, refetch: refetchComentarios } = useQuery<ComentariosPublicosQueryData>(GET_COMENTARIOS_PUBLICOS, { variables: { artigoId, page: 0, pageSize: 20 }, skip: !artigoId, onError: (err) => toast.error(`Erro: ${err.message}`) });

    const comentariosPublicos = comentariosData?.obterComentariosPublicos ?? [];
    const comentariosEditoriais = artigo?.interacoes.comentariosEditoriais ?? [];

    const handleLoadMoreComments = () => {
        if (!comentariosData) return;
        toast.loading('Carregando mais...', { id: 'load-comments' });
        fetchMore({
            variables: { page: Math.ceil(comentariosPublicos.length / COMENTARIOS_PAGE_SIZE), pageSize: COMENTARIOS_PAGE_SIZE },
            updateQuery: (prev, { fetchMoreResult }) => {
                toast.dismiss('load-comments');
                if (!fetchMoreResult || fetchMoreResult.obterComentariosPublicos.length === 0) { toast.success('Fim dos comentários.'); return prev; }
                return { obterComentariosPublicos: [...prev.obterComentariosPublicos, ...fetchMoreResult.obterComentariosPublicos] };
            },
        }).catch(err => toast.error(`Erro: ${err.message}`));
    };

    if (loadingArtigo) return <Layout><p className="text-center mt-20">Carregando...</p></Layout>;
    if (errorArtigo && !artigo) return <Layout><p className="text-center mt-20 text-red-600">Erro ao carregar.</p></Layout>;
    if (!artigo) return <Layout><p className="text-center mt-20">Artigo não encontrado.</p></Layout>;

    if (artigo.tipo === 'Administrativo') {
        return (
            <Layout>
                <div className="w-[90%] mx-auto my-8">
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-8">{artigo.titulo}</h1>
                    <div className="prose prose-lg max-w-none" dangerouslySetInnerHTML={{ __html: artigo.conteudoAtual.content }} />
                </div>
            </Layout>
        );
    }

    const midiaDestaque = artigo.conteudoAtual.midias[0]; // Usando a primeira mídia do conteúdo como destaque

    const showPrintButton = artigo.tipo === 'Artigo';
    const totalPublicComments = artigo.interacoes.totalComentariosPublicos || 0; // Usando o campo corrigido
    const hasMoreComments = comentariosPublicos.length < totalPublicComments;
    const refetchAll = () => { refetchArtigoView(); refetchComentarios(); };

    return (
        <Layout>
            <div className="print-container">
                <div className="print-hide">
                    {midiaDestaque && (
                        <div className="w-[90%] mx-auto relative h-[400px]">
                            {/* FIX: Image fallback */}
                            <Image src={midiaDestaque.url || '/faviccon.png'} alt={midiaDestaque.textoAlternativo || artigo.titulo} fill className="object-cover rounded-lg shadow-lg" priority />
                        </div>
                    )}
                </div>
                <article className="print-container-content w-[90%] mx-auto">
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mt-8 mb-8 print-title">{artigo.titulo}</h1>
                    <div className="print-authors">
                        {artigo.autores.map(autor => <AuthorCard key={autor.usuarioId} usuarioId={autor.usuarioId} nome={autor.nome} urlFoto={autor.url} />)}
                    </div>
                    <div className="prose prose-lg max-w-none mt-4 mx-auto" style={{ margin: '1% auto 2% auto', width: '96%' }}>
                        {midiaDestaque && (
                            <div className="print-hide w-full flex justify-center my-6">
                                {/* FIX: Image fallback */}
                                <Image src={midiaDestaque.url || '/faviccon.png'} alt={midiaDestaque.textoAlternativo || artigo.titulo} width={800} height={600} className="object-contain rounded-md" />
                            </div>
                        )}
                        <div className="print-content" dangerouslySetInnerHTML={{ __html: artigo.conteudoAtual.content }} />
                    </div>
                    {artigo.volume && (
                        <div className="print-volume w-[90%] my-10 mx-auto p-6 bg-gray-50 rounded-lg shadow-sm border">
                            <h3 className="text-xl font-semibold text-gray-800 flex items-center gap-2"><Layers className="text-emerald-600" /> Publicado em: {artigo.volume.volumeTitulo}</h3>
                            <p className="text-gray-600 mt-2">{artigo.volume.volumeResumo}</p>
                            <Link href={`/volume/${artigo.volume.id}`} className="text-sm text-emerald-600 hover:text-emerald-800 hover:underline font-medium mt-3 inline-block print-hide">Clique aqui para ver o volume completo</Link>
                        </div>
                    )}
                    {showPrintButton && (
                        <div className="w-[90%] mx-auto text-center my-10 print-hide">
                            <button onClick={() => window.print()} className="btn-primary mx-auto"><Printer size={20} /> Imprimir artigo para PDF</button>
                        </div>
                    )}
                </article>
            </div>
            <div className="w-[90%] mx-auto mt-12 print-hide">
                {comentariosEditoriais.length > 0 && (
                    <section className="mb-10">
                        <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2"><MessageSquare className="text-emerald-600" /> Comentários da equipe editorial:</h2>
                        <div className="space-y-4">{comentariosEditoriais.map(comment => <CommentCard key={comment.id} comment={comment} artigoId={artigoId} isPublic={false} permitirRespostas={false} onCommentAction={refetchAll} />)}</div>
                    </section>
                )}
                {artigo.permitirComentario && <section className="mb-10"><CreateCommentCard artigoId={artigoId} onCommentPosted={refetchAll} /></section>}
                {comentariosPublicos.length > 0 && (
                    <section className="mb-10">
                        <h2 className="text-2xl font-semibold mb-6 text-gray-800 flex items-center gap-2 border-b border-gray-200 pb-2"><MessageSquare className="text-gray-700" /> Comentários dos leitores:</h2>
                        <div className="space-y-4">{comentariosPublicos.map(comment => <CommentCard key={comment.id} comment={comment} artigoId={artigoId} isPublic={true} permitirRespostas={artigo.permitirComentario} onCommentAction={refetchAll} />)}</div>
                        {hasMoreComments && <div className="text-center mt-8"><button onClick={handleLoadMoreComments} disabled={loadingComentarios} className="btn-secondary">{loadingComentarios ? 'Carregando...' : 'Carregar mais comentários'}</button></div>}
                    </section>
                )}
            </div>
        </Layout>
    );
}