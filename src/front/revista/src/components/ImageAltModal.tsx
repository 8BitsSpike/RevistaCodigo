'use client';

import { useState } from 'react';

interface ImageAltModalProps {
    isOpen: boolean;
    onConfirm: (altText: string) => void;
}

export default function ImageAltModal({ isOpen, onConfirm }: ImageAltModalProps) {
    const [altText, setAltText] = useState('');

    if (!isOpen) return null;

    const handleSubmit = () => {
        if (altText.trim()) {
            onConfirm(altText);
            setAltText('');
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-white p-6 rounded-lg shadow-xl w-[90%] max-w-md">
                <h3 className="text-lg font-bold mb-4 text-gray-800">Acessibilidade da Imagem</h3>
                <p className="text-sm text-gray-600 mb-4">
                    Descreva a imagem enviada para garantir a acessibilidade do artigo para pessoas com deficiências.
                </p>

                <textarea
                    value={altText}
                    onChange={(e) => setAltText(e.target.value)}
                    placeholder="Ex: Um gráfico de barras mostrando o crescimento..."
                    className="w-full h-32 p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
                />

                <div className="mt-4 flex justify-end">
                    <button
                        onClick={handleSubmit}
                        disabled={!altText.trim()}
                        className="px-6 py-2 bg-emerald-600 text-white rounded-md font-medium hover:bg-emerald-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition"
                    >
                        Enviar
                    </button>
                </div>
            </div>
        </div>
    );
}