'use client';

import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
// CSS Imports
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

    // --- EFEITO DE INICIALIZAÇÃO E ORDEM DE CARREGAMENTO ---
    useEffect(() => {
        const loadEditor = async () => {
            if (typeof window === 'undefined') return;

            // 1. Carrega Highlight.js primeiro
            const hljsModule = await import('highlight.js');
            const hljs = hljsModule.default || hljsModule;
            
            // 2. Atribui ao window OBRIGATORIAMENTE antes de carregar o Quill
            (window as any).hljs = hljs;

            // 3. Carrega React Quill (que vai carregar Quill, que vai procurar window.hljs)
            const RQModule = await import('react-quill-new');
            const ReactQuill = RQModule.default || RQModule;
            const Quill = ReactQuill.Quill || RQModule.Quill;

            // 4. Configurações adicionais do Quill
            if (!(window as any).QuillConfigured) {
                if (!Quill.imports['modules/syntax']) {
                     // @ts-ignore
                    Quill.register('modules/syntax', true);
                }

                const Inline = Quill.import('blots/inline') as any;
                if (!Quill.imports['formats/highlight']) {
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
                (window as any).QuillConfigured = true;
            }

            // 5. Salva o componente no estado para renderizar
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
        // Agora é seguro usar syntax: true porque garantimos o window.hljs
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

    // Efeitos de Highlight e Eventos (Só rodam se o componente existir)
    useEffect(() => {
        if (!QuillComponent || !reactQuillRef.current) return;
        const editor = reactQuillRef.current.getEditor();

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

        if (mode === 'comment' && onTextSelect) {
            const selHandler = (range: Range, old: Range, source: string) => {
                if (source === 'user' && range && range.length > 0) onTextSelect(range);
            };
            editor.on('selection-change', selHandler);
            return () => editor.off('selection-change', selHandler);
        }
    }, [mode, staffComments, onHighlightClick, onTextSelect, QuillComponent]);

    // Se o componente ainda não carregou (está fazendo os imports), mostra loading
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
                    readOnly={mode === 'comment'}
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