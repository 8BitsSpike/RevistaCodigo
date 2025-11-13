export default function Footer() {
  return (
    <footer className="w-full border-t py-6 mt-12">
      <div className="max-w-4xl mx-auto px-4 text-sm text-center">
        © {new Date().getFullYear()} Revista Brasileira de Educação Básica
      </div>
    </footer>
  );
}
