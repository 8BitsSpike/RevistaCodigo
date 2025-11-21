'use client';

import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import dynamic from 'next/dynamic';
import 'react-quill/dist/quill.snow.css';
import 'highlight.js/styles/monokai-sublime.css';
import { StaffComentario } from '@/types/index';
import ImageAltModal from './ImageAltModal';
import toast from 'react-hot-toast';
// Apenas tipos para o TypeScript não reclamar. Não gera código JS.
import type ReactQuillType from 'react-quill';
import type { Range } from 'quill';

// --- MUDANÇA 1: Importação dinâmica padrão do componente ---
// Não tentamos fazer mágica aqui dentro. Apenas carregamos o componente visual.
const ReactQuill = dynamic(() => import('react-quill'), {
    ssr: false,
    loading: () => <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse flex items-center justify-center text-gray-400">Carregando Editor...</div>,
});

interface EditorialQuillEditorProps {
    mode: 'edit' | 'comment';
    initialContent: string;
    staffComments?: StaffComentario[];
    onContentChange?: (html: string) => void;
    onMediaChange?: (midia: { id: string; url: string; alt: string }) => void;
    onTextSelect?: (range: Range) => void;
    onHighlightClick?: (comment: StaffComentario) => void;
}

export default function EditorialQuillEditor({
    mode,
    initialContent,
    staffComments = [],
    onContentChange,
    onMediaChange,
    onTextSelect,
    onHighlightClick,
}: EditorialQuillEditorProps) {
    const reactQuillRef = useRef<ReactQuillType>(null);
    const [isAltModalOpen, setIsAltModalOpen] = useState(false);
    const [pendingImage, setPendingImage] = useState<{ id: string; url: string } | null>(null);
    // Estado para controlar se o Quill e seus módulos já foram carregados
    const [quillLoaded, setQuillLoaded] = useState(false);

    // --- MUDANÇA 2: O Coração da Solução ---
    // Tudo que toca no objeto 'Quill' ou 'document' acontece AQUI DENTRO.
    useEffect(() => {
        // Proteção dupla: só roda no navegador e se o ref estiver pronto
        if (typeof window === 'undefined' || !reactQuillRef.current) return;

        // 1. Importação SEGURA das bibliotecas usando 'require'
        // O servidor Next.js nunca verá isso.
        const Quill = require('quill');
        const hljs = require('highlight.js');

        // 2. Configura o highlight.js na janela global para o ReactQuill encontrar
        // @ts-ignore
        window.hljs = hljs;

        // 3. Registra o Módulo de Sintaxe (se ainda não existir)
        if (!Quill.imports['modules/syntax']) {
             // @ts-ignore
            Quill.register('modules/syntax', true);
        }

        // 4. Registra o HighlightBlot Customizado (se ainda não existir)
        if (!Quill.imports['formats/highlight']) {
            const Inline = Quill.import('blots/inline');
            class HighlightBlot extends Inline {
                static blotName = 'highlight';
                static tagName = 'span';
                static create(value: string) {
                    const node = super.create();
                    node.setAttribute('data-comment-id', value);
                    node.style.backgroundColor = '#FFF9C4';
                    node.style.cursor = 'pointer';
                    return node;
                }
            }
            Quill.register(HighlightBlot, true);
        }

        // Marca como carregado para liberar a renderização dos módulos
        setQuillLoaded(true);

    }, []); // Roda apenas uma vez na montagem do componente no cliente

    // --- Handlers (Imagem, etc) ---
    const imageHandler = useCallback(() => {
        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();
        input.onchange = () => {
            const file = input.files ? input.files[0] : null;
            if (file) {
                const reader = new FileReader();
                reader.onloadstart = () => toast.loading('Processando...', { id: 'img' });
                reader.onloadend = () => {
                    setPendingImage({ id: `img-${Date.now()}`, url: reader.result as string });
                    toast.dismiss('img');
                    setIsAltModalOpen(true);
                };
                reader.readAsDataURL(file);
            }
        };
    }, []);

    const handleAltConfirm = (alt: string) => {
        const editor = reactQuillRef.current?.getEditor();
        if (pendingImage && editor) {
            if (onMediaChange) onMediaChange({ ...pendingImage, alt });
            const range = editor.getSelection(true);
            // @ts-ignore
            editor.insertEmbed(range.index, 'image', pendingImage.url);
            // @ts-ignore
            editor.setSelection(range.index + 1, 0);
            toast.success('Imagem adicionada');
        }
        setIsAltModalOpen(false);
        setPendingImage(null);
    };

    // --- Configuração dos Módulos (Memoizado) ---
    const modules = useMemo(() => {
        // Só retorna a configuração se o Quill já estiver carregado e registrado
        if (!quillLoaded) return {};

        return {
            // Agora é seguro usar, pois o hljs foi colocado no window pelo useEffect
            syntax: true, 
            toolbar: {
                container: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike', 'blockquote', 'code-block'],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['link', 'image'],
                    ['clean']
                ],
                handlers: { image: imageHandler }
            },
            history: { userOnly: true }
        };
    }, [quillLoaded, imageHandler]);

    // --- Efeitos de Eventos (Clique e Seleção) ---
    useEffect(() => {
        if (!reactQuillRef.current || !quillLoaded) return;
        const editor = reactQuillRef.current.getEditor();

        // Aplica Highlights Iniciais
        if (mode === 'comment' && staffComments.length > 0) {
            setTimeout(() => {
                staffComments.forEach(c => {
                    try {
                        if (c.comment.startsWith('{')) {
                            const data = JSON.parse(c.comment);
                            if (data?.selection) {
                                // @ts-ignore
                                editor.formatText(data.selection.index, data.selection.length, 'highlight', c.id, 'silent');
                            }
                        }
                    } catch (e) {}
                });
            }, 500);
        }

        // Handler de Clique nos Destaques
        if (mode === 'comment' && onHighlightClick) {
            const clickHandler = (e: any) => {
                let node = e.target;
                // @ts-ignore
                while (node && node !== editor.root) {
                    if (node.tagName === 'SPAN' && node.getAttribute('data-comment-id')) {
                        const id = node.getAttribute('data-comment-id');
                        const comment = staffComments.find(c => c.id === id);
                        if (comment) onHighlightClick(comment);
                        return;
                    }
                    node = node.parentElement;
                }
            };
            // @ts-ignore
            editor.root.addEventListener('click', clickHandler);
            // @ts-ignore
            return () => editor.root.removeEventListener('click', clickHandler);
        }

        // Handler de Seleção de Texto
        if (mode === 'comment' && onTextSelect) {
            const selHandler = (range: Range, old: Range, source: string) => {
                if (source === 'user' && range && range.length > 0) onTextSelect(range);
            };
            editor.on('selection-change', selHandler);
            return () => editor.off('selection-change', selHandler);
        }
    }, [mode, staffComments, onHighlightClick, onTextSelect, quillLoaded]);

    // Se não estiver carregado, mostra o loading (evita renderizar o ReactQuill sem os módulos prontos)
    if (!quillLoaded) {
        return <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse flex items-center justify-center text-gray-400">Inicializando Editor...</div>;
    }
const QuillComponent = ReactQuill as any;

    return (
        <>
            <ImageAltModal isOpen={isAltModalOpen} onConfirm={handleAltConfirm} />
            <div className="bg-white rounded-lg border border-gray-300 editorial-editor-container">
                <QuillComponent
                    ref={reactQuillRef} // Agora o erro vai sumir
                    theme="snow"
                    value={initialContent || ''}
                    onChange={onContentChange}
                    readOnly={mode === 'comment'}
                    modules={mode === 'edit' ? modules : { ...modules, toolbar: false }}
                    style={{ minHeight: '60vh' }}
                />
            </div>
        </>
    );
}