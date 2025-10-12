﻿using System;
using Artigo.Intf.Enums;
using System.Collections.Generic;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa uma requisição de fluxo de trabalho que exige aprovação ou revisão pela equipe editorial.
    /// Funciona como uma caixa de entrada/fila de ações pendentes.
    /// </sumario>
    public class Pending
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // Referência à entidade alvo da requisição (Artigo.Id, Autor.Id, Staff.Id, etc.).
        public string TargetEntityId { get; set; } = string.Empty;

        // Tipo de entidade alvo (ajuda o serviço a saber qual coleção modificar).
        public TargetEntityType TargetType { get; set; } = TargetEntityType.Artigo;

        // O ID do usuário (UsuarioId externo) que solicitou a ação.
        public string RequesterUsuarioId { get; set; } = string.Empty;

        // O tipo de comando ou ação sendo solicitado (Ex: "Publicar", "Rejeitar", "MudarFuncao").
        public string CommandType { get; set; } = string.Empty;

        // Detalhes da requisição ou comentário explicativo do solicitante.
        public string Commentary { get; set; } = string.Empty;

        // Parâmetros do Comando: Armazena dados complexos ou a query original (CMD) em formato JSON (string serializada).
        // Exemplos: NovoStatus="Published", NovoEditorId="id_editor_x".
        public string CommandParametersJson { get; set; } = string.Empty;

        // Metadados
        public DateTime DateRequested { get; set; } = DateTime.UtcNow;

        // O status da requisição (Em analise, Aprovado, Rejeitado).
        public PendingStatus Status { get; set; } = PendingStatus.AwaitingReview;
    }
}
