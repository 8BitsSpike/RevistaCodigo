import type { Metadata } from "next";
import SearchClientWrapper from "./SearchClient";

export const metadata: Metadata = {
    title: "Busca | RBEB",
    description: "Buscar artigos na Revista Brasileira de Educação Básica.",
};

export default function SearchPage() {
    return <SearchClientWrapper />;
}