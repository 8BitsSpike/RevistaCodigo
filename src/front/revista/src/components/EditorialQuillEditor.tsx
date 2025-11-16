'use client';

import { useState, useRef, useEffect, useCallback } from 'react';
import dynamic from 'next/dynamic';
import 'react-quill/dist/quill.snow.css';
import hljs from 'highlight.js';
import 'highlight.js/styles/monokai-sublime.css';
import Quill, { type Range } from 'quill';
import type ReactQuillType from 'react-quill';
import { StaffComentario } from '@/types/index';
import ImageAltModal from './ImageAltModal';
import toast from 'react-hot-toast';

// --- Configuração (Executada Imediatamente) ---

// Registra o módulo de sintaxe (highlight.js)
// @ts-ignore
Quill.register('modules/syntax', true);
// Registra o formato customizado 'highlight'
const Inline = Quill.import('blots/inline') as any;
class HighlightBlot extends Inline {
    static blotName = 'highlight';
    static tagName = 'span';

    static create(value: string | boolean) {
        const node = super.create() as HTMLElement;
        if (typeof value === 'string') {
            node.setAttribute('data-comment-id', value);
            node.style.backgroundColor = '#FFF9C4';
            node.style.cursor = 'pointer';
        }
        return node;
    }
}
Quill.register(HighlightBlot, true);

// --- Importação Dinâmica do ReactQuill ---
const ReactQuill = dynamic(
    () => import('react-quill'),
    {
        ssr: false,
        loading: () => <div className="w-full h-96 bg-gray-100 rounded-md animate-pulse" />,
    }
);

const ReactQuillComponent = ReactQuill as any;

// --- Tipos de Props ---
interface EditorialQuillEditorProps {
    mode: 'edit' | 'comment';
    initialContent: string;
    staffComments?: StaffComentario[];
    onContentChange?: (html: string) => void;
    onMediaChange?: (midia: { id: string; url: string; alt: string }) => void;
    onTextSelect?: (range: Range) => void;
    onHighlightClick?: (comment: StaffComentario) => void;
}

// --- Componente ---
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

    // --- Manipulador de Imagem ---
    const imageHandler = useCallback(() => {
        const editor = reactQuillRef.current?.getEditor();
        if (!editor) return;

        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();

        input.onchange = () => {
            const file = input.files ? input.files[0] : null;
            if (file) {
                const reader = new FileReader();
                reader.onloadstart = () => toast.loading('Processando imagem...', { id: 'img-upload' });
                reader.onloadend = () => {
                    const base64 = reader.result as string;
                    const tempId = `img-${Date.now()}`;
                    setPendingImage({ id: tempId, url: base64 });
                    toast.dismiss('img-upload');
                    setIsAltModalOpen(true);
                };
                reader.onerror = () => toast.error('Falha ao ler imagem.', { id: 'img-upload' });
                reader.readAsDataURL(file);
            }
        };
    }, []);

    // Confirmação do Modal de Alt Text
    const handleAltTextConfirm = (altText: string) => {
        const editor = reactQuillRef.current?.getEditor();
        if (!pendingImage || !editor) return;

        if (onMediaChange) {
            onMediaChange({
                id: pendingImage.id,
                url: pendingImage.url,
                alt: altText,
            });
        }

        const range = editor.getSelection(true);
        editor.insertEmbed(range.index, 'image', pendingImage.url, 'user');
        editor.setSelection(range.index + 1, 0, 'user');

        setIsAltModalOpen(false);
        setPendingImage(null);
        toast.success('Imagem adicionada com acessibilidade!');
    };

    // --- Módulos e Formatos do Quill ---
    const modules = {
        syntax: {
            highlight: (text: string) => hljs.highlightAuto(text).value,
        },
        toolbar: {
            container: [
                [{ 'header': [1, 2, 3, false] }],
                [{ 'font': [] }],
                [{ 'size': ['small', false, 'large', 'huge'] }],
                ['bold', 'italic', 'underline', 'strike'],
                ['blockquote', 'code-block'],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                ['link', 'image'],
                ['clean'],
            ],
            handlers: {
                image: imageHandler,
            },
        },
        history: {
            userOnly: true
        }
    };

    const modulesComment = {
        ...modules,
        toolbar: false,
    };

    // useEffect para aplicar destaques e ouvintes
    useEffect(() => {
        if (!reactQuillRef.current) {
            return;
        }

        const editor = reactQuillRef.current.getEditor();

        // Aplica os destaques existentes
        if (mode === 'comment' && staffComments.length > 0) {
            staffComments.forEach(comment => {
                try {
                    const data = JSON.parse(comment.comment);
                    if (data && data.selection) {
                        const { index, length } = data.selection;
                        editor.formatText(index, length, 'highlight', comment.id, 'silent');
                    }
                } catch (e) {
                    // Ignora
                }
            });
        }

        // --- Lógica de Eventos ---

        // Ouvinte de clique (para abrir comentários)
        if (mode === 'comment' && onHighlightClick) {
            const clickHandler = (e: Event) => {
                let node = e.target as HTMLElement;
                while (node && node !== editor.root) {
                    if (node.tagName === 'SPAN' && node.hasAttribute('data-comment-id')) {
                        const commentId = node.getAttribute('data-comment-id');
                        const clickedComment = staffComments.find(c => c.id === commentId);
                        if (clickedComment) {
                            onHighlightClick(clickedComment);
                        }
                        return;
                    }
                    node = node.parentElement as HTMLElement;
                }
            };

            editor.root.addEventListener('click', clickHandler);
            return () => {
                editor.root.removeEventListener('click', clickHandler);
            };
        }

        // Ouvinte de seleção (para criar comentários)
        if (mode === 'comment' && onTextSelect) {
            const selectionHandler = (range: Range | null, oldRange: Range | null, source: string) => {
                if (source === 'user' && range && range.length > 0) {
                    onTextSelect(range);
                }
            };

            editor.on('selection-change', selectionHandler);
            return () => {
                editor.off('selection-change', selectionHandler);
            };
        }
    }, [mode, staffComments, onHighlightClick, onTextSelect, initialContent]);

    return (
        <>
            <ImageAltModal
                isOpen={isAltModalOpen}
                onConfirm={handleAltTextConfirm}
            />

            <div className="bg-white rounded-lg overflow-hidden border border-gray-300">
                <ReactQuillComponent
                    ref={reactQuillRef}
                    theme="snow"
                    readOnly={mode === 'comment'}
                    value={initialContent}
                    onChange={onContentChange}
                    modules={mode === 'edit' ? modules : modulesComment}
                    style={{ minHeight: '60vh', backgroundColor: 'white' }}
                />
            </div>
        </>
    );
}