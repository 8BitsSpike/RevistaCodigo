'use client';
import Image from 'next/image';
import { useMutation } from '@apollo/client/react';
import { ATUALIZAR_STAFF, OBTER_STAFF_LIST } from '@/graphql/queries';
import { User, ArrowUp, ArrowDown, X, RefreshCcw } from 'lucide-react';
import { useState } from 'react';
import CommentaryModal from './CommentaryModal';
import toast from 'react-hot-toast';

export interface StaffMember { usuarioId: string; nome: string; url: string; job: 'Administrador' | 'EditorChefe' | 'EditorBolsista' | 'Aposentado'; isActive: boolean; }
interface StaffCardProps { staff: StaffMember; onUpdate: () => void; }
type PendingAction = { title: string; newJob: StaffMember['job'] | null; newActiveStatus: boolean | null; commentaryPrefix: string; } | null;
const jobDisplay: Record<StaffMember['job'], string> = { Administrador: 'Administrador', EditorChefe: 'Editor Chefe', EditorBolsista: 'Editor Bolsista', Aposentado: 'Aposentado(a)' };

export default function StaffCard({ staff, onUpdate }: StaffCardProps) {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [pendingAction, setPendingAction] = useState<PendingAction>(null);

    const [atualizarStaff, { loading }] = useMutation(ATUALIZAR_STAFF, {
        refetchQueries: [{ query: OBTER_STAFF_LIST, variables: { page: 0, pageSize: 50 } }],
        onCompleted: () => { toast.success('Staff atualizado!'); onUpdate(); setIsModalOpen(false); },
        onError: (err) => { toast.error(`Erro: ${err.message}`); setIsModalOpen(false); }
    });

    const handleActionClick = (action: PendingAction) => { setPendingAction(action); setIsModalOpen(true); };
    const handleConfirmAction = (commentary: string) => {
        if (!pendingAction) return;
        const jobPayload = pendingAction.newJob ? pendingAction.newJob : (staff.job !== 'Aposentado' ? staff.job : 'EditorBolsista');
        toast.loading('Processando...', { id: 'staff-action' });
        atualizarStaff({ variables: { staffUsuarioId: staff.usuarioId, newJob: jobPayload, newActiveStatus: pendingAction.newActiveStatus !== null ? pendingAction.newActiveStatus : staff.isActive, commentary: `${pendingAction.commentaryPrefix}: ${commentary}` } }).finally(() => toast.dismiss('staff-action'));
    };

    return (
        <>
            <CommentaryModal isOpen={isModalOpen} title={pendingAction?.title || "Confirmar"} loading={loading} onClose={() => setIsModalOpen(false)} onSubmit={handleConfirmAction} />
            <li className="card-std w-[90%] mx-auto my-[10px] px-[2%] py-4 flex items-center justify-between">
                <div className="flex flex-col md:flex-row md:items-center md:gap-3">
                    <span className="text-lg font-semibold text-gray-800">{staff.nome}</span>
                    <div className="flex items-center gap-2 text-sm text-gray-500">
                        <span className={`font-medium ${staff.isActive ? 'text-green-600' : 'text-red-600'}`}>{staff.isActive ? 'Em ativa' : 'Aposentado(a)'}</span>
                        <span>|</span>
                        <span>{jobDisplay[staff.job] || staff.job}</span>
                    </div>
                </div>
                <div className="flex items-center gap-4">
                    <div className="flex flex-col sm:flex-row gap-2">
                        {staff.isActive ? (
                            <>
                                {staff.job === 'EditorBolsista' && <button onClick={() => handleActionClick({ title: `Promover ${staff.nome}`, newJob: 'EditorChefe', newActiveStatus: null, commentaryPrefix: "Promoção" })} disabled={loading} className="px-3 py-1 text-xs font-medium bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition disabled:opacity-50" title="Promover"><ArrowUp size={14} className="inline mr-1" />Promover</button>}
                                {(staff.job === 'EditorChefe' || staff.job === 'Administrador') && <button onClick={() => handleActionClick({ title: `Demover ${staff.nome}`, newJob: 'EditorBolsista', newActiveStatus: null, commentaryPrefix: "Remoção" })} disabled={loading} className="px-3 py-1 text-xs font-medium bg-yellow-100 text-yellow-700 rounded hover:bg-yellow-200 transition disabled:opacity-50" title="Demover"><ArrowDown size={14} className="inline mr-1" />Demover</button>}
                                <button onClick={() => handleActionClick({ title: `Aposentar ${staff.nome}`, newJob: null, newActiveStatus: false, commentaryPrefix: "Aposentado" })} disabled={loading} className="px-3 py-1 text-xs font-medium bg-red-100 text-red-700 rounded hover:bg-red-200 transition disabled:opacity-50" title="Aposentar"><X size={14} className="inline mr-1" />Aposentar</button>
                            </>
                        ) : (
                            <button onClick={() => handleActionClick({ title: `Reinstaurar ${staff.nome}`, newJob: null, newActiveStatus: true, commentaryPrefix: "Reinstaurado" })} disabled={loading} className="px-3 py-1 text-xs font-medium bg-green-100 text-green-700 rounded hover:bg-green-200 transition disabled:opacity-50" title="Reinstaurar"><RefreshCcw size={14} className="inline mr-1" />Reinstaurar</button>
                        )}
                    </div>
                    <div className="relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0" style={{ width: 60, height: 60 }}>
                        {/* FIX: Image fallback */}
                        <Image src={staff.url || '/faviccon.png'} alt={staff.nome} fill className="object-cover" />
                    </div>
                </div>
            </li>
        </>
    );
}