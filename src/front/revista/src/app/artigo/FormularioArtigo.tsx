'use client';
import React, { useState, useRef, useEffect } from 'react';
import 'quill/dist/quill.snow.css';
import type Quill from 'quill';

export default function FormularioArtigo() {
  const [titulo, setTitulo] = useState('');
  const [resumo, setResumo] = useState('');
  const [autores, setAutores] = useState<string[]>(['']);
  const [imagem, setImagem] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [artigo, setArtigo] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const MAX_RESUMO = 150;

  const quillRef = useRef<HTMLDivElement>(null);
  const quillInstance = useRef<Quill | null>(null);
  const initializedRef = useRef(false);

  useEffect(() => {
    if (initializedRef.current) return;

    const initializeQuill = async () => {
      const { default: QuillModule } = await import('quill');

      if (quillRef.current && !quillInstance.current) {
        const toolbarOptions = [
          ['bold', 'italic', 'underline', 'strike'],
          ['blockquote', 'code-block'],
          [{ 'list': 'ordered' }, { 'list': 'bullet' }],
          [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
          [{ 'color': [] }, { 'background': [] }],
          ['link', 'image'],
          ['clean']
        ];

        const quill = new QuillModule(quillRef.current, {
          theme: 'snow',
          placeholder: 'Escreva o conteúdo do artigo...',
          modules: {
            toolbar: toolbarOptions,
          },
        });

        if (artigo) {
          quill.root.innerHTML = artigo;
        }

        quillInstance.current = quill;
        initializedRef.current = true;
        quill.on('text-change', () => {
          const htmlContent = quill.root.innerHTML;
          setArtigo(htmlContent);
        });
      }
    };

    initializeQuill();
    return () => {
    };
  }, []);


  const handleImagem = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setImagem(file);
    setPreview(file ? URL.createObjectURL(file) : null);
  };

  const handleAutorChange = (index: number, value: string) => {
    const novos = [...autores];
    novos[index] = value;
    setAutores(novos);
  };

  const adicionarAutor = () => setAutores([...autores, '']);
  const removerAutor = (index: number) => setAutores(autores.filter((_, i) => i !== index));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!titulo.trim()) return setError('O título é obrigatório.');
    if (!resumo.trim()) return setError('O resumo é obrigatório.');
    if (resumo.length > MAX_RESUMO) return setError(`O resumo deve ter no máximo ${MAX_RESUMO} caracteres.`);
    if (!autores.some(a => a.trim())) return setError('Adicione ao menos um autor.');

    const isArtigoEmpty = artigo === '<p><br></p>' || artigo.trim() === '';
    if (isArtigoEmpty) return setError('O conteúdo do artigo é obrigatório.');

    setLoading(true);
    try {
      const query = `
      mutation CriarNovoArtigo($input: CreateArtigoRequestInput!) {
        criarArtigo(input: $input) {
          id
          titulo
          status
          editorial {
            id
          }
        }
      }
    `;

      const variables = {
        input: {
          titulo,
          resumo,
          conteudo: artigo,
          tipo: "Artigo",
          idsAutor: autores.filter(a => a.trim()),
          referenciasAutor: []
        }
      };

      const res = await fetch('/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ query, variables }),
      });

      const data = await res.json();

      if (data.errors) {
        throw new Error(data.errors[0].message || 'Erro desconhecido ao criar o artigo');
      }

      setSuccess('Artigo enviado com sucesso! 🎉');
      setTitulo('');
      setResumo('');
      setArtigo('');
      setAutores(['']);
      setImagem(null);
      setPreview(null);
      quillInstance.current?.setText('');

    } catch (err: any) {
      setError(err.message || 'Erro desconhecido. 😢');
    } finally {
      setLoading(false);
    }
  };


  return (<div className="w-full font-sans">
    <h2 className="text-2xl font-bold text-gray-800 mb-6">Publicar Artigo</h2>

    <form className="space-y-6" onSubmit={handleSubmit}>
      {/* Título */}
      <div>
        <label className="block text-sm font-semibold text-gray-700">Título</label>
        <input
          type="text"
          value={titulo}
          onChange={(e) => setTitulo(e.target.value)}
          placeholder="Digite o título do artigo"
          className="mt-2 block w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
          disabled={loading}
        />
      </div>

      {/* Resumo */}
      <div>
        <label className="block text-sm font-semibold text-gray-700">
          Resumo (máx. {MAX_RESUMO} caracteres)
        </label>
        <textarea
          value={resumo}
          onChange={(e) => setResumo(e.target.value.slice(0, MAX_RESUMO))}
          placeholder="Breve resumo do artigo"
          className="mt-2 block w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 h-24"
          disabled={loading}
        />
        <p className="text-xs text-gray-500 text-right mt-1">
          {resumo.length}/{MAX_RESUMO}
        </p>
      </div>

      {/* Upload de Imagem */}
      <div>
        <label className="block text-sm font-semibold text-gray-700">Imagem de capa</label>
        <input
          placeholder='Escolha a imagem de capa para seu artigo'
          type="file"
          accept="image/*"
          onChange={handleImagem}
          className="mt-2 block w-full text-sm text-gray-700"
          disabled={loading}
        />
        {preview && (
          <div className="mt-3 flex justify-center">
            <img
              src={preview}
              alt="Preview"
              className="w-32 h-32 object-cover rounded-lg border border-gray-300"
            />
          </div>
        )}
      </div>

      {/* Autores */}
      <div>
        <label className="block text-sm font-semibold text-gray-700">Autores</label>
        {autores.map((autor, i) => (
          <div key={i} className="flex items-center gap-2 mt-2">
            <input
              type="text"
              value={autor}
              onChange={(e) => handleAutorChange(i, e.target.value)}
              placeholder={`Autor ${i + 1}`}
              className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
              disabled={loading}
            />
            {autores.length > 1 && (
              <button
                type="button"
                onClick={() => removerAutor(i)}
                className="text-sm px-3 py-1 bg-red-100 text-red-600 rounded hover:bg-red-200"
              >
                Remover
              </button>
            )}
          </div>
        ))}
        <button
          type="button"
          onClick={adicionarAutor}
          className="mt-3 text-sm px-4 py-2 bg-emerald-100 text-emerald-700 rounded hover:bg-emerald-200"
          disabled={loading}
        >
          + Adicionar autor
        </button>
      </div>

      {/* Corpo do Artigo com Quill Editor */}
      <div>
        <label className="block text-sm font-semibold text-gray-700 mb-2">Conteúdo do Artigo</label>
        {/* div onde o Quill é exibido */}
        <div ref={quillRef} className="min-h-80">
        </div>
      </div>

      {/* Mensagens */}
      {error && (
        <div className="p-4 text-sm font-medium text-red-700 bg-red-100 border border-red-300 rounded-lg">
          {error}
        </div>
      )}
      {success && (
        <div className="p-4 text-sm font-medium text-green-700 bg-green-100 border border-green-300 rounded-lg">
          {success}
        </div>
      )}

      {/* Botões */}
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={() => {
            setTitulo('');
            setResumo('');
            setAutores(['']);
            setImagem(null);
            setPreview(null);
            setArtigo('');
            setError('');
            setSuccess('');
            quillInstance.current?.setText('');
          }}
          className="px-4 py-2 rounded-lg border border-gray-300 bg-gray-50 hover:bg-gray-100"
          disabled={loading}
        >
          Limpar
        </button>

        <button
          type="submit"
          disabled={loading}
          className={`px-4 py-2 rounded-lg text-white font-medium ${loading
            ? 'bg-emerald-400 cursor-not-allowed'
            : 'bg-emerald-600 hover:bg-emerald-700 focus:ring-4 focus:ring-emerald-500 focus:ring-opacity-50 hover:scale-[1.01]'
            } transition duration-200`}
        >
          {loading ? 'Publicando...' : 'Publicar'}
        </button>
      </div>
    </form>
  </div>
  );
}