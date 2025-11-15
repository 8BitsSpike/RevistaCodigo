'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Image from 'next/image';
import { Search, X, User } from 'lucide-react';
import { StaffMember } from '@/components/StaffCard';
import { TipoArtigo, StatusArtigo } from '@/types/enums'; // (Precisaremos criar este arquivo)

// --- Tipos ---

// Define os tipos de busca
type SearchType = 'titulo' | 'autor' | 'tipo' | 'status' | 'todos';

// Define os valores que o componente de busca retorna
export interface SearchVariables {
    searchType: SearchType;
    searchTerm?: string;
    searchStatus?: StatusArtigo;
    searchTipo?: TipoArtigo;
    pageSize: number;
}

// Props que o componente recebe
interface ArtigoSearchProps {
    staffList: StaffMember[]; // Para a busca de autores
    onSearch: (variables: SearchVariables) => void;
    loading: boolean;
}

// Interface para o usuário buscado na UsuarioAPI
interface UsuarioBusca {
    id: string; // Do UsuarioAPI
    name: string;
    sobrenome?: string;
    foto?: string;
}

const API_USUARIO_BASE = 'https://localhost:44387/api/Usuario';

// --- Componente ---

export default function ArtigoSearch({ staffList, onSearch, loading }: ArtigoSearchProps) {

    // --- Estados do Formulário ---
    const [searchType, setSearchType] = useState<SearchType>('todos');
    const [pageSize, setPageSize] = useState(15);

    // Estados para os diferentes inputs
    const [textTerm, setTextTerm] = useState('');
    const [selectedStatus, setSelectedStatus] = useState<StatusArtigo>(StatusArtigo.EmRevisao);
    const [selectedTipo, setSelectedTipo] = useState<TipoArtigo>(TipoArtigo.Artigo);

    // Estados para a busca de autor
    const [authorSearchQuery, setAuthorSearchQuery] = useState('');
    const [authorSearchResults, setAuthorSearchResults] = useState<UsuarioBusca[]>([]);
    const [selectedAuthor, setSelectedAuthor] = useState<UsuarioBusca | null>(null);

    // --- Lógica de Busca de Autor (Autocomplete) ---
    useEffect(() => {
        if (searchType !== 'autor' || authorSearchQuery.length < 3) {
            setAuthorSearchResults([]);
            return;
        }

        const delayDebounceFn = setTimeout(async () => {
            const token = localStorage.getItem('jwtToken');
            if (!token) return;

            try {
                const res = await fetch(`${API_USUARIO_BASE}/Search?name=${authorSearchQuery}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data = await res.json();
                    // Filtra usuários já selecionados
                    const filtered = data.filter((u: any) => u.id !== selectedAuthor?.id);
                    setAuthorSearchResults(filtered);
                }
            } catch (err) {
                console.error("Erro buscando usuários", err);
            }
        }, 500); // 500ms debounce

        return () => clearTimeout(delayDebounceFn);
    }, [authorSearchQuery, searchType, selectedAuthor]);

    const handleSelectAuthor = (author: UsuarioBusca) => {
        setSelectedAuthor(author);
        setAuthorSearchQuery('');
        setAuthorSearchResults([]);
    };

    // --- Submissão da Busca ---
    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        let variables: SearchVariables = {
            searchType: searchType,
            pageSize: Number(pageSize) || 15,
        };

        switch (searchType) {
            case 'titulo':
                variables.searchTerm = textTerm;
                break;
            case 'autor':
                // Usa o ID do usuário (UsuarioId) como o termo de busca
                variables.searchTerm = selectedAuthor?.id;
                break;
            case 'tipo':
                variables.searchTipo = selectedTipo;
                break;
            case 'status':
                variables.searchStatus = selectedStatus;
                break;
            case 'todos':
                // Não adiciona mais nada, busca todos >=( 
                break;
        }

        onSearch(variables);
    };

    // Renderiza o input de busca apropriado
    const renderSearchInput = () => {
        switch (searchType) {
            case 'titulo':
                return (
                    <input
                        type="text"
                        value={textTerm}
                        onChange={(e) => setTextTerm(e.target.value)}
                        placeholder="Título do artigo"
                        className="w-full p-3 border border-gray-300 rounded-lg"
                    />
                    // TODO: Implementar autocomplete de título caso a parceira fique rica (obterArtigosCardListPorTitulo)
                );

            case 'autor':
                if (selectedAuthor) {
                    return (
                        <div className="group relative flex items-center justify-between bg-emerald-50 border border-emerald-200 px-3 py-2 rounded-lg">
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                    {selectedAuthor.foto && <Image src={selectedAuthor.foto} alt={selectedAuthor.name} fill className="object-cover" />}
                                </div>
                                <span className="text-sm font-medium text-emerald-800">{selectedAuthor.name} {selectedAuthor.sobrenome}</span>
                            </div>
                            <button
                                onClick={() => setSelectedAuthor(null)}
                                className="text-red-500 hover:text-red-700 p-1 rounded-full hover:bg-red-100"
                                title="Click para remover"
                            >
                                <X size={16} />
                            </button>
                        </div>
                    );
                }
                return (
                    <div className="relative">
                        <input
                            type="text"
                            value={authorSearchQuery}
                            onChange={(e) => setAuthorSearchQuery(e.target.value)}
                            className="w-full p-3 border border-gray-300 rounded-lg"
                            placeholder="Nome do autor"
                        />
                        {authorSearchResults.length > 0 && (
                            <ul className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto">
                                {authorSearchResults.map(u => (
                                    <li
                                        key={u.id}
                                        onClick={() => handleSelectAuthor(u)}
                                        className="flex items-center gap-3 p-3 hover:bg-gray-50 cursor-pointer transition"
                                    >
                                        <div className="w-10 h-10 relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                                            {u.foto ? <Image src={u.foto} alt={u.name} fill className="object-cover" /> : <User size={20} className="text-gray-400 m-auto" />}
                                        </div>
                                        <span className="font-medium text-gray-800">{u.name} {u.sobrenome}</span>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                );

            case 'tipo':
                return (
                    <select
                        value={selectedTipo}
                        onChange={(e) => setSelectedTipo(e.target.value as TipoArtigo)}
                        className="w-full p-3 border border-gray-300 rounded-lg bg-white"
                    >
                        {Object.values(TipoArtigo).map(tipo => (
                            <option key={tipo} value={tipo}>{tipo}</option>
                        ))}
                    </select>
                );

            case 'status':
                return (
                    <select
                        value={selectedStatus}
                        onChange={(e) => setSelectedStatus(e.target.value as StatusArtigo)}
                        className="w-full p-3 border border-gray-300 rounded-lg bg-white"
                    >
                        {Object.values(StatusArtigo).map(status => (
                            <option key={status} value={status}>{status}</option>
                        ))}
                    </select>
                );

            case 'todos':
                return <p className="text-sm text-gray-500 p-3">Buscando todos os artigos...</p>;
        }
    };

    return (
        <form onSubmit={handleSubmit} className="p-4 bg-gray-50 rounded-lg shadow-sm border">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
                {/* Tipo de Busca */}
                <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">Buscar por:</label>
                    <select
                        value={searchType}
                        onChange={(e) => setSearchType(e.target.value as SearchType)}
                        className="w-full p-3 border border-gray-300 rounded-lg bg-white"
                    >
                        <option value="todos">Todos</option>
                        <option value="titulo">Título do artigo</option>
                        <option value="autor">Nome do autor</option>
                        <option value="tipo">Tipo do artigo</option>
                        <option value="status">Situação do artigo</option>
                    </select>
                </div>

                {/* Input Condicional */}
                <div className="md:col-span-2">
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                        {searchType === 'todos' ? 'Configuração' : 'Termo da Busca'}
                    </label>
                    {renderSearchInput()}
                </div>

                {/* Paginação e Submit */}
                <div className="flex gap-2">
                    <div className="flex-1">
                        <label className="block text-sm font-semibold text-gray-700 mb-2">Por página:</label>
                        <input
                            type="number"
                            value={pageSize}
                            onChange={(e) => setPageSize(Number(e.target.value))}
                            className="w-full p-3 border border-gray-300 rounded-lg"
                            placeholder="15"
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