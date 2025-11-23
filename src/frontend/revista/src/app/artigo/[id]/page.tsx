import { Suspense } from 'react';
import type { Metadata } from 'next';
import ArtigoClient from './ArtigoClient';
import Layout from '@/components/Layout';

export const metadata: Metadata = {
    title: "Artigo | RBEB",
    description: "Visualização de artigo",
};

function ArtigoPageContent() {
    return <ArtigoClient />;
}

export default function ArtigoPageWrapper() {
    return (
        <Suspense fallback={<Layout><p className="text-center mt-20 text-gray-600">Carregando...</p></Layout>}>
            <ArtigoPageContent />
        </Suspense>
    );
}