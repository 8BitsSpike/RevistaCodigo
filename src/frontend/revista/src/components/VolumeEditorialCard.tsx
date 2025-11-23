'use client';
import { useState, useRef, ChangeEvent, useEffect } from 'react';
import { useMutation, useLazyQuery } from '@apollo/client/react';
import { ATUALIZAR_METADADOS_VOLUME, CRIAR_VOLUME, OBTER_VOLUME_POR_ID, GET_ARTIGOS_BY_IDS, SEARCH_ARTIGOS_EDITORIAL_BY_TITLE } from '@/graphql/queries';
import { Edit, Save, XCircle, Image as ImageIcon, Plus } from 'lucide-react';
import CommentaryModal from './CommentaryModal';
import { StatusVolume, MesVolume } from '@/types/enums';
import toast from 'react-hot-toast';
import Image from 'next/image';
export interface VolumeCardData { 
    id: string; 
    volumeTitulo: string; 
    volumeResumo: string; 
    imagemCapa?: { idMidia?: string; url: string; textoAlternativo: string; } | null; 
}

interface VolumeFormData { 
    edicao: number; 
    volumeTitulo: string; 
    volumeResumo: string; 
    m: MesVolume; 
    n: number; 
    year: number; 
    status: StatusVolume; 
    imagemCapa?: { url: string; textoAlternativo: string; idMidia?: string; } | null; 
    artigoIds: string[]; 
}

interface ArtigoNaLista { id: string; titulo: string; midiaDestaque?: { url: string; textoAlternativo: string; } | null; }
interface VolumeEditorialCardProps { mode: 'view' | 'create'; initialData?: VolumeCardData; onUpdate: () => void; }

export default function VolumeEditorialCard({ mode, initialData, onUpdate }: VolumeEditorialCardProps) {
    const [isEditing, setIsEditing] = useState(mode === 'create');
    const [isModalOpen, setIsModalOpen] = useState(false);
    
    const [formData, setFormData] = useState<Partial<VolumeFormData>>({ 
        volumeTitulo: initialData?.volumeTitulo || '', 
        volumeResumo: initialData?.volumeResumo || '', 
        status: StatusVolume.EmRevisao, 
        m: MesVolume.Janeiro, 
        edicao: 1,
        n: 1, 
        year: new Date().getFullYear(), 
        artigoIds: [] 
    });

    const [artigosNoVolume, setArtigosNoVolume] = useState<ArtigoNaLista[]>([]);
    const [newCoverImage, setNewCoverImage] = useState<string | null>(initialData?.imagemCapa?.url || null);
    const [artigoSearchTerm, setArtigoSearchTerm] = useState('');
    const [artigoSearchResults, setArtigoSearchResults] = useState<ArtigoNaLista[]>([]);
    const [selectedArtigo, setSelectedArtigo] = useState<ArtigoNaLista | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (mode === 'view' && initialData) {
            setNewCoverImage(initialData.imagemCapa?.url || null);
        }
    }, [mode, initialData]);

    const [loadVolumeData, { loading: loadingData }] = useLazyQuery(OBTER_VOLUME_POR_ID, {
        fetchPolicy: 'network-only',
        onCompleted: (data) => {
            if (data.obterVolumePorId) {
                setFormData({
                    ...data.obterVolumePorId,
                    edicao: Number(data.obterVolumePorId.edicao),
                    n: Number(data.obterVolumePorId.n),
                    year: Number(data.obterVolumePorId.year),
                    imagemCapa: data.obterVolumePorId.imagemCapa
                });
                setNewCoverImage(data.obterVolumePorId.imagemCapa?.url || null);
                if (data.obterVolumePorId.artigoIds?.length > 0) {
                    loadArtigoTitles({ variables: { ids: data.obterVolumePorId.artigoIds } });
                }
            }
        }, onError: (err) => toast.error(err.message)
    });
    
    const [loadArtigoTitles, { loading: loadingArtigos }] = useLazyQuery(GET_ARTIGOS_BY_IDS, { onCompleted: (data) => setArtigosNoVolume(data.obterArtigoCardListPorLista) });
    const [runArtigoSearch] = useLazyQuery(SEARCH_ARTIGOS_EDITORIAL_BY_TITLE, { onCompleted: (data) => setArtigoSearchResults(data.searchArtigosEditorialByTitle || []) });
    
    const [criarVolume, { loading: loadingCreate }] = useMutation(CRIAR_VOLUME, { 
        onCompleted: () => { toast.success("Volume criado!"); setIsModalOpen(false); onUpdate(); }, 
        onError: (err) => { toast.error("Erro ao criar: " + err.message); setIsModalOpen(false); } 
    });
    
    const [atualizarVolume, { loading: loadingUpdate }] = useMutation(ATUALIZAR_METADADOS_VOLUME, { 
        onCompleted: () => { toast.success("Volume atualizado!"); setIsEditing(false); setIsModalOpen(false); onUpdate(); }, 
        onError: (err) => { toast.error("Erro ao atualizar: " + err.message); setIsModalOpen(false); } 
    });

    const loading = loadingData || loadingArtigos || loadingCreate || loadingUpdate;

    const handleEditClick = () => { 
        if (initialData?.id) { 
            loadVolumeData({ variables: { idVolume: initialData.id } }); 
            setIsEditing(true); 
        } 
    };
    const handleCancel = () => { if (mode === 'create') onUpdate(); else setIsEditing(false); };
    
    const handleFormChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => { 
        const { name, value } = e.target; 
        if (name === 'n' || name === 'year' || name === 'edicao') setFormData(prev => ({ ...prev, [name]: parseInt(value) || 0 })); 
        else setFormData(prev => ({ ...prev, [name]: value })); 
    };
    
    const handleImageChange = (e: ChangeEvent<HTMLInputElement>) => { 
        const file = e.target.files?.[0]; 
        if (file) { 
            const reader = new FileReader(); 
            reader.onloadend = () => setNewCoverImage(reader.result as string); 
            reader.readAsDataURL(file); 
        } 
    };

    const handleArtigoSearch = () => { if (artigoSearchTerm.length < 3) return; runArtigoSearch({ variables: { searchTerm: artigoSearchTerm, pagina: 0, tamanho: 5 } }); };
    const handleSelectArtigo = (artigo: ArtigoNaLista) => { setSelectedArtigo(artigo); setArtigoSearchTerm(artigo.titulo); setArtigoSearchResults([]); };
    const handleAddArtigo = () => { if (selectedArtigo && !formData.artigoIds?.includes(selectedArtigo.id)) { setArtigosNoVolume(prev => [...prev, selectedArtigo]); setFormData(prev => ({ ...prev, artigoIds: [...(prev.artigoIds || []), selectedArtigo.id] })); setSelectedArtigo(null); setArtigoSearchTerm(''); } };
    const handleRemoveArtigo = (id: string) => { setArtigosNoVolume(artigosNoVolume.filter(a => a.id !== id)); setFormData(prev => ({ ...prev, artigoIds: (prev.artigoIds || []).filter(artId => artId !== id) })); };

    const handleConfirmSubmit = (commentary: string) => {
        let imagemCapaInput = null;

        if (newCoverImage && newCoverImage.startsWith('data:image')) {
             imagemCapaInput = {
                midiaID: `capa-${Date.now()}`, 
                url: newCoverImage, 
                alt: `Capa ${formData.volumeTitulo}` 
             };
        } else if (formData.imagemCapa) {
             imagemCapaInput = {
                 midiaID: formData.imagemCapa.idMidia || `capa-existente-${Date.now()}`,
                 url: formData.imagemCapa.url,
                 alt: formData.imagemCapa.textoAlternativo || ''
             };
        }

        const baseData = {
            edicao: Number(formData.edicao), 
            volumeTitulo: formData.volumeTitulo, 
            volumeResumo: formData.volumeResumo, 
            m: formData.m as MesVolume, 
            n: Number(formData.n), 
            year: Number(formData.year),
            imagemCapa: imagemCapaInput
        };

        if (mode === 'create') { 

            const createInput = {
                ...baseData

            };

            toast.loading('Criando...', { id: 'vol' }); 
            criarVolume({ variables: { input: createInput, commentary } })
                .then(() => { /* onCompleted handles this */ })
                .catch(err => console.error("Erro criar:", err))
                .finally(() => toast.dismiss('vol')); 

        } else { 

            const updateInput = {
                ...baseData,
                status: formData.status as StatusVolume, 
                artigoIds: formData.artigoIds || []
            };

            toast.loading('Atualizando...', { id: 'vol' }); 
            atualizarVolume({ variables: { volumeId: initialData!.id, input: updateInput, commentary } })
                .finally(() => toast.dismiss('vol')); 
        }
    };

    if (mode === 'view' && !isEditing) {
        if (!initialData) return null;
        return (
            <li className="card-std flex items-center" style={{ width: '98%', margin: '10px 1%', padding: '1% 0.5%' }}>
                <div className="w-[10%] flex-shrink-0 relative min-h-[60px]">
                    <Image src={initialData.imagemCapa?.url || '/faviccon.png'} alt={initialData.imagemCapa?.textoAlternativo || initialData.volumeTitulo} fill className="object-cover rounded-md" />
                </div>
                <div className="flex-1 p-3 min-w-0 mx-4">
                    <strong className="text-gray-800 truncate block">{initialData.volumeTitulo}</strong>
                    <p className="text-gray-600 text-sm mt-1 line-clamp-2">{initialData.volumeResumo}</p>
                </div>
                <button onClick={handleEditClick} className="btn-secondary text-sm mr-4"><Edit size={14} /> Editar</button>
            </li>
        );
    }

    return (
        <>
            <CommentaryModal isOpen={isModalOpen} title={mode === 'create' ? "Justificar Criação" : "Justificar Atualização"} loading={loadingCreate || loadingUpdate} onClose={() => setIsModalOpen(false)} onSubmit={handleConfirmSubmit} />
            <li className="card-std" style={{ width: '98%', margin: '10px 1%', padding: '1.5rem' }}>
                {loadingData && <p>Carregando...</p>}
                <div className="flex flex-col md:flex-row gap-6">
                    <div className="w-full md:w-1/4 flex flex-col items-center">
                        <div className="relative w-[150px] h-[150px] rounded-md overflow-hidden bg-gray-200 border">
                            <Image src={newCoverImage || '/faviccon.png'} alt="Imagem de Capa" fill className="object-cover" />
                        </div>
                        <input type="file" accept="image/*" ref={fileInputRef} onChange={handleImageChange} className="hidden" />
                        <button onClick={() => fileInputRef.current?.click()} className="mt-3 btn-secondary text-sm"><ImageIcon size={16} /> Mudar imagem</button>
                    </div>
                    <div className="flex-1 grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div className="md:col-span-2"><label className="block text-sm font-semibold">Título</label><textarea name="volumeTitulo" value={formData.volumeTitulo} onChange={handleFormChange} className="input-std mt-1 resize-none" rows={2} /></div>
                        <div><label className="block text-sm font-semibold">Edição (Nº)</label><input type="number" name="edicao" value={formData.edicao} onChange={handleFormChange} className="input-std mt-1" /></div>
                        <div className="md:col-span-3"><label className="block text-sm font-semibold">Resumo</label><textarea name="volumeResumo" value={formData.volumeResumo} onChange={handleFormChange} className="input-std mt-1 resize-none" rows={3} /></div>
                        <div>
                            <label className="block text-sm font-semibold">Status</label>
                            <select name="status" value={formData.status} onChange={handleFormChange} className="input-std mt-1">
                                {Object.values(StatusVolume).map(s => <option key={s} value={s}>{s}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-semibold">Mês</label>
                            <select name="m" value={formData.m} onChange={handleFormChange} className="input-std mt-1">
                                {Object.values(MesVolume).map(m => <option key={m} value={m}>{m}</option>)}
                            </select>
                        </div>
                        <div><label className="block text-sm font-semibold">Ano</label><input type="number" name="year" value={formData.year} onChange={handleFormChange} className="input-std mt-1" /></div>
                        <div><label className="block text-sm font-semibold">Volume (N)</label><input type="number" name="n" min="0" max="10" value={formData.n} onChange={handleFormChange} className="input-std mt-1" /></div>
                    </div>
                </div>
                
                <div className="mt-6 pt-6 border-t">
                    <h4 className="text-lg font-semibold mb-4">Artigos neste Volume</h4>
                    {loadingArtigos ? <p>Carregando...</p> : (
                        <ul className="space-y-2 mb-4">
                            {artigosNoVolume.map(art => (
                                <li key={art.id} className="flex justify-between items-center p-2 bg-gray-50 rounded-md border">
                                    <div className="flex items-center gap-2">
                                        <div className="relative w-10 h-10 rounded overflow-hidden bg-gray-200">
                                            <Image src={art.midiaDestaque?.url || '/faviccon.png'} alt={art.titulo} fill className="object-cover" />
                                        </div>
                                        <span className="text-sm font-medium">{art.titulo}</span>
                                    </div>
                                    <button onClick={() => handleRemoveArtigo(art.id)} className="text-red-500 hover:text-red-700 p-1"><XCircle size={18} /></button>
                                </li>
                            ))}
                        </ul>
                    )}
                    <div className="relative">
                        <div className="flex gap-2">
                            <label htmlFor="add-article" className="sr-only">Adicionar artigo</label>
                            <input id="add-article" type="text" value={artigoSearchTerm} onChange={(e) => { setArtigoSearchTerm(e.target.value); handleArtigoSearch(); }} placeholder="Titulo do artigo..." className="input-std" />
                            <button type="button" onClick={handleAddArtigo} disabled={!selectedArtigo} className="btn-primary"><Plus size={18} /></button>
                        </div>
                        {artigoSearchResults.length > 0 && (
                            <div className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto p-2">
                                <ul className="divide-y divide-gray-100">
                                    {artigoSearchResults.map(art => (
                                        <li key={art.id} onClick={() => handleSelectArtigo(art)} className="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer">
                                            <div className="relative w-10 h-10 rounded overflow-hidden bg-gray-200 flex-shrink-0">
                                                <Image src={art.midiaDestaque?.url || '/faviccon.png'} alt={art.titulo} fill className="object-cover" />
                                            </div>
                                            <span className="text-sm font-medium">{art.titulo}</span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        )}
                    </div>
                </div>
                
                <div className="flex justify-center gap-4 mt-8 pt-6 border-t">
                    <button onClick={handleCancel} disabled={loading} className="btn-secondary">Descartar</button>
                    <button onClick={() => setIsModalOpen(true)} disabled={loading} className="btn-primary"><Save size={18} /> Salvar</button>
                </div>
            </li>
        </>
    );
}