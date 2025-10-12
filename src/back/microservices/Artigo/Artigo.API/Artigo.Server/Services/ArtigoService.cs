using Artigo.DbContext.Repositories;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public ArtigoService(
            IArtigoRepository artigoRepository,
            IAutorRepository autorRepository,
            IStaffRepository staffRepository,
            IEditorialRepository editorialRepository,
            IArtigoHistoryRepository historyRepository,
            IPendingRepository pendingRepository,
            IInteractionRepository interactionRepository,
            IMapper mapper)
        {
            _artigoRepository = artigoRepository;
            _autorRepository = autorRepository;
            _staffRepository = staffRepository;
            _editorialRepository = editorialRepository;
            _historyRepository = historyRepository;
            _pendingRepository = pendingRepository;
            _interactionRepository = interactionRepository;
            _mapper = mapper;
        }

        // ----------------------------------------------------
        // I. Métodos de Autorização (Regras de Negócio)
        // ----------------------------------------------------

        /// <sumario>
        /// Verifica se um usuario pode ler um artigo com base no seu status e permissao.
        /// Regra: Publicado (qualquer um) OU Membro da Equipe Editorial (nao publicado).
        /// </sumario>
        private bool CanReadArtigo(Artigo artigo, Staff staff, string currentUsuarioId)
        {
            // 1. Qualquer um pode ler se estiver publicado
            if (artigo.Status == ArtigoStatus.Published)
            {
                return true;
            }

            // Se nao estiver publicado, deve ser membro da equipe editorial
            if (staff == null)
            {
                return false; // Usuario sem registro de Staff nao le rascunhos
            }

            // 2. Verifica se o usuario faz parte da equipe editorial do artigo
            var editorial = _editorialRepository.GetByIdAsync(artigo.EditorialId).Result; // Uso sincrono para simplificar a demo, mas idealmente seria async

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

        /// <sumario>
        /// Verifica se um usuario pode editar um artigo (nao publicado).
        /// Regra: Autores, Co-autores, ou membros da Equipe Editorial designados.
        /// </sumario>
        private bool CanEditArtigo(Artigo artigo, Staff staff, string currentUsuarioId)
        {
            // Se estiver publicado, nao pode ser editado via esta mutacao
            if (artigo.Status == ArtigoStatus.Published)
            {
                return false;
            }

            // Reutiliza a lógica de leitura para a maioria dos casos
            if (CanReadArtigo(artigo, staff, currentUsuarioId))
            {
                // Nota: Esta regra assume que qualquer um que pode ler o rascunho pode edita-lo. 
                // Se a regra fosse mais restrita (apenas AutorPrincipal), a logica seria mais complexa.
                return true;
            }
            return false;
        }

        /// <sumario>
        /// Verifica se um usuario pode mudar o ArtigoStatus.
        /// Regra: Apenas EditorBolsista e EditorChefe (qualquer artigo).
        /// </sumario>
        private bool CanModifyStatus(Staff staff)
        {
            if (staff == null) return false;

            return staff.Job == JobRole.EditorBolsista || staff.Job == JobRole.EditorChefe || staff.Job == JobRole.Administrador;
        }

        /// <sumario>
        /// Verifica se um usuario pode criar um ComentarioEditorial.
        /// Regra: Apenas usuarios listados na EditorialTeam daquele artigo.
        /// </sumario>
        private bool CanCreateEditorialComment(Editorial editorial, string currentUsuarioId)
        {
            if (editorial == null) return false;
            var team = editorial.Team;

            // Todos os membros da equipe editorial
            var allowedUserIds = team.InitialAuthorId
                .Concat(team.ReviewerIds)
                .Concat(team.CorrectorIds)
                .Concat(new[] { team.EditorId })
                .ToList();

            return allowedUserIds.Contains(currentUsuarioId);
        }

        /// <sumario>
        /// Verifica se um usuario pode editar a colecao Volume.
        /// Regra: Apenas membros da Staff.
        /// </sumario>
        private bool CanEditVolume(Staff staff)
        {
            return staff != null;
        }

        /// <sumario>
        /// Verifica se um usuario pode criar um Pending (requisitador).
        /// Regra: Apenas EditorBolsista.
        /// </sumario>
        private bool CanCreatePending(Staff staff)
        {
            if (staff == null) return false;
            return staff.Job == JobRole.EditorBolsista || staff.Job == JobRole.Administrador;
        }

        /// <sumario>
        /// Verifica se um usuario pode modificar o PendingStatus.
        /// Regra: Apenas EditorChefes e Administradores.
        /// </sumario>
        private bool CanModifyPendingStatus(Staff staff)
        {
            if (staff == null) return false;
            return staff.Job == JobRole.EditorChefe || staff.Job == JobRole.Administrador;
        }

        // ----------------------------------------------------
        // II. Metodos de Leitura (Queries)
        // ----------------------------------------------------

        public async Task<ArtigoDTO?> GetByIdAsync(string artigoId, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            if (!CanReadArtigo(artigo, staff, currentUsuarioId))
            {
                // Nao autorizado a ler este artigo
                return null;
            }

            return _mapper.Map<ArtigoDTO>(artigo);
        }

        public async Task<List<ArtigoDTO>> GetByStatusAsync(ArtigoStatus status, string currentUsuarioId)
        {
            // Nota: Esta query é mais complexa, pois precisa filtrar por autorizacao apos o filtro de status.
            // Para simplificar, assumimos que apenas 'Published' é visível para todos os usuarios.

            var artigos = await _artigoRepository.GetByStatusAsync(status);
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            var authorizedArtigos = artigos
                .Where(a => CanReadArtigo(a, staff, currentUsuarioId))
                .ToList();

            return _mapper.Map<List<ArtigoDTO>>(authorizedArtigos);
        }

        // ----------------------------------------------------
        // III. Metodos de Escrita (Mutations)
        // ----------------------------------------------------

        public async Task<ArtigoDTO> CreateArtigoAsync(CreateArtigoRequest request, string currentUsuarioId)
        {
            // 1. Mapear DTO de entrada para entidade de domínio
            var artigo = _mapper.Map<Artigo>(request);

            // 2. Lógica de Negócio / Inicialização
            artigo.Id = Guid.NewGuid().ToString(); // Gerar um ID temporário (será ObjectId no repositório)
            artigo.Status = ArtigoStatus.Draft;
            artigo.AutorIds.Add(currentUsuarioId); // Adiciona o criador como Autor Principal

            // 3. Orquestração e Persistência
            // Transacao Lógica (Deve ser atomica se possivel, mas aqui faremos sequencialmente)

            // 3.1. Criar ArtigoHistory inicial
            var initialHistory = new ArtigoHistory
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigo.Id,
                Version = ArtigoVersion.Original,
                Content = request.Content // Conteudo principal vem do Request
            };
            await _historyRepository.AddAsync(initialHistory);

            // 3.2. Criar Editorial (e equipe)
            var editorial = new Editorial
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigo.Id,
                Position = EditorialPosition.Submitted,
                CurrentHistoryId = initialHistory.Id,
                HistoryIds = new List<string> { initialHistory.Id },
                Team = new EditorialTeam { InitialAuthorId = artigo.AutorIds } // Autores sao a equipe inicial
            };
            await _editorialRepository.AddAsync(editorial);

            // 3.3. Ligar Editorial ao Artigo
            artigo.EditorialId = editorial.Id;

            // 3.4. Atualizar registro do Autor (se nao existir, cria-o no repositório)
            var autor = await _autorRepository.GetByUsuarioIdAsync(currentUsuarioId) ?? new Autor { Id = Guid.NewGuid().ToString(), UsarioId = currentUsuarioId };
            autor.ArtigoWorkIds.Add(artigo.Id);
            autor.Contribuicoes.Add(new ContribuicaoEditorial { ArtigoId = artigo.Id, Role = ContribuicaoRole.AutorPrincipal });
            await _autorRepository.UpsertAsync(autor); // Upsert garante criacao ou atualizacao

            // 3.5. Persistir Artigo final
            var novoArtigo = await _artigoRepository.AddAsync(artigo);

            return _mapper.Map<ArtigoDTO>(novoArtigo);
        }

        public async Task<ArtigoDTO?> UpdateArtigoAsync(string artigoId, ArtigoDTO updateDto, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) return null;

            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao de Edicao
            if (!CanEditArtigo(artigo, staff, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este artigo.");
            }

            // 2. Mapear e atualizar campos (apenas Titulo, Resumo, Tipo, etc.)
            _mapper.Map(updateDto, artigo);

            // 3. Persistir no Repositorio
            await _artigoRepository.UpdateAsync(artigo);

            return _mapper.Map<ArtigoDTO>(artigo);
        }

        public async Task UpdateArtigoStatusAsync(string artigoId, ArtigoStatus newStatus, string currentUsuarioId)
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
                // Lógica real envolveria buscar um VolumeId ativo aqui
                // artigo.VolumeId = await GetActiveVolumeIdAsync(); 
            }

            // 4. Persistir
            await _artigoRepository.UpdateAsync(artigo);
        }

        // ----------------------------------------------------
        // IV. Metodos de Interacao e Workflow
        // ----------------------------------------------------

        public async Task CreatePublicCommentAsync(string artigoId, string content, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null || artigo.Status != ArtigoStatus.Published)
            {
                throw new InvalidOperationException("Comentários públicos só são permitidos em artigos publicados.");
            }

            // 1. Regra de Negócio: Nao ha regra de autorizacao para ComentarioPublico (qualquer um pode)

            // 2. Criar a Interacao
            var comment = new Interaction
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigoId,
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = InteractionType.ComentarioPublico,
                FamilyId = "0" // Comentario de nivel superior
            };

            // 3. Persistir e atualizar Artigo (denormalizacao)
            await _interactionRepository.AddAsync(comment);

            // Atualizar contadores no Artigo (Subset Pattern)
            artigo.TotalComentarios++;
            artigo.TotalInteracoes++;
            await _artigoRepository.UpdateAsync(artigo);
        }

        public async Task CreateEditorialCommentAsync(string artigoId, string content, string currentUsuarioId)
        {
            var artigo = await _artigoRepository.GetByIdAsync(artigoId);
            if (artigo == null) throw new KeyNotFoundException("Artigo não encontrado.");

            var editorial = await _editorialRepository.GetByIdAsync(artigo.EditorialId);
            if (editorial == null) throw new KeyNotFoundException("Registro editorial não encontrado.");

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreateEditorialComment(editorial, currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não faz parte da equipe editorial deste artigo.");
            }

            // 2. Criar a Interacao
            var comment = new Interaction
            {
                Id = Guid.NewGuid().ToString(),
                ArtigoId = artigoId,
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = InteractionType.ComentarioEditorial,
                FamilyId = "0" // Comentario de nivel superior (no ciclo editorial)
            };

            // 3. Persistir e ligar ao Editorial
            await _interactionRepository.AddAsync(comment);

            // Adicionar referencia ao Editorial (HistoryComments)
            editorial.CommentIds.Add(comment.Id);
            await _editorialRepository.UpdateAsync(editorial);
        }

        // --- Pending Methods (Queue Management) ---

        public async Task CreatePendingRequestAsync(string targetEntityId, TargetEntityType targetType, string cmd, string cmt, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanCreatePending(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editores Bolsistas e Administradores podem criar requisições pendentes.");
            }

            // 2. Criar Pending
            var pending = new Pending
            {
                Id = Guid.NewGuid().ToString(),
                AddressId = targetEntityId,
                TargetType = targetType,
                CMD = cmd,
                Cmt = cmt,
                UserId = currentUsuarioId,
                Date = DateTime.UtcNow,
                Status = PendingStatus.AwaitingReview
            };

            // 3. Persistir
            await _pendingRepository.AddAsync(pending);
        }

        public async Task UpdatePendingStatusAsync(string pendingId, PendingStatus newStatus, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanModifyPendingStatus(staff))
            {
                throw new UnauthorizedAccessException("Apenas Editor Chefe e Administradores podem aprovar ou rejeitar requisições.");
            }

            var pending = await _pendingRepository.GetByIdAsync(pendingId);
            if (pending == null) throw new KeyNotFoundException("Requisição pendente não encontrada.");

            // 2. Atualizar Status
            pending.Status = newStatus;

            // Nota: Lógica adicional para EXECUTAR o CMD se o status for Approved seria inserida aqui.

            // 3. Persistir
            await _pendingRepository.UpdateAsync(pending);
        }

        // --- Volume Methods ---

        public async Task CreateVolumeAsync(Volume volume, string currentUsuarioId)
        {
            var staff = await _staffRepository.GetByUsuarioIdAsync(currentUsuarioId);

            // 1. Aplicar Regra de Autorizacao
            if (!CanEditVolume(staff))
            {
                throw new UnauthorizedAccessException("Apenas membros da equipe podem criar um volume.");
            }

            volume.Id = Guid.NewGuid().ToString();
            await _volumeRepository.AddAsync(volume);
        }
    }
}
