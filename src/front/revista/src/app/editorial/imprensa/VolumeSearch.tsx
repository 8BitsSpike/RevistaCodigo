'use client';

import { useState } from 'react';
import { Search } from 'lucide-react';
import { StatusVolume } from '@/types/enums';

// --- Tipos ---

// Define os tipos de busca
type SearchType = 'recentes' | 'ano' | 'status';

// Define os valores que o componente de busca retorna
export interface VolumeSearchVariables {
    searchType: SearchType;
    searchTerm?: string | number; // Usado para 'ano'
    searchStatus?: StatusVolume;
    pageSize: number;
}

// Props que o componente recebe
interface VolumeSearchProps {
    onSearch: (variables: VolumeSearchVariables) => void;
    loading: boolean;
}

export default function VolumeSearch({ onSearch, loading }: VolumeSearchProps) {

    // --- Estados do Formulário ---
    const [searchType, setSearchType] = useState<SearchType>('recentes');
    const [pageSize, setPageSize] = useState(10); // Mínimo de 10

    const [textTerm, setTextTerm] = useState(new Date().getFullYear().toString());
    const [selectedStatus, setSelectedStatus] = useState<StatusVolume>(StatusVolume.EmRevisao);

    // --- Submissão da Busca ---
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

        // Adiciona os termos corretos
        switch (searchType) {
            case 'ano':
                variables.searchTerm = Number(textTerm) || new Date().getFullYear();
                break;
            case 'status':
                variables.searchStatus = selectedStatus;
                break;
            case 'recentes':
            default:
                // Não precisa de mais nada
                break;
        }

        onSearch(variables);
    };

    // Renderiza o input de busca apropriado
    const renderSearchInput = () => {
        switch (searchType) {
            case 'ano':
                return (
                    <input
                        type="number"
                        value={textTerm}
                        onChange={(e) => setTextTerm(e.target.value)}
                        placeholder="Ano da busca:"
                        className="w-full p-3 border border-gray-300 rounded-lg"
                    />
                );

            case 'status':
                return (
                    <select
                        value={selectedStatus}
                        onChange={(e) => setSelectedStatus(e.target.value as StatusVolume)}
                        className="w-full p-3 border border-gray-300 rounded-lg bg-white"
                    >
                        {Object.values(StatusVolume).map(status => (
                            <option key={status} value={status}>{status}</option>
                        ))}
                    </select>
                );

            case 'recentes':
                return <p className="text-sm text-gray-500 p-3">Buscando as edições mais recentes...</p>;
        }
    };

    return (
        <form onSubmit={handleSubmit} className="p-4 bg-gray-50 rounded-lg shadow-sm border">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
                {/* 1. Tipo de Busca */}
                <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">Buscar por:</label>
                    <select
                        value={searchType}
                        onChange={(e) => setSearchType(e.target.value as SearchType)}
                        className="w-full p-3 border border-gray-300 rounded-lg bg-white"
                    >
                        <option value="recentes">Edições recentes</option>
                        <option value="ano">Busca por ano</option>
                        <option value="status">Situação da edição</option>
                    </select>
                </div>

                {/* 2. Input Condicional */}
                <div className="md:col-span-2">
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                        {searchType === 'recentes' ? 'Configuração' : 'Termo da Busca'}
                    </label>
                    {renderSearchInput()}
                </div>

                {/* 3. Paginação e Submit */}
                <div className="flex gap-2">
                    <div className="flex-1">
                        <label className="block text-sm font-semibold text-gray-700 mb-2">Por página:</label>
                        <input
                            type="number"
                            min="10"
                            max="50"
                            value={pageSize}
                            onChange={(e) => setPageSize(Number(e.target.value))}
                            className="w-full p-3 border border-gray-300 rounded-lg"
                            placeholder="10-50"
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={loading}
                        className="self-end px-4 py-3 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition disabled:bg-gray-400"
                    >
                        <Search size={20} />
                    </button>
                </div>
            </div>
        </form>
    );
}