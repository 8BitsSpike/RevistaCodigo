import { Suspense } from 'react';
import type { Metadata } from "next";
import ArtigoEditPageWrapper from "./ArtigoEditClient";
import Layout from '@/components/Layout';

export const metadata: Metadata = {
    title: "Edição Editorial | RBEB",
    description: "Página de revisão e edição de artigo.",
};

export default function ArtigoEditPage() {
    return <ArtigoEditPageWrapper />;
}