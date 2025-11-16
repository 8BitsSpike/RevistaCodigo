'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useMutation } from '@apollo/client/react';
import { ATUALIZAR_METADADOS_ARTIGO } from '@/graphql/queries';
import { useRouter } from 'next/navigation';
import { Send, Eye, Edit } from 'lucide-react';
import CommentaryModal from './CommentaryModal';
import { StatusArtigo, TipoArtigo } from '@/types/enums';
import toast from 'react-hot-toast';

// Interface para os dados do artigo que este card espera
export interface ArtigoEditorial {
    id: string;
    titulo: string;
    resumo: string;
    status: StatusArtigo;
    tipo: TipoArtigo;
    permitirComentario: boolean;
}

interface ArticleEditorialCardProps {
    artigo: ArtigoEditorial;
    onUpdate: () => void;
}

// Opções do dropdown de Status (baseado no Enum)
const statusOptions = Object.values(StatusArtigo).map(status => ({
    label: status.replace(/([A-Z])/g, ' $1').trim(), // Adiciona espaço (ex: EmRevisao -> Em Revisao)
    value: status,
}));


export default function ArticleEditorialCard({ artigo, onUpdate }: ArticleEditorialCardProps) {
    const router = useRouter();

    const [selectedStatus, setSelectedStatus] = useState<StatusArtigo>(artigo.status);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);
    const [atualizarMetadados, { loading }] = useMutation(
        ATUALIZAR_METADADOS_ARTIGO,
        {
            onCompleted: () => {
                toast.success('Status do artigo atualizado!');
                setIsModalOpen(false);
                onUpdate();
            },
            onError: (err) => {
                toast.error(`Erro ao atualizar: ${err.message}`);
                setIsModalOpen(false);
            }
        }
    );

    const handleStatusSubmit = () => {
        if (selectedStatus === artigo.status) {
            return;
        }
        setIsModalOpen(true);
    };

    const handleConfirmStatusChange = (commentary: string) => {
        toast.loading('Atualizando status...', { id: 'status-update' });

        atualizarMetadados({
            variables: {
                id: artigo.id,
                input: {
                    status: selectedStatus,
                    // Garante que outros campos 'nullable' sejam enviados como null
                    titulo: null,
                    resumo: null,
                    tipo: null,
                    idsAutor: null,
                    referenciasAutor: null,
                    permitirComentario: null
                },
                commentary: commentary,
            },
        }).finally(() => {
            toast.dismiss('status-update'); // Limpa o toast de loading
        });
    };

    return (
        <>
            <CommentaryModal
                isOpen={isModalOpen}
                title={`Alterar Status: ${artigo.titulo.substring(0, 20)}...`}
                loading={loading}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleConfirmStatusChange}
            />

            <li
                className="bg-white shadow border border-gray-100 rounded-lg flex flex-col md:flex-row md:items-center md:justify-between"
                style={{
                    width: '98%',
                    margin: '10px 1%',
                    padding: '1% 0.5%'
                }}
            >
                {/* Lado Esquerdo: Título e Resumo */}
                <div className="flex-1 p-3 min-w-0">
                    <div className="flex items-center gap-2">
                        <span className="font-semibold text-sm">Título:</span>
                        <strong className="text-gray-800 truncate">{artigo.titulo}</strong>
                    </div>

                    <div className="mt-2">
                        <span className="font-semibold text-sm">Resumo:</span>
                        <p className={`text-gray-600 text-sm mt-1 ${!isExpanded ? 'line-clamp-3' : ''}`}>
                            {artigo.resumo}
                        </p>
                        {artigo.resumo.length > 80 && (
                            <button
                                onClick={() => setIsExpanded(prev => !prev)}
                                className="text-emerald-600 text-xs font-medium hover:underline mt-1"
                            >
                                {isExpanded ? '... Menos' : '... Ler mais'}
                            </button>
                        )}
                    </div>
                </div>

                {/* Divisor Visual */}
                <div className="border-t md:border-t-0 md:border-l border-gray-200 mx-4 my-2 md:my-0"></div>

                {/* Lado Direito: Ações */}
                <div className="flex items-center gap-4 p-3 flex-wrap">
                    {/* Status */}
                    <div className="flex items-center gap-2">
                        <select
                            value={selectedStatus}
                            onChange={(e) => setSelectedStatus(e.target.value as StatusArtigo)}
                            disabled={loading}
                            className="border border-gray-300 rounded-md px-3 py-2 bg-white text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                        >
                            {statusOptions.map(opt => (
                                <option key={opt.value} value={opt.value}>{opt.label}</option>
                            ))}
                        </select>
                        <button
                            onClick={handleStatusSubmit}
                            disabled={loading || selectedStatus === artigo.status}
                            className="p-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700 transition disabled:bg-gray-400"
                            title="Salvar alteração de status"
                        >
                            <Send size={16} />
                        </button>
                    </div>

                    <span className="text-gray-300 hidden lg:block">|</span>

                    {/* Botões de Navegação */}
                    <div className="flex items-center gap-2">
                        <Link
                            href={`/artigo/${artigo.id}`}
                            target="_blank"
                            className="flex items-center gap-1.5 px-3 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition text-sm"
                            title="Ler artigo (visualização pública)"
                        >
                            <Eye size={14} />
                            Ler
                        </Link>
                        <Link
                            href={`/editorial/artigo/edicao?Id=${artigo.id}`}
                            className="flex items-center gap-1.5 px-3 py-2 bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 transition text-sm"
                            title="Editar conteúdo ou equipe"
                        >
                            <Edit size={14} />
                            Editar
                        </Link>
                    </div>
                </div>
            </li>
        </>
    );
}