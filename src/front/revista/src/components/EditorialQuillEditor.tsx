'use client';

import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import 'react-quill-new/dist/quill.snow.css';
import 'highlight.js/styles/monokai-sublime.css';

import { StaffComentario } from '@/types/index';
import ImageAltModal from './ImageAltModal';
import toast from 'react-hot-toast';
import type { Range } from 'quill';

interface EditorialQuillEditorProps {
    mode: 'edit' | 'comment';
    initialContent: string;
    staffComments?: StaffComentario[];
    onContentChange?: (html: string) => void;
    onMediaChange?: (midia: { id: string; url: string; alt: string }) => void;
    onTextSelect?: (range: Range) => void;
    onHighlightClick?: (comment: StaffComentario) => void;
}

const EditorialQuillEditorInternal = ({
    mode,
    initialContent,
    staffComments = [],
    onContentChange,
    onMediaChange,
    onTextSelect,
    onHighlightClick,
}: EditorialQuillEditorProps) => {
    
    const reactQuillRef = useRef<any>(null);
    const [isAltModalOpen, setIsAltModalOpen] = useState(false);
    const [pendingImage, setPendingImage] = useState<{ id: string; url: string } | null>(null);
    
    // Estado para guardar o componente ReactQuill carregado dinamicamente
    const [QuillComponent, setQuillComponent] = useState<any>(null);

    // --- EFEITO DE INICIALIZAÇÃO ---
    useEffect(() => {
        const loadEditor = async () => {
            if (typeof window === 'undefined') return;

            // 1. Carrega Highlight.js
            const hljsModule = await import('highlight.js');
            const hljs = hljsModule.default || hljsModule;
            (window as any).hljs = hljs;

            // 2. Carrega React Quill
            const RQModule = await import('react-quill-new');
            const ReactQuill = RQModule.default || RQModule;
            const Quill = ReactQuill.Quill || RQModule.Quill;

            // 3. Configurações do Quill
            if (!(window as any).QuillConfigured) {
                if (!Quill.imports['modules/syntax']) {
                     // @ts-ignore
                    Quill.register('modules/syntax', true);
                }

                const Inline = Quill.import('blots/inline');
                if (!Quill.imports['formats/highlight']) {
                    // @ts-ignore
                    class HighlightBlot extends (Inline as any) {
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
                    // @ts-ignore
                    Quill.register(HighlightBlot, true);
                }
                (window as any).QuillConfigured = true;
            }

            setQuillComponent(() => ReactQuill);
        };

        loadEditor();
    }, []);

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

    const modules = useMemo(() => ({
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
    }), [imageHandler]);

    // --- CORREÇÃO: Handler Nativo de Seleção ---
    // Usamos este método na prop onChangeSelection em vez de addEventListener manual
    const handleSelectionChange = (range: any, source: any, editor: any) => {
        if (mode === 'comment' && onTextSelect) {
            // source === 'user' garante que foi o mouse/teclado do usuário
            // range.length > 0 garante que algo foi selecionado (não é apenas um clique)
            if (source === 'user' && range && range.length > 0) {
                console.log("Texto selecionado:", range); // Debug
                onTextSelect(range);
            }
        }
    };

    // Efeitos APENAS para Highlights visuais e Cliques em comentários já existentes
    useEffect(() => {
        if (!QuillComponent || !reactQuillRef.current) return;
        const editor = reactQuillRef.current.getEditor();

        // Aplica Highlights (amarelo)
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

        // Clique em um Highlight existente
        if (mode === 'comment' && onHighlightClick) {
            const clickHandler = (e: any) => {
                let node = e.target;
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

        // REMOVIDO: O ouvinte manual de 'selection-change' daqui.
        // Agora usamos a prop onChangeSelection abaixo.

    }, [mode, staffComments, onHighlightClick, QuillComponent]);

    if (!QuillComponent) {
        return <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse flex items-center justify-center text-gray-400">Inicializando Editor...</div>;
    }

    return (
        <>
            <ImageAltModal isOpen={isAltModalOpen} onConfirm={handleAltConfirm} />
            <div className="bg-white rounded-lg border border-gray-300 editorial-editor-container">
                <QuillComponent
                    ref={reactQuillRef}
                    theme="snow"
                    value={initialContent || ''}
                    onChange={onContentChange}
                    
                    // --- CORREÇÃO: Prop nativa é mais confiável ---
                    onChangeSelection={handleSelectionChange}
                    
                    readOnly={mode === 'comment'} // Garante que seja readOnly, mas permita seleção
                    modules={mode === 'edit' ? modules : { ...modules, toolbar: false }}
                    style={{ minHeight: '60vh' }}
                />
            </div>
        </>
    );
};

import dynamic from 'next/dynamic';

export default dynamic(() => Promise.resolve(EditorialQuillEditorInternal), {
    ssr: false,
    loading: () => <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse flex items-center justify-center text-gray-400">Carregando Editor...</div>
});