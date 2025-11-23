import { Suspense } from 'react';
import type { Metadata } from "next";
import ImprensaClient from "./ImprensaClient";
import Layout from '@/components/Layout';


export const metadata: Metadata = {
    title: "Sala de Imprensa | RBEB",
    description: "Gerenciar volumes e edições da revista.",
};


export default function ImprensaPage() {
    return (
        <Suspense
            fallback={
                <Layout>
                    <p className="text-center mt-20 text-gray-600">Carregando...</p>
                </Layout>
            }
        >
            <ImprensaClient />
        </Suspense>
    );
}