'use client';

import { useState, useEffect } from 'react';
import { useMutation } from '@apollo/client/react';
import {
    ADD_STAFF_COMENTARIO,
    UPDATE_STAFF_COMENTARIO,
    DELETE_STAFF_COMENTARIO
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import toast from 'react-hot-toast';
import { Pencil, Trash2, MessageSquareReply, X, Send } from 'lucide-react';
import { StaffComentario } from '@/types/index';
import { StaffMember } from './StaffCard';
import { formatDateTime } from '@/lib/dateUtils';

interface StaffCommentCardProps {
    comment: StaffComentario;
    historyId: string;
    onClose: () => void;
    onCommentChange: () => void;
    staffList: StaffMember[];
}

export default function StaffCommentCard({
    comment,
    historyId,
    onClose,
    onCommentChange,
    staffList
}: StaffCommentCardProps) {

    const { user } = useAuth();
    const [isStaff, setIsStaff] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [isReplying, setIsReplying] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);
    const [editedContent, setEditedContent] = useState(comment.comment);
    const [replyContent, setReplyContent] = useState('');
    const isAuthor = user?.id === comment.usuarioId;

    useEffect(() => {
        if (typeof window !== 'undefined') {
            setIsStaff(localStorage.getItem('isStaff') === 'true');
        }
    }, [user]);

    const [deleteComment, { loading: loadingDelete }] = useMutation(DELETE_STAFF_COMENTARIO, {
        variables: {
            historyId: historyId,
            comentarioId: comment.id,
        },
        onCompleted: () => {
            toast.success('Comentário de staff deletado.');
            onCommentChange();
            onClose();
        },
        onError: (err) => toast.error(`Erro ao deletar: ${err.message}`)
    });

    const [updateComment, { loading: loadingUpdate }] = useMutation(UPDATE_STAFF_COMENTARIO, {
        onCompleted: () => {
            toast.success('Comentário atualizado.');
            setIsEditing(false);
            onCommentChange();
        },
        onError: (err) => toast.error(`Erro ao atualizar: ${err.message}`)
    });

    const [addComment, { loading: loadingReply }] = useMutation(ADD_STAFF_COMENTARIO, {
        onCompleted: () => {
            toast.success('Resposta enviada.');
            setIsReplying(false);
            setReplyContent('');
            onCommentChange();
        },
        onError: (err) => toast.error(`Erro ao responder: ${err.message}`)
    });

    const handleUpdate = () => {
        if (!editedContent.trim()) return;
        updateComment({
            variables: {
                historyId: historyId,
                comentarioId: comment.id,
                newContent: editedContent,
            },
        });
    };

    const handleDelete = () => {
        if (window.confirm('Tem certeza que deseja deletar este comentário?')) {
            deleteComment();
        }
    };

    const handleReply = () => {
        if (!replyContent.trim()) return;
        addComment({
            variables: {
                historyId: historyId,
                comment: replyContent,
                parent: comment.id,
            },
        });
    };

    const author = staffList.find(s => s.usuarioId === comment.usuarioId);
    const authorName = author ? author.nome : `ID: ${comment.usuarioId.substring(0, 5)}...`;

    return (
        <div
            className={`relative bg-white shadow-lg border border-gray-200 rounded-lg ${comment.parent ? 'ml-[2%]' : ''}`}
            style={{
                width: comment.parent ? '98%' : '100%',
                margin: '10px 0',
                padding: '12px 10px',
            }}
        >
            <button
                onClick={onClose}
                className="absolute top-2 right-2 text-gray-400 hover:text-gray-700"
                title="Fechar"
            >
                <X size={16} />
            </button>

            <div className="flex justify-between items-start mb-2">
                <div>
                    <span className="font-semibold text-sm text-gray-800">{authorName}</span>
                    <span className="text-xs text-gray-500 ml-2">{formatDateTime(comment.data)}</span>
                </div>
            </div>

            {isEditing ? (
                <div className="mt-2">
                    <textarea
                        value={editedContent}
                        onChange={(e) => setEditedContent(e.target.value)}
                        className="input-std h-24 text-sm"
                    />
                    <div className="flex justify-end gap-2 mt-2">
                        <button onClick={() => setIsEditing(false)} className="px-2 py-1 text-xs" disabled={loadingUpdate}>Cancelar</button>
                        <button onClick={handleUpdate} className="px-2 py-1 text-xs bg-emerald-600 text-white rounded" disabled={loadingUpdate}>
                            {loadingUpdate ? '...' : 'Salvar'}
                        </button>
                    </div>
                </div>
            ) : (
                <div>
                    <p className={`text-sm text-gray-700 whitespace-pre-wrap ${!isExpanded ? 'line-clamp-3' : ''}`}>
                        {comment.comment}
                    </p>
                    {comment.comment.length > 100 && (
                        <button
                            onClick={() => setIsExpanded(prev => !prev)}
                            className="text-emerald-600 text-xs font-medium hover:underline mt-1"
                        >
                            {isExpanded ? '... Menos' : '... Ler mais'}
                        </button>
                    )}
                </div>
            )}

            {!isEditing && (
                <div className="flex justify-between items-center mt-3 pt-2 border-t border-gray-100">
                    <button
                        onClick={() => setIsReplying(prev => !prev)}
                        className="flex items-center gap-1 text-xs text-gray-600 hover:text-emerald-600 font-medium"
                    >
                        <MessageSquareReply size={14} />
                        Responder
                    </button>

                    {(isAuthor || isStaff) && (
                        <div className="flex gap-2">
                            <button
                                onClick={() => setIsEditing(true)}
                                title="Editar"
                                className="text-gray-500 hover:text-emerald-600 transition"
                            >
                                <Pencil size={14} />
                            </button>
                            <button
                                onClick={handleDelete}
                                title="Deletar"
                                className="text-gray-500 hover:text-red-600 transition"
                                disabled={loadingDelete}
                            >
                                <Trash2 size={14} />
                            </button>
                        </div>
                    )}
                </div>
            )}

            {isReplying && (
                <div className="mt-3 pt-3 border-t border-gray-100">
                    <textarea
                        value={replyContent}
                        onChange={(e) => setReplyContent(e.target.value)}
                        placeholder="Escreva sua resposta..."
                        className="input-std h-20 text-sm"
                    />
                    <div className="flex justify-end gap-2 mt-2">
                        <button onClick={() => setIsReplying(false)} className="px-2 py-1 text-xs" disabled={loadingReply}>Cancelar</button>
                        <button onClick={handleReply} className="px-2 py-1 text-xs bg-emerald-600 text-white rounded" disabled={loadingReply || !replyContent.trim()}>
                            {loadingReply ? '...' : <Send size={14} />}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}