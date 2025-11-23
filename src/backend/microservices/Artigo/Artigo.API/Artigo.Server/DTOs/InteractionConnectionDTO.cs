using Artigo.Intf.Entities;
using System.Collections.Generic;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO para o "Artigo View" que representa uma conexão paginada com Interações.
    /// Isso permite retornar uma "fatia" (slice) dos comentários (ex: os 10 primeiros)
    /// e também o número total de comentários.
    /// </sumario>
    public class InteractionConnectionDTO
    {
        /// <sumario>
        /// A lista de comentários editoriais (geralmente não paginada).
        /// </sumario>
        public List<Interaction> ComentariosEditoriais { get; set; } = [];

        /// <sumario>
        /// A fatia paginada de comentários públicos.
        /// </sumario>
        public List<Interaction> ComentariosPublicos { get; set; } = [];

        /// <sumario>
        /// O número total de comentários públicos disponíveis.
        /// </sumario>
        public int TotalComentariosPublicos { get; set; } = 0;
    }
}