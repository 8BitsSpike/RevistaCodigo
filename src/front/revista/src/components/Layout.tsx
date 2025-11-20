'use client';

import Header from './Header';
import Footer from './Footer';

interface LayoutProps {
  children: React.ReactNode;
  hero?: React.ReactNode;
  // FIX: Added pageType prop
  pageType?: 'default' | 'editorial';
}

export default function Layout({ children, hero, pageType = 'default' }: LayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      {/* FIX: Pass pageType to Header */}
      <Header pageType={pageType} />

      {hero && <div className="w-full">{hero}</div>}

      <main className="flex-1 max-w-6xl w-full mx-auto px-4 py-8">
        {children}
      </main>

      <Footer />
    </div>
  );
}