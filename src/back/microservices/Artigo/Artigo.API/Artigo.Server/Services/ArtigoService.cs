using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using Artigo.Server.Interfaces;
using AutoMapper;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using MongoDB.Bson; // Necessário para ObjectId.GenerateNewId()

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
        private readonly IExternalUserService _externalUserService;

        public ArtigoService(
            IArtigoRepository artigoRepository,
            IAutorRepository autorRepository,
            IStaffRepository staffRepository,
            IEditorialRepository editorialRepository,
            IArtigoHistoryRepository historyRepository,
            IPendingRepository pendingRepository,
            IInteractionRepository interactionRepository,
            IVolumeRepository volumeRepository,
            IExternalUserService externalUserService,
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
                _volumeRepository = volumeRepository;
                _externalUserService = externalUserService;
                _mapper = mapper;
            }
        }

        // ----------------------------------------------------
        // I. Métodos de Autorização (Regras de Negócio)
        // ... (Seção I inalterada)
        // ----------------------------------------------------

        private async Task<bool> CanReadArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            // 1. Qualquer um pode ler se estiver publicado
            if (artigo.Status == StatusArtigo.Publicado)
            {
                return true;
            }

            // Se nao estiver publicado, deve ser membro da equipe editorial
            if (staff == null)
            {
                return false;
            }

            // 2. Verifica se o usuario faz parte da equipe editorial do artigo
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

        private async Task<bool> CanEditArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            if (artigo.Status == StatusArtigo.Publicado)
            {
                return false;
            }
            if (await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                return true;
            }
            return false;
        }

        private bool CanModifyStatus(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorBolsista || staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
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

        private bool CanCreatePending(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorBolsista || staff.Job == FuncaoTrabalho.Administrador;
        }

        private bool CanModifyPendingStatus(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
        }

        /// <sumario>
        /// Regra: Apenas EditorChefe e Administrador podem criar um novo registro de Staff.
        /// </sumario>
        private bool CanCreateStaff(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
        }


        // ----------------------------------------------------
        // II. Metodos de Leitura (Queries)
        // ... (Seção II inalterada)
        // ----------------------------------------------------

        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoPublicadoAsync(string id)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null || artigo.Status != StatusArtigo.Publicado) return null;
            return artigo;
        }

        // NOVO MÉTODO: Implementação para Visitantes
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPublicadosParaVisitantesAsync()
        {
            // Regra: Retorna todos os artigos com Status "Published"
            return await _artigoRepository.GetByStatusAsync(StatusArtigo.Publicado);
        }

        // RENOMEADO: GetArtigoForEditorialAsync -> ObterArtigoParaEditorialAsync
        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoParaEditorialAsync(string id, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                return null;
            }

            return artigo;
        }

        // RENOMEADO: GetArtigosByStatusAsync -> ObterArtigosPorStatusAsync
        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorStatusAsync(StatusArtigo status, string currentUsuarioId)
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

        public async Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string conteudoInicial, string currentUsuarioId)
        {
            // 1. Lógica de Negócio / Inicialização
            // REMOVIDO: Artigo.Id = Guid.NewGuid().ToString();
            artigo.Status = StatusArtigo.Rascunho;

            // Garante que o usuário logado seja o autor principal (se ainda não estiver na lista)
            if (!artigo.AutorIds.Contains(currentUsuarioId))
            {
                artigo.AutorIds.Insert(0, currentUsuarioId);
            }

            // 2. Orquestração e Persistência

            // 2.0. PERSISTIR ARTIGO PRINCIPAL PRIMEIRO PARA OBTER O ID
            await _artigoRepository.AddAsync(artigo);
            // AGORA: artigo.Id contém o ObjectId gerado.

            // 2.1. Criar ArtigoHistory inicial
            var initialHistory = new Artigo.Intf.Entities.ArtigoHistory
            {
                // Id = string.Empty, o repositório irá gerar o ObjectId.
                ArtigoId = artigo.Id, // Agora a ID do Artigo está preenchida
                Version = VersaoArtigo.Original,
                Content = conteudoInicial
            };
            await _historyRepository.AddAsync(initialHistory); // O repositório irá gerar o ObjectId para initialHistory

            // 2.2. Criar Editorial (e equipe)
            var editorial = new Artigo.Intf.Entities.Editorial
            {
                // Id = string.Empty, o repositório irá gerar o ObjectId.
                ArtigoId = artigo.Id, // Agora a ID do Artigo está preenchida
                Position = PosicaoEditorial.Submetido,
                CurrentHistoryId = initialHistory.Id,
                HistoryIds = new List<string> { initialHistory.Id },
                Team = new EditorialTeam { InitialAuthorId = artigo.AutorIds }
            };
            await _editorialRepository.AddAsync(editorial); // O repositório irá gerar o ObjectId para editorial

            // 2.3. Ligar Editorial ao Artigo
            artigo.EditorialId = editorial.Id;
            // O Artigo já foi persistido, então esta atualização (EditorialId) será feita no final (Passo 2.6).

            // 2.4. Atualizar registro do Autor (se nao existir, cria-o no repositório)
            var autor = await _autorRepository.GetByUsuarioIdAsync(currentUsuarioId) ?? new Artigo.Intf.Entities.Autor { Id = ObjectId.GenerateNewId().ToString(), UsuarioId = currentUsuarioId };
            autor.ArtigoWorkIds.Add(artigo.Id); // Agora a ID do Artigo está preenchida
            autor.Contribuicoes.Add(new ContribuicaoEditorial { ArtigoId = artigo.Id, Role = FuncaoContribuicao.AutorPrincipal });
            autor = await _autorRepository.UpsertAsync(autor);

            // 2.5. Atualizar Artigo final (Para persistir a referência EditorialId)
            // Nota: O método AddAsync não retorna o Artigo, mas já o modifica na memória.
            // Precisamos garantir que o Artigo final seja atualizado com o EditorialId.
            await _artigoRepository.UpdateAsync(artigo);

            return artigo;
        }

        // RENOMEADO: UpdateArtigoMetadataAsync -> AtualizarMetadadosArtigoAsync
        public async Task<bool> AtualizarMetadadosArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string currentUsuarioId)
        {
            var existingArtigo = await _artigoRepository.GetByIdAsync(artigo.Id);
            if (existingArtigo == null) return false;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao de Edicao
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

        // RENOMEADO: ChangeArtigoStatusAsync -> AlterarStatusArtigoAsync
        public async Task<bool> AlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, string currentUsuarioId)
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
            if (newStatus == StatusArtigo.Publicado)
            {
                artigo.DataPublicacao = DateTime.UtcNow;
            }

            // 4. Persistir
            return await _artigoRepository.UpdateAsync(artigo);
        }

        // RENOMEADO: UpdateArtigoContentAsync -> AtualizarConteudoArtigoAsync
        public async Task<bool> AtualizarConteudoArtigoAsync(string artigoId, string newContent, string currentUsuarioId)
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
            VersaoArtigo nextVersion = (VersaoArtigo)Math.Min(nextVersionNum, (int)VersaoArtigo.Final);

            // 2.3. Cria o novo registro ArtigoHistory
            var newHistory = new ArtigoHistory
            {
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
        // ... (Seção IV inalterada)
        // ----------------------------------------------------

        // RENOMEADO: CreatePublicCommentAsync -> CriarComentarioPublicoAsync
        public async Task<Artigo.Intf.Entities.Interaction> CriarComentarioPublicoAsync(string artigoId, Artigo.Intf.Entities.Interaction newComment, string? parentCommentId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null || artigo.Status != StatusArtigo.Publicado)
            {
                throw new InvalidOperationException("Comentários públicos só são permitidos em artigos publicados.");
            }

            // NOVO: Busca o nome do usuário para desnormalização
            var externalUser = await _externalUserService.GetUserByIdAsync(newComment.UsuarioId);
            newComment.UsuarioNome = externalUser?.Name ?? "Usuário Desconhecido";

            // 1. Regra de Negócio: Nao ha regra de autorizacao para ComentarioPublico (qualquer um pode)

            // 2. Criar a Interacao
            // REMOVIDO: newComment.Id = Guid.NewGuid().ToString();
            newComment.ArtigoId = artigoId;
            newComment.Type = TipoInteracao.ComentarioPublico;
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

        // RENOMEADO: CreateEditorialCommentAsync -> CriarComentarioEditorialAsync
        public async Task<Artigo.Intf.Entities.Interaction> CriarComentarioEditorialAsync(string artigoId, Artigo.Intf.Entities.Interaction newComment, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.EditorialId);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial não encontrado.");

            // NOVO: Busca o nome do usuário para desnormalização
            var externalUser = await _externalUserService.GetUserByIdAsync(newComment.UsuarioId);
            newComment.UsuarioNome = externalUser?.Name ?? "Usuário Desconhecido";

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreateEditorialComment(editorial, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não faz parte da equipe editorial deste artigo.");
            }

            // 2. Criar a Interacao
            // REMOVIDO: newComment.Id = Guid.NewGuid().ToString();
            newComment.ArtigoId = artigoId;
            newComment.Type = TipoInteracao.ComentarioEditorial;
            newComment.ParentCommentId = null;

            // 3. Persistir e ligar ao Editorial
            await _editorialRepository.AddCommentIdAsync(editorial.Id, newComment.Id);

            return newComment;
        }

        // --- Pending Methods (Queue Management) ---

        // RENOMEADO: CreatePendingRequestAsync -> CriarRequisicaoPendenteAsync
        public async Task<Artigo.Intf.Entities.Pending> CriarRequisicaoPendenteAsync(Artigo.Intf.Entities.Pending newRequest, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreatePending(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editores Bolsistas e Administradores podem criar novas requisições pendentes.");
            }

            // 2. Lógica de Negócio: Inicializar a requisição
            // REMOVIDO: newRequest.Id = Guid.NewGuid().ToString();
            newRequest.RequesterUsuarioId = currentUsuarioId; // Garantir que o solicitante esteja correto
            newRequest.DateRequested = DateTime.UtcNow;
            newRequest.Status = Artigo.Intf.Enums.StatusPendente.AguardandoRevisao;

            // 3. Persistir
            await _pendingRepository.AddAsync(newRequest);

            return newRequest;
        }

        // RENOMEADO: ResolvePendingRequestAsync -> ResolverRequisicaoPendenteAsync
        public async Task<bool> ResolverRequisicaoPendenteAsync(string pendingId, bool isApproved, string currentUsuarioId)
        {
            var pendingRequest = await _pendingRepository.GetByIdAsync(pendingId);
            if (pendingRequest == null) throw new KeyNotFoundException($"Requisição pendente com ID {pendingId} não encontrada.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao (Only EditorChefe/Administrador)
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para resolver requisições pendentes.");
            }

            // NOVO: Define o IdAprovador e DataAprovacao antes de qualquer persistência, garantindo o registro.
            pendingRequest.IdAprovador = currentUsuarioId;
            pendingRequest.DataAprovacao = DateTime.UtcNow;

            // Rejeita ou Arquiva a requisição imediatamente se não for aprovada
            if (!isApproved)
            {
                pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Rejeitado;
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
                        // Ex: TargetType=Artigo, CommandType=ChangeArtigoStatus, Parameters={"NewStatus":"EmRevisao"}
                        if (pendingRequest.TargetType != Artigo.Intf.Enums.TipoEntidadeAlvo.Artigo) throw new InvalidOperationException("TargetType inválido para o comando ChangeArtigoStatus.");

                        if (parameters.TryGetValue("NewStatus", out string? newStatusString) && Enum.TryParse<StatusArtigo>(newStatusString, true, out StatusArtigo newStatus))
                        {
                            // Chamamos o método do Service para aplicar a lógica de transição de status
                            executionSuccess = await AlterarStatusArtigoAsync(pendingRequest.TargetEntityId, newStatus, currentUsuarioId);
                        }
                        break;

                    case "UpdateStaffJob":
                        // Ex: TargetType=Staff, CommandType=UpdateStaffJob, Parameters={"NewJob":"EditorChefe"}
                        if (pendingRequest.TargetType != Artigo.Intf.Enums.TipoEntidadeAlvo.Staff) throw new InvalidOperationException("TargetType inválido para o comando UpdateStaffJob.");

                        // NOTE: Esta chamada deve ser implementada em IStaffService, mas usamos o Repositório para simplificar o fluxo de execução aqui.
                        var staffToUpdate = await _staffRepository.GetByIdAsync(pendingRequest.TargetEntityId);

                        if (staffToUpdate != null && parameters.TryGetValue("NewJob", out string? newJobString) && Enum.TryParse<FuncaoTrabalho>(newJobString, true, out FuncaoTrabalho newJob))
                        {
                            if (newJobString.Equals(newJob.ToString(), StringComparison.OrdinalIgnoreCase)) // Confirma que o valor é válido para o enum
                            {
                                staffToUpdate.Job = newJob;
                                executionSuccess = await _staffRepository.UpdateAsync(staffToUpdate);
                            }
                        }
                        break;

                    // Adicionar outros comandos (e.g., Publicar Volume, Mudar Nome do Artigo) aqui
                    default:
                        throw new InvalidOperationException($"Comando '{pendingRequest.CommandType}' não reconhecido para execução.");
                }

                // 3. Finaliza a Requisição se a execução for bem-sucedida
                if (executionSuccess)
                {
                    pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Aprovado;
                }
                else
                {
                    // Se a execução da mudança falhar (e.g., ID não existe), rejeitamos a pending request.
                    pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Rejeitado;
                    // Lançar exceção para o log, mas registrar o status de rejeição.
                    throw new InvalidOperationException($"Falha na execução do comando '{pendingRequest.CommandType}' no item alvo ID {pendingRequest.TargetEntityId}.");
                }
            }
            catch (Exception ex)
            {
                // Captura falhas de deserialização ou exceções de execução e marca como rejeitado.
                pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Rejeitado;
                // Re-lança a exceção para que o ErrorFilter do GraphQL a capture.
                throw new InvalidOperationException($"Falha ao processar a requisição pendente: {ex.Message}", ex);
            }
            finally
            {
                // 4. Persiste o status final (Aprovado ou Rejeitado)
                await _pendingRepository.UpdateAsync(pendingRequest);
            }

            return pendingRequest.Status == Artigo.Intf.Enums.StatusPendente.Aprovado;
        }

        // --- Volume Methods ---

        // RENOMEADO: UpdateVolumeMetadataAsync -> AtualizarMetadadosVolumeAsync
        public async Task<bool> AtualizarMetadadosVolumeAsync(Artigo.Intf.Entities.Volume updatedVolume, string currentUsuarioId)
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
            return await _volumeRepository.UpdateAsync(updatedVolume);
        }

        // =========================================================================
        // STAFF MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Cria um novo registro de Staff para um usuário externo e define sua função de trabalho.
        /// REGRA: Apenas Administradores ou Editores Chefes podem executar esta ação.
        /// </summary>
        public async Task<Staff> CriarNovoStaffAsync(string usuarioId, FuncaoTrabalho job, string currentUsuarioId)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreateStaff(requestingStaff))
            {
                throw new UnauthorizedAccessException("Apenas Administradores ou Editores Chefes podem adicionar novos membros Staff.");
            }

            // 2. Lógica de Negócio: Evitar duplicação
            var existingStaff = await _staffRepository.GetByUsuarioIdAsync(usuarioId);
            if (existingStaff != null)
            {
                throw new InvalidOperationException($"O usuário com ID '{usuarioId}' já é um membro Staff (Função: {existingStaff.Job}).");
            }

            // 3. Criar e Persistir o novo registro Staff
            var newStaff = new Staff
            {
                Id = string.Empty, // Garante que o ID seja string.Empty para o Repositório gerar o ObjectId
                UsuarioId = usuarioId, // ID externa
                Job = job,
                IsActive = true
            };

            await _staffRepository.AddAsync(newStaff); // Repositório irá gerar o ObjectId

            return newStaff;
        }
    }
}