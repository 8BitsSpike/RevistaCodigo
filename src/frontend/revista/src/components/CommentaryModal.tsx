'use client';

import { useState, useEffect } from 'react';
import { X } from 'lucide-react';

interface CommentaryModalProps {
    isOpen: boolean;
    title: string; // Título dinâmico (ex: "Justificar Promoção")
    onClose: () => void;
    onSubmit: (commentary: string) => void;
    loading: boolean; // Recebe o estado de 'loading' do hook de mutação
}

export default function CommentaryModal({
    isOpen,
    title,
    onClose,
    onSubmit,
    loading = false
}: CommentaryModalProps) {

    const [commentary, setCommentary] = useState('');

    // Limpa o comentário quando o modal é fechado
    useEffect(() => {
        if (isOpen) {
            setCommentary('');
        }
    }, [isOpen]);

    if (!isOpen) return null;

    const handleSubmit = () => {
        if (commentary.trim()) {
            onSubmit(commentary);
        }
    };

    return (
        // Overlay
        <div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4"
            onClick={onClose} // Fecha ao clicar fora
        >
            {/* Caixa do Modal */}
            <div
                className="bg-white rounded-lg shadow-xl w-full max-w-lg"
                onClick={(e) => e.stopPropagation()} // Impede que o clique dentro feche o modal
            >
                {/* Header */}
                <div className="flex justify-between items-center p-4 border-b">
                    <h2 className="text-xl font-semibold text-gray-800">
                        Justificativa: {title}
                    </h2>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-600"
                        aria-label="Fechar"
                        disabled={loading}
                    >
                        <X size={24} />
                    </button>
                </div>

                {/* Formulário */}
                <div className="p-6">
                    <label htmlFor="commentary" className="block text-sm font-medium text-gray-700">
                        Justificativa
                    </label>
                    <textarea
                        id="commentary"
                        value={commentary}
                        onChange={(e) => setCommentary(e.target.value)}
                        placeholder="Explique sua solicitação para avaliação e possível aprovação."
                        className="mt-2 w-full h-32 p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
                        disabled={loading}
                    />
                </div>

                {/* Footer */}
                <div className="flex justify-end p-4 bg-gray-50 border-t rounded-b-lg">
                    <button
                        onClick={handleSubmit}
                        disabled={!commentary.trim() || loading}
                        className="px-6 py-2 bg-emerald-600 text-white font-bold rounded-md shadow-sm hover:bg-emerald-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed"
                    >
                        {loading ? 'Enviando...' : 'ENVIAR'}
                    </button>
                </div>
            </div>
        </div>
    );
}