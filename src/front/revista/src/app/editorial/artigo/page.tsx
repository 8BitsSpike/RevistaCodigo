import { Suspense } from 'react';
import type { Metadata } from "next";
import ArtigoControlClient from "./ArtigoControlClient";
import Layout from '@/components/Layout';

export const metadata: Metadata = {
    title: "Controle de Artigos | RBEB",
    description: "Gerenciar e editar artigos da revista.",
};

export default function ArtigoControlPage() {
    return (
        <Suspense
            fallback={
                <Layout>
                    <p className="text-center mt-20 text-gray-600">Carregando...</p>
                </Layout>
            }
        >
            <ArtigoControlClient />
        </Suspense>
    );
}