import { Suspense } from 'react';
import type { Metadata } from "next";
import PendenciasClient from "./PendenciasClient";
import Layout from '@/components/Layout';

// Define os metadados para esta página
export const metadata: Metadata = {
    title: "Controle de Pendências | RBEB",
    description: "Gerenciar solicitações pendentes da equipe editorial.",
};


export default function PendenciasPage() {
    return (
        <Suspense
            fallback={
                <Layout>
                    <p className="text-center mt-20 text-gray-600">Carregando...</p>
                </Layout>
            }
        >
            <PendenciasClient />
        </Suspense>
    );
}