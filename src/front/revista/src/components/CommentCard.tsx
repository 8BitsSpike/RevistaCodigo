'use client';

import { useState, useEffect, useRef } from 'react';
import { useMutation } from '@apollo/client/react';
import { Pencil, Trash2, MessageSquareReply } from 'lucide-react';
import {
    DELETAR_INTERACAO,
    ATUALIZAR_INTERACAO,
    GET_COMENTARIOS_PUBLICOS,
    GET_ARTIGO_VIEW
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import CreateCommentCard from './CreateCommentCard';

export interface Comment {
    id: string;
    artigoId: string;
    usuarioId: string;
    usuarioNome: string;
    content: string;
    dataCriacao: string;
    parentCommentId: string | null;
    replies: Comment[];
    __typename: string; // Necessário para o Apollo Cache
}

// Define as props que o CommentCard recebe
interface CommentCardProps {
    comment: Comment;
    artigoId: string;
    isPublic: boolean;      // É um comentário público?
    permitirRespostas: boolean; // O artigo permite comentários?
    onCommentDeleted: () => void; // Função para recarregar a lista
}

export default function CommentCard({
    comment,
    artigoId,
    isPublic,
    permitirRespostas,
    onCommentDeleted
}: CommentCardProps) {

    const { user } = useAuth();
    const [isStaff, setIsStaff] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [isReplying, setIsReplying] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);
    const [editedContent, setEditedContent] = useState(comment.content);

    // Verifica se o usuário logado é o autor deste comentário
    const isAuthor = user?.id === comment.usuarioId;

    useEffect(() => {
        if (typeof window !== 'undefined') {
            setIsStaff(localStorage.getItem('isStaff') === 'true');
        }
    }, [user]);

    const MAX_LINES = 3;
    const contentRef = useRef<HTMLParagraphElement>(null);
    const [isTruncated, setIsTruncated] = useState(false);

    useEffect(() => {
        if (contentRef.current) {
            const maxHeight = 1.5 * 16 * MAX_LINES;
            if (contentRef.current.scrollHeight > maxHeight) {
                setIsTruncated(true);
            }
        }
    }, [comment.content]);

    // --- Mutações ---

    // Mutação para Deletar
    const [deleteInteraction, { loading: loadingDelete }] = useMutation(DELETAR_INTERACAO, {
        variables: {
            interacaoId: comment.id,
            commentary: "Excluído pelo usuário"
        },
        onCompleted: onCommentDeleted, // Chama a função para recarregar
        onError: (err) => console.error("Erro ao deletar:", err.message),
    });

    // Mutação para Atualizar
    const [updateInteraction, { loading: loadingUpdate }] = useMutation(ATUALIZAR_INTERACAO, {
        onCompleted: () => setIsEditing(false), // Fecha o modo de edição
        onError: (err) => console.error("Erro ao atualizar:", err.message),
    });

    // --- Handlers ---

    const handleUpdate = () => {
        if (!editedContent.trim()) return;
        updateInteraction({
            variables: {
                interacaoId: comment.id,
                newContent: editedContent,
                commentary: "Editado pelo usuário",
            },
        });
    };

    // Formata a data (ex: 13 de novembro de 2025)
    const formattedDate = new Date(comment.dataCriacao).toLocaleDateString('pt-BR', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
    });

    return (
        <div
            className={`bg-white shadow border border-gray-100 rounded-lg ${comment.parentCommentId ? 'ml-[2%]' : ''}`}
            style={{
                width: comment.parentCommentId ? '98%' : '100%',
                margin: '0.5% 1%',
                padding: '20px 0.5%',
            }}
        >
            {/* Card Principal */}
            <div className="px-4">
                {/* Header do Card: Nome, Data e Botões de Ação */}
                <div className="flex justify-between items-start mb-2">
                    <div>
                        <span className="font-semibold text-gray-800">{comment.usuarioNome}</span>
                        <span className="text-xs text-gray-500 ml-2">{formattedDate}</span>
                    </div>

                    {/* Botões de Ação (Editar/Deletar) */}
                    {(isAuthor || isStaff) && !isEditing && (
                        <div className="flex gap-2 flex-shrink-0">
                            <button
                                onClick={() => setIsEditing(true)}
                                title="Editar"
                                className="text-gray-500 hover:text-emerald-600 transition"
                                disabled={loadingDelete || loadingUpdate}
                            >
                                <Pencil size={16} />
                            </button>
                            <button
                                onClick={() => deleteInteraction()}
                                title="Deletar"
                                className="text-gray-500 hover:text-red-600 transition"
                                disabled={loadingDelete || loadingUpdate}
                            >
                                {loadingDelete ? '...' : <Trash2 size={16} />}
                            </button>
                        </div>
                    )}
                </div>

                {/* Corpo do Card: Conteúdo ou Textarea de Edição */}
                {isEditing ? (
                    <div className="mt-2">
                        <textarea
                            placeholder='Escreva seu comentário'
                            value={editedContent}
                            onChange={(e) => setEditedContent(e.target.value)}
                            className="w-full h-24 p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500"
                        />
                        <div className="flex justify-end gap-3 mt-2">
                            <button
                                onClick={() => setIsEditing(false)}
                                className="px-3 py-1 rounded-md text-sm text-gray-700 bg-gray-200 hover:bg-gray-300"
                                disabled={loadingUpdate}
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={handleUpdate}
                                className="px-3 py-1 rounded-md text-sm bg-emerald-600 text-white hover:bg-emerald-700"
                                disabled={loadingUpdate}
                            >
                                {loadingUpdate ? 'Salvando...' : 'Salvar'}
                            </button>
                        </div>
                    </div>
                ) : (
                    // Modo de Leitura
                    <div>
                        <p
                            ref={contentRef}
                            className={`text-gray-700 whitespace-pre-wrap ${!isExpanded ? 'line-clamp-3' : ''}`}
                            style={{ lineHeight: '1.5rem', fontSize: '16px' }}
                        >
                            {comment.content}
                        </p>
                        {/* Botão "Ler mais" */}
                        {isTruncated && (
                            <button
                                onClick={() => setIsExpanded(prev => !prev)}
                                className="text-emerald-600 text-sm font-medium hover:underline mt-1"
                            >
                                {isExpanded ? '... Menos' : '... Ler mais'}
                            </button>
                        )}
                    </div>
                )}

                {/* Footer do Card: Botão Responder */}
                {isPublic && permitirRespostas && !isEditing && (
                    <div className="flex justify-end mt-3">
                        <button
                            onClick={() => setIsReplying(prev => !prev)}
                            className="flex items-center gap-1 text-sm text-gray-600 hover:text-emerald-600 font-medium"
                        >
                            <MessageSquareReply size={16} />
                            Responder
                        </button>
                    </div>
                )}
            </div>

            {/* Seção de Resposta (CreateCommentCard) */}
            {isReplying && (
                <div className="mt-4 px-2">
                    <CreateCommentCard
                        artigoId={artigoId}
                        parentCommentId={comment.id} // Envia o ID deste comentário como 'pai'
                        onCommentPosted={() => {
                            setIsReplying(false);   // Fecha o formulário
                            onCommentDeleted();     // Recarrega a lista (o onCommentDeleted recarrega tudo)
                        }}
                        onCancel={() => setIsReplying(false)} // Botão Cancelar
                    />
                </div>
            )}

            {/* Seção de Respostas (Recursiva) */}
            {comment.replies && comment.replies.length > 0 && (
                <div className="mt-2 border-t border-gray-100 pt-2">
                    {comment.replies.map(reply => (
                        <CommentCard
                            key={reply.id}
                            comment={reply}
                            artigoId={artigoId}
                            isPublic={isPublic}
                            permitirRespostas={permitirRespostas}
                            onCommentDeleted={onCommentDeleted}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}