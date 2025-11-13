"use client";

import Link from 'next/link';
import Image from 'next/image';
import { usePathname, useRouter } from 'next/navigation';
import { useState, FormEvent, useRef, useEffect } from 'react';
import { Menu, X, Search, User, LogOut, ListFilter, Check } from 'lucide-react';
import useAuth from '@/hooks/useAuth';

export type HeaderProps = {
    siteTitle?: string;
};

export default function Header({ siteTitle = 'RBEB' }: HeaderProps) {
    const { user, logout } = useAuth();
    const pathname = usePathname() || '/';
    const [open, setOpen] = useState(false);
    const router = useRouter();
    const [submenuOpen, setSubmenuOpen] = useState(false);
    const [query, setQuery] = useState('');
    const [searchType, setSearchType] = useState<'titulo' | 'autor'>('titulo');
    const [searchMenuOpen, setSearchMenuOpen] = useState(false);
    const [showHint, setShowHint] = useState(false);
    const hintTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const closeTimeout = useRef<ReturnType<typeof setTimeout> | null>(null);

    const [isStaff, setIsStaff] = useState(false);

    useEffect(() => {
        if (typeof window !== 'undefined') {
            const staffStatus = localStorage.getItem('isStaff') === 'true';
            setIsStaff(staffStatus);
        }
    }, [user]);

    const secondary = [
        { label: 'Edições Especiais', href: '/volume' },
        { label: 'Artigos', href: '/artigo/tipos?tipo=Artigo' },
        { label: 'Opinião', href: '/artigo/tipos?tipo=Opiniao' },
        { label: 'Indicação', href: '/artigo/tipos?tipo=Indicacao' },
        { label: 'Entrevistas', href: '/artigo/tipos?tipo=Entrevistas' },
        { label: 'Vídeos', href: '/artigo/tipos?tipo=Videos' },
        { label: 'Blog da Editoria', href: '/artigo/tipos?tipo=Blog' },
        { label: 'Sala dos Professores', href: '/professores' }
    ];

    const topLinks = [
        { label: 'Pensar Educação', href: '/pensar-educacao' },
        { label: 'Responsabilidade institucional', href: '/responsabilidade' },
        { label: 'Financiamento', href: '/financiamento' },
        { label: 'Apoio', href: '/apoio' },
        { label: 'Parceiros', href: '/parceiros' }
    ];

    const clearCloseTimeout = () => {
        if (closeTimeout.current) clearTimeout(closeTimeout.current as unknown as number);
        closeTimeout.current = null;
    };

    const scheduleClose = (delay = 150) => {
        clearCloseTimeout();
        closeTimeout.current = setTimeout(() => setSubmenuOpen(false), delay);
    };

    const handleSearch = (e: FormEvent) => {
        e.preventDefault();
        if (!query) return;
        router.push(`/search?q=${encodeURIComponent(query)}&tipo=${searchType}`);
        setOpen(false);
    };

    const handleProfile = () => router.push('/profile');

    const handleLogout = () => {
        logout();
        setIsStaff(false);
        router.push('/');
    };

    const handleSearchFocus = () => {
        if (hintTimerRef.current) {
            clearTimeout(hintTimerRef.current);
            hintTimerRef.current = null;
        }
        setShowHint(true);
    };

    const handleSearchMouseLeave = () => {
        hintTimerRef.current = setTimeout(() => {
            setShowHint(false);
        }, 2000);
    };

    const handleSearchMouseEnter = () => {
        if (hintTimerRef.current) {
            clearTimeout(hintTimerRef.current);
            hintTimerRef.current = null;
        }
    };

    return (
        <header id="site-header" className="w-full bg-white sticky top-0 z-40 shadow-sm">
            {/* Barra Superior (Logo, Links Topo, Login) */}
            <div className="max-w-6xl mx-auto flex items-center justify-between px-6 py-4">
                <Link href="/" className="flex items-center gap-4">
                    <div className="w-16 h-15 rounded overflow-hidden bg-transparent flex items-center justify-center">
                        <Image src="/faviccon.png" alt="RBEB" width={100} height={100} />
                    </div>
                    <div>
                        <h1 className="text-lg font-semibold">{siteTitle}</h1>
                        <div className="text-xs text-gray-500">Revista Brasileira de Educação Básica</div>
                    </div>
                </Link>

                <nav aria-label="Top navigation" className="hidden md:flex items-center gap-6 text-sm text-gray-600 mt-6">
                    {topLinks.map((l) => (
                        <Link
                            key={l.href}
                            href={l.href}
                            className="hover:text-[#189F66] focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[#189F66] rounded"
                        >
                            {l.label}
                        </Link>
                    ))}
                </nav>

                <div className="hidden md:flex items-center gap-4 py-2 rounded-md mt-6">
                    {user ? (
                        <>
                            <div
                                onClick={handleProfile}
                                className="flex items-center gap-2 cursor-pointer p-2 rounded-md hover:bg-gray-50"
                                title="Ver Perfil"
                            >
                                <div className="relative h-10 w-10 rounded-full overflow-hidden bg-gray-200 border border-emerald-600">
                                    {user.foto ? (
                                        <Image
                                            src={user.foto}
                                            alt="Foto de perfil"
                                            fill
                                            className="object-cover"
                                        />
                                    ) : (
                                        <User className="h-full w-full text-gray-500 p-2" />
                                    )}
                                </div>
                                <span className="text-sm font-medium text-gray-700">
                                    {user.name}
                                </span>
                            </div>

                            <button
                                aria-label="Sair"
                                onClick={handleLogout}
                                className="p-2 rounded-md hover:bg-gray-50 text-gray-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[#F69A73] cursor-pointer"
                                title="Sair"
                            >
                                <LogOut size={18} />
                            </button>
                        </>
                    ) : (
                        <Link
                            href="/login"
                            className="px-4 py-2 rounded-md bg-emerald-600 text-white hover:bg-emerald-700 transition"
                        >
                            Login
                        </Link>
                    )}
                </div>

                <div className="md:hidden">
                    <button
                        aria-label="Abrir menu"
                        aria-expanded={open} // (MODIFICADO) Revertido para boolean
                        onClick={() => setOpen((s) => !s)}
                        className="p-2 rounded-md focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[#F69A73]"
                    >
                        {open ? <X size={20} /> : <Menu size={20} />}
                    </button>
                </div>
            </div>

            {isStaff && (
                <div className="max-w-6xl mx-auto px-6 pb-4 flex justify-start">
                    <button
                        onClick={() => router.push('/editorial')}
                        className="px-4 py-2 rounded-md bg-emerald-600 text-white hover:bg-emerald-700 transition shadow-sm font-medium"
                    >
                        Sala Editorial
                    </button>
                </div>
            )}

            <div className="border-t border-gray-200">
                <nav className="max-w-7xl mx-auto px-4 py-6 hidden md:block">
                    <ul className="flex gap-6 items-center text-sm">
                        {secondary.map((item) => {
                            const isActive = item.href === '/' ? pathname === '/' : pathname.startsWith(item.href);

                            if (item.label === 'RBEB') {
                                return (
                                    <li
                                        key={item.href}
                                        className="relative"
                                        onMouseEnter={() => { clearCloseTimeout(); setSubmenuOpen(true); }}
                                        onMouseLeave={() => scheduleClose()}
                                    >
                                        <Link
                                            href="/"
                                            aria-haspopup="true"
                                            aria-expanded={submenuOpen} // (MODIFICADO) Revertido para boolean
                                            className={`inline-block py-1 ${isActive
                                                ? 'text-[#F69A73] border-b-2 border-[#F69A73]'
                                                : 'text-gray-700 hover:text-[#F69A73]'
                                                } transition focus:outline-none`}
                                        >
                                            {item.label}
                                        </Link>

                                        {submenuOpen && (
                                            <div
                                                className="absolute left-1/2 mt-2 w-56 bg-white shadow-lg rounded-md p-2 z-50"
                                                style={{ transform: 'translateX(-50%)', overflow: 'visible' }}
                                                onMouseEnter={() => clearCloseTimeout()}
                                                onMouseLeave={() => scheduleClose()}
                                            >
                                                <ul className="flex flex-col text-sm">
                                                    <li>
                                                        <Link href="/rbeb/apresentacao" className="block px-3 py-2 hover:bg-gray-50 rounded">Apresentação</Link>
                                                    </li>
                                                    <li>
                                                        <Link href="/rbeb/autoras-e-autores" className="block px-3 py-2 hover:bg-gray-50 rounded">Autoras e autores</Link>
                                                    </li>
                                                    <li>
                                                        <Link href="/rbeb/corpo-editorial" className="block px-3 py-2 hover:bg-gray-50 rounded">Corpo Editorial</Link>
                                                    </li>
                                                    <li>
                                                        <Link href="/rbeb/editoria-executiva" className="block px-3 py-2 hover:bg-gray-50 rounded">Editoria Executiva</Link>
                                                    </li>
                                                </ul>
                                            </div>
                                        )}
                                    </li>
                                );
                            }

                            return (
                                <li key={item.href}>
                                    <Link
                                        href={item.href}
                                        className={`inline-block py-1 ${isActive
                                            ? 'text-[#F69A73] border-b-2 border-[#F69A73]'
                                            : 'text-gray-700 hover:text-[#F69A73]'
                                            } transition focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[#F69A73]`}
                                    >
                                        {item.label}
                                    </Link>
                                </li>
                            );
                        })}

                        <li className="ml-4 relative">
                            <form onSubmit={handleSearch} className="flex items-center">
                                <button
                                    type="button"
                                    onClick={() => setSearchMenuOpen(o => !o)}
                                    className="p-2 h-full border border-gray-200 rounded-l-md bg-gray-50 hover:bg-gray-100"
                                    aria-label="Abrir filtros de busca"
                                    aria-expanded={searchMenuOpen} // (MODIFICADO) Revertido para boolean
                                >
                                    <ListFilter size={16} className="text-gray-600" />
                                </button>

                                <div className="relative flex-grow">
                                    <span className="absolute left-2 top-1/2 -translate-y-1/2 text-gray-400"><Search /></span>
                                    <input
                                        aria-label="Pesquisar"
                                        value={query}
                                        onChange={(e) => setQuery(e.target.value)}
                                        onFocus={handleSearchFocus}
                                        onMouseLeave={handleSearchMouseLeave}
                                        onMouseEnter={handleSearchMouseEnter}
                                        placeholder={`Buscar por ${searchType}...`}
                                        className="w-40 md:w-64 pl-8 px-3 py-1 border-y border-r border-gray-200 rounded-r-md text-sm focus:outline-none focus:ring-2 focus:ring-[#F69A73]"
                                    />

                                    {showHint && (
                                        <div
                                            className="absolute top-full left-0 mt-2 p-2 bg-white border border-gray-200 rounded-md shadow-lg text-xs text-gray-700 z-50 transition-all duration-300"
                                            onMouseEnter={handleSearchMouseEnter}
                                            onMouseLeave={handleSearchMouseLeave}
                                        >
                                            Clique na caixa ao lado para definir o filtro de busca.
                                        </div>
                                    )}
                                </div>
                            </form>

                            {searchMenuOpen && (
                                <div className="absolute top-full right-0 mt-2 w-48 bg-white border border-gray-200 rounded-md shadow-lg z-50 transition-all duration-1000 animate-fade-in">
                                    <ul>
                                        <li>
                                            <button
                                                onClick={() => { setSearchType('titulo'); setSearchMenuOpen(false); }}
                                                className="w-full text-left px-3 py-2 text-sm flex items-center justify-between hover:bg-gray-100"
                                            >
                                                Busca por título
                                                {searchType === 'titulo' && <Check size={16} className="text-emerald-600" />}
                                            </button>
                                        </li>
                                        <li>
                                            <button
                                                onClick={() => { setSearchType('autor'); setSearchMenuOpen(false); }}
                                                className="w-full text-left px-3 py-2 text-sm flex items-center justify-between hover:bg-gray-100"
                                            >
                                                Nome do autor
                                                {searchType === 'autor' && <Check size={16} className="text-emerald-600" />}
                                            </button>
                                        </li>
                                    </ul>
                                </div>
                            )}
                        </li>
                    </ul>
                </nav>

                {open && (
                    <div className="md:hidden border-t bg-white">
                        <div className="px-4 py-3">
                            <div className="flex flex-col gap-3">
                                {secondary.map((s) => (
                                    <div key={s.href}>
                                        <Link href={s.href} className="text-gray-800 font-medium py-2 block" onClick={() => setOpen(false)}>
                                            {s.label}
                                        </Link>
                                        {s.label === 'RBEB' && (
                                            <div className="pl-4">
                                                <Link href="/rbeb/apresentacao" className="block py-1">Apresentação</Link>
                                                <Link href="/rbeb/autoras-e-autores" className="block py-1">Autoras e autores</Link>
                                                <Link href="/rbeb/corpo-editorial" className="block py-1">Corpo Editorial</Link>
                                                <Link href="/rbeb/editoria-executiva" className="block py-1">Editoria Executiva</Link>
                                            </div>
                                        )}
                                    </div>
                                ))}

                                <form onSubmit={handleSearch} className="pt-2 relative">
                                    <span className="absolute left-3 top-3 text-gray-400"><Search /></span>
                                    <input
                                        aria-label="Pesquisar"
                                        value={query}
                                        onChange={(e) => setQuery(e.target.value)}
                                        placeholder="Pesquisar"
                                        className="w-full pl-10 px-3 py-2 border border-gray-200 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-[#F69A73]"
                                    />
                                </form>

                                <div className="pt-4 space-y-3">
                                    {isStaff && (
                                        <button
                                            onClick={() => { router.push('/editorial'); setOpen(false); }}
                                            className="w-full px-4 py-2 rounded-md bg-emerald-600 text-white hover:bg-emerald-700 transition"
                                        >
                                            Sala Editorial
                                        </button>
                                    )}
                                    {user ? (
                                        <button
                                            onClick={handleLogout}
                                            className="w-full px-4 py-2 rounded-md bg-red-500 text-white hover:bg-red-600 transition"
                                        >
                                            Sair
                                        </button>
                                    ) : (
                                        <Link
                                            href="/login"
                                            className="w-full block text-center px-4 py-2 rounded-md bg-emerald-600 text-white hover:bg-emerald-700 transition"
                                            onClick={() => setOpen(false)}
                                        >
                                            Login
                                        </Link>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </header>
    );
}