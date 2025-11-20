'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import { Search, X, User } from 'lucide-react';
import { StaffMember } from '@/components/StaffCard';
import { USER_API_BASE } from '@/lib/fetcher';
import { TipoArtigo, StatusArtigo } from '@/types/enums';

type SearchType = 'titulo' | 'autor' | 'tipo' | 'status' | 'todos';
export interface SearchVariables { searchType: SearchType; searchTerm?: string; searchStatus?: StatusArtigo; searchTipo?: TipoArtigo; pageSize: number; }
interface ArtigoSearchProps { staffList: StaffMember[]; onSearch: (variables: SearchVariables) => void; loading: boolean; }
interface UsuarioBusca { id: string; name: string; sobrenome?: string; foto?: string; }

export default function ArtigoSearch({ staffList, onSearch, loading }: ArtigoSearchProps) {
    const [searchType, setSearchType] = useState<SearchType>('todos');
    const [pageSize, setPageSize] = useState(15);
    const [textTerm, setTextTerm] = useState('');
    const [selectedStatus, setSelectedStatus] = useState<StatusArtigo>(StatusArtigo.EmRevisao);
    const [selectedTipo, setSelectedTipo] = useState<TipoArtigo>(TipoArtigo.Artigo);
    const [authorSearchQuery, setAuthorSearchQuery] = useState('');
    const [authorSearchResults, setAuthorSearchResults] = useState<UsuarioBusca[]>([]);
    const [selectedAuthor, setSelectedAuthor] = useState<UsuarioBusca | null>(null);

    useEffect(() => {
        if (searchType !== 'autor' || authorSearchQuery.length < 3) {
            setAuthorSearchResults([]);
            return;
        }
        const delayDebounceFn = setTimeout(async () => {
            const token = localStorage.getItem('userToken');
            if (!token) return;
            try {
                const res = await fetch(`${USER_API_BASE}/UserSearch?nome=${authorSearchQuery}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data = await res.json();
                    const filtered = data.filter((u: any) => u.id !== selectedAuthor?.id);
                    setAuthorSearchResults(filtered);
                }
            } catch (err) { console.error(err); }
        }, 500);
        return () => clearTimeout(delayDebounceFn);
    }, [authorSearchQuery, searchType, selectedAuthor]);

    const handleSelectAuthor = (author: UsuarioBusca) => { setSelectedAuthor(author); setAuthorSearchQuery(''); setAuthorSearchResults([]); };
    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        let variables: SearchVariables = { searchType: searchType, pageSize: Number(pageSize) || 15, };
        switch (searchType) {
            case 'titulo': variables.searchTerm = textTerm; break;
            case 'autor': variables.searchTerm = selectedAuthor?.id; break;
            case 'tipo': variables.searchTipo = selectedTipo; break;
            case 'status': variables.searchStatus = selectedStatus; break;
            case 'todos': break;
        }
        onSearch(variables);
    };

    const renderSearchInput = () => {
        switch (searchType) {
            case 'titulo': return <><label htmlFor="stitle" className="sr-only">Titulo</label><input id="stitle" type="text" value={textTerm} onChange={(e) => setTextTerm(e.target.value)} placeholder="Título do artigo" className="input-std" /></>;
            case 'autor':
                if (selectedAuthor) {
                    return (
                        <div className="group relative flex items-center justify-between bg-emerald-50 border border-emerald-200 px-3 py-2 rounded-lg">
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                    {/* FIX: Image fallback */}
                                    <Image src={selectedAuthor.foto || '/faviccon.png'} alt={selectedAuthor.name} fill className="object-cover" />
                                </div>
                                <span className="text-sm font-medium text-emerald-800">{selectedAuthor.name} {selectedAuthor.sobrenome}</span>
                            </div>
                            <button onClick={() => setSelectedAuthor(null)} className="text-red-500 hover:text-red-700 p-1 rounded-full hover:bg-red-100"><X size={16} /></button>
                        </div>
                    );
                }
                return (
                    <div className="relative">
                        <label htmlFor="sautor" className="sr-only">Autor</label>
                        <input id="sautor" type="text" value={authorSearchQuery} onChange={(e) => setAuthorSearchQuery(e.target.value)} className="input-std" placeholder="Nome do autor" />
                        {authorSearchResults.length > 0 && (
                            <ul className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto">
                                {authorSearchResults.map(u => (
                                    <li key={u.id} onClick={() => handleSelectAuthor(u)} className="flex items-center gap-3 p-3 hover:bg-gray-50 cursor-pointer transition">
                                        <div className="w-10 h-10 relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                                            {/* FIX: Image fallback */}
                                            <Image src={u.foto || '/faviccon.png'} alt={u.name} fill className="object-cover" />
                                        </div>
                                        <span className="font-medium text-gray-800">{u.name} {u.sobrenome}</span>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                );
            case 'tipo': return <><label htmlFor="stype" className="sr-only">Tipo</label><select id="stype" value={selectedTipo} onChange={(e) => setSelectedTipo(e.target.value as TipoArtigo)} className="input-std">{Object.values(TipoArtigo).map(tipo => <option key={tipo} value={tipo}>{tipo}</option>)}</select></>;
            case 'status': return <><label htmlFor="sstatus" className="sr-only">Status</label><select id="sstatus" value={selectedStatus} onChange={(e) => setSelectedStatus(e.target.value as StatusArtigo)} className="input-std">{Object.values(StatusArtigo).map(status => <option key={status} value={status}>{status}</option>)}</select></>;
            case 'todos': return <p className="text-sm text-gray-500 p-3">Buscando todos os artigos...</p>;
        }
    };

    return (
        <form onSubmit={handleSubmit} className="p-4 bg-gray-50 rounded-lg shadow-sm border">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
                <div><label className="block text-sm font-semibold text-gray-700 mb-2">Buscar por:</label><select value={searchType} onChange={(e) => setSearchType(e.target.value as SearchType)} className="input-std"><option value="todos">Todos</option><option value="titulo">Título do artigo</option><option value="autor">Nome do autor</option><option value="tipo">Tipo do artigo</option><option value="status">Situação do artigo</option></select></div>
                <div className="md:col-span-2"><label className="block text-sm font-semibold text-gray-700 mb-2">{searchType === 'todos' ? 'Configuração' : 'Termo da Busca'}</label>{renderSearchInput()}</div>
                <div className="flex gap-2"><div className="flex-1"><label className="block text-sm font-semibold text-gray-700 mb-2">Por página:</label><input type="number" value={pageSize} onChange={(e) => setPageSize(Number(e.target.value))} className="input-std" placeholder="15" /></div><button type="submit" disabled={loading} className="self-end btn-primary"><Search size={20} /></button></div>
            </div>
        </form>
    );
}