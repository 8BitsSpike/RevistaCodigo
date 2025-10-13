using System.Collections.Generic;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa o registro local de um Autor no sistema.
    /// Esta entidade funciona como uma tabela de ligacao (link table) para o serviço externo de Usuario (UserApi),
    /// armazenando apenas os dados de relacionamento e historico de contribuicao.
    /// </sumario>
    public class Autor
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // O ID do usuário no sistema externo (UsuarioApi) ao qual este Autor se refere.
        public string UsuarioId { get; set; } = string.Empty;

        // Historico de Artigos criados ou co-criados pelo autor.
        // Referencia a colecao Artigo.
        public List<string> ArtigoWorkIds { get; set; } = [];

        // Historico de contribuicoes no ciclo editorial (revisao, correcao, edicao)
        public List<ContribuicaoEditorial> Contribuicoes { get; set; } = [];
    }

    /// <sumario>
    /// Objeto embutido para rastrear o papel do Autor em cada ciclo editorial.
    /// </sumario>
    public class ContribuicaoEditorial
    {
        // Referencia o Artigo no qual a contribuicao ocorreu.
        public string ArtigoId { get; set; } = string.Empty;

        // O papel desempenhado pelo autor naquele ciclo.
        public ContribuicaoRole Role { get; set; }
    }
}
