'use client';

import Image from 'next/image';
import { useMutation } from '@apollo/client/react';
import { ATUALIZAR_STAFF, OBTER_STAFF_LIST } from '@/graphql/queries';
import { User, Check, RefreshCcw, ArrowDown, ArrowUp, X } from 'lucide-react';
import { useState } from 'react';
import CommentaryModal from './CommentaryModal';
import toast from 'react-hot-toast';

// Define a estrutura dos dados do staff que esperamos
export interface StaffMember {
    usuarioId: string;
    nome: string;
    url: string;
    job: 'Administrador' | 'EditorChefe' | 'EditorBolsista' | 'Aposentado';
    isActive: boolean;
}

interface StaffCardProps {
    staff: StaffMember;
    onUpdate: () => void; // Função para recarregar a lista
}

type PendingAction = {
    title: string;
    newJob: StaffMember['job'] | null;
    newActiveStatus: boolean | null;
    commentaryPrefix: string;
} | null;

// Mapeia o Enum para o texto em Português
const jobDisplay: Record<StaffMember['job'], string> = {
    Administrador: 'Administrador',
    EditorChefe: 'Editor Chefe',
    EditorBolsista: 'Editor Bolsista',
    Aposentado: 'Aposentado(a)',
};

export default function StaffCard({ staff, onUpdate }: StaffCardProps) {

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [pendingAction, setPendingAction] = useState<PendingAction>(null);

    const [atualizarStaff, { loading }] = useMutation(ATUALIZAR_STAFF, {
        refetchQueries: [{ query: OBTER_STAFF_LIST, variables: { page: 0, pageSize: 50 } }],
        onCompleted: (data) => {
            const action = pendingAction?.newActiveStatus === false ? 'aposentado' : (pendingAction?.newActiveStatus === true ? 'reinstaurado' : 'atualizado');
            toast.success(`Staff ${action} com sucesso!`);
            onUpdate();
            setIsModalOpen(false);
        },
        onError: (err) => {
            toast.error(`Erro ao atualizar staff: ${err.message}`);
            setIsModalOpen(false);
        }
    });

    const handleActionClick = (action: PendingAction) => {
        setPendingAction(action);
        setIsModalOpen(true);
    };

    const handleConfirmAction = (commentary: string) => {
        if (!pendingAction) return;

        const jobPayload = pendingAction.newJob ? pendingAction.newJob : (staff.job !== 'Aposentado' ? staff.job : 'EditorBolsista');

        // (NOVO) Toast de carregamento
        toast.loading(`Executando ação: ${pendingAction.title}...`, { id: 'staff-action' });

        atualizarStaff({
            variables: {
                staffUsuarioId: staff.usuarioId,
                newJob: jobPayload,
                newActiveStatus: pendingAction.newActiveStatus !== null ? pendingAction.newActiveStatus : staff.isActive,
                commentary: `${pendingAction.commentaryPrefix}: ${commentary}`,
            },
        }).finally(() => {
            toast.dismiss('staff-action'); // Limpa o toast de loading
        });
    };

    return (
        <>
            <CommentaryModal
                isOpen={isModalOpen}
                title={pendingAction?.title || "Confirmar Ação"}
                loading={loading}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleConfirmAction}
            />

            <li
                className="w-[90%] mx-auto my-[10px] px-[2%] py-4 bg-white shadow-md rounded-lg flex items-center justify-between"
                style={{ paddingLeft: '2%', paddingRight: '2%' }}
            >
                {/* Bloco de Informações (Esquerda) */}
                <div className="flex flex-col md:flex-row md:items-center md:gap-3">
                    <span className="text-lg font-semibold text-gray-800">{staff.nome}</span>
                    <div className="flex items-center gap-2 text-sm text-gray-500">
                        <span className={`font-medium ${staff.isActive ? 'text-green-600' : 'text-red-600'}`}>
                            {staff.isActive ? 'Em ativa' : 'Aposentado(a)'}
                        </span>
                        <span>|</span>
                        <span>{jobDisplay[staff.job] || staff.job}</span>
                    </div>
                </div>

                {/* Bloco de Ações e Imagem (Direita) */}
                <div className="flex items-center gap-4">
                    <div className="flex flex-col sm:flex-row gap-2">

                        {staff.isActive && (
                            <>
                                {staff.job === 'EditorBolsista' ? (
                                    <button
                                        onClick={() => handleActionClick({
                                            title: `Promover ${staff.nome}`,
                                            newJob: 'EditorChefe',
                                            newActiveStatus: null,
                                            commentaryPrefix: "Promoção para Editor Chefe"
                                        })}
                                        disabled={loading}
                                        className="px-3 py-1 text-xs font-medium bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition disabled:opacity-50"
                                        title="Promover para Editor Chefe"
                                    >
                                        <ArrowUp size={14} className="inline mr-1" />
                                        Promover
                                    </button>
                                ) : staff.job === 'EditorChefe' || staff.job === 'Administrador' ? (
                                    <button
                                        onClick={() => handleActionClick({
                                            title: `Demover ${staff.nome}`,
                                            newJob: 'EditorBolsista',
                                            newActiveStatus: null,
                                            commentaryPrefix: "Remoção para Editor Bolsista"
                                        })}
                                        disabled={loading}
                                        className="px-3 py-1 text-xs font-medium bg-yellow-100 text-yellow-700 rounded hover:bg-yellow-200 transition disabled:opacity-50"
                                        title="Demover para Editor Bolsista"
                                    >
                                        <ArrowDown size={14} className="inline mr-1" />
                                        Demover
                                    </button>
                                ) : null}

                                <button
                                    onClick={() => handleActionClick({
                                        title: `Aposentar ${staff.nome}`,
                                        newJob: null,
                                        newActiveStatus: false,
                                        commentaryPrefix: "Usuário Aposentado"
                                    })}
                                    disabled={loading}
                                    className="px-3 py-1 text-xs font-medium bg-red-100 text-red-700 rounded hover:bg-red-200 transition disabled:opacity-50"
                                    title="Aposentar"
                                >
                                    <X size={14} className="inline mr-1" />
                                    Aposentar
                                </button>
                            </>
                        )}

                        {!staff.isActive && (
                            <button
                                onClick={() => handleActionClick({
                                    title: `Reinstaurar ${staff.nome}`,
                                    newJob: null,
                                    newActiveStatus: true,
                                    commentaryPrefix: "Usuário Reinstaurado"
                                })}
                                disabled={loading}
                                className="px-3 py-1 text-xs font-medium bg-green-100 text-green-700 rounded hover:bg-green-200 transition disabled:opacity-50"
                                title="Reinstaurar"
                            >
                                <RefreshCcw size={14} className="inline mr-1" />
                                Reinstaurar
                            </button>
                        )}
                    </div>

                    <div
                        className="relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0"
                        style={{ width: 60, height: 60 }}
                    >
                        {staff.url ? (
                            <Image
                                src={staff.url}
                                alt={staff.nome}
                                fill
                                className="object-cover"
                            />
                        ) : (
                            <User className="w-full h-full text-gray-400 p-2" />
                        )}
                    </div>
                </div>
            </li>
        </>
    );
}