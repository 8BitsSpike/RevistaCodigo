import Image from 'next/image';
import Link from 'next/link';

type ImageType = {
  url: string;
  textoAlternativo: string;
};

export type Edition = {
  id: string;
  title: string;
  resumo?: string;
  imagem?: ImageType | null;
};

export type EditionListProps = {
  editions?: Edition[];
};

export default function EditionList({ editions = [] }: EditionListProps) {
  return (
    <ul className="w-full flex flex-col items-center">
      {editions.map((e) => (
        <li
          key={e.id}
          // Cada 'li' é um card com 90% de largura e margem vertical
          className="w-[90%] my-[2vh] bg-white shadow-lg rounded-lg overflow-hidden flex"
        >
          <Link href={`/volume/${e.id}`} className="flex w-full">
            <div className="w-[40%] flex-shrink-0 relative min-h-[150px]">
              {e.imagem ? (
                <Image
                  src={e.imagem.url}
                  alt={e.imagem.textoAlternativo || e.title}
                  fill
                  className="object-cover"
                />
              ) : (
                // Um placeholder caso não haja imagem
                <div className="w-full h-full bg-gray-200 flex items-center justify-center">
                  <span className="text-gray-500 text-sm">Sem Imagem</span>
                </div>
              )}
            </div>
            <div className="flex flex-col justify-center flex-1 px-[5%] py-4">
              <h4 className="text-xl font-semibold text-gray-900">{e.title}</h4>
              {e.resumo && (
                <p className="text-sm text-gray-600 mt-2">{e.resumo}</p>
              )}
            </div>
          </Link>
        </li>
      ))}
    </ul>
  );
}