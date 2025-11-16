'use client';

import { useState, useEffect } from 'react';
import { useLazyQuery } from '@apollo/client/react';
import { OBTER_PENDENTES } from '@/graphql/queries';
import { StaffMember } from '@/components/StaffCard';
import PendingCard, { PendingItem } from '@/components/PendingCard';
import { Search, X, User } from 'lucide-react';
import Image from 'next/image';
import toast from 'react-hot-toast';

// --- Tipos ---

type SearchFilter = 'tipoPendencia' | 'solicitante' | 'responsavel' | 'statusAprovado' | 'statusRecusado' | 'statusArquivado' | 'statusAguardando';
type StatusPendente = 'AguardandoRevisao' | 'Aprovado' | 'Rejeitado' | 'Arquivado';

interface PendingQueryData {
    obterPendentes: PendingItem[];
}

interface PendingSearchProps {
    staffList: StaffMember[];
    onUpdate: () => void;
    isAdmin: boolean;
}

const placeholderMap: Record<SearchFilter, string> = {
    tipoPendencia: "Tipo de pendência (ex: UpdateStaff)",
    solicitante: "Nome do solicitante",
    responsavel: "Nome do responsável",
    statusAprovado: "Quantas pendências por página?",
    statusRecusado: "Quantas pendências por página?",
    statusArquivado: "Quantas pendências por página?",
    statusAguardando: "Quantas pendências por página?",
};

const commandTypes = [
    'ChangeArtigoStatus', 'UpdateArtigoMetadata', 'UpdateArtigoContent',
    'CreateStaff', 'UpdateStaff', 'DeleteInteracao',
    'CreateVolume', 'UpdateVolume', 'UpdateEditorialTeam'
];

export default function PendingSearch({ staffList, onUpdate, isAdmin }: PendingSearchProps) {
    const [filterType, setFilterType] = useState<SearchFilter>('statusAguardando');
    const [searchTerm, setSearchTerm] = useState('');
    const [staffQuery, setStaffQuery] = useState('');
    const [selectedStaff, setSelectedStaff] = useState<StaffMember | null>(null);
    const [staffResults, setStaffResults] = useState<StaffMember[]>([]);
    const [commandQuery, setCommandQuery] = useState('');
    const [commandResults, setCommandResults] = useState<string[]>([]);

    const [runSearch, { data: searchData, loading: searchLoading, error: searchError, refetch }] = useLazyQuery<PendingQueryData>(OBTER_PENDENTES, {
        fetchPolicy: 'network-only',
        onCompleted: (data) => {
            if (data.obterPendentes.length === 0) {
                toast.success('Nenhum resultado encontrado para esta busca.');
            }
        },
        onError: (err) => {
            toast.error(`Erro ao buscar pendências: ${err.message}`);
        }
    });

    const searchResults = searchData?.obterPendentes ?? [];
    const [hasSearched, setHasSearched] = useState(false);

    useEffect(() => {
        if (staffQuery.length < 2) {
            setStaffResults([]);
            return;
        }
        const filtered = staffList.filter(s =>
            s.nome.toLowerCase().includes(staffQuery.toLowerCase())
        );
        setStaffResults(filtered.slice(0, 5));
    }, [staffQuery, staffList]);

    useEffect(() => {
        if (commandQuery.length < 2) {
            setCommandResults([]);
            return;
        }
        const filtered = commandTypes.filter(c =>
            c.toLowerCase().includes(commandQuery.toLowerCase())
        );
        setCommandResults(filtered);
    }, [commandQuery]);


    const handleSearch = () => {
        let variables: any = { pagina: 0 };
        let status: StatusPendente | null = null;

        let numericValue = parseInt(searchTerm) || 10;
        if (numericValue > 100) numericValue = 100;
        variables.tamanho = 10;

        switch (filterType) {
            case 'statusAguardando':
                status = 'AguardandoRevisao';
                variables = { pagina: 0, tamanho: numericValue, status };
                break;
            case 'statusAprovado':
                status = 'Aprovado';
                variables = { pagina: 0, tamanho: numericValue, status };
                break;
            case 'statusRecusado':
                status = 'Rejeitado';
                variables = { pagina: 0, tamanho: numericValue, status };
                break;
            case 'statusArquivado':
                status = 'Rejeitado'; // Fallback
                variables = { pagina: 0, tamanho: numericValue, status };
                break;
            case 'solicitante':
                if (selectedStaff) variables.requesterUsuarioId = selectedStaff.usuarioId;
                break;
            case 'responsavel':
                if (selectedStaff) variables.idAprovador = selectedStaff.usuarioId;
                break;
            case 'tipoPendencia':
                if (commandQuery) variables.commandType = commandQuery;
                break;
        }

        toast.loading('Buscando pendências...', { id: 'search-toast' });
        runSearch({ variables }).finally(() => {
            toast.dismiss('search-toast'); // Limpa o toast
        });
        setHasSearched(true);
    };

    const handleSearchUpdate = () => {
        if (refetch) {
            refetch();
        }
        onUpdate();
    };

    const isNumericFilter = filterType.startsWith('status');
    const isStaffFilter = filterType === 'solicitante' || filterType === 'responsavel';
    const isCommandFilter = filterType === 'tipoPendencia';

    return (
        <div
            className="bg-gray-50 rounded-lg shadow-sm p-6"
            style={!hasSearched ? { minHeight: '5vh' } : {}}
        >
            {/* Barra de Filtro e Busca */}
            <div className="flex flex-wrap items-center gap-2">
                <span className="text-sm font-semibold">Filtra pendências por:</span>
                <select
                    value={filterType}
                    onChange={(e) => setFilterType(e.target.value as SearchFilter)}
                    className="border border-gray-300 rounded-md px-3 py-2 bg-white text-sm"
                >
                    <option value="statusAguardando">Aguardando revisão</option>
                    <option value="statusAprovado">Situação aprovada</option>
                    <option value="statusRecusado">Situação recusada</option>
                    <option value="solicitante">Solicitante</option>
                    <option value="responsavel">Responsável pela aprovação</option>
                    <option value="tipoPendencia">Tipo de pendência</option>
                </select>

                {/* Input de Texto Condicional */}
                <div className="flex-grow relative">
                    {isNumericFilter && (
                        <input
                            type="number"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder={placeholderMap[filterType]}
                            className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm"
                        />
                    )}
                    {isStaffFilter && (
                        <div>
                            <input
                                type="text"
                                value={selectedStaff ? selectedStaff.nome : staffQuery}
                                onChange={(e) => {
                                    setStaffQuery(e.target.value);
                                    setSelectedStaff(null);
                                }}
                                placeholder={placeholderMap[filterType]}
                                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm"
                            />
                            {staffResults.length > 0 && (
                                <ul className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-y-auto">
                                    {staffResults.map(s => (
                                        <li
                                            key={s.usuarioId}
                                            onClick={() => { setSelectedStaff(s); setStaffResults([]); setStaffQuery(''); }}
                                            className="p-2 hover:bg-gray-100 cursor-pointer text-sm"
                                        >
                                            {s.nome}
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>
                    )}
                    {isCommandFilter && (
                        <div>
                            <input
                                type="text"
                                value={commandQuery}
                                onChange={(e) => setCommandQuery(e.target.value)}
                                placeholder={placeholderMap[filterType]}
                                className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm"
                            />
                            {commandResults.length > 0 && (
                                <ul className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-y-auto">
                                    {commandResults.map(c => (
                                        <li
                                            key={c}
                                            onClick={() => { setCommandQuery(c); setCommandResults([]); }}
                                            className="p-2 hover:bg-gray-100 cursor-pointer text-sm"
                                        >
                                            {c}
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>
                    )}
                </div>

                <button
                    onClick={() => handleSearch()}
                    className="bg-emerald-600 text-white px-4 py-2 rounded-md hover:bg-emerald-700"
                >
                    <Search size={18} />
                </button>
            </div>

            {/* Área de Resultados */}
            {hasSearched && (
                <div
                    className="mt-6"
                    style={{ maxHeight: '500px', overflowY: 'auto' }}
                >
                    {searchLoading && <p className="text-center text-sm">Buscando...</p>}
                    {/* O erro é tratado pelo toast */}
                    {!searchLoading && searchResults.length === 0 && (
                        <p className="text-center text-gray-500 text-sm">Nenhum resultado encontrado.</p>
                    )}

                    {searchResults.length > 0 && (
                        <ul className="space-y-4">
                            {searchResults.map(pending => (
                                <PendingCard
                                    key={pending.id}
                                    pending={pending}
                                    staffList={staffList}
                                    onUpdate={handleSearchUpdate}
                                    isAdmin={isAdmin}
                                />
                            ))}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}