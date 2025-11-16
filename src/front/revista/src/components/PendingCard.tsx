'use client';

import { useState, useRef } from 'react';
import { useMutation } from '@apollo/client/react';
import { RESOLVER_REQUISICAO_PENDENTE, OBTER_PENDENTES } from '@/graphql/queries';
import { StaffMember } from './StaffCard';
import { Check, X, Clock, User, ChevronDown } from 'lucide-react';
import Image from 'next/image';
import toast from 'react-hot-toast';

// --- Tipos ---

export interface PendingItem {
    id: string;
    targetEntityId: string;
    targetType: string;
    status: 'AguardandoRevisao' | 'Aprovado' | 'Rejeitado' | 'Arquivado';
    dateRequested: string;
    requesterUsuarioId: string;
    commentary: string;
    commandType: string;
    commandParametersJson: string;
    idAprovador?: string | null;
    dataAprovacao?: string | null;
}

interface PendingCardProps {
    pending: PendingItem;
    staffList: StaffMember[];
    onUpdate: () => void;
    isAdmin: boolean;
}

// --- Mapeamento de Status e Helpers ---

const statusDisplay = {
    AguardandoRevisao: { text: 'Aguardando revisão', color: 'text-yellow-600' },
    Aprovado: { text: 'Aprovado', color: 'text-green-600' },
    Rejeitado: { text: 'Rejeitado', color: 'text-red-600' },
    Arquivado: { text: 'Arquivado', color: 'text-gray-500' },
};

const StaffPopover = ({ staff }: { staff: StaffMember }) => (
    <div className="absolute top-full left-1/2 -translate-x-1/2 mt-2 w-64 bg-white shadow-xl rounded-lg p-3 z-20 border border-gray-200">
        <div className="flex items-center gap-3">
            <div className="relative h-16 w-16 rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                {staff.url ? (
                    <Image src={staff.url} alt={staff.nome} fill className="object-cover" />
                ) : (
                    <User className="w-full h-full text-gray-400 p-3" />
                )}
            </div>
            <div>
                <p className="font-semibold text-gray-800">{staff.nome}</p>
                <p className="text-sm text-gray-600">{jobDisplay[staff.job] || staff.job}</p>
            </div>
        </div>
    </div>
);

const jobDisplay: Record<string, string> = {
    Administrador: 'Administrador',
    EditorChefe: 'Editor Chefe',
    EditorBolsista: 'Editor Bolsista',
    Aposentado: 'Aposentado(a)',
};


// --- Componente Principal ---

export default function PendingCard({
    pending,
    staffList,
    onUpdate,
    isAdmin
}: PendingCardProps) {

    const [isExpanded, setIsExpanded] = useState(false);
    const [showSolicitante, setShowSolicitante] = useState(false);
    const [showResponsavel, setShowResponsavel] = useState(false);
    const [selectedAction, setSelectedAction] = useState<'Aprovar' | 'Rejeitar'>('Aprovar');

    const solicitanteTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
    const responsavelTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

    const solicitante = staffList.find(s => s.usuarioId === pending.requesterUsuarioId);
    const responsavel = staffList.find(s => s.usuarioId === pending.idAprovador);

    const dataSolicitacao = new Date(pending.dateRequested);
    const dataResolucao = pending.dataAprovacao ? new Date(pending.dataAprovacao) : null;

    const [resolverPendencia, { loading }] = useMutation(RESOLVER_REQUISICAO_PENDENTE, {
        refetchQueries: [
            {
                query: OBTER_PENDENTES,
                variables: { pagina: 0, tamanho: 20, status: 'AguardandoRevisao' }
            },
            {
                query: OBTER_PENDENTES,
                variables: { pagina: 0, tamanho: 20, status: 'Aprovado' }
            },
            {
                query: OBTER_PENDENTES,
                variables: { pagina: 0, tamanho: 20, status: 'Rejeitado' }
            },
        ],
        onCompleted: (data) => {
            // O 'data.resolverRequisicaoPendente' deve ser 'true'
            if (data.resolverRequisicaoPendente) {
                toast.success(`Pendência ${selectedAction === 'Aprovar' ? 'aprovada' : 'rejeitada'}!`);
            } else {
                toast.error('Falha ao resolver pendência.');
            }
            onUpdate();
        },
        onError: (err) => {
            toast.error(`Erro ao resolver pendência: ${err.message}`);
        }
    });

    const handleActionSubmit = () => {
        const isApproved = selectedAction === 'Aprovar';
        toast.loading(`Processando ${selectedAction}...`, { id: 'pending-action' });

        resolverPendencia({
            variables: {
                pendingId: pending.id,
                isApproved: isApproved,
            }
        }).finally(() => {
            toast.dismiss('pending-action');
        });
    };

    const handleShow = (setter: React.Dispatch<React.SetStateAction<boolean>>, timer: React.MutableRefObject<any>) => {
        if (timer.current) clearTimeout(timer.current);
        setter(true);
    };
    const handleHide = (setter: React.Dispatch<React.SetStateAction<boolean>>, timer: React.MutableRefObject<any>) => {
        timer.current = setTimeout(() => setter(false), 2000);
    };

    return (
        <li
            className="bg-white shadow border border-gray-100 rounded-lg"
            style={{
                width: '98%',
                margin: '10px 1%',
                padding: '1% 0.5%'
            }}
        >
            <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm">
                {/* ... (ID, Condição, etc) ... */}
                <span className="font-semibold">ID:</span>
                <span className="text-gray-600 font-mono text-xs">{pending.id}</span>
                <span className="text-gray-300">|</span>

                <span className="font-semibold">Condição:</span>
                <span className={`font-medium ${statusDisplay[pending.status].color}`}>
                    {statusDisplay[pending.status].text}
                </span>
                <span className="text-gray-300">|</span>

                <span className="font-semibold">Solicitante:</span>
                <div
                    className="relative inline-block"
                    onMouseEnter={() => handleShow(setShowSolicitante, solicitanteTimer)}
                    onMouseLeave={() => handleHide(setShowSolicitante, solicitanteTimer)}
                >
                    <span className="text-emerald-700 cursor-pointer">{solicitante?.nome || 'Desconhecido'}</span>
                    {showSolicitante && solicitante && <StaffPopover staff={solicitante} />}
                </div>
                <span className="text-gray-300">|</span>

                <span className="font-semibold">Tipo:</span>
                <span className="text-gray-600">{pending.commandType}</span>
                <span className="text-gray-300">|</span>

                <span className="font-semibold">Em um:</span>
                <span className="text-gray-600">{pending.targetType}</span>
                <span className="text-gray-300">|</span>

                <span className="font-semibold">Criada em:</span>
                <div className="text-center text-gray-600">
                    <div>{dataSolicitacao.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}</div>
                    <div className="text-xs">{dataSolicitacao.getFullYear()}</div>
                </div>
            </div>

            {/* Comentário (Justificativa) */}
            <div className="mt-3 pt-3 border-t border-gray-100">
                <span className="font-semibold text-sm">Motivo:</span>
                <p className={`text-gray-700 text-sm mt-1 ${!isExpanded ? 'line-clamp-3' : ''}`}>
                    {pending.commentary}
                </p>
                {pending.commentary.length > 80 && (
                    <button
                        onClick={() => setIsExpanded(prev => !prev)}
                        className="text-emerald-600 text-xs font-medium hover:underline mt-1"
                    >
                        {isExpanded ? '... Menos' : '... Ler mais'}
                    </button>
                )}
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100">
                {pending.status === 'AguardandoRevisao' && isAdmin ? (
                    <div className="flex items-center gap-3">
                        <span className="text-sm font-semibold">Ação:</span>
                        <div className="relative">
                            <select
                                value={selectedAction}
                                onChange={(e) => setSelectedAction(e.target.value as any)}
                                disabled={loading}
                                className="appearance-none border border-gray-300 rounded-md px-3 py-2 bg-white text-sm"
                            >
                                <option value="Aprovar">Aprovar</option>
                                <option value="Rejeitar">Rejeitar</option>
                            </select>
                            <ChevronDown size={16} className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
                        </div>
                        <button
                            onClick={handleActionSubmit}
                            disabled={loading}
                            className="px-3 py-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700 transition disabled:bg-gray-400"
                            aria-label="Confirmar Ação"
                        >
                            <Check size={16} />
                        </button>
                        {loading && <span className="text-sm text-gray-500">Processando...</span>}
                    </div>
                ) : pending.status !== 'AguardandoRevisao' ? (
                    <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
                        <span className="font-semibold">Resolvida em:</span>
                        {dataResolucao ? (
                            <div className="text-center text-gray-600">
                                <div>{dataResolucao.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}</div>
                                <div className="text-xs">{dataResolucao.getFullYear()}</div>
                            </div>
                        ) : (
                            <span className="text-gray-500 italic">Data não registrada</span>
                        )}
                        <span className="text-gray-300">|</span>

                        <span className="font-semibold">Resolvida por:</span>
                        <div
                            className="relative inline-block"
                            onMouseEnter={() => handleShow(setShowResponsavel, responsavelTimer)}
                            onMouseLeave={() => handleHide(setShowResponsavel, responsavelTimer)}
                        >
                            <span className="text-emerald-700 cursor-pointer">{responsavel?.nome || 'N/A'}</span>
                            {showResponsavel && responsavel && <StaffPopover staff={responsavel} />}
                        </div>
                    </div>
                ) : null}
            </div>
        </li>
    );
}