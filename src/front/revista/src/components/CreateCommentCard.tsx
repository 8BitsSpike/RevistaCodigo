'use client';

import { useState } from 'react';
import { useMutation } from '@apollo/client/react';
import {
    CRIAR_COMENTARIO_PUBLICO,
    GET_COMENTARIOS_PUBLICOS,
    GET_ARTIGO_VIEW
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';


export type CreateCommentCard = {
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
}: CreateCommentType) {

    const { user } = useAuth();
    const [content, setContent] = useState('');

    const [submitComment, { loading, error }] = useMutation(
        CRIAR_COMENTARIO_PUBLICO,
        {
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

    // (Simulação) Precisamos pegar o nome de usuário. O hook useAuth não o provê.
    // Vamos usar um placeholder por agora, mas o ideal é que o hook useAuth
    // buscasse o nome do usuário da UsuarioAPI e o armazenasse.
    // TODO: Substituir "Nome do Usuário" pelo nome real vindo do hook useAuth/localStorage
    const usuarioNome = "Nome do Usuário Logado";

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!content.trim() || !user) return;

        try {
            await submitComment({
                variables: {
                    artigoId,
                    content,
                    usuarioNome: usuarioNome,
                    parentCommentId,
                },
            });
            setContent('');
            onCommentPosted();
        } catch (err) {
            console.error("Erro ao enviar comentário:", err);
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
                placeholder="Escreva seu comentário..."
                className="w-full h-24 p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500"
                required
            />
            <div className="flex justify-end gap-3 mt-3">
                {/* O botão 'Cancelar' só aparece se a prop onCancel for passada (ex: em uma resposta) */}
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
            {error && <p className="text-red-600 text-sm mt-2">Erro: {error.message}</p>}
        </form>
    );
}

export type CreateCommentType = typeof CreateCommentCard;