import type { Metadata } from "next";
import EditorialClient from "./EditorialClient";

export const metadata: Metadata = {
    title: "Sala Editorial | RBEB",
    description: "Gerenciamento da equipe editorial e de publicações.",
};

export default function EditorialPage() {
    return <EditorialClient />;
}