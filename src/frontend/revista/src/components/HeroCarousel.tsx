"use client";

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';

export type Slide = { title: string; kicker?: string; excerpt?: string; href?: string; image?: string; };
const DEFAULT_INTERVAL = 6000;

export default function HeroCarousel({ slides = [] as Slide[], fullScreen = false }: { slides?: Slide[]; fullScreen?: boolean }) {
  const [index, setIndex] = useState(0);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const timerRef = useRef<number | null>(null);
  const [computedHeight, setComputedHeight] = useState<number | null>(null);

  useEffect(() => {
    if (slides.length > 1) {
      timerRef.current = window.setTimeout(() => setIndex((i) => (i + 1) % slides.length), DEFAULT_INTERVAL);
    }
    return () => { if (timerRef.current) window.clearTimeout(timerRef.current); };
  }, [index, slides.length]);

  const go = (to: number) => setIndex(((to % slides.length) + slides.length) % slides.length);
  const next = () => go(index + 1);
  const prev = () => go(index - 1);

  if (!slides || slides.length === 0) return null;

  return (
    <section ref={containerRef} tabIndex={0} className={`relative w-full ${fullScreen ? 'h-screen' : ''}`}>
      <div className={`relative overflow-hidden ${fullScreen ? 'h-screen rounded-none' : 'rounded-lg'}`}>
        <div className="flex transition-transform duration-700 ease-in-out" style={{ transform: `translateX(-${index * 100}%)` }}>
          {slides.map((s, i) => (
            <article key={i} className="w-full flex-shrink-0 relative">
              <div className={`relative w-full ${fullScreen ? '' : 'h-72 sm:h-96 md:h-[520px]'} bg-gray-100`} style={fullScreen && computedHeight ? { height: computedHeight } : undefined}>
                {/* FIX: Image fallback */}
                <Image src={s.image || '/faviccon.png'} alt={s.title} fill sizes="(max-width: 768px) 100vw, 50vw" className="object-cover" unoptimized />
                <div className="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent" />
              </div>
              <div className={`absolute inset-0 ${fullScreen ? 'flex items-center' : 'flex items-end'}`}>
                <div className={`max-w-4xl w-full mx-auto p-6 md:p-12 text-white ${fullScreen ? 'text-center' : ''}`}>
                  {s.kicker && <div className={`text-sm uppercase tracking-wider text-[#F69A73] ${fullScreen ? 'mb-4' : 'mb-2'}`}>{s.kicker}</div>}
                  <h2 className={`${fullScreen ? 'text-3xl md:text-6xl' : 'text-2xl md:text-4xl'} font-semibold leading-tight drop-shadow ${fullScreen ? 'mb-6 md:mb-8' : 'mb-2'}`}>{s.title}</h2>
                  {s.excerpt && <p className={`${fullScreen ? 'mt-4' : 'mt-2'} ${fullScreen ? 'text-lg md:text-xl' : 'text-sm md:text-base'} text-white/90 max-w-2xl ${fullScreen ? 'mx-auto' : ''}`}>{s.excerpt}</p>}
                  {s.href && <div className={`mt-4 ${fullScreen ? 'justify-center flex' : ''}`}><Link href={s.href} className="inline-block bg-white text-black px-4 py-2 rounded-md font-medium hover:opacity-90">Leia a matéria</Link></div>}
                </div>
              </div>
            </article>
          ))}
        </div>
      </div>
      <button onClick={prev} aria-label="Anterior" className="absolute left-4 top-1/2 -translate-y-1/2 bg-white/80 backdrop-blur-sm p-2 rounded-full shadow hover:bg-white cursor-pointer"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M15 18L9 12L15 6" stroke="#111827" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg></button>
      <button onClick={next} aria-label="Próximo" className="absolute right-4 top-1/2 -translate-y-1/2 bg-white/80 backdrop-blur-sm p-2 rounded-full shadow hover:bg-white cursor-pointer"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M9 18L15 12L9 6" stroke="#111827" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg></button>
      <div className="absolute left-1/2 -translate-x-1/2 bottom-4 flex gap-2">
        {slides.map((_, i) => <button key={i} aria-label={`Slide ${i + 1}`} onClick={() => go(i)} className={`w-3 h-3 rounded-full ${i === index ? 'bg-white' : 'bg-white/60'}`} />)}
      </div>
    </section>
  );
}