"use client";

import { useRouter } from 'next/navigation';

export default function ProfileClient() {
  const router = useRouter();

  const handleLogout = () => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('userToken');
    }
    router.push('/');
  };

  return (
    <button
      onClick={handleLogout}
      className="px-4 py-2 bg-red-50 text-red-700 border border-red-100 rounded-md text-sm hover:bg-red-100"
      aria-label="Sair"
    >
      Sair
    </button>
  );
}
