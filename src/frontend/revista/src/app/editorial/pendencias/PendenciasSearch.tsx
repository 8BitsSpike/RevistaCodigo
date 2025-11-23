'use client';

import { useState, useEffect } from 'react';
import { useLazyQuery } from '@apollo/client/react';
import { OBTER_PENDENTES } from '@/graphql/queries';
import { StaffMember } from '@/components/StaffCard';
import PendingCard, { PendingItem } from '@/components/PendingCard';
import { Search } from 'lucide-react';
import toast from 'react-hot-toast';
import { StatusPendente, TipoEntidadeAlvo } from '@/types/enums';

type SearchFilter = 'tipoPendencia' | 'solicitante' | 'responsavel' | 'statusAprovado' | 'statusRecusado' | 'statusArquivado' | 'statusAguardando';

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

    const [runSearch, { data: searchData, loading: searchLoading, refetch }] = useLazyQuery<PendingQueryData>(OBTER_PENDENTES, {
        fetchPolicy: 'network-only',
        onCompleted: (data) => {
            if (!data.obterPendentes || data.obterPendentes.length === 0) {
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
        // Inicializa com paginação padrão
        const variables: any = { 
            pagina: 0, 
            tamanho: 10 
        };

        let numericValue = 10;
        if (searchTerm) {
             const parsed = parseInt(searchTerm);
             if (!isNaN(parsed)) numericValue = parsed > 100 ? 100 : parsed;
        }
        
        if (filterType.startsWith('status')) {
            variables.tamanho = numericValue;
        }

        switch (filterType) {
            case 'statusAguardando':
                variables.status = StatusPendente.AguardandoRevisao;
                break;
            case 'statusAprovado':
                variables.status = StatusPendente.Aprovado;
                break;
            case 'statusRecusado':
                variables.status = StatusPendente.Rejeitado;
                break;
            case 'statusArquivado':
                variables.status = StatusPendente.Arquivado;
                break;
            case 'solicitante':
                if (selectedStaff) variables.requesterUsuarioId = selectedStaff.usuarioId;
                break;
            case 'responsavel':
                if (selectedStaff) variables.idAprovador = selectedStaff.usuarioId; 
                break;
            case 'tipoPendencia':
                
                toast.error("Busca por tipo de comando requer atualização na Query GraphQL.");
                return; 
        }

        toast.loading('Buscando pendências...', { id: 'search-toast' });
        
        const cleanVariables = Object.fromEntries(Object.entries(variables).filter(([_, v]) => v != null));

        runSearch({ variables: cleanVariables }).finally(() => {
            toast.dismiss('search-toast');
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
            <div className="flex flex-wrap items-center gap-2">
                <label htmlFor="filter-type" className="text-sm font-semibold">Filtra pendências por:</label>
                <select
                    id="filter-type"
                    value={filterType}
                    onChange={(e) => {
                        setFilterType(e.target.value as SearchFilter);
                        setSearchTerm(''); // Limpa inputs ao trocar filtro
                        setStaffQuery('');
                        setSelectedStaff(null);
                        setCommandQuery('');
                    }}
                    className="border border-gray-300 rounded-md px-3 py-2 bg-white text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500"
                >
                    <option value="statusAguardando">Aguardando revisão</option>
                    <option value="statusAprovado">Situação aprovada</option>
                    <option value="statusRecusado">Situação recusada</option>
                    <option value="solicitante">Solicitante</option>

                </select>

                <div className="flex-grow relative">
                    {isNumericFilter && (
                        <>
                            <label htmlFor="numeric-filter" className="sr-only">Quantidade</label>
                            <input
                                id="numeric-filter"
                                type="number"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder={placeholderMap[filterType]}
                                className="input-std"
                            />
                        </>
                    )}
                    {isStaffFilter && (
                        <div>
                            <label htmlFor="staff-filter" className="sr-only">Buscar Staff</label>
                            <input
                                id="staff-filter"
                                type="text"
                                value={selectedStaff ? selectedStaff.nome : staffQuery}
                                onChange={(e) => {
                                    setStaffQuery(e.target.value);
                                    setSelectedStaff(null);
                                }}
                                placeholder={placeholderMap[filterType]}
                                className="input-std"
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
                            <label htmlFor="command-filter" className="sr-only">Tipo de Comando</label>
                            <input
                                id="command-filter"
                                type="text"
                                value={commandQuery}
                                onChange={(e) => setCommandQuery(e.target.value)}
                                placeholder={placeholderMap[filterType]}
                                className="input-std"
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
                    className="btn-primary"
                    aria-label="Buscar"
                >
                    <Search size={18} />
                </button>
            </div>

            {hasSearched && (
                <div
                    className="mt-6"
                    style={{ maxHeight: '500px', overflowY: 'auto' }}
                >
                    {searchLoading && <p className="text-center text-sm">Buscando...</p>}
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