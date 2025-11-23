using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Inputs;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Security.Claims;

namespace Artigo.Server.Services
{
    /// <sumario>
    /// Implementação do contrato IArtigoService. Contem toda a lógica de negócio,
    /// orquestração entre repositórios e as regras de autorização.
    /// </sumario>
    public class ArtigoService : IArtigoService
    {
        private readonly IUnitOfWork _uow;
        private readonly IArtigoRepository _artigoRepository;
        private readonly IAutorRepository _autorRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEditorialRepository _editorialRepository;
        private readonly IArtigoHistoryRepository _historyRepository;
        private readonly IPendingRepository _pendingRepository;
        private readonly IInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly IVolumeRepository _volumeRepository;

        /// <sumario>
        /// Opções de serialização JSON atualizadas.
        /// </sumario>
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // Converte Enums para Strings
            Converters = { new JsonStringEnumConverter() }
        };

        public ArtigoService(
            IUnitOfWork uow,
            IArtigoRepository artigoRepository,
            IAutorRepository autorRepository,
            IStaffRepository staffRepository,
            IEditorialRepository editorialRepository,
            IArtigoHistoryRepository historyRepository,
            IPendingRepository pendingRepository,
            IInteractionRepository interactionRepository,
            IVolumeRepository volumeRepository,
            IMapper mapper)
        {
            {
                _uow = uow;
                _artigoRepository = artigoRepository;
                _autorRepository = autorRepository;
                _staffRepository = staffRepository;
                _editorialRepository = editorialRepository;
                _historyRepository = historyRepository;
                _pendingRepository = pendingRepository;
                _interactionRepository = interactionRepository;
                _volumeRepository = volumeRepository;
                _mapper = mapper;
            }
        }

        // ----------------------------------------------------
        // I. Métodos de Autorização (Regras de Negócio)
        // ----------------------------------------------------
        private async Task<bool> CanReadArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            if (artigo.Status == StatusArtigo.Publicado)
            {
                return true;
            }

            if (staff != null)
            {
                return true;
            }

            var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);
            if (editorial == null) return false;

            var team = editorial.Team;

            // Garante que as listas não sejam nulas antes de concatenar
            var authors = team.InitialAuthorId ?? new List<string>();
            var reviewers = team.ReviewerIds ?? new List<string>();
            var correctors = team.CorrectorIds ?? new List<string>();
            var editors = team.EditorIds ?? new List<string>();
            var allTeamIds = authors
                .Concat(reviewers)
                .Concat(correctors)
                .Concat(editors);

            // Verifica se o ID existe na lista, ignorando maiúsculas/minúsculas
            return allTeamIds.Any(id => string.Equals(id, currentUsuarioId, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> CanEditArtigoAsync(Artigo.Intf.Entities.Artigo artigo, Artigo.Intf.Entities.Staff? staff, string currentUsuarioId)
        {
            if (artigo.Status == StatusArtigo.Publicado)
            {
                return staff != null;
            }
            if (await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                return true;
            }
            return false;
        }

        private bool IsStaff(Artigo.Intf.Entities.Staff? staff)
        {
            return staff != null && staff.IsActive;
        }

        private bool CanModifyStatus(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorBolsista || staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
        }

        private bool CanEditVolume(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
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

        private bool CanCreateStaff(Artigo.Intf.Entities.Staff? staff)
        {
            if (staff == null) return false;
            return staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.Administrador;
        }


        // -------------------------------------------------------------------------
        // II. ARTIGO CORE MANAGEMENT
        // Acesso: Público (se Publicado) ou Autor/Staff (se em edição)
        // -------------------------------------------------------------------------

        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoPublicadoAsync(string id)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null || artigo.Status != StatusArtigo.Publicado) return null;
            return artigo;
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPublicadosParaVisitantesAsync(int pagina, int tamanho)
        {
            return await _artigoRepository.GetByStatusAsync(StatusArtigo.Publicado, pagina, tamanho);
        }

        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoParaEditorialAsync(string id, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(id);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                if (artigo.Status != StatusArtigo.Publicado)
                {
                    throw new UnauthorizedAccessException("Usuário não tem permissão para ler este artigo.");
                }
            }

            return artigo;
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorStatusAsync(StatusArtigo status, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para esta lista.");
            }

            var artigos = await _artigoRepository.GetByStatusAsync(status, pagina, tamanho);
            return artigos;
        }

        public async Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string conteudoInicial, List<MidiaEntry> midiasCompletas, List<Autor> autores, string currentUsuarioId, string commentary)
        {
            await _uow.StartTransactionAsync();
            var session = _uow.GetSessionHandle();

            try
            {
                artigo.Status = StatusArtigo.Rascunho;

                var autorIds = new List<string>();
                foreach (var autorInput in autores)
                {
                    var upsertedAutor = await _autorRepository.UpsertAsync(autorInput, session);
                    autorIds.Add(upsertedAutor.Id);
                }

                var autorPrincipal = await _autorRepository.GetByUsuarioIdAsync(currentUsuarioId, session);
                if (autorPrincipal == null)
                {
                    var principalInput = autores.FirstOrDefault(a => a.UsuarioId == currentUsuarioId);
                    if (principalInput != null)
                    {
                        autorPrincipal = await _autorRepository.UpsertAsync(principalInput, session);
                    }
                    else
                    {
                        throw new InvalidOperationException("Usuário logado não possui um registro de Autor e não foi fornecido nos dados de entrada.");
                    }
                }
                if (!autorIds.Contains(autorPrincipal.Id))
                {
                    autorIds.Insert(0, autorPrincipal.Id);
                }
                artigo.AutorIds = autorIds;

                await _artigoRepository.AddAsync(artigo, session);

                var initialHistory = new Artigo.Intf.Entities.ArtigoHistory
                {
                    ArtigoId = artigo.Id,
                    Version = VersaoArtigo.Original,
                    Content = conteudoInicial,
                    Midias = midiasCompletas
                };
                await _historyRepository.AddAsync(initialHistory, session);

                var autorUsuarioIds = autores.Select(a => a.UsuarioId).Distinct().ToList();
                if (!autorUsuarioIds.Contains(currentUsuarioId))
                {
                    autorUsuarioIds.Insert(0, currentUsuarioId);
                }

                var editorial = new Artigo.Intf.Entities.Editorial
                {
                    ArtigoId = artigo.Id,
                    Position = PosicaoEditorial.Submetido,
                    CurrentHistoryId = initialHistory.Id,
                    HistoryIds = new List<string> { initialHistory.Id },
                    Team = new EditorialTeam { InitialAuthorId = autorUsuarioIds }
                };
                await _editorialRepository.AddAsync(editorial, session);

                artigo.EditorialId = editorial.Id;

                autorPrincipal.ArtigoWorkIds.Add(artigo.Id);
                autorPrincipal.Contribuicoes.Add(new ContribuicaoEditorial { ArtigoId = artigo.Id, Role = FuncaoContribuicao.AutorPrincipal });
                await _autorRepository.UpsertAsync(autorPrincipal, session);

                await _artigoRepository.UpdateAsync(artigo, session);

                await _uow.CommitTransactionAsync();

                return artigo;
            }
            catch (Exception)
            {
                await _uow.AbortTransactionAsync();
                throw;
            }
        }

        public async Task<bool> AtualizarMetadadosArtigoAsync(string artigoId, UpdateArtigoMetadataInput input, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(input, _jsonSerializerOptions);

                var pending = new Pending
                {
                    TargetEntityId = artigoId,
                    TargetType = TipoEntidadeAlvo.Artigo,
                    CommandType = "UpdateArtigoMetadata",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return true;
            }

            var existingArtigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (existingArtigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            if (staff?.Job == FuncaoTrabalho.EditorChefe || staff?.Job == FuncaoTrabalho.Administrador)
            {
                return await ExecuteAtualizarMetadadosArtigoAsync(existingArtigo, input, staff, currentUsuarioId);
            }

            if (!await CanEditArtigoAsync(existingArtigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este artigo.");
            }

            return await ExecuteAtualizarMetadadosArtigoAsync(existingArtigo, input, staff, currentUsuarioId);
        }

        private async Task<bool> ExecuteAtualizarMetadadosArtigoAsync(Artigo.Intf.Entities.Artigo existingArtigo, UpdateArtigoMetadataInput input, Staff? staff, string currentUsuarioId, object? sessionHandle = null)
        {
            bool artigoUpdated = ApplyMetadataUpdates(existingArtigo, input);
            if (artigoUpdated)
            {
                await _artigoRepository.UpdateAsync(existingArtigo, sessionHandle);
            }

            if (input.Posicao.HasValue)
            {
                if (!CanModifyStatus(staff))
                {
                    throw new UnauthorizedAccessException("Usuário não tem permissão para alterar a Posição Editorial.");
                }

                var editorial = await _editorialRepository.GetByIdAsync(existingArtigo.EditorialId, sessionHandle);
                if (editorial == null)
                {
                    throw new KeyNotFoundException("Registro editorial não encontrado para este artigo.");
                }

                await _editorialRepository.UpdatePositionAsync(editorial.Id, input.Posicao.Value, sessionHandle);
            }

            return true;
        }

        private bool ApplyMetadataUpdates(Artigo.Intf.Entities.Artigo existingArtigo, UpdateArtigoMetadataInput input)
        {
            bool updated = false;
            if (input.Titulo != null)
            {
                existingArtigo.Titulo = input.Titulo;
                updated = true;
            }
            if (input.Resumo != null)
            {
                existingArtigo.Resumo = input.Resumo;
                updated = true;
            }
            if (input.Tipo.HasValue)
            {
                existingArtigo.Tipo = input.Tipo.Value;
                updated = true;
            }
            if (input.ReferenciasAutor != null)
            {
                existingArtigo.AutorReference = input.ReferenciasAutor;
                updated = true;
            }
            if (input.IdsAutor != null)
            {
                existingArtigo.AutorIds = input.IdsAutor;
                updated = true;
            }
            if (input.Status.HasValue)
            {
                existingArtigo.Status = input.Status.Value;
                updated = true;
            }
            if (input.PermitirComentario.HasValue)
            {
                existingArtigo.PermitirComentario = input.PermitirComentario.Value;
                updated = true;
            }
            return updated;
        }

        public async Task<bool> AlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(new { NewStatus = newStatus.ToString() }, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = artigoId,
                    TargetType = TipoEntidadeAlvo.Artigo,
                    CommandType = "ChangeArtigoStatus",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return true;
            }

            if (staff?.Job == FuncaoTrabalho.EditorChefe || staff?.Job == FuncaoTrabalho.Administrador)
            {
                return await ExecuteAlterarStatusArtigoAsync(artigoId, newStatus);
            }

            throw new UnauthorizedAccessException("Usuário não tem permissão para alterar o status do artigo.");
        }

        private async Task<bool> ExecuteAlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, object? sessionHandle = null)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId, sessionHandle);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            artigo.Status = newStatus;
            if (newStatus == StatusArtigo.Publicado)
            {
                artigo.DataPublicacao = DateTime.UtcNow;
            }
            return await _artigoRepository.UpdateAsync(artigo, sessionHandle);
        }

        public async Task<bool> AtualizarConteudoArtigoAsync(string artigoId, string newContent, List<MidiaEntry> midias, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(new { Content = newContent, Midias = midias }, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = artigoId,
                    TargetType = TipoEntidadeAlvo.Artigo,
                    CommandType = "UpdateArtigoContent",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return true;
            }

            if (staff?.Job == FuncaoTrabalho.EditorChefe || staff?.Job == FuncaoTrabalho.Administrador)
            {
                return await ExecuteAtualizarConteudoArtigoAsync(artigoId, newContent, midias, currentUsuarioId);
            }

            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            if (!await CanEditArtigoAsync(artigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar o conteúdo deste artigo.");
            }
            return await ExecuteAtualizarConteudoArtigoAsync(artigoId, newContent, midias, currentUsuarioId);
        }

        private async Task<bool> ExecuteAtualizarConteudoArtigoAsync(string artigoId, string newContent, List<MidiaEntry> midias, string currentUsuarioId, object? sessionHandle = null)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId, sessionHandle);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.EditorialId, sessionHandle);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial não encontrado.");

            int nextVersionNum = editorial.HistoryIds.Count;
            VersaoArtigo nextVersion = (VersaoArtigo)Math.Min(nextVersionNum, (int)VersaoArtigo.Final);

            var newHistory = new ArtigoHistory
            {
                ArtigoId = artigoId,
                Version = nextVersion,
                Content = newContent,
                Midias = midias,
                DataRegistro = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(newHistory, sessionHandle);

            editorial.CurrentHistoryId = newHistory.Id;
            editorial.HistoryIds.Add(newHistory.Id);

            bool success = await _editorialRepository.UpdateHistoryAsync(editorial.Id, newHistory.Id, editorial.HistoryIds, sessionHandle);

            artigo.DataEdicao = DateTime.UtcNow;
            artigo.MidiaDestaque = midias.FirstOrDefault();
            await _artigoRepository.UpdateAsync(artigo, sessionHandle);

            return success;
        }

        public async Task<Editorial> AtualizarEquipeEditorialAsync(string artigoId, EditorialTeam team, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigoId);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial do artigo não encontrado.");

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(team, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = editorial.Id,
                    TargetType = TipoEntidadeAlvo.Editorial,
                    CommandType = "UpdateEditorialTeam",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return editorial;
            }

            if (staff?.Job == FuncaoTrabalho.EditorChefe || staff?.Job == FuncaoTrabalho.Administrador)
            {
                await _editorialRepository.UpdateTeamAsync(editorial.Id, team);
                editorial.Team = team;
                return editorial;
            }

            throw new UnauthorizedAccessException("Usuário não tem permissão para modificar a equipe editorial.");
        }

        // -------------------------------------------------------------------------
        // III. ARTIGO QUERY FORMATS
        // Acesso: Público ou Staff/Autor (para MeusArtigos/EditorialView)
        // -------------------------------------------------------------------------

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListAsync(int pagina, int tamanho)
        {
            return await _artigoRepository.ObterArtigosCardListAsync(pagina, tamanho);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho)
        {
            return await _artigoRepository.ObterArtigosCardListPorTipoAsync(tipo, pagina, tamanho);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTituloAsync(string searchTerm, int pagina, int tamanho)
        {
            return await _artigoRepository.SearchArtigosCardListByTitleAsync(searchTerm, pagina, tamanho);
        }

        public async Task<Autor?> ObterAutorCardAsync(string autorId)
        {
            return await _autorRepository.GetByIdAsync(autorId);
        }

        public async Task<Volume?> ObterVolumeCardAsync(string volumeId)
        {
            return await _volumeRepository.GetByIdAsync(volumeId);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorNomeAutorAsync(string searchTerm, int pagina, int tamanho)
        {
            int skip = pagina * tamanho;

            var autoresRegistrados = await _autorRepository.SearchAutoresByNameAsync(searchTerm);
            var autorIds = autoresRegistrados.Select(a => a.Id).ToList().AsReadOnly();

            Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> taskAutoresRegistrados;
            if (autorIds.Count > 0)
            {
                taskAutoresRegistrados = _artigoRepository.SearchArtigosCardListByAutorIdsAsync(autorIds);
            }
            else
            {
                taskAutoresRegistrados = Task.FromResult<IReadOnlyList<Artigo.Intf.Entities.Artigo>>(new List<Artigo.Intf.Entities.Artigo>());
            }

            Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> taskAutoresReferencia = _artigoRepository.SearchArtigosCardListByAutorReferenceAsync(searchTerm);

            await Task.WhenAll(taskAutoresRegistrados, taskAutoresReferencia);

            var artigosPorId = taskAutoresRegistrados.Result;
            var artigosPorReferencia = taskAutoresReferencia.Result;

            var artigosCombinados = new Dictionary<string, Artigo.Intf.Entities.Artigo>();

            foreach (var artigo in artigosPorId)
            {
                artigosCombinados[artigo.Id] = artigo;
            }
            foreach (var artigo in artigosPorReferencia)
            {
                artigosCombinados[artigo.Id] = artigo;
            }

            return artigosCombinados.Values
                .OrderByDescending(a => a.DataCriacao)
                .Skip(skip)
                .Take(tamanho)
                .ToList()
                .AsReadOnly();
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorListaIdsAsync(IReadOnlyList<string> ids)
        {
            var artigos = await _artigoRepository.GetByIdsAsync(ids);
            return artigos.Where(a => a.Status == StatusArtigo.Publicado).ToList();
        }

        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoViewAsync(string artigoId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null || artigo.Status != StatusArtigo.Publicado)
            {
                return null;
            }
            return artigo;
        }

        public async Task<Artigo.Intf.Entities.Artigo?> ObterArtigoEditorialViewAsync(string artigoId, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (staff != null && (staff.Job == FuncaoTrabalho.Administrador || staff.Job == FuncaoTrabalho.EditorChefe || staff.Job == FuncaoTrabalho.EditorBolsista))
            {
                return artigo;
            }

            var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);
            if (editorial == null)
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar os dados editoriais deste artigo.");
            }

            var team = editorial.Team;
            var allowedUsuarioIds = team.InitialAuthorId
                .Concat(team.ReviewerIds)
                .Concat(team.CorrectorIds)
                .ToList();

            if (allowedUsuarioIds.Contains(currentUsuarioId))
            {
                return artigo;
            }

            throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar os dados editoriais deste artigo.");
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterMeusArtigosCardListAsync(string currentUsuarioId)
        {
            var autor = await _autorRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (autor == null || autor.ArtigoWorkIds == null || !autor.ArtigoWorkIds.Any())
            {
                return new List<Artigo.Intf.Entities.Artigo>();
            }
            var artigos = await _artigoRepository.ObterArtigosCardListPorAutorIdAsync(autor.Id);
            return artigos;
        }

        // --- Métodos de Busca Editorial (Staff) ---

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosEditorialPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para esta busca.");
            }
            return await _artigoRepository.ObterArtigosEditorialPorTipoAsync(tipo, pagina, tamanho);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByTitleAsync(string searchTerm, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para esta busca.");
            }
            return await _artigoRepository.SearchArtigosEditorialByTitleAsync(searchTerm, pagina, tamanho);
        }

        public async Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByAutorIdsAsync(IReadOnlyList<string> autorIds, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para esta busca.");
            }
            return await _artigoRepository.SearchArtigosEditorialByAutorIdsAsync(autorIds, pagina, tamanho);
        }


        // -------------------------------------------------------------------------
        // IV. VOLUME MANAGEMENT
        // Acesso: Público (ObterVolumesListAsync/ObterVolumeViewAsync) ou Staff (Busca e Mutação)
        // -------------------------------------------------------------------------

        public async Task<IReadOnlyList<Volume>> ObterVolumesListAsync(int pagina, int tamanho)
        {
            return await _volumeRepository.ObterVolumesListAsync(pagina, tamanho);
        }

        public async Task<Volume?> ObterVolumeViewAsync(string volumeId)
        {
            var volume = await _volumeRepository.GetByIdAsync(volumeId);
            if (volume == null || volume.Status != StatusVolume.Publicado)
            {
                return null;
            }
            return volume;
        }

        public async Task<Volume> CriarVolumeAsync(Volume novoVolume, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            novoVolume.Status = StatusVolume.EmRevisao;

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(novoVolume, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = "N/A",
                    TargetType = TipoEntidadeAlvo.Volume,
                    CommandType = "CreateVolume",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return novoVolume;
            }

            if (!CanEditVolume(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para criar novos volumes.");
            }

            await _volumeRepository.AddAsync(novoVolume);
            return novoVolume;
        }

        public async Task<bool> AtualizarMetadadosVolumeAsync(string volumeId, UpdateVolumeMetadataInput input, string currentUsuarioId, string commentary)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(input, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = volumeId,
                    TargetType = TipoEntidadeAlvo.Volume,
                    CommandType = "UpdateVolume",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return true;
            }

            if (!CanEditVolume(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para atualizar os metadados do volume.");
            }

            var existingVolume = await _volumeRepository.GetByIdAsync(volumeId);
            if (existingVolume == null)
            {
                throw new KeyNotFoundException($"Volume com ID {volumeId} não encontrado.");
            }

            ApplyVolumeMetadataUpdates(existingVolume, input);

            return await _volumeRepository.UpdateAsync(existingVolume);
        }

        private void ApplyVolumeMetadataUpdates(Volume existingVolume, UpdateVolumeMetadataInput input)
        {
            if (input.Edicao.HasValue)
                existingVolume.Edicao = input.Edicao.Value;
            if (input.VolumeTitulo != null)
                existingVolume.VolumeTitulo = input.VolumeTitulo;
            if (input.VolumeResumo != null)
                existingVolume.VolumeResumo = input.VolumeResumo;
            if (input.M.HasValue)
                existingVolume.M = input.M.Value;
            if (input.N.HasValue)
                existingVolume.N = input.N.Value;
            if (input.Year.HasValue)
                existingVolume.Year = input.Year.Value;
            if (input.Status.HasValue)
                existingVolume.Status = input.Status.Value;
            if (input.ImagemCapa != null)
                existingVolume.ImagemCapa = input.ImagemCapa;
            if (input.ArtigoIds != null)
                existingVolume.ArtigoIds = input.ArtigoIds;
        }

        public async Task<IReadOnlyList<Volume>> ObterVolumesAsync(int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para listar volumes.");
            }
            return await _volumeRepository.GetAllAsync(pagina, tamanho);
        }

        public async Task<IReadOnlyList<Volume>> ObterVolumesPorAnoAsync(int ano, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para listar volumes por ano.");
            }
            return await _volumeRepository.GetByYearAsync(ano, pagina, tamanho);
        }

        public async Task<IReadOnlyList<Volume>> ObterVolumesPorStatusAsync(StatusVolume status, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para listar volumes por status.");
            }
            return await _volumeRepository.ObterVolumesPorStatusAsync(status, pagina, tamanho);
        }

        public async Task<Volume?> ObterVolumePorIdAsync(string idVolume, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para buscar um volume.");
            }
            return await _volumeRepository.GetByIdAsync(idVolume);
        }


        // -------------------------------------------------------------------------
        // V. INTERACTION & STAFF COMMENT MANAGEMENT
        // Acesso: Autenticado, Staff ou Membro da Equipe Editorial
        // -------------------------------------------------------------------------

        public async Task<Interaction> CriarComentarioPublicoAsync(string artigoId, Interaction newComment, string? parentCommentId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null)
                throw new InvalidOperationException("Artigo não encontrado.");
            if (artigo.Status != StatusArtigo.Publicado)
                throw new InvalidOperationException("Comentários públicos só são permitidos em artigos publicados.");
            if (artigo.PermitirComentario == false)
                throw new InvalidOperationException("Comentários não são permitidos neste artigo.");

            if (!string.IsNullOrEmpty(parentCommentId))
            {
                var parentComment = await _interactionRepository.GetByIdAsync(parentCommentId);
                if (parentComment == null || parentComment.Type != TipoInteracao.ComentarioPublico)
                {
                    throw new InvalidOperationException("Não é possível responder a este comentário.");
                }
            }

            newComment.ArtigoId = artigoId;
            newComment.Type = TipoInteracao.ComentarioPublico;
            newComment.ParentCommentId = parentCommentId;

            await _interactionRepository.AddAsync(newComment);

            int totalComentarios = artigo.TotalComentarios + 1;
            int totalInteracoes = artigo.TotalInteracoes + 1;
            await _artigoRepository.UpdateMetricsAsync(artigoId, totalComentarios, totalInteracoes);

            artigo.TotalComentarios = totalComentarios;
            artigo.TotalInteracoes = totalInteracoes;

            return newComment;
        }

        public async Task<Interaction> CriarComentarioEditorialAsync(string artigoId, Interaction newComment, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var editorial = await _editorialRepository.GetByArtigoIdAsync(artigo.EditorialId);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial não encontrado.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!await CanReadArtigoAsync(artigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não faz parte da equipe editorial deste artigo.");
            }

            newComment.ArtigoId = artigoId;
            newComment.Type = TipoInteracao.ComentarioEditorial;
            newComment.ParentCommentId = null;

            await _interactionRepository.AddAsync(newComment);

            await _editorialRepository.AddCommentIdAsync(editorial.Id, newComment.Id);

            return newComment;
        }

        public async Task<Interaction> AtualizarInteracaoAsync(string interacaoId, string newContent, string currentUsuarioId, string commentary)
        {
            var interacao = await _interactionRepository.GetByIdAsync(interacaoId);
            if (interacao == null) throw new KeyNotFoundException("Comentário não encontrado.");

            if (interacao.UsuarioId != currentUsuarioId)
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este comentário.");
            }

            interacao.Content = newContent;
            interacao.DataUltimaEdicao = DateTime.UtcNow;

            await _interactionRepository.UpdateAsync(interacao);
            return interacao;
        }

        public async Task<bool> DeletarInteracaoAsync(string interacaoId, string currentUsuarioId, string commentary)
        {
            var interacao = await _interactionRepository.GetByIdAsync(interacaoId);
            if (interacao == null) throw new KeyNotFoundException("Comentário não encontrado.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (interacao.UsuarioId == currentUsuarioId || staff?.Job == FuncaoTrabalho.Administrador || staff?.Job == FuncaoTrabalho.EditorChefe)
            {
                return await _interactionRepository.DeleteAsync(interacaoId);
            }

            if (staff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(new { InteracaoId = interacaoId }, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = interacaoId,
                    TargetType = TipoEntidadeAlvo.Comentario,
                    CommandType = "DeleteInteracao",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return true;
            }

            throw new UnauthorizedAccessException("Usuário não tem permissão para deletar este comentário.");
        }

        public async Task<IReadOnlyList<Interaction>> ObterComentariosPublicosAsync(string artigoId, int pagina, int tamanho)
        {
            return await _interactionRepository.GetPublicCommentsAsync(artigoId, pagina, tamanho);
        }

        // --- Métodos StaffComentario (para ArtigoHistory) ---

        public async Task<ArtigoHistory> AddStaffComentarioAsync(string historyId, string usuarioId, string comment, string? parent)
        {

            var history = await _historyRepository.GetByIdAsync(historyId);
            if (history == null) throw new KeyNotFoundException("Versão do histórico não encontrada.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(usuarioId);
            bool isStaff = (staff != null && staff.IsActive);

            if (!isStaff)
            {
                var artigo = await _artigoRepository.GetByIdAsync(history.ArtigoId);
                if (artigo == null) throw new KeyNotFoundException("Artigo associado ao histórico não encontrado.");

                var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);
                if (editorial == null) throw new UnauthorizedAccessException("Usuário não tem permissão para comentar nesta versão.");

                var team = editorial.Team;
                var allowedUsuarioIds = team.InitialAuthorId
                    .Concat(team.ReviewerIds)
                    .Concat(team.CorrectorIds)
                    .ToList();

                if (!allowedUsuarioIds.Contains(usuarioId))
                {
                    throw new UnauthorizedAccessException("Usuário não é Staff nem membro da equipe editorial deste artigo.");
                }
            }

            var newComment = new StaffComentario
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UsuarioId = usuarioId,
                Data = DateTime.UtcNow,
                Parent = parent,
                Comment = comment
            };

            history.StaffComentarios.Add(newComment);
            await _historyRepository.UpdateAsync(history);
            return history;
        }

        public async Task<ArtigoHistory> UpdateStaffComentarioAsync(string historyId, string comentarioId, string newContent, string currentUsuarioId)
        {
            var history = await _historyRepository.GetByIdAsync(historyId);
            if (history == null) throw new KeyNotFoundException("Versão do histórico não encontrada.");

            var comment = history.StaffComentarios.FirstOrDefault(c => c.Id == comentarioId);
            if (comment == null) throw new KeyNotFoundException("Comentário não encontrado.");

            if (comment.UsuarioId != currentUsuarioId)
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este comentário.");

            comment.Comment = newContent;
            comment.Data = DateTime.UtcNow;

            await _historyRepository.UpdateAsync(history);
            return history;
        }

        public async Task<ArtigoHistory> DeleteStaffComentarioAsync(string historyId, string comentarioId, string currentUsuarioId)
        {
            var history = await _historyRepository.GetByIdAsync(historyId);
            if (history == null) throw new KeyNotFoundException("Versão do histórico não encontrada.");

            var comment = history.StaffComentarios.FirstOrDefault(c => c.Id == comentarioId);
            if (comment == null) throw new KeyNotFoundException("Comentário não encontrado.");

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (comment.UsuarioId != currentUsuarioId && staff?.Job != FuncaoTrabalho.Administrador && staff?.Job != FuncaoTrabalho.EditorChefe)
                throw new UnauthorizedAccessException("Usuário não tem permissão para deletar este comentário.");

            history.StaffComentarios.Remove(comment);

            await _historyRepository.UpdateAsync(history);
            return history;
        }


        // -------------------------------------------------------------------------
        // VI. PENDING (FLUXO DE APROVAÇÃO) MANAGEMENT
        // Acesso: Staff (Criação - Bolsista, Resolução - Admin/Chefe)
        // -------------------------------------------------------------------------

        public async Task<Pending> CriarRequisicaoPendenteAsync(Pending newRequest, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!CanCreatePending(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editores Bolsistas e Administradores podem criar novas requisições pendentes.");
            }

            newRequest.RequesterUsuarioId = currentUsuarioId;
            newRequest.DateRequested = DateTime.UtcNow;
            newRequest.Status = Artigo.Intf.Enums.StatusPendente.AguardandoRevisao;

            await _pendingRepository.AddAsync(newRequest);
            return newRequest;
        }

        public async Task<bool> ResolverRequisicaoPendenteAsync(string pendingId, bool isApproved, string currentUsuarioId)
        {
            await _uow.StartTransactionAsync();
            var session = _uow.GetSessionHandle();

            try
            {
                var pendingRequest = await _pendingRepository.GetByIdAsync(pendingId, session);
                if (pendingRequest == null) throw new KeyNotFoundException($"Requisição pendente com ID {pendingId} não encontrada.");

                var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId, session);

                if (!CanModifyPendingStatus(staff))
                {
                    throw new UnauthorizedAccessException("Usuário não tem permissão para resolver requisições pendentes.");
                }

                pendingRequest.IdAprovador = currentUsuarioId;
                pendingRequest.DataAprovacao = DateTime.UtcNow;

                if (!isApproved)
                {
                    pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Rejeitado;
                    await _pendingRepository.UpdateAsync(pendingRequest, session);
                    await _uow.CommitTransactionAsync();
                    return true;
                }

                bool executionSuccess = false;
                switch (pendingRequest.CommandType)
                {
                    case "ChangeArtigoStatus":
                        var statusParams = JsonSerializer.Deserialize<Dictionary<string, string>>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (statusParams != null && statusParams.TryGetValue("NewStatus", out var statusString) && Enum.TryParse<StatusArtigo>(statusString, true, out var newStatus))
                        {
                            executionSuccess = await ExecuteAlterarStatusArtigoAsync(pendingRequest.TargetEntityId, newStatus, session);
                        }
                        break;

                    case "UpdateArtigoMetadata":
                        var metaParams = JsonSerializer.Deserialize<UpdateArtigoMetadataInput>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (metaParams != null)
                        {
                            var artigo = await _artigoRepository.GetByIdAsync(pendingRequest.TargetEntityId, session);
                            if (artigo != null)
                            {
                                executionSuccess = await ExecuteAtualizarMetadadosArtigoAsync(artigo, metaParams, staff, currentUsuarioId, session);
                            }
                        }
                        break;

                    case "UpdateArtigoContent":
                        var contentParams = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (contentParams != null &&
                            contentParams.TryGetValue("Content", out var contentElement) &&
                            contentParams.TryGetValue("Midias", out var midiasElement))
                        {
                            string newContent = contentElement.GetString() ?? string.Empty;
                            var midias = midiasElement.Deserialize<List<MidiaEntry>>(_jsonSerializerOptions) ?? new List<MidiaEntry>();
                            executionSuccess = await ExecuteAtualizarConteudoArtigoAsync(pendingRequest.TargetEntityId, newContent, midias, currentUsuarioId, session);
                        }
                        break;

                    case "CreateStaff":
                        var createStaffParams = JsonSerializer.Deserialize<Staff>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (createStaffParams != null)
                        {
                            var newStaff = await ExecuteCriarNovoStaffAsync(createStaffParams, currentUsuarioId, session);
                            executionSuccess = (newStaff != null);
                        }
                        break;

                    case "UpdateStaff":
                        var updateStaffParams = JsonSerializer.Deserialize<UpdateStaffInput>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (updateStaffParams != null)
                        {
                            var updatedStaff = await ExecuteAtualizarStaffAsync(updateStaffParams, session);
                            executionSuccess = (updatedStaff != null);
                        }
                        break;

                    case "DeleteInteracao":
                        var delParams = JsonSerializer.Deserialize<Dictionary<string, string>>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (delParams != null && delParams.TryGetValue("InteracaoId", out var interacaoId))
                        {
                            executionSuccess = await _interactionRepository.DeleteAsync(interacaoId, session);
                        }
                        break;

                    case "CreateVolume":
                        var createVolumeParams = JsonSerializer.Deserialize<Volume>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        if (createVolumeParams != null)
                        {
                            createVolumeParams.Status = StatusVolume.EmRevisao;
                            await _volumeRepository.AddAsync(createVolumeParams, session);
                            executionSuccess = true;
                        }
                        break;

                    case "UpdateVolume":
                        var updateVolumeParams = JsonSerializer.Deserialize<UpdateVolumeMetadataInput>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        var existingVolume = await _volumeRepository.GetByIdAsync(pendingRequest.TargetEntityId, session);
                        if (updateVolumeParams != null && existingVolume != null)
                        {
                            ApplyVolumeMetadataUpdates(existingVolume, updateVolumeParams);
                            executionSuccess = await _volumeRepository.UpdateAsync(existingVolume, session);
                        }
                        break;

                    case "UpdateEditorialTeam":
                        var teamParams = JsonSerializer.Deserialize<EditorialTeam>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                        var editorial = await _editorialRepository.GetByIdAsync(pendingRequest.TargetEntityId, session);
                        if (teamParams != null && editorial != null)
                        {
                            executionSuccess = await _editorialRepository.UpdateTeamAsync(editorial.Id, teamParams, session);
                        }
                        break;

                    default:
                        if (pendingRequest.CommandType == "UpdateStaffJob")
                        {
                            var staffParams = JsonSerializer.Deserialize<Dictionary<string, string>>(pendingRequest.CommandParametersJson, _jsonSerializerOptions);
                            var staffToUpdate = await _staffRepository.GetByUsuarioIdAsync(pendingRequest.TargetEntityId, session);
                            if (staffToUpdate != null && staffParams != null && staffParams.TryGetValue("NewJob", out var newJobString) && Enum.TryParse<FuncaoTrabalho>(newJobString, true, out var newJob))
                            {
                                staffToUpdate.Job = newJob;
                                executionSuccess = await _staffRepository.UpdateAsync(staffToUpdate, session);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Comando '{pendingRequest.CommandType}' não reconhecido para execução.");
                        }
                        break;
                }

                if (executionSuccess)
                {
                    pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Aprovado;
                }
                else
                {
                    pendingRequest.Status = Artigo.Intf.Enums.StatusPendente.Rejeitado;
                    throw new InvalidOperationException($"Falha na execução do comando '{pendingRequest.CommandType}' no item alvo ID {pendingRequest.TargetEntityId}.");
                }

                await _pendingRepository.UpdateAsync(pendingRequest, session);
                await _uow.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _uow.AbortTransactionAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<Pending>> ObterPendentesAsync(int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar a fila de requisições pendentes.");
            }
            return await _pendingRepository.GetAllAsync(pagina, tamanho);
        }

        public async Task<IReadOnlyList<Pending>> ObterPendentesPorStatusAsync(StatusPendente status, int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar a fila de requisições pendentes.");
            }
            return await _pendingRepository.BuscarPendenciasPorStatus(status, pagina, tamanho);
        }

        public async Task<IReadOnlyList<Pending>> ObterPendenciasPorEntidadeIdAsync(string targetEntityId, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar a fila de requisições pendentes.");
            }
            return await _pendingRepository.BuscarPendenciaPorEntidadeId(targetEntityId);
        }

        public async Task<IReadOnlyList<Pending>> ObterPendenciasPorTipoDeEntidadeAsync(TipoEntidadeAlvo targetType, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar a fila de requisições pendentes.");
            }
            return await _pendingRepository.BuscarPendenciaPorTipoDeEntidade(targetType);
        }

        public async Task<IReadOnlyList<Pending>> ObterPendenciasPorRequisitanteIdAsync(string requesterUsuarioId, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar a fila de requisições pendentes.");
            }
            return await _pendingRepository.BuscarPendenciaPorRequisitanteId(requesterUsuarioId);
        }


        // -------------------------------------------------------------------------
        // VII. STAFF & AUTOR MANAGEMENT
        // Acesso: Staff (Busca) ou Dono (Busca de próprio Autor/Staff)
        // -------------------------------------------------------------------------

        public async Task<Staff> CriarNovoStaffAsync(Staff novoStaff, string currentUsuarioId, string commentary)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (requestingStaff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(novoStaff, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = novoStaff.UsuarioId,
                    TargetType = TipoEntidadeAlvo.Staff,
                    CommandType = "CreateStaff",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);
                return novoStaff;
            }

            await _uow.StartTransactionAsync();
            var session = _uow.GetSessionHandle();
            try
            {
                var createdStaff = await ExecuteCriarNovoStaffAsync(novoStaff, currentUsuarioId, session);
                await _uow.CommitTransactionAsync();
                return createdStaff;
            }
            catch (Exception)
            {
                await _uow.AbortTransactionAsync();
                throw;
            }
        }

        private async Task<Staff> ExecuteCriarNovoStaffAsync(Staff novoStaff, string currentUsuarioId, object? sessionHandle = null)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId, sessionHandle);

            if (!CanCreateStaff(requestingStaff))
            {
                throw new UnauthorizedAccessException("Apenas Administradores ou Editores Chefes podem adicionar novos membros Staff.");
            }

            var existingStaff = await _staffRepository.GetByUsuarioIdAsync(novoStaff.UsuarioId, sessionHandle);
            if (existingStaff != null)
            {
                throw new InvalidOperationException($"O usuário com ID '{novoStaff.UsuarioId}' já é um membro Staff (Função: {existingStaff.Job}).");
            }

            novoStaff.IsActive = true;

            await _staffRepository.AddAsync(novoStaff, sessionHandle);
            return novoStaff;
        }

        public async Task<Staff> AtualizarStaffAsync(UpdateStaffInput input, string currentUsuarioId, string commentary)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (requestingStaff?.Job == FuncaoTrabalho.EditorBolsista)
            {
                var parameters = JsonSerializer.Serialize(input, _jsonSerializerOptions);
                var pending = new Pending
                {
                    TargetEntityId = input.UsuarioId,
                    TargetType = TipoEntidadeAlvo.Staff,
                    CommandType = "UpdateStaff",
                    CommandParametersJson = parameters,
                    Commentary = commentary,
                    RequesterUsuarioId = currentUsuarioId
                };
                await _pendingRepository.AddAsync(pending);

                var originalStaff = await _staffRepository.GetByUsuarioIdAsync(input.UsuarioId);
                if (originalStaff == null) throw new KeyNotFoundException("Staff alvo não encontrado.");
                return originalStaff;
            }

            if (requestingStaff?.Job == FuncaoTrabalho.EditorChefe || requestingStaff?.Job == FuncaoTrabalho.Administrador)
            {
                return await ExecuteAtualizarStaffAsync(input);
            }

            throw new UnauthorizedAccessException("Usuário não tem permissão para atualizar registros de Staff.");
        }

        private async Task<Staff> ExecuteAtualizarStaffAsync(UpdateStaffInput input, object? sessionHandle = null)
        {
            var staffToUpdate = await _staffRepository.GetByUsuarioIdAsync(input.UsuarioId, sessionHandle);
            if (staffToUpdate == null)
            {
                throw new KeyNotFoundException($"Staff com UsuarioId {input.UsuarioId} não encontrado.");
            }

            if (input.Job.HasValue)
            {
                staffToUpdate.Job = input.Job.Value;
            }
            if (input.IsActive.HasValue)
            {
                staffToUpdate.IsActive = input.IsActive.Value;
            }

            await _staffRepository.UpdateAsync(staffToUpdate, sessionHandle);
            return staffToUpdate;
        }

        public async Task<IReadOnlyList<Autor>> ObterAutoresAsync(int pagina, int tamanho, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (staff == null)
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para listar todos os autores.");
            }
            return await _autorRepository.GetAllAsync(pagina, tamanho);
        }

        public async Task<Autor?> ObterAutorPorIdAsync(string idAutor, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (IsStaff(staff))
            {
                return await _autorRepository.GetByIdAsync(idAutor);
            }

            var autor = await _autorRepository.GetByIdAsync(idAutor);
            if (autor == null)
            {
                return null;
            }

            if (autor.UsuarioId == currentUsuarioId)
            {
                return autor;
            }

            throw new UnauthorizedAccessException("Usuário deve ser Staff ou o próprio autor para buscar este registro.");
        }

        public async Task<Staff?> ObterStaffPorIdAsync(string staffId, string currentUsuarioId)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(requestingStaff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar dados de Staff.");
            }
            return await _staffRepository.GetByIdAsync(staffId);
        }

        public async Task<IReadOnlyList<Staff>> ObterStaffListAsync(int pagina, int tamanho, string currentUsuarioId)
        {
            var requestingStaff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            if (!IsStaff(requestingStaff))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar dados de Staff.");
            }
            return await _staffRepository.GetAllAsync(pagina, tamanho);
        }

        public async Task<bool> VerificarStaffAsync(string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);
            return IsStaff(staff);
        }
    }
}