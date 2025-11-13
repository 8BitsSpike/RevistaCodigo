import Header from './Header';
import Footer from './Footer';

export default function Layout({ children, hero }: { children: React.ReactNode; hero?: React.ReactNode }) {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      {}
      {hero && <div className="w-full">{hero}</div>}
      <main className="flex-1 max-w-4xl w-full mx-auto px-4 py-8">{children}</main>
      <Footer />
    </div>
  );
}
