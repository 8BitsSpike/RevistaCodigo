'use client';

import { useState, useEffect, useRef } from 'react';
import { useMutation } from '@apollo/client/react';
import { Pencil, Trash2, MessageSquareReply } from 'lucide-react';
import { DELETAR_INTERACAO, ATUALIZAR_INTERACAO } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import CreateCommentCard from './CreateCommentCard';
import toast from 'react-hot-toast';
import { formatDate } from '@/lib/dateUtils';

export interface Comment {
    id: string;
    artigoId: string;
    usuarioId: string;
    usuarioNome: string;
    content: string;
    dataCriacao: string;
    parentCommentId: string | null;
    replies: Comment[];
    __typename: string;
}

interface CommentCardProps {
    comment: Comment;
    artigoId: string;
    isPublic: boolean;
    permitirRespostas: boolean;
    onCommentAction: () => void;
}

export default function CommentCard({
    comment,
    artigoId,
    isPublic,
    permitirRespostas,
    onCommentAction
}: CommentCardProps) {

    const { user } = useAuth();
    const [isStaff, setIsStaff] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [isReplying, setIsReplying] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);
    const [editedContent, setEditedContent] = useState(comment.content);

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
            const lineHeight = parseFloat(window.getComputedStyle(contentRef.current).lineHeight);
            const maxHeight = lineHeight * MAX_LINES;
            if (contentRef.current.scrollHeight > maxHeight) {
                setIsTruncated(true);
            }
        }
    }, [comment.content]);

    const [deleteInteraction, { loading: loadingDelete }] = useMutation(DELETAR_INTERACAO, {
        variables: {
            interacaoId: comment.id,
            commentary: "Excluído pelo usuário"
        },
        onCompleted: () => {
            toast.success('Comentário deletado.');
            onCommentAction();
        },
        onError: (err) => {
            console.error("Erro ao deletar:", err.message);
            toast.error(`Erro ao deletar: ${err.message}`);
        },
    });

    const [updateInteraction, { loading: loadingUpdate }] = useMutation(ATUALIZAR_INTERACAO, {
        onCompleted: () => {
            toast.success('Comentário atualizado.');
            setIsEditing(false);
            onCommentAction();
        },
        onError: (err) => {
            console.error("Erro ao atualizar:", err.message);
            toast.error(`Erro ao atualizar: ${err.message}`);
        },
    });

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

    return (
        <div
            className={`card-std ${comment.parentCommentId ? 'ml-[2%]' : ''}`}
            style={{
                width: comment.parentCommentId ? '98%' : '100%',
                margin: '10px 1%',
                padding: '20px 0.5%',
            }}
        >
            <div className="px-4">
                <div className="flex justify-between items-start mb-2">
                    <div>
                        <span className="font-semibold text-gray-800">{comment.usuarioNome}</span>
                        <span className="text-xs text-gray-500 ml-2">{formatDate(comment.dataCriacao)}</span>
                    </div>

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

                {isEditing ? (
                    <div className="mt-2">
                        <textarea
                            value={editedContent}
                            onChange={(e) => setEditedContent(e.target.value)}
                            className="input-std h-24"
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
                    <div>
                        <p
                            ref={contentRef}
                            className={`text-gray-700 whitespace-pre-wrap ${!isExpanded ? 'line-clamp-3' : ''}`}
                            style={{ lineHeight: '1.5rem', fontSize: '16px' }}
                        >
                            {comment.content}
                        </p>
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

            {isReplying && (
                <div className="mt-4 px-2">
                    <CreateCommentCard
                        artigoId={artigoId}
                        parentCommentId={comment.id}
                        onCommentPosted={() => {
                            setIsReplying(false);
                            onCommentAction();
                        }}
                        onCancel={() => setIsReplying(false)}
                    />
                </div>
            )}

            {comment.replies && comment.replies.length > 0 && (
                <div className="mt-0 border-t border-gray-100 pt-2">
                    {comment.replies.map(reply => (
                        <CommentCard
                            key={reply.id}
                            comment={reply}
                            artigoId={artigoId}
                            isPublic={isPublic}
                            permitirRespostas={permitirRespostas}
                            onCommentAction={onCommentAction}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}