"use client";

import Image from 'next/image';
import Link from 'next/link';

type ImageType = { url: string; textoAlternativo: string; };
export type Edition = { id: string; title: string; resumo?: string; imagem?: ImageType | null; };
export type EditionListProps = { editions?: Edition[]; };

export default function EditionList({ editions = [] }: EditionListProps) {
  return (
    <ul className="w-full flex flex-col items-center">
      {editions.map((e) => (
        <li key={e.id} className="card-std w-[90%] my-[2vh] flex">
          {/* FIX: Changed href to query param format */}
          <Link href={`/volume?id=${e.id}`} className="flex w-full">
            <div className="w-[40%] flex-shrink-0 relative min-h-[150px]">
              <Image src={e.imagem?.url || '/faviccon.png'} alt={e.imagem?.textoAlternativo || e.title} fill className="object-cover" />
            </div>
            <div className="flex flex-col justify-center flex-1 px-[5%] py-4">
              <h4 className="text-xl font-semibold text-gray-900">{e.title}</h4>
              {e.resumo && <p className="text-sm text-gray-600 mt-2">{e.resumo}</p>}
            </div>
          </Link>
        </li>
      ))}
    </ul>
  );
}