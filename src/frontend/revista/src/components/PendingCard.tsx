'use client';
import { useState, useRef } from 'react';
import { useMutation } from '@apollo/client/react';
import { RESOLVER_REQUISICAO_PENDENTE, OBTER_PENDENTES } from '@/graphql/queries';
import { StaffMember } from './StaffCard';
import { Check, X, User, ChevronDown } from 'lucide-react';
import Image from 'next/image';
import toast from 'react-hot-toast';
import { formatDayMonth } from '@/lib/dateUtils';
import { StatusPendente } from '@/types/enums';

export interface PendingItem { 
    id: string; 
    targetEntityId: string; 
    targetType: string; 
    status: StatusPendente; 
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
const statusDisplay: Record<string, { text: string; color: string }> = {
    [StatusPendente.AguardandoRevisao]: { text: 'Aguardando revisão', color: 'text-yellow-600' },
    [StatusPendente.Aprovado]: { text: 'Aprovado', color: 'text-green-600' },
    [StatusPendente.Rejeitado]: { text: 'Rejeitado', color: 'text-red-600' },
    [StatusPendente.Arquivado]: { text: 'Arquivado', color: 'text-gray-500' },
    
    // Fallbacks (caso o backend envie string antiga)
    'AguardandoRevisao': { text: 'Aguardando revisão', color: 'text-yellow-600' },
    'Aprovado': { text: 'Aprovado', color: 'text-green-600' },
    'Rejeitado': { text: 'Rejeitado', color: 'text-red-600' },
    'Arquivado': { text: 'Arquivado', color: 'text-gray-500' }
};

const jobDisplay: Record<string, string> = { 
    Administrador: 'Administrador', 
    EditorChefe: 'Editor Chefe', 
    EditorBolsista: 'Editor Bolsista', 
    Aposentado: 'Aposentado(a)' 
};

const StaffPopover = ({ staff }: { staff: StaffMember }) => (
    <div className="absolute top-full left-1/2 -translate-x-1/2 mt-2 w-64 bg-white shadow-xl rounded-lg p-3 z-20 border border-gray-200">
        <div className="flex items-center gap-3">
            <div className="relative h-16 w-16 rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                <Image src={staff.url || '/faviccon.png'} alt={staff.nome} fill className="object-cover" />
            </div>
            <div>
                <p className="font-semibold text-gray-800">{staff.nome}</p>
                <p className="text-sm text-gray-600">{jobDisplay[staff.job] || staff.job}</p>
            </div>
        </div>
    </div>
);

export default function PendingCard({ pending, staffList, onUpdate, isAdmin }: PendingCardProps) {
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
            { query: OBTER_PENDENTES, variables: { pagina: 0, tamanho: 20, status: StatusPendente.AguardandoRevisao } },
            { query: OBTER_PENDENTES, variables: { pagina: 0, tamanho: 20, status: StatusPendente.Aprovado } },
            { query: OBTER_PENDENTES, variables: { pagina: 0, tamanho: 20, status: StatusPendente.Rejeitado } }
        ],
        onCompleted: (data) => { 
            if (data.resolverRequisicaoPendente) toast.success('Pendência resolvida!'); 
            else toast.error('Falha.'); 
            onUpdate(); 
        },
        onError: (err) => toast.error(`Erro: ${err.message}`)
    });

    const handleActionSubmit = () => {
        toast.loading('Processando...', { id: 'pending-action' });
        resolverPendencia({ variables: { pendingId: pending.id, isApproved: selectedAction === 'Aprovar' } })
            .finally(() => toast.dismiss('pending-action'));
    };

    const handleShow = (setter: any, timer: any) => { if (timer.current) clearTimeout(timer.current); setter(true); };
    const handleHide = (setter: any, timer: any) => { timer.current = setTimeout(() => setter(false), 2000); };

    // Proteção contra status desconhecido
    const displayInfo = statusDisplay[pending.status] || { text: pending.status, color: 'text-gray-500' };

    return (
        <li className="card-std p-4" style={{ width: '98%', margin: '10px 1%' }}>
            <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm">
                <span className="font-semibold">ID:</span><span className="text-gray-600 font-mono text-xs">{pending.id}</span>
                <span className="text-gray-300">|</span>
                
                <span className="font-semibold">Condição:</span>
                <span className={`font-medium ${displayInfo.color}`}>
                    {displayInfo.text}
                </span>
                
                <span className="text-gray-300">|</span>
                <span className="font-semibold">Solicitante:</span>
                <div className="relative inline-block" onMouseEnter={() => handleShow(setShowSolicitante, solicitanteTimer)} onMouseLeave={() => handleHide(setShowSolicitante, solicitanteTimer)}>
                    <span className="text-emerald-700 cursor-pointer">{solicitante?.nome || 'Desconhecido'}</span>
                    {showSolicitante && solicitante && <StaffPopover staff={solicitante} />}
                </div>
                <span className="text-gray-300">|</span>
                <span className="font-semibold">Tipo:</span><span className="text-gray-600">{pending.commandType}</span>
                <span className="text-gray-300">|</span>
                <span className="font-semibold">Em um:</span><span className="text-gray-600">{pending.targetType}</span>
                <span className="text-gray-300">|</span>
                <span className="font-semibold">Criada em:</span><div className="text-center text-gray-600"><div>{formatDayMonth(pending.dateRequested)}</div><div className="text-xs">{dataSolicitacao.getFullYear()}</div></div>
            </div>
            <div className="mt-3 pt-3 border-t border-gray-100">
                <span className="font-semibold text-sm">Motivo:</span>
                <p className={`text-gray-700 text-sm mt-1 ${!isExpanded ? 'line-clamp-3' : ''}`}>{pending.commentary}</p>
                {pending.commentary.length > 80 && <button onClick={() => setIsExpanded(prev => !prev)} className="text-emerald-600 text-xs font-medium hover:underline mt-1">{isExpanded ? '... Menos' : '... Ler mais'}</button>}
            </div>
            <div className="mt-4 pt-4 border-t border-gray-100">
                {pending.status === StatusPendente.AguardandoRevisao && isAdmin ? (
                    <div className="flex items-center gap-3">
                        <span className="text-sm font-semibold">Ação:</span>
                        <div className="relative">
                            <select value={selectedAction} onChange={(e) => setSelectedAction(e.target.value as any)} disabled={loading} className="appearance-none border border-gray-300 rounded-md px-3 py-2 bg-white text-sm"><option value="Aprovar">Aprovar</option><option value="Rejeitar">Rejeitar</option></select>
                            <ChevronDown size={16} className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
                        </div>
                        <button onClick={handleActionSubmit} disabled={loading} className="btn-primary"><Check size={16} /></button>
                    </div>
                ) : pending.status !== StatusPendente.AguardandoRevisao ? (
                    <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
                        <span className="font-semibold">Resolvida em:</span>{dataResolucao ? <div className="text-center text-gray-600"><div>{formatDayMonth(pending.dataAprovacao!)}</div><div className="text-xs">{dataResolucao.getFullYear()}</div></div> : <span className="text-gray-500 italic">Data não registrada</span>}
                        <span className="text-gray-300">|</span>
                        <span className="font-semibold">Resolvida por:</span>
                        <div className="relative inline-block" onMouseEnter={() => handleShow(setShowResponsavel, responsavelTimer)} onMouseLeave={() => handleHide(setShowResponsavel, responsavelTimer)}>
                            <span className="text-emerald-700 cursor-pointer">{responsavel?.nome || 'N/A'}</span>
                            {showResponsavel && responsavel && <StaffPopover staff={responsavel} />}
                        </div>
                    </div>
                ) : null}
            </div>
        </li>
    );
}