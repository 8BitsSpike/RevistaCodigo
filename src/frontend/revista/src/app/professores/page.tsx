import type { Metadata } from "next";

import SalaProfessoresClient from "./SalaProfessoresClient";

export const metadata: Metadata = {
    title: "Sala dos Professores | RBEB",
    description: "Gerencie seus artigos e submissões.",
};

export default function SalaProfessoresPage() {
    return <SalaProfessoresClient />;
}