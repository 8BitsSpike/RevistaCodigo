using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o registro local de um Autor no sistema.
    /// Esta entidade funciona como uma tabela de ligação (link table) para o serviço externo de Usuario (UserApi),
    /// armazenando apenas os dados de relacionamento e histórico de contribuição.
    /// </sumario>
    public class Autor
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // O ID do usuário no sistema externo (UsuarioApi) ao qual este Autor se refere.
        public string UsuarioId { get; set; } = string.Empty;

        // Nome de exibição do usuário (obtido da mutação, vindo do UsuarioAPI).
        public string Nome { get; set; } = string.Empty;

        // URL da mídia de perfil/avatar do usuário (obtido da mutação).
        public string Url { get; set; } = string.Empty;

        // Histórico de Artigos criados ou co-criados pelo autor.
        // Referência a coleção Artigo.
        public List<string> ArtigoWorkIds { get; set; } = [];

        // Historico de contribuições no ciclo editorial (revisão, correção, edição)
        public List<ContribuicaoEditorial> Contribuicoes { get; set; } = [];
    }

    /// <sumario>
    /// Objeto embutido para rastrear o papel do Autor em cada ciclo editorial.
    /// </sumario>
    public class ContribuicaoEditorial
    {
        // Referência o Artigo no qual a contribuição ocorreu.
        public string ArtigoId { get; set; } = string.Empty;

        // O papel desempenhado pelo autor naquele ciclo.
        public FuncaoContribuicao Role { get; set; }
    }
}