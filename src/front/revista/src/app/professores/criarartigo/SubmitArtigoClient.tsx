'use client';

import React, { useState, useRef, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Image from 'next/image';
import { useMutation } from '@apollo/client/react';
import { CRIAR_ARTIGO, GET_MEUS_ARTIGOS } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ImageAltModal from '@/components/ImageAltModal';
import { Check, X, Trash2, Plus, UploadCloud } from 'lucide-react';
import 'quill/dist/quill.snow.css';
import type Quill from 'quill';
import toast from 'react-hot-toast'; // (NOVO) Importa o toast

// --- Interfaces ---
interface UsuarioBusca {
    id: string; // Do UsuarioAPI
    name: string;
    sobrenome?: string;
    foto?: string;
}

interface MidiaEntry {
    midiaID: string;
    url: string;
    alt: string;
}

interface CriarArtigoData {
    criarArtigo: {
        id: string;
        titulo: string;
        status: string;
        editorial: {
            id: string;
        };
    };
}

const API_USUARIO_BASE = 'https://localhost:44387/api/Usuario';

export default function SubmitArtigoClient() {
    const router = useRouter();
    const { user } = useAuth();

    // --- Estados do Formulário ---
    const [titulo, setTitulo] = useState('');
    const [resumo, setResumo] = useState('');
    const [quillContent, setQuillContent] = useState('');

    const [selectedAuthors, setSelectedAuthors] = useState<UsuarioBusca[]>([]);
    const [authorSearchQuery, setAuthorSearchQuery] = useState('');
    const [authorSearchResults, setAuthorSearchResults] = useState<UsuarioBusca[]>([]);

    const [externalRefs, setExternalRefs] = useState<string[]>([]);
    const [newRef, setNewRef] = useState('');

    const [midias, setMidias] = useState<MidiaEntry[]>([]);
    const [capaPreview, setCapaPreview] = useState<string | null>(null);

    const [isAltModalOpen, setIsAltModalOpen] = useState(false);
    const [pendingImage, setPendingImage] = useState<{ id: string; url: string } | null>(null);

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');

    const quillRef = useRef<HTMLDivElement>(null);
    const quillInstance = useRef<Quill | null>(null);
    const initializedRef = useRef(false);

    // (MODIFICADO) Mutation com handlers de toast
    const [criarArtigo] = useMutation<CriarArtigoData>(CRIAR_ARTIGO, {
        onCompleted: (data) => {
            if (data?.criarArtigo?.id) {
                toast.success('Artigo enviado com sucesso!');
                router.push('/sessoes-especiais');
            } else {
                toast.error('Falha ao enviar artigo. Resposta inesperada.');
            }
        },
        onError: (err) => {
            console.error("Erro ao criar artigo", err);
            setErrorMsg(err.message || "Erro ao enviar artigo.");
            toast.error(`Erro ao enviar: ${err.message}`);
        },
        // (NOVO) Atualiza a query de "meus artigos" no cache
        refetchQueries: [
            { query: GET_MEUS_ARTIGOS }
        ]
    });

    // --- 1. Busca de Usuários (Co-autores) ---
    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (authorSearchQuery.length < 3) {
                setAuthorSearchResults([]);
                return;
            }

            const token = localStorage.getItem('jwtToken');
            if (!token) return;

            try {
                const res = await fetch(`${API_USUARIO_BASE}/Search?name=${authorSearchQuery}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) {
                    const data = await res.json();
                    const filtered = data.filter((u: any) =>
                        u.id !== user?.id &&
                        !selectedAuthors.some(sel => sel.id === u.id)
                    );
                    setAuthorSearchResults(filtered);
                }
            } catch (err) {
                console.error("Erro buscando usuários", err);
            }
        }, 500);

        return () => clearTimeout(delayDebounceFn);
    }, [authorSearchQuery, selectedAuthors, user?.id]);

    const addAuthor = (author: UsuarioBusca) => {
        setSelectedAuthors([...selectedAuthors, author]);
        setAuthorSearchQuery('');
        setAuthorSearchResults([]);
    };

    const removeAuthor = (id: string) => {
        setSelectedAuthors(selectedAuthors.filter(a => a.id !== id));
    };

    // --- 2. Referências Notáveis ---
    const addExternalRef = () => {
        if (newRef.trim()) {
            setExternalRefs([...externalRefs, newRef.trim()]);
            setNewRef('');
        }
    };

    const removeExternalRef = (index: number) => {
        setExternalRefs(externalRefs.filter((_, i) => i !== index));
    };

    // --- 3. Configuração do Quill e Manipulação de Imagem ---

    const imageHandler = () => {
        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();

        input.onchange = () => {
            const file = input.files ? input.files[0] : null;
            if (file) {
                const reader = new FileReader();
                reader.onloadstart = () => {
                    toast.loading('Processando imagem...', { id: 'image-upload' });
                }
                reader.onloadend = () => {
                    const base64 = reader.result as string;
                    const tempId = `img-${Date.now()}`;

                    setPendingImage({
                        id: tempId,
                        url: base64
                    });
                    toast.dismiss('image-upload');
                    setIsAltModalOpen(true);
                };
                reader.onerror = () => {
                    toast.error('Falha ao ler imagem.', { id: 'image-upload' });
                }
                reader.readAsDataURL(file);
            }
        };
    };

    const handleAltTextConfirm = (altText: string) => {
        if (!pendingImage || !quillInstance.current) return;

        const newMedia: MidiaEntry = {
            midiaID: pendingImage.id,
            url: pendingImage.url,
            alt: altText
        };
        setMidias(prev => [...prev, newMedia]);

        if (midias.length === 0) {
            setCapaPreview(pendingImage.url);
        }

        const range = quillInstance.current.getSelection(true);
        quillInstance.current.insertEmbed(range.index, 'image', pendingImage.url);
        quillInstance.current.setSelection(range.index + 1);

        setIsAltModalOpen(false);
        setPendingImage(null);
        toast.success('Imagem adicionada com acessibilidade!');
    };

    useEffect(() => {
        if (initializedRef.current) return;

        const initializeQuill = async () => {
            const { default: QuillModule } = await import('quill');

            if (quillRef.current && !quillInstance.current) {
                const toolbarOptions = {
                    container: [
                        [{ 'header': [1, 2, 3, false] }],
                        ['bold', 'italic', 'underline', 'strike'],
                        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                        ['blockquote', 'code-block'],
                        [{ 'color': [] }, { 'background': [] }],
                        ['link', 'image'],
                        ['clean']
                    ],
                    handlers: {
                        image: imageHandler
                    }
                };

                const quill = new QuillModule(quillRef.current, {
                    theme: 'snow',
                    placeholder: 'Escreva o conteúdo do artigo aqui...',
                    modules: {
                        toolbar: toolbarOptions,
                    },
                });

                quillInstance.current = quill;
                initializedRef.current = true;

                quill.on('text-change', () => {
                    setQuillContent(quill.root.innerHTML);
                });
            }
        };

        initializeQuill();
    }, []);


    // --- 4. Submissão e Deleção ---

    const handleSubmit = async () => {
        if (!titulo.trim() || !resumo.trim() || quillContent === '<p><br></p>' || !quillContent.trim()) {
            const msg = "Por favor, preencha o título, resumo e conteúdo do artigo.";
            setErrorMsg(msg);
            toast.error(msg);
            return;
        }

        setIsSubmitting(true);
        setErrorMsg('');
        toast.loading('Enviando seu artigo...', { id: 'submit-artigo' }); // (NOVO)

        const autoresInput = selectedAuthors.map(a => ({
            usuarioId: a.id,
            nome: `${a.name} ${a.sobrenome || ''}`.trim(),
            url: a.foto || ''
        }));

        const midiasInput = midias.map(m => ({
            midiaID: m.midiaID,
            url: m.url,
            alt: m.alt
        }));

        // (MODIFICADO) Lógica de try/catch não é mais necessária aqui
        // pois é tratada pelos handlers do 'useMutation'
        await criarArtigo({
            variables: {
                input: {
                    titulo,
                    resumo,
                    conteudo: quillContent,
                    tipo: 'Artigo',
                    autores: autoresInput,
                    referenciasAutor: externalRefs,
                    midias: midiasInput
                },
                commentary: "Submissão inicial pelo autor"
            }
        });

        toast.dismiss('submit-artigo'); // Limpa o toast de loading
        setIsSubmitting(false); // O 'finally' foi removido
    };

    const handleDelete = () => {
        if (confirm("Tem certeza? Isso apagará todo o progresso.")) {
            // TODO: Chamar API para deletar as imagens Base64 (se necessário)
            toast.success('Rascunho deletado.');
            router.push('/sessoes-especiais');
        }
    };

    if (!user) return null;

    return (
        <Layout>
            <ImageAltModal
                isOpen={isAltModalOpen}
                onConfirm={handleAltTextConfirm}
            />

            <div className="max-w-4xl mx-auto mt-10 mb-20">
                <h1 className="text-3xl font-bold mb-8 text-gray-800 border-b pb-4">
                    Submeter Novo Artigo
                </h1>

                {/* O 'errorMsg' agora é primariamente tratado pelo toast */}
                {errorMsg && (
                    <div className="mb-6 p-4 bg-red-100 text-red-700 rounded-md border border-red-300">
                        {errorMsg}
                    </div>
                )}

                <div className="space-y-8">

                    {/* Título */}
                    <div>
                        <label className="block text-lg font-semibold text-gray-700 mb-2">Título do Artigo</label>
                        <input
                            type="text"
                            value={titulo}
                            onChange={(e) => setTitulo(e.target.value)}
                            className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none"
                            placeholder="Digite o título principal"
                        />
                    </div>

                    {/* Resumo */}
                    <div>
                        <label className="block text-lg font-semibold text-gray-700 mb-2">Resumo</label>
                        <textarea
                            value={resumo}
                            onChange={(e) => setResumo(e.target.value)}
                            className="w-full p-3 border border-gray-300 rounded-lg h-32 focus:ring-2 focus:ring-emerald-500 outline-none resize-none"
                            placeholder="Digite um resumo curto do artigo..."
                        />
                    </div>

                    {/* --- Área de Autores (RBEB) --- */}
                    <div>
                        <label className="block text-lg font-semibold text-gray-700 mb-2">Autores (RBEB)</label>
                        <p className="text-sm text-gray-500 mb-2">Adicione co-autores que já possuem cadastro.</p>

                        <div className="relative">
                            <input
                                type="text"
                                value={authorSearchQuery}
                                onChange={(e) => setAuthorSearchQuery(e.target.value)}
                                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none"
                                placeholder="Buscar autor por nome..."
                            />
                            {authorSearchResults.length > 0 && (
                                <ul className="absolute top-full left-0 right-0 bg-white border border-gray-200 shadow-lg rounded-md mt-1 z-10 max-h-60 overflow-y-auto">
                                    {authorSearchResults.map(u => (
                                        <li
                                            key={u.id}
                                            onClick={() => addAuthor(u)}
                                            className="flex items-center gap-3 p-3 hover:bg-gray-50 cursor-pointer transition"
                                        >
                                            <div className="w-10 h-10 relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0">
                                                {u.foto ? (
                                                    <Image src={u.foto} alt={u.name} fill className="object-cover" />
                                                ) : (
                                                    <span className="w-full h-full flex items-center justify-center text-gray-500 text-xs">IMG</span>
                                                )}
                                            </div>
                                            <span className="font-medium text-gray-800">{u.name} {u.sobrenome}</span>
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>

                        <div className="flex flex-wrap gap-3 mt-4">
                            <div className="flex items-center gap-2 bg-emerald-50 border border-emerald-200 px-3 py-2 rounded-full">
                                <div className="w-8 h-8 relative rounded-full overflow-hidden bg-gray-200">
                                    {user?.foto && <Image src={user.foto} alt="Eu" fill className="object-cover" />}
                                </div>
                                <span className="text-sm font-medium text-emerald-800">Você (Autor Principal)</span>
                            </div>

                            {selectedAuthors.map(author => (
                                <div
                                    key={author.id}
                                    className="group relative flex items-center gap-2 bg-white border border-gray-300 px-3 py-2 rounded-full cursor-pointer hover:border-red-300 hover:bg-red-50 transition"
                                    onClick={() => removeAuthor(author.id)}
                                >
                                    <span className="text-sm font-medium text-gray-700">{author.name} {author.sobrenome}</span>
                                    <Check size={16} className="text-emerald-600" />

                                    <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block w-max bg-black/80 text-white text-xs px-2 py-1 rounded">
                                        Click para remover
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* --- Referências Notáveis --- */}
                    <div>
                        <label className="block text-lg font-semibold text-gray-700 mb-2">Referências Notáveis</label>
                        <div className="flex gap-2">
                            <input
                                type="text"
                                value={newRef}
                                onChange={(e) => setNewRef(e.target.value)}
                                className="flex-1 p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none"
                                placeholder="Autores que não fazem parte da RBEB que contribuiram para o artigo"
                            />
                            <button
                                type="button"
                                onClick={addExternalRef}
                                className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 rounded-lg"
                            >
                                <Plus />
                            </button>
                        </div>

                        <ul className="mt-3 space-y-2">
                            {externalRefs.map((ref, idx) => (
                                <li key={idx} className="flex justify-between items-center bg-gray-50 p-2 rounded border border-gray-200">
                                    <span className="text-sm text-gray-700">{ref}</span>
                                    <button onClick={() => removeExternalRef(idx)} className="text-red-500 hover:text-red-700">
                                        <X size={16} />
                                    </button>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* --- Editor Quill --- */}
                    <div>
                        <label className="block text-lg font-semibold text-gray-700 mb-2">Conteúdo do Artigo</label>
                        <div className="bg-white rounded-lg overflow-hidden border border-gray-300">
                            <div ref={quillRef} style={{ minHeight: '400px', backgroundColor: 'white' }} />
                        </div>
                        <p className="text-xs text-gray-500 mt-2">
                            Imagens inseridas serão enviadas automaticamente. Clique no ícone de imagem na barra de ferramentas.
                        </p>
                    </div>

                    {capaPreview && (
                        <div className="p-4 bg-gray-50 border border-gray-200 rounded-lg flex items-center gap-4">
                            <div className="w-20 h-20 relative rounded-full overflow-hidden bg-gray-200">
                                <Image src={capaPreview} alt="Capa" fill className="object-cover" />
                            </div>
                            <div>
                                <p className="text-sm font-semibold text-gray-700">Imagem de Capa Definida</p>
                                <p className="text-xs text-gray-500">A primeira imagem inserida será usada como destaque.</p>
                            </div>
                        </div>
                    )}

                    {/* Botões de Ação */}
                    <div className="flex justify-between pt-8 border-t mt-8">
                        <button
                            type="button"
                            onClick={handleDelete}
                            disabled={isSubmitting}
                            className="px-6 py-3 rounded-lg border border-red-200 text-red-600 font-medium hover:bg-red-50 transition flex items-center gap-2"
                        >
                            <Trash2 size={20} />
                            Deletar Artigo
                        </button>

                        <button
                            type="button"
                            onClick={handleSubmit}
                            disabled={isSubmitting}
                            className="px-8 py-3 rounded-lg bg-emerald-600 text-white font-bold shadow-md hover:bg-emerald-700 transition flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
                        >
                            <UploadCloud size={20} />
                            {isSubmitting ? 'Enviando...' : 'Enviar Artigo'}
                        </button>
                    </div>

                </div>
            </div>
        </Layout>
    );
}