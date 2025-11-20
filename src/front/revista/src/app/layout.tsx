import type { Metadata } from "next";
import { ApolloWrapper } from "@/components/ApolloWrapper";
import { Geist, Geist_Mono } from "next/font/google";
import { Toaster } from "react-hot-toast";
import './globals.css';

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Revista Brasileira de Educação Básica",
  description: "Portal de publicações acadêmicas",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    // FIX: Added suppressHydrationWarning here to ignore browser extension attributes
    <html lang="pt-BR" suppressHydrationWarning>
      <body
        suppressHydrationWarning
        className={`${geistSans.variable} ${geistMono.variable} antialiased bg-gray-50 text-gray-900`}
      >
        <ApolloWrapper>
          <Toaster
            position="top-right"
            toastOptions={{
              duration: 4000,
              style: {
                background: '#333',
                color: '#fff',
              },
            }}
          />
          {children}
        </ApolloWrapper>
      </body>
    </html>
  );
}