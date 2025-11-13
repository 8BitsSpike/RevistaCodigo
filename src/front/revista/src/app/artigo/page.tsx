import type { Metadata } from 'next';
import Layout from '@/components/Layout';
import FormularioArtigo from './FormularioArtigo';

export const metadata: Metadata = {
  };

export default function ArtigosPage() {
  return (
    <Layout>
      <div className="p-8">
     <FormularioArtigo />
      </div>
    </Layout>
  );
}