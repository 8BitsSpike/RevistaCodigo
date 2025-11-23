'use client';
import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { User, FileText } from 'lucide-react';
import { USER_API_BASE } from '@/lib/fetcher';

interface PerfilAutor { biografia?: string; }
interface AuthorCardProps { usuarioId: string; nome: string; urlFoto?: string; }

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
                const res = await fetch(`${USER_API_BASE}/${usuarioId}?token=${token}`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                if (res.ok) setPerfil(await res.json());
            } catch (err) { console.error(err); } finally { setLoading(false); }
        };
        fetchAutorProfile();
    }, [usuarioId]);

    return (
        <div className="w-[90%] my-4 mx-auto bg-white shadow-lg rounded-lg overflow-hidden flex">
            <div className="w-[40%] flex-shrink-0 relative min-h-[150px] bg-gray-100">
                {/* FIX: Image fallback */}
                <Image src={urlFoto || '/faviccon.png'} alt={nome} fill className="object-cover" />
            </div>
            <div className="flex flex-col justify-between flex-1 p-4">
                <div>
                    <h3 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                        <User size={20} className="text-emerald-600" /> {nome}
                    </h3>
                    <div className="mt-2">
                        <h4 className="font-semibold text-gray-700 flex items-center gap-1 text-sm">
                            <FileText size={16} className="text-emerald-600" /> Biografia
                        </h4>
                        {loading ? <p className="text-sm text-gray-500 italic">Carregando...</p> : <p className="text-sm text-gray-600 mt-1 line-clamp-3">{perfil?.biografia || 'Nenhuma biografia.'}</p>}
                    </div>
                </div>
                <div className="mt-4 text-right">
                    <Link href={`/profile?id=${usuarioId}`} className="text-sm text-emerald-600 hover:underline font-medium">Clique aqui para saber mais</Link>
                </div>
            </div>
        </div>
    );
}