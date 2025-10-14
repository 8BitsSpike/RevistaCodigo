using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using AutoMapper;

namespace Artigo.Server.Services
{
    /// <sumario>
    /// Implementacao do contrato IArtigoService. Contem toda a logica de negocio,
    /// orquestracao entre repositorios e as regras de autorizacao.
    /// </sumario>
    public class ArtigoService : IArtigoService
    {
        private readonly IArtigoRepository _artigoRepository;
        private readonly IAutorRepository _autorRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEditorialRepository _editorialRepository;
        private readonly IArtigoHistoryRepository _historyRepository;
        private readonly IPendingRepository _pendingRepository;
        private readonly IInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly IVolumeRepository _volumeRepository;

        public ArtigoService(
            IArtigoRepository artigoRepository,
            IAutorRepository autorRepository,
            IStaffRepository staffRepository,
            IEditorialRepository editorialRepository,
            IArtigoHistoryRepository historyRepository,
            IPendingRepository pendingRepository,
            IInteractionRepository interactionRepository,
            IVolumeRepository volumeRepository, // It's here
        IMapper mapper)
        {
            {
                _artigoRepository = artigoRepository;
                _autorRepository = autorRepository;
                _staffRepository = staffRepository;
                _editorialRepository = editorialRepository;
                _historyRepository = historyRepository;
                _pendingRepository = pendingRepository;
                _interactionRepository = interactionRepository;
                _volumeRepository = volumeRepository; // It's assigned here
                _mapper = mapper;
            }
        }

        // ----------------------------------------------------
        // I. Métodos de Autorização (Regras de Negócio)
        // ----------------------------------------------------

            // Método alterado para ser assíncrono para evitar deadlocks e usar 'await'.
            // FIX: Altera a assinatura para aceitar Staff?
        private async Task<bool> CanReadArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            // 1. Qualquer um pode ler se estiver publicado
            if (artigo.Status == ArtigoStatus.Published)
            {
                return true;
            }

            // Se nao estiver publicado, deve ser membro da equipe editorial
            if (staff == null) // This check is already correct
            {
                return false;
            }

            // 2. Verifica se o usuario faz parte da equipe editorial do artigo
            // Usando 'await' em vez de '.Result'
            var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);

            if (editorial == null) return false;

            var team = editorial.Team;

            // Membros da equipe incluem autores e todos os papeis designados
            var allowedUserIds = team.InitialAuthorId
                .Concat(team.ReviewerIds)
                .Concat(team.CorrectorIds)
                .Concat(new[] { team.EditorId })
                .ToList();

            // Verifica se o ID do usuario logado esta em qualquer um dos grupos permitidos
            return allowedUserIds.Contains(currentUsuarioId);
        }

        // O método de edição agora é assíncrono para chamar CanReadArtigoAsync
        private async Task<bool> CanEditArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            if (artigo.Status == ArtigoStatus.Published)
            {
                return false;
            }
            if (await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                return true;
            }
            return false;
        }

        // (Métodos de autorização síncronos como CanModifyStatus permanecem síncronos, pois não fazem I/O)

        private bool CanModifyStatus(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == JobRole.EditorBolsista || staff.Job == JobRole.EditorChefe || staff.Job == JobRole.Administrador;
        }

        private bool CanCreateEditorialComment(Artigo.Intf.Entities.Editorial editorial, string currentUsuarioId)
        {
            if (editorial == null) return false;
            var team = editorial.Team;

            var allowedUserIds = team.InitialAuthorId
                .Concat(team.ReviewerIds)
                .Concat(team.CorrectorIds)
                .Concat(new[] { team.EditorId })
                .ToList();

            return allowedUserIds.Contains(currentUsuarioId);
        }

        private bool CanEditVolume(Artigo.Intf.Entities.Staff? staff)
        {
            return staff != null;
        }

        // FIX: Altera a assinatura para aceitar Staff?
        private bool CanCreatePending(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == JobRole.EditorBolsista || staff.Job == JobRole.Administrador;
        }

        // FIX: Altera a assinatura para aceitar Staff?
        private bool CanModifyPendingStatus(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == JobRole.EditorChefe || staff.Job == JobRole.Administrador;
        }


        // ----------------------------------------------------
        // II. Metodos de Leitura (Queries)
        // ----------------------------------------------------

        public async Task<Artigo.Intf.Entities.Artigo?> GetPublishedArtigoAsync(string id)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null || artigo.Status != ArtigoStatus.Published) return null;
            return artigo; // Retorna a entidade Artigo (IArtigoService exige entidade)
        }

        public async Task<Artigo.Intf.Entities.Artigo?> GetArtigoForEditorialAsync(string id, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // Chamando o método assíncrono corrigido.
            if (!await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                return null;
            }

            return artigo; // Retorna a entidade Artigo (IArtigoService exige entidade)
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetArtigosByStatusAsync(ArtigoStatus status, string currentUsuarioId)
        {
            var artigos = await _artigoRepository.GetByStatusAsync(status);
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // O filtro agora usa await/async para a verificação de autorização.
            var authorizedArtigos = new List<Artigo.Intf.Entities.Artigo>();
            foreach (var artigo in artigos)
            {
                if (await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
                {
                    authorizedArtigos.Add(artigo);
                }
            }

            return authorizedArtigos;
        }

        // ----------------------------------------------------
        // III. Metodos de Escrita (Mutations)
        // ----------------------------------------------------

        // FIX: Assinatura alterada para corresponder ao contrato IArtigoService, aceitando 'initialContent'.
        public async Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string initialContent, string currentUsuarioId)
        {
            // 1. Lógica de Negócio / Inicialização
            artigo.Id = Guid.NewGuid().ToString();
            artigo.Status = ArtigoStatus.Draft;

            // Garante que o usuário logado seja o autor principal (se ainda não estiver na lista)
            if (!artigo.AutorIds.Contains(currentUsuarioId))
            {
                artigo.AutorIds.Insert(0, currentUsuarioId);
            }

            // 2. Orquestração e Persistência

            // 2.1. Criar ArtigoHistory inicial
            var initialHistory = new Artigo.Intf.Entities.ArtigoHistory
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigo.Id,
                Version = ArtigoVersion.Original,
                Content = initialContent // FIX: Usa o novo parâmetro
            };
            await _historyRepository.AddAsync(initialHistory);

            // 2.2. Criar Editorial (e equipe)
            var editorial = new Artigo.Intf.Entities.Editorial
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigo.Id,
                Position = EditorialPosition.Submitted,
                CurrentHistoryId = initialHistory.Id,
                HistoryIds = new List<string> { initialHistory.Id },
                Team = new EditorialTeam { InitialAuthorId = artigo.AutorIds }
            };
            await _editorialRepository.AddAsync(editorial);

            // 2.3. Ligar Editorial ao Artigo
            artigo.EditorialId = editorial.Id;

            // 2.4. Atualizar registro do Autor (se nao existir, cria-o no repositório)
            var autor = await _autorRepository.GetByUsuarioIdAsync(currentUsuarioId) ?? new Artigo.Intf.Entities.Autor { Id = Guid.NewGuid().ToString(), UsuarioId = currentUsuarioId };
            autor.ArtigoWorkIds.Add(artigo.Id);
            autor.Contribuicoes.Add(new ContribuicaoEditorial { ArtigoId = artigo.Id, Role = ContribuicaoRole.AutorPrincipal });
            autor = await _autorRepository.UpsertAsync(autor);

            // 2.5. Persistir Artigo final
            await _artigoRepository.AddAsync(artigo);

            // FIX: Retorna a entidade Artigo (conforme o contrato IArtigoService)
            return artigo;
        }

        // O método UpdateArtigoMetadataAsync foi renomeado de UpdateArtigoAsync no arquivo original
        public async Task<bool> UpdateArtigoMetadataAsync(Artigo.Intf.Entities.Artigo artigo, string currentUsuarioId)
        {
            var existingArtigo = await _artigoRepository.GetByIdAsync(artigo.Id);
            if (existingArtigo == null) return false;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao de Edicao
            // Chamando o método assíncrono corrigido.
            if (!await CanEditArtigoAsync(existingArtigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este artigo.");
            }

            // 2. Mapear e atualizar campos (apenas Titulo, Resumo, Tipo, etc.)
            existingArtigo.Titulo = artigo.Titulo;
            existingArtigo.Resumo = artigo.Resumo;
            existingArtigo.Tipo = artigo.Tipo;
            existingArtigo.AutorReference = artigo.AutorReference;

            // 3. Persistir no Repositorio
            return await _artigoRepository.UpdateAsync(existingArtigo);
        }
        // O método ChangeArtigoStatusAsync foi renomeado de UpdateArtigoStatusAsync no arquivo original
        public async Task<bool> ChangeArtigoStatusAsync(string artigoId, ArtigoStatus newStatus, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao de Status
            if (!CanModifyStatus(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editores Bolsistas e Chefes podem alterar o status do artigo.");
            }

            // 2. Lógica de Negócio: Atualizar Artigo
            artigo.Status = newStatus;

            // 3. Se estiver publicando, atualizar DataPublicacao e VolumeId (simplificado aqui)
            if (newStatus == ArtigoStatus.Published)
            {
                artigo.DataPublicacao = DateTime.UtcNow;
            }

            // 4. Persistir
            return await _artigoRepository.UpdateAsync(artigo);
        }

        public async Task<bool> UpdateArtigoContentAsync(string artigoId, string newContent, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new System.Collections.Generic.KeyNotFoundException("Artigo não encontrado.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao de Edição
            if (!await CanEditArtigoAsync(artigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar o conteúdo deste artigo.");
            }

            // 2. Orquestração da Nova Versão de Conteúdo

            // 2.1. Busca o registro Editorial
            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.EditorialId);
            if (editorial == null) throw new System.Collections.Generic.KeyNotFoundException("Registro editorial não encontrado.");

            // 2.2. Determina a nova versão baseada no status atual
            // Para simplificar, assumimos que qualquer edição após a original é a "Primeira Edição" 
            // e criamos uma nova versão numérica se já houver uma.
            int nextVersionNum = editorial.HistoryIds.Count;
            ArtigoVersion nextVersion = (ArtigoVersion)Math.Min(nextVersionNum, (int)ArtigoVersion.Final);

            // 2.3. Cria o novo registro ArtigoHistory
            var newHistory = new ArtigoHistory
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigoId,
                Version = nextVersion,
                Content = newContent,
                DataRegistro = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(newHistory);

            // 2.4. Atualiza o Editorial para apontar para a nova versão
            editorial.CurrentHistoryId = newHistory.Id;
            editorial.HistoryIds.Add(newHistory.Id);

            // 2.5. Persiste a atualização no Editorial
            bool success = await _editorialRepository.UpdateHistoryAsync(editorial.Id, newHistory.Id, editorial.HistoryIds);

            // 2.6. Atualiza a Data de Edição no Artigo principal
            artigo.DataEdicao = DateTime.UtcNow;
            await _artigoRepository.UpdateAsync(artigo); // Atualiza apenas os metadados de data

            return success;
        }


        // ----------------------------------------------------
        // IV. Metodos de Interacao e Workflow
        // ----------------------------------------------------

        public async Task<Artigo.Intf.Entities.Interaction> CreatePublicCommentAsync(string artigoId, Artigo.Intf.Entities.Interaction newComment, string? parentCommentId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null || artigo.Status != ArtigoStatus.Published)
            {
                throw new InvalidOperationException("Comentários públicos só são permitidos em artigos publicados.");
            }

            // 1. Regra de Negócio: Nao ha regra de autorizacao para ComentarioPublico (qualquer um pode)

            // 2. Criar a Interacao
            newComment.Id = Guid.NewGuid().ToString();
            newComment.ArtigoId = artigoId;
            newComment.Type = InteractionType.ComentarioPublico;
            // FIX: Usar ParentCommentId
            newComment.ParentCommentId = parentCommentId;

            // 3. Persistir e atualizar Artigo (denormalizacao)
            await _interactionRepository.AddAsync(newComment);

            // Atualizar contadores no Artigo (Subset Pattern)
            int totalComentarios = artigo.TotalComentarios + 1;
            int totalInteracoes = artigo.TotalInteracoes + 1;
            await _artigoRepository.UpdateMetricsAsync(artigoId, totalComentarios, totalInteracoes);

            // Atualiza o DTO local para retorno
            artigo.TotalComentarios = totalComentarios;
            artigo.TotalInteracoes = totalInteracoes;

            return newComment;
        }

        public async Task<Artigo.Intf.Entities.Interaction> CreateEditorialCommentAsync(string artigoId, Artigo.Intf.Entities.Interaction newComment, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.EditorialId);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial não encontrado.");

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreateEditorialComment(editorial, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não faz parte da equipe editorial deste artigo.");
            }

            // 2. Criar a Interacao
            newComment.Id = Guid.NewGuid().ToString();
            newComment.ArtigoId = artigoId;
            newComment.Type = InteractionType.ComentarioEditorial;
            // FIX: Garantir que ParentCommentId é nulo para comentários editoriais
            newComment.ParentCommentId = null;

            // 3. Persistir e ligar ao Editorial
            await _interactionRepository.AddAsync(newComment);

            // Adicionar referencia ao Editorial
            await _editorialRepository.AddCommentIdAsync(editorial.Id, newComment.Id);

            return newComment;
        }

        // --- Pending Methods (Queue Management) ---

        public async Task<Artigo.Intf.Entities.Pending> CreatePendingRequestAsync(Artigo.Intf.Entities.Pending newRequest, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreatePending(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editores Bolsistas e Administradores podem criar novas requisições pendentes.");
            }

            // 2. Lógica de Negócio: Inicializar a requisição
            newRequest.Id = Guid.NewGuid().ToString();
            newRequest.RequesterUsuarioId = currentUsuarioId; // Garantir que o solicitante esteja correto
            newRequest.DateRequested = DateTime.UtcNow;
            newRequest.Status = Artigo.Intf.Enums.PendingStatus.AwaitingReview;

            // 3. Persistir
            await _pendingRepository.AddAsync(newRequest);

            return newRequest;
        }

        public async Task<bool> ResolvePendingRequestAsync(string pendingId, bool isApproved, string currentUsuarioId)
        {
            var pendingRequest = await _pendingRepository.GetByIdAsync(pendingId);
            if (pendingRequest == null) throw new KeyNotFoundException($"Requisição pendente com ID {pendingId} não encontrada.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao (Only EditorChefe/Administrador)
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para resolver requisições pendentes.");
            }

            // Rejeita ou Arquiva a requisição imediatamente se não for aprovada
            if (!isApproved)
            {
                pendingRequest.Status = Artigo.Intf.Enums.PendingStatus.Rejected;
                return await _pendingRepository.UpdateAsync(pendingRequest);
            }

            // 2. Processa a Aprovação
            try
            {
                // **Atenção:** Esta lógica DEVE ser executada dentro de uma transação MongoDB se a Atomicidade for crucial.

                // 2.1. Deserializa os parâmetros para saber o que fazer
                // Utiliza System.Text.Json para deserializar a string para um Dictionary<string, string>
                var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(pendingRequest.CommandParametersJson)
                                 ?? new Dictionary<string, string>();

                // 2.2. Execução Condicional Baseada no CommandType
                bool executionSuccess = false;

                switch (pendingRequest.CommandType)
                {
                    case "ChangeArtigoStatus":
                        // Ex: TargetType=Artigo, CommandType=ChangeArtigoStatus, Parameters={"NewStatus":"InReview"}
                        if (pendingRequest.TargetType != Artigo.Intf.Enums.TargetEntityType.Artigo) throw new InvalidOperationException("TargetType inválido para o comando ChangeArtigoStatus.");

                        if (parameters.TryGetValue("NewStatus", out string? newStatusString) && Enum.TryParse<ArtigoStatus>(newStatusString, true, out ArtigoStatus newStatus))
                        {
                            // Chamamos o método do Service para aplicar a lógica de transição de status
                            executionSuccess = await ChangeArtigoStatusAsync(pendingRequest.TargetEntityId, newStatus, currentUsuarioId);
                        }
                        break;

                    case "UpdateStaffJob":
                        // Ex: TargetType=Staff, CommandType=UpdateStaffJob, Parameters={"NewJob":"EditorChefe"}
                        if (pendingRequest.TargetType != Artigo.Intf.Enums.TargetEntityType.Staff) throw new InvalidOperationException("TargetType inválido para o comando UpdateStaffJob.");

                        // NOTE: Esta chamada deve ser implementada em IStaffService, mas usamos o Repositório para simplificar o fluxo de execução aqui.
                        var staffToUpdate = await _staffRepository.GetByIdAsync(pendingRequest.TargetEntityId);
                        if (staffToUpdate != null && parameters.TryGetValue("NewJob", out string? newJobString) && Enum.TryParse<JobRole>(newJobString, true, out JobRole newJob))
                        {
                            staffToUpdate.Job = newJob;
                            executionSuccess = await _staffRepository.UpdateAsync(staffToUpdate);
                        }
                        break;

                    // Adicionar outros comandos (e.g., Publicar Volume, Mudar Nome do Artigo) aqui
                    default:
                        throw new InvalidOperationException($"Comando '{pendingRequest.CommandType}' não reconhecido para execução.");
                }

                // 3. Finaliza a Requisição se a execução for bem-sucedida
                if (executionSuccess)
                {
                    pendingRequest.Status = Artigo.Intf.Enums.PendingStatus.Approved;
                }
                else
                {
                    // Se a execução da mudança falhar (e.g., ID não existe), rejeitamos a pending request.
                    pendingRequest.Status = Artigo.Intf.Enums.PendingStatus.Rejected;
                    // Lançar exceção para o log, mas registrar o status de rejeição.
                    throw new InvalidOperationException($"Falha na execução do comando '{pendingRequest.CommandType}' no item alvo ID {pendingRequest.TargetEntityId}.");
                }
            }
            catch (Exception ex)
            {
                // Captura falhas de deserialização ou exceções de execução e marca como rejeitado.
                pendingRequest.Status = Artigo.Intf.Enums.PendingStatus.Rejected;
                // Re-lança a exceção para que o ErrorFilter do GraphQL a capture.
                throw new InvalidOperationException($"Falha ao processar a requisição pendente: {ex.Message}", ex);
            }
            finally
            {
                // 4. Persiste o status final (Aprovado ou Rejeitado)
                await _pendingRepository.UpdateAsync(pendingRequest);
            }

            return pendingRequest.Status == Artigo.Intf.Enums.PendingStatus.Approved;
        }

        // --- Volume Methods ---

        public async Task<bool> UpdateVolumeMetadataAsync(Artigo.Intf.Entities.Volume updatedVolume, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao (Apenas Staff pode editar Volumes)
            if (!CanEditVolume(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para atualizar os metadados do volume.");
            }

            // 2. Busca e verifica a existência
            var existingVolume = await _volumeRepository.GetByIdAsync(updatedVolume.Id);
            if (existingVolume == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Volume com ID {updatedVolume.Id} não encontrado.");
            }

            // 3. Persiste a atualização
            // O repositório usa ReplaceOneAsync, então passamos o objeto com os novos dados.
            return await _volumeRepository.UpdateAsync(updatedVolume);
        }
    }
}