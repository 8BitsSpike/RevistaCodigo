namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO para o "Volume Card" format.
    /// Contém informações públicas resumidas de um volume (edição).
    /// </sumario>
    public class VolumeCardDTO
    {
        public string Id { get; set; } = string.Empty;
        public string VolumeTitulo { get; set; } = string.Empty;
        public string VolumeResumo { get; set; } = string.Empty;

        // Mapeia a imagem de capa do volume
        public MidiaEntryDTO? ImagemCapa { get; set; }
    }
}