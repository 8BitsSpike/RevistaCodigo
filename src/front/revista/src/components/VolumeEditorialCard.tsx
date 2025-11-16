'use client';

import { useState, useEffect, useRef, ChangeEvent } from 'react';
import { useMutation, useLazyQuery } from '@apollo/client/react';
import { ATUALIZAR_METADADOS_VOLUME, CRIAR_VOLUME, OBTER_VOLUME_POR_ID, GET_ARTIGOS_BY_IDS, SEARCH_ARTIGOS_EDITORIAL_BY_TITLE } from '@/graphql/queries';
import { useRouter } from 'next/navigation';
import { Edit, Save, XCircle, Image as ImageIcon, Search, Plus, Trash2 } from 'lucide-react';
import CommentaryModal from './CommentaryModal';
import { StatusVolume, MesVolume } from '@/types/enums';
import toast from 'react-hot-toast';
import Image from 'next/image';

// --- Tipos ---

// Tipo para os dados básicos do card (vindo da busca)
export interface VolumeCardData {
    id: string;
    volumeTitulo: string;
    volumeResumo: string;
    imagemCapa?: {
        url: string;
        textoAlternativo: string;
    } | null;
}

// Tipo para os dados completos do formulário (vindo de OBTER_VOLUME_POR_ID)
interface VolumeFormData {
    edicao: number;
    volumeTitulo: string;
    volumeResumo: string;
    m: MesVolume;
    n: number;
    year: number;
    status: StatusVolume;
    imagemCapa?: {
        url: string;
        alt: string;
        midiaID: string;
    } | null;
    artigoIds: string[];
}

// Tipo para um artigo na lista (vindo de GET_ARTIGOS_BY_IDS)
interface ArtigoNaLista {
    id: string;
    titulo: string;
    midiaDestaque?: { url: string; textoAlternativo: string; } | null;
}

// Props do componente
interface VolumeEditorialCardProps {
    mode: 'view' | 'create';
    initialData?: VolumeCardData;
    onUpdate: () => void;
}

// --- Componente ---

export default function VolumeEditorialCard({
    mode,
    initialData,
    onUpdate
}: VolumeEditorialCardProps) {

    const router = useRouter();
    const [isEditing, setIsEditing] = useState(mode === 'create');
    const [isModalOpen, setIsModalOpen] = useState(false);

    // Estado do formulário
    const [formData, setFormData] = useState<Partial<VolumeFormData>>({
        volumeTitulo: initialData?.volumeTitulo || '',
        volumeResumo: initialData?.volumeResumo || '',
        status: StatusVolume.EmRevisao,
        m: MesVolume.Janeiro,
        n: 1,
        year: new Date().getFullYear(),
        artigoIds: [],
    });

    // Estado para a lista de artigos (com títulos)
    const [artigosNoVolume, setArtigosNoVolume] = useState<ArtigoNaLista[]>([]);
    // Estado para a nova imagem de capa (Base64)
    const [newCoverImage, setNewCoverImage] = useState<string | null>(null);

    // Estados da busca de artigos
    const [artigoSearchTerm, setArtigoSearchTerm] = useState('');
    const [artigoSearchResults, setArtigoSearchResults] = useState<ArtigoNaLista[]>([]);
    const [selectedArtigo, setSelectedArtigo] = useState<ArtigoNaLista | null>(null);

    const fileInputRef = useRef<HTMLInputElement>(null);

    // --- Data Fetching ---

    // 1. Busca os dados completos do volume QUANDO o usuário clica em "Editar"
    const [loadVolumeData, { loading: loadingData }] = useLazyQuery(OBTER_VOLUME_POR_ID, {
        fetchPolicy: 'network-only',
        onCompleted: (data) => {
            if (data.obterVolumePorId) {
                setFormData(data.obterVolumePorId);
                setNewCoverImage(data.obterVolumePorId.imagemCapa?.url || null);
                if (data.obterVolumePorId.artigoIds.length > 0) {
                    loadArtigoTitles({ variables: { ids: data.obterVolumePorId.artigoIds } });
                }
            }
        },
        onError: (err) => toast.error(`Erro ao carregar volume: ${err.message}`)
    });

    // Busca os títulos dos artigos do volume
    const [loadArtigoTitles, { loading: loadingArtigos }] = useLazyQuery(GET_ARTIGOS_BY_IDS, {
        onCompleted: (data) => {
            setArtigosNoVolume(data.obterArtigoCardListPorLista);
        }
    });

    // Busca de artigos (autocomplete) para adicionar ao volume
    const [runArtigoSearch, { loading: loadingArtigoSearch }] = useLazyQuery(SEARCH_ARTIGOS_EDITORIAL_BY_TITLE, {
        onCompleted: (data) => {
            setArtigoSearchResults(data.searchArtigosEditorialByTitle || []);
        }
    });

    // --- Mutações ---
    const [criarVolume, { loading: loadingCreate }] = useMutation(CRIAR_VOLUME, {
        onCompleted: () => {
            toast.success("Volume criado com sucesso!");
            setIsModalOpen(false);
            onUpdate(); // Recarrega a lista principal
        },
        onError: (err) => {
            toast.error(`Erro ao criar: ${err.message}`);
            setIsModalOpen(false);
        }
    });

    const [atualizarVolume, { loading: loadingUpdate }] = useMutation(ATUALIZAR_METADADOS_VOLUME, {
        onCompleted: () => {
            toast.success("Volume atualizado com sucesso!");
            setIsEditing(false); // Retorna ao modo 'view'
            setIsModalOpen(false);
            onUpdate();
        },
        onError: (err) => {
            toast.error(`Erro ao atualizar: ${err.message}`);
            setIsModalOpen(false);
        }
    });

    const loading = loadingData || loadingArtigos || loadingCreate || loadingUpdate;

    // --- Handlers ---

    const handleEditClick = () => {
        if (initialData?.id) {
            loadVolumeData({ variables: { idVolume: initialData.id } });
            setIsEditing(true);
        }
    };

    const handleCancel = () => {
        if (mode === 'create') {
            onUpdate();
        } else {
            setIsEditing(false);
        }
    };

    const handleFormChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        // Converte N e Year para números
        if (name === 'n' || name === 'year' || name === 'edicao') {
            setFormData(prev => ({ ...prev, [name]: parseInt(value) || 0 }));
        } else {
            setFormData(prev => ({ ...prev, [name]: value }));
        }
    };

    const handleImageChange = (e: ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                setNewCoverImage(reader.result as string); // Salva a string Base64
            };
            reader.readAsDataURL(file);
        }
    };

    const handleArtigoSearch = () => {
        if (artigoSearchTerm.length < 3) return;
        runArtigoSearch({ variables: { searchTerm: artigoSearchTerm, pagina: 0, tamanho: 5 } });
    };

    const handleSelectArtigo = (artigo: ArtigoNaLista) => {
        setSelectedArtigo(artigo);
        setArtigoSearchTerm(artigo.titulo);
        setArtigoSearchResults([]);
    };

    const handleAddArtigo = () => {
        if (selectedArtigo && !formData.artigoIds?.includes(selectedArtigo.id)) {
            setArtigosNoVolume(prev => [...prev, selectedArtigo]);
            setFormData(prev => ({ ...prev, artigoIds: [...(prev.artigoIds || []), selectedArtigo.id] }));
            setSelectedArtigo(null);
            setArtigoSearchTerm('');
        }
    };

    const handleRemoveArtigo = (id: string) => {
        setArtigosNoVolume(artigosNoVolume.filter(a => a.id !== id));
        setFormData(prev => ({ ...prev, artigoIds: (prev.artigoIds || []).filter(artId => artId !== id) }));
    };

    const handleConfirmSubmit = (commentary: string) => {
        const input = {
            edicao: formData.edicao,
            volumeTitulo: formData.volumeTitulo,
            volumeResumo: formData.volumeResumo,
            m: formData.m,
            n: formData.n,
            year: formData.year,
            status: formData.status,
            artigoIds: formData.artigoIds,
            // Envia a nova imagem apenas se ela tiver mudado (se for base64)
            imagemCapa: newCoverImage && newCoverImage.startsWith('data:image')
                ? { midiaID: `capa-${Date.now()}`, url: newCoverImage, alt: `Capa para ${formData.volumeTitulo}` }
                : null
        };

        if (mode === 'create') {
            toast.loading('Criando novo volume...', { id: 'volume-save' });
            criarVolume({
                variables: { input, commentary }
            }).finally(() => toast.dismiss('volume-save'));
        } else {
            toast.loading('Atualizando volume...', { id: 'volume-save' });
            atualizarVolume({
                variables: { volumeId: initialData!.id, input, commentary }
            }).finally(() => toast.dismiss('volume-save'));
        }
    };

    // --- Renderização ---

    // Modo Visualização (Card Simples)
    if (mode === 'view' && !isEditing) {
        if (!initialData) return null;
        return (
            <li
                className="bg-white shadow border border-gray-100 rounded-lg flex items-center"
                style={{ width: '98%', margin: '10px 1%', padding: '1% 0.5%' }}
            >
                <div className="w-[10%] flex-shrink-0 relative min-h-[60px]">
                    <Image
                        src={initialData.imagemCapa?.url || '/default-avatar.png'}
                        alt={initialData.imagemCapa?.textoAlternativo || initialData.volumeTitulo}
                        fill
                        className="object-cover rounded-md"
                    />
                </div>
                <div className="flex-1 p-3 min-w-0 mx-4">
                    <strong className="text-gray-800 truncate block">{initialData.volumeTitulo}</strong>
                    <p className="text-gray-600 text-sm mt-1 line-clamp-2">{initialData.volumeResumo}</p>
                </div>
                <button
                    onClick={handleEditClick}
                    className="flex items-center gap-1.5 px-3 py-2 bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 transition text-sm mr-4"
                >
                    <Edit size={14} />
                    Editar
                </button>
            </li>
        );
    }

    // Modo Edição ou Criação (Formulário Completo)
    return (
        <>
            <CommentaryModal
                isOpen={isModalOpen}
                title={mode === 'create' ? "Justificar Criação" : "Justificar Atualização"}
                loading={loadingCreate || loadingUpdate}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleConfirmSubmit}
            />

            <li
                className="bg-white shadow-lg border border-gray-200 rounded-lg"
                style={{ width: '98%', margin: '10px 1%', padding: '1.5rem' }}
            >
                {loadingData && <p>Carregando dados da edição...</p>}

                <div className="flex flex-col md:flex-row gap-6">
                    {/* Coluna Esquerda: Imagem */}
                    <div className="w-full md:w-1/4 flex flex-col items-center">
                        <div className="relative w-[150px] h-[150px] rounded-md overflow-hidden bg-gray-200 border">
                            <Image
                                src={newCoverImage || '/default-avatar.png'}
                                alt="Imagem de Capa"
                                fill
                                className="object-cover"
                            />
                        </div>
                        <input
                            type="file"
                            accept="image/*"
                            ref={fileInputRef}
                            onChange={handleImageChange}
                            className="hidden"
                        />
                        <button
                            onClick={() => fileInputRef.current?.click()}
                            className="mt-3 px-3 py-2 text-sm bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 flex items-center gap-2"
                        >
                            <ImageIcon size={16} />
                            Mudar imagem de capa
                        </button>
                    </div>

                    {/* Coluna Direita: Campos */}
                    <div className="flex-1 grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div className="md:col-span-2">
                            <label className="block text-sm font-semibold">Título da Edição</label>
                            <textarea
                                name="volumeTitulo"
                                value={formData.volumeTitulo}
                                onChange={handleFormChange}
                                placeholder="Título da edição"
                                className="w-full p-2 border border-gray-300 rounded-md mt-1 resize-none"
                                rows={2}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Edição (Nº)</label>
                            <input
                                type="number"
                                name="edicao"
                                value={formData.edicao}
                                onChange={handleFormChange}
                                placeholder="1"
                                className="w-full p-2 border border-gray-300 rounded-md mt-1"
                            />
                        </div>
                        <div className="md:col-span-3">
                            <label className="block text-sm font-semibold">Resumo da Edição</label>
                            <textarea
                                name="volumeResumo"
                                value={formData.volumeResumo}
                                onChange={handleFormChange}
                                placeholder="Resumo da edição"
                                className="w-full p-2 border border-gray-300 rounded-md mt-1 resize-none"
                                rows={3}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Status</label>
                            <select name="status" value={formData.status} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1 bg-white">
                                {Object.values(StatusVolume).map(s => <option key={s} value={s}>{s}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Mês</label>
                            <select name="m" value={formData.m} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1 bg-white">
                                {Object.values(MesVolume).map(m => <option key={m} value={m}>{m}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Ano</label>
                            <input type="number" name="year" value={formData.year} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1" />
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Volume (N)</label>
                            <input type="number" name="n" min="0" max="10" value={formData.n} onChange={handleFormChange} className="w-full p-2 border border-gray-300 rounded-md mt-1" />
                        </div>
                    </div>
                </div>

                {/* Seção de Artigos */}
                <div className="mt-6 pt-6 border-t">
                    <h4 className="text-lg font-semibold mb-4">Artigos neste Volume</h4>

                    {/* Lista de Artigos Atuais */}
                    {loadingArtigos ? <p>Carregando artigos...</p> : (
                        <ul className="space-y-2 mb-4">
                            {artigosNoVolume.map(art => (
                                <li key={art.id} className="flex justify-between items-center p-2 bg-gray-50 rounded-md border">
                                    <div className="flex items-center gap-2">
                                        <div className="relative w-10 h-10 rounded overflow-hidden bg-gray-200">
                                            {art.midiaDestaque?.url && <Image src={art.midiaDestaque.url} alt={art.titulo} fill className="object-cover" />}
                                        </div>
                                        <span className="text-sm font-medium">{art.titulo}</span>
                                    </div>
                                    <button onClick={() => handleRemoveArtigo(art.id)} className="text-red-500 hover:text-red-700 p-1">
                                        <XCircle size={18} />
                                    </button>
                                </li>
                            ))}
                        </ul>
                    )}

                    {/* Adicionar Novo Artigo */}
                    <div className="relative">
                        <div className="flex gap-2">
                            <input
                                type="text"
                                value={artigoSearchTerm}
                                onChange={(e) => {
                                    setArtigoSearchTerm(e.target.value);
                                    handleArtigoSearch();
                                }}
                                placeholder="Titulo do artigo a ser adicionado"
                                className="flex-1 p-2 border border-gray-300 rounded-md"
                            />
                            <button
                                type="button"
                                onClick={handleAddArtigo}
                                disabled={!selectedArtigo}
                                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
                            >
                                <Plus size={18} />
                            </button>
                        </div>
                        {/* Pop-up de Busca de Artigo */}
                        {artigoSearchResults.length > 0 && (
                            <div className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto p-2">
                                <p className="text-xs text-gray-600 px-2 pb-2 border-b">Este é o resultado da busca. Clique no título do artigo para escolher o artigo.</p>
                                <ul className="divide-y divide-gray-100">
                                    {artigoSearchResults.map(art => (
                                        <li
                                            key={art.id}
                                            onClick={() => handleSelectArtigo(art)}
                                            className="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer"
                                        >
                                            <div className="relative w-10 h-10 rounded overflow-hidden bg-gray-200 flex-shrink-0">
                                                {art.midiaDestaque?.url && <Image src={art.midiaDestaque.url} alt={art.titulo} fill className="object-cover" />}
                                            </div>
                                            <span className="text-sm font-medium">{art.titulo}</span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        )}
                    </div>
                </div>

                {/* Botões de Ação Finais */}
                <div className="flex justify-center gap-4 mt-8 pt-6 border-t">
                    <button
                        onClick={handleCancel}
                        disabled={loading}
                        className="px-6 py-2 rounded-lg border border-gray-300 bg-gray-100 text-gray-700 font-medium hover:bg-gray-200 transition"
                    >
                        Descartar
                    </button>
                    <button
                        onClick={() => setIsModalOpen(true)}
                        disabled={loading}
                        className="px-6 py-2 rounded-lg bg-emerald-600 text-white font-bold shadow hover:bg-emerald-700 transition"
                    >
                        <Save size={18} className="inline mr-2" />
                        Salvar
                    </button>
                </div>
            </li>
        </>
    );
}