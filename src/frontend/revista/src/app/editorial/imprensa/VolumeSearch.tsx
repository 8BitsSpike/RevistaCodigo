'use client';

import { useState } from 'react';
import { Search } from 'lucide-react';
import { StatusVolume } from '@/types/enums';

type SearchType = 'recentes' | 'ano' | 'status';

export interface VolumeSearchVariables {
    searchType: SearchType;
    searchTerm?: string | number;
    searchStatus?: StatusVolume;
    pageSize: number;
}

interface VolumeSearchProps {
    onSearch: (variables: VolumeSearchVariables) => void;
    loading: boolean;
}

export default function VolumeSearch({ onSearch, loading }: VolumeSearchProps) {
    const [searchType, setSearchType] = useState<SearchType>('recentes');
    const [pageSize, setPageSize] = useState(10);

    const [textTerm, setTextTerm] = useState(new Date().getFullYear().toString());
    const [selectedStatus, setSelectedStatus] = useState<StatusVolume>(StatusVolume.EmRevisao);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        let finalPageSize = Number(pageSize) || 10;
        if (finalPageSize > 50) finalPageSize = 50;
        if (finalPageSize < 10) finalPageSize = 10;
        setPageSize(finalPageSize);

        let variables: VolumeSearchVariables = {
            searchType: searchType,
            pageSize: finalPageSize,
        };

        switch (searchType) {
            case 'ano':
                variables.searchTerm = Number(textTerm) || new Date().getFullYear();
                break;
            case 'status':
                variables.searchStatus = selectedStatus;
                break;
            case 'recentes':
            default:
                break;
        }

        onSearch(variables);
    };

    const renderSearchInput = () => {
        switch (searchType) {
            case 'ano':
                return (
                    <>
                        <label htmlFor="search-ano" className="sr-only">Ano da busca</label>
                        <input
                            id="search-ano"
                            type="number"
                            value={textTerm}
                            onChange={(e) => setTextTerm(e.target.value)}
                            placeholder="Ano da busca:"
                            className="input-std"
                        />
                    </>
                );

            case 'status':
                return (
                    <>
                        <label htmlFor="search-status" className="sr-only">Status da Edição</label>
                        <select
                            id="search-status"
                            value={selectedStatus}
                            onChange={(e) => setSelectedStatus(e.target.value as StatusVolume)}
                            className="input-std"
                        >
                            {Object.values(StatusVolume).map(status => (
                                <option key={status} value={status}>{status}</option>
                            ))}
                        </select>
                    </>
                );

            case 'recentes':
                return <p className="text-sm text-gray-500 p-3">Buscando as edições mais recentes...</p>;
        }
    };

    return (
        <form onSubmit={handleSubmit} className="p-4 bg-gray-50 rounded-lg shadow-sm border">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
                <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2" htmlFor="search-type">Buscar por:</label>
                    <select
                        id="search-type"
                        value={searchType}
                        onChange={(e) => setSearchType(e.target.value as SearchType)}
                        className="input-std"
                    >
                        <option value="recentes">Edições recentes</option>
                        <option value="ano">Busca por ano</option>
                        <option value="status">Situação da edição</option>
                    </select>
                </div>

                <div className="md:col-span-2">
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                        {searchType === 'recentes' ? 'Configuração' : 'Termo da Busca'}
                    </label>
                    {renderSearchInput()}
                </div>

                <div className="flex gap-2">
                    <div className="flex-1">
                        <label className="block text-sm font-semibold text-gray-700 mb-2" htmlFor="page-size">Por página:</label>
                        <input
                            id="page-size"
                            type="number"
                            min="10"
                            max="50"
                            value={pageSize}
                            onChange={(e) => setPageSize(Number(e.target.value))}
                            className="input-std"
                            placeholder="10-50"
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={loading}
                        className="self-end btn-primary"
                        aria-label="Buscar"
                    >
                        <Search size={20} />
                    </button>
                </div>
            </div>
        </form>
    );
}