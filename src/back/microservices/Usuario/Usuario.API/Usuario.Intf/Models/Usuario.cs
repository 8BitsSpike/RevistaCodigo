using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 

namespace Usuario.Intf.Models
{
    /// <summary>
    /// Modelo que representa o Usuário no banco de dados MongoDB, incluindo campos de perfil e segurança.
    /// </summary>
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }

        // --- CAMPOS ESSENCIAIS E PERFIL ---

        //O tipo 1 é usuario comum - padrão 
        [BsonElement("tipo")]
        public string? Tipo { get; set; } = "1"; 

        [BsonElement("name")]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string? Name { get; set; } = string.Empty;

        [BsonElement("sobrenome")]
        public string? Sobrenome { get; set; } = string.Empty; 

        [BsonElement("email")]
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; } = string.Empty;

        [BsonElement("password")]
        [JsonIgnore] 
        public string? Password { get; set; } = string.Empty;

        [BsonElement("foto")]
        public string? Foto { get; set; } = string.Empty;

        [BsonElement("biografia")]
        public string? Biografia { get; set; } = string.Empty;

        /// <summary>
        /// Lista de informações institucionais
        /// </summary>
        [BsonElement("infoInstitucionais")]
        public List<InfoInstitucional>? InfoInstitucionais { get; set; } = new List<InfoInstitucional>();

        /// <summary>
        /// Lista de informações de atuação
        /// </summary>

        [BsonElement("Atuacao")]
        public List<Atuacao>? Atuacoes { get; set; } = new List<Atuacao>();

        // --- CAMPOS PARA RECUPERAÇÃO DE SENHA ---

        [BsonElement("resetToken")]
        [JsonIgnore]
        public string? ResetToken { get; set; } = string.Empty;

        [BsonElement("resetTokenExpiry")]
        [JsonIgnore]
        public DateTime? ResetTokenExpiry { get; set; }

        [BsonElement("isTokenUsed")]
        [JsonIgnore]
        public bool IsTokenUsed { get; set; } = false;

        // --- GERAÇÃO DE CLAIMS PARA JWT ---

        /// <summary>
        /// Retorna as Claims (declarações) necessárias para a criação do Token JWT.
        /// </summary>
        public Claim[] GetClaims()
        {
            return new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.Id.ToString()),
                new Claim(ClaimTypes.Name, this.Name ?? ""),
                new Claim(ClaimTypes.Email, this.Email ?? "")
            };
        }
    }
}