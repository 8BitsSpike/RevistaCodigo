'use client';

import { useState } from 'react';
import { useMutation } from '@apollo/client/react';
import {
    CRIAR_COMENTARIO_PUBLICO,
    GET_COMENTARIOS_PUBLICOS,
    GET_ARTIGO_VIEW
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import toast from 'react-hot-toast'; // (NOVO) Importa o toast

// Tipos das props que o componente aceita
interface CreateCommentCardProps {
    artigoId: string;
    parentCommentId?: string | null;
    onCommentPosted: () => void;
    onCancel?: () => void;
}

export default function CreateCommentCard({
    artigoId,
    parentCommentId = null,
    onCommentPosted,
    onCancel,
}: CreateCommentCardProps) {

    const { user } = useAuth();
    const [content, setContent] = useState('');

    const [submitComment, { loading, error }] = useMutation(
        CRIAR_COMENTARIO_PUBLICO,
        {
            // (NOVO) Adiciona handlers de onCompleted e onError
            onCompleted: () => {
                toast.success('Comentário enviado com sucesso!');
                setContent('');
                onCommentPosted();
            },
            onError: (err) => {
                console.error("Erro ao enviar comentário:", err);
                toast.error(`Erro ao enviar comentário: ${err.message}`);
            },
            // Atualiza o cache do Apollo
            refetchQueries: [
                {
                    query: GET_COMENTARIOS_PUBLICOS,
                    variables: {
                        artigoId: artigoId,
                        page: 0,
                        pageSize: 20
                    },
                },
                {
                    query: GET_ARTIGO_VIEW,
                    variables: { artigoId: artigoId }
                }
            ]
        }
    );

    const usuarioNome = user?.name || "Leitor Anônimo";

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!content.trim() || !user) return;

        // (MODIFICADO) A lógica de try/catch foi movida para os handlers do useMutation
        await submitComment({
            variables: {
                artigoId,
                content,
                usuarioNome: usuarioNome,
                parentCommentId,
            },
        });
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
                placeholder="Escreva seu comentário..."
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
            {/* O erro agora é tratado pelo toast, mas podemos manter isso se quisermos */}
            {error && <p className="text-red-600 text-sm mt-2">Erro: {error.message}</p>}
        </form>
    );
}

export type CreateCommentType = typeof CreateCommentCard;