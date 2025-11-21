'use client';

import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
// Importação de CSS
import 'react-quill/dist/quill.snow.css';
import 'highlight.js/styles/monokai-sublime.css';

import { StaffComentario } from '@/types/index';
import ImageAltModal from './ImageAltModal';
import toast from 'react-hot-toast';
import type ReactQuillType from 'react-quill';
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

// Componente Interno (que será exportado dinamicamente)
const EditorialQuillEditorInternal = ({
    mode,
    initialContent,
    staffComments = [],
    onContentChange,
    onMediaChange,
    onTextSelect,
    onHighlightClick,
}: EditorialQuillEditorProps) => {
    
    const reactQuillRef = useRef<ReactQuillType>(null);
    const [isAltModalOpen, setIsAltModalOpen] = useState(false);
    const [pendingImage, setPendingImage] = useState<{ id: string; url: string } | null>(null);
    
    // Carrega as libs de forma síncrona (seguro pois estamos no cliente)
    const ReactQuill = require('react-quill');
    const Quill = ReactQuill.Quill;
    const hljs = require('highlight.js');

    // Configuração Única (fora do render loop, mas dentro do componente)
    // CORREÇÃO AQUI: Usamos (window as any) para o TypeScript não reclamar
    if (typeof window !== 'undefined' && !(window as any).QuillConfigured) {
        
        // @ts-ignore
        window.hljs = hljs;
        
        if (!Quill.imports['modules/syntax']) {
             // @ts-ignore
            Quill.register('modules/syntax', true);
        }

        const Inline = Quill.import('blots/inline');
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
        
        // Marca como configurado no objeto window global
        (window as any).QuillConfigured = true;
    }

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

    // Efeitos de Highlight e Eventos
    useEffect(() => {
        if (!reactQuillRef.current) return;
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
    }, [mode, staffComments, onHighlightClick, onTextSelect]);

    // Ajuste final para TypeScript aceitar a ref no componente importado via require
    const QuillComp = ReactQuill as any;

    return (
        <>
            <ImageAltModal isOpen={isAltModalOpen} onConfirm={handleAltConfirm} />
            <div className="bg-white rounded-lg border border-gray-300 editorial-editor-container">
                <QuillComp
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

// EXPORTAÇÃO DINÂMICA
import dynamic from 'next/dynamic';

export default dynamic(() => Promise.resolve(EditorialQuillEditorInternal), {
    ssr: false,
    loading: () => <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse flex items-center justify-center text-gray-400">Carregando Editor...</div>
});