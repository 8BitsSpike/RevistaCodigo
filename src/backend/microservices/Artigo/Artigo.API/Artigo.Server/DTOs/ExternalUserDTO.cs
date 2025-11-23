namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para representar informações básicas do usuário 
    /// (Nome e Mídia/Avatar) buscadas do sistema externo (UsuarioAPI).
    /// </sumario>
    public class ExternalUserDTO
    {
        /// <sumario>
        /// O ID do usuário no sistema externo (UsuarioAPI), usado como chave.
        /// </sumario>
        public string UsuarioId { get; set; } = string.Empty;

        /// <sumario>
        /// Nome completo ou nome de exibição do usuário.
        /// </sumario>
        public string Name { get; set; } = string.Empty;

        /// <sumario>
        /// URL ou referência à mídia de perfil/avatar do usuário.
        /// </sumario>
        public string MediaUrl { get; set; } = string.Empty;
    }
}