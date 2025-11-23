import type { Metadata } from "next";
import SubmitArtigoClient from "./SubmitArtigoClient";

export const metadata: Metadata = {
    title: "Submeter Artigo | RBEB",
    description: "Envie seu trabalho para a Revista Brasileira de Educação Básica.",
};

export default function SubmitArtigoPage() {
    return <SubmitArtigoClient />;
}