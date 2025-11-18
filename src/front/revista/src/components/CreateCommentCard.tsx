'use client';

import { useState } from 'react';
import { useMutation } from '@apollo/client/react';
import {
    CRIAR_COMENTARIO_PUBLICO,
    GET_COMENTARIOS_PUBLICOS,
    GET_ARTIGO_VIEW,
    CRIAR_COMENTARIO_EDITORIAL,
    OBTER_ARTIGO_EDITORIAL_VIEW
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import toast from 'react-hot-toast';

// Tipos das props que o componente aceita
interface CreateCommentCardProps {
    artigoId: string;
    parentCommentId?: string | null;
    onCommentPosted: () => void;
    onCancel?: () => void;
    isEditorial?: boolean;
}

export default function CreateCommentCard({
    artigoId,
    parentCommentId = null,
    onCommentPosted,
    onCancel,
    isEditorial = false,
}: CreateCommentCardProps) {

    const { user } = useAuth();
    const [content, setContent] = useState('');

    // --- Handlers Comuns ---
    const handleCompleted = (type: string) => {
        toast.success(`Comentário ${type} enviado com sucesso!`);
        setContent('');
        onCommentPosted();
    };

    const handleError = (type: string, err: Error) => {
        console.error(`Erro ao enviar comentário ${type}:`, err);
        toast.error(`Erro ao enviar comentário: ${err.message}`);
    };

    // --- Mutação Comentário Público ---
    const [submitPublicComment, { loading: loadingPublic }] = useMutation(
        CRIAR_COMENTARIO_PUBLICO,
        {
            onCompleted: () => handleCompleted('público'),
            onError: (err) => handleError('público', err),
            refetchQueries: [
                {
                    query: GET_COMENTARIOS_PUBLICOS,
                    variables: {
                        artigoId: artigoId,
                        page: 0,
                        pageSize: 20
                    },
                },
                // Atualiza a contagem de comentários na ArtigoView (pública)
                {
                    query: GET_ARTIGO_VIEW,
                    variables: { artigoId: artigoId }
                }
            ]
        }
    );

    // --- Mutação Comentário Editorial ---
    const [submitEditorialComment, { loading: loadingEditorial }] = useMutation(
        CRIAR_COMENTARIO_EDITORIAL,
        {
            onCompleted: () => handleCompleted('editorial'),
            onError: (err) => handleError('editorial', err),
            // Atualiza a lista de comentários na ArtigoEditorialView (de staff)
            refetchQueries: [
                {
                    query: OBTER_ARTIGO_EDITORIAL_VIEW,
                    variables: { artigoId: artigoId }
                }
            ]
        }
    );

    const loading = loadingPublic || loadingEditorial; // Combina o estado de loading
    const usuarioNome = localStorage.userName || "Usuário";

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!content.trim() || !user) return;
        if (isEditorial) {
            // --- Chama a Mutação Editorial ---
            await submitEditorialComment({
                variables: {
                    artigoId,
                    content,
                    usuarioNome: usuarioNome,
                },
            });
        } else {
            // --- Chama a Mutação Pública ---
            await submitPublicComment({
                variables: {
                    artigoId,
                    content,
                    usuarioNome: usuarioNome,
                    parentCommentId,
                },
            });
        }
    };

    return (
        <form
            onSubmit={handleSubmit}
            className={`bg-gray-50 border border-gray-200 rounded-lg p-4 ${parentCommentId ? 'ml-[2%]' : ''}`}
            style={{
                paddingTop: '20px',
                paddingBottom: '20px',
                paddingLeft: '0.5%',
                paddingRight: '0.5%',
            }}
        >
            <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder={isEditorial ? "Escreva seu comentário editorial..." : "Escreva seu comentário..."}
                className="w-full h-24 p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500"
                required
            />
            <div className="flex justify-end gap-3 mt-3">
                {onCancel && (
                    <button
                        type="button"
                        onClick={onCancel}
                        className="px-4 py-2 rounded-md text-gray-700 bg-gray-200 hover:bg-gray-300 transition"
                        disabled={loading}
                    >
                        Cancelar
                    </button>
                )}
                <button
                    type="submit"
                    className="px-4 py-2 rounded-md bg-emerald-600 text-white hover:bg-emerald-700 transition disabled:bg-gray-400"
                    disabled={loading || !content.trim()}
                >
                    {loading ? 'Enviando...' : 'Enviar'}
                </button>
            </div>
            {/* O erro agora é tratado pelo toast */}
        </form>
    );
}

export type CreateCommentType = typeof CreateCommentCard;