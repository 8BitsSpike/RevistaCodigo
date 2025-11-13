import Image from 'next/image';
import Link from 'next/link';

type ImageType = {
  url: string;
  textoAlternativo: string;
};

export type ArticleCardProps = {
  id: string;
  title: string;
  excerpt?: string;
  imagem?: ImageType | null;
  href: string;
};

export default function ArticleCard({ id, title, excerpt, imagem, href }: ArticleCardProps) {
  return (
    <li
      key={id}
      className="w-[90%] my-[2vh] bg-white shadow-lg rounded-lg overflow-hidden flex"
    >
      <Link href={href} className="flex w-full">
        <div className="w-[40%] flex-shrink-0 relative min-h-[150px]">
          {imagem ? (
            <Image
              src={imagem.url}
              alt={imagem.textoAlternativo || title}
              fill
              className="object-cover"
            />
          ) : (
            <div className="w-full h-full bg-gray-200 flex items-center justify-center">
              <span className="text-gray-500 text-sm">Sem Imagem</span>
            </div>
          )}
        </div>
        <div className="flex flex-col justify-center flex-1 px-[5%] py-4">
          <h4 className="text-xl font-semibold text-gray-900">{title}</h4>
          {excerpt && (
            <p className="text-sm text-gray-600 mt-2">{excerpt}</p>
          )}
        </div>
      </Link>
    </li>
  );
}