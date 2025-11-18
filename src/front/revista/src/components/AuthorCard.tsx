'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { User, FileText } from 'lucide-react';

// URL da UsuarioAPI
const API_BASE = 'https://localhost:54868/api/Usuario';

// Props que o componente recebe da página (da query GET_ARTIGO_VIEW)
interface AuthorCardProps {
    usuarioId: string;
    nome: string;
    urlFoto?: string;
}

// Interface para os dados do perfil da UsuarioAPI
interface PerfilAutor {
    biografia?: string;
    // Outros campos podem ser adicionados se necessário
}

export default function AuthorCard({ usuarioId, nome, urlFoto }: AuthorCardProps) {
    const [perfil, setPerfil] = useState<PerfilAutor | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAutorProfile = async () => {
            const token = localStorage.getItem('userToken');
            if (!usuarioId || !token) {
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                const res = await fetch(`${API_BASE}/${usuarioId}?token=${token}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });

                if (res.ok) {
                    const data = await res.json();
                    setPerfil(data);
                } else {
                    console.error(`Falha ao buscar perfil do autor: ${usuarioId}`);
                }
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchAutorProfile();
    }, [usuarioId]);

    return (
        <div className="w-[90%] my-4 mx-auto bg-white shadow-lg rounded-lg overflow-hidden flex">
            {/* Container da Imagem (40%) */}
            <div className="w-[40%] flex-shrink-0 relative min-h-[150px] bg-gray-100">
                <Image
                    src={urlFoto || '/default-avatar.png'}
                    alt={nome}
                    fill
                    className="object-cover"
                />
            </div>

            {/* Container do Texto (60%) */}
            <div className="flex flex-col justify-between flex-1 p-4">
                <div>
                    {/* Nome */}
                    <h3 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                        <User size={20} className="text-emerald-600" />
                        {nome}
                    </h3>

                    {/* Biografia */}
                    <div className="mt-2">
                        <h4 className="font-semibold text-gray-700 flex items-center gap-1 text-sm">
                            <FileText size={16} className="text-emerald-600" />
                            Biografia
                        </h4>
                        {loading ? (
                            <p className="text-sm text-gray-500 italic mt-1">Carregando biografia...</p>
                        ) : (
                            <p className="text-sm text-gray-600 mt-1 line-clamp-3">
                                {perfil?.biografia || 'Biografia não disponível.'}
                            </p>
                        )}
                    </div>
                </div>

                {/* Link para o Perfil */}
                <div className="mt-4 text-right">
                    <Link
                        href={`/profile?id=${usuarioId}`}
                        className="text-sm text-emerald-600 hover:text-emerald-800 hover:underline font-medium"
                    >
                        Clique aqui para saber mais
                    </Link>
                </div>
            </div>
        </div>
    );
}