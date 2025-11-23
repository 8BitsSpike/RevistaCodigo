
import type { Metadata } from 'next';
import Layout from '@/components/Layout';

export async function generateMetadata({ params }: any): Promise<Metadata> {
  const p = await params;
  const { slug } = p;
  return {
    title: `${slug} - RBEB`,
    description: `Post ${slug}`,
  };
}

export default async function PostPage({ params }: any) {
  const p = await params; 
  const { year, month, day, slug } = p;
  return (
    <Layout>
      <h1>{slug}</h1>
      <p>Post page placeholder for {year}/{month}/{day} - {slug}</p>
    </Layout>
  );
}
