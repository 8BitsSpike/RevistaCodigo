import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Editorial | RBEB",
    description: "Gestão editorial da revista",
};

// Mudamos o nome de RootLayout para EditorialLayout para não confundir
export default function EditorialLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        // --- CORREÇÃO CRUCIAL ---
        // 1. Trocamos <body> por <div>.
        // 2. Removemos ApolloWrapper e Toaster (eles já vêm do pai src/app/layout.tsx).
        // 3. Removemos a config de fontes (já herdada do pai).
        
        <div className="editorial-layout min-h-screen bg-gray-50">
            {children}
        </div>
    );
}