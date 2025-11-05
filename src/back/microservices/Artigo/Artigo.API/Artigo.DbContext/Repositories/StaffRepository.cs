using Artigo.DbContext.Data;
using Artigo.DbContext.Interfaces;
using Artigo.DbContext.PersistenceModels;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artigo.DbContext.Repositories
{
    /// <sumario>
    /// Implementação do contrato de persistência IStaffRepository.
    /// Gerencia a lista de membros da equipe editorial e suas funções (FuncaoTrabalho).
    /// Essencial para a camada de Autorizacao.
    /// </sumario>
    public class StaffRepository : IStaffRepository
    {
        private readonly IMongoCollection<StaffModel> _staff;
        private readonly IMapper _mapper;

        public StaffRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _staff = dbContext.Staffs;
            _mapper = mapper;
        }

        /// <sumario>
        /// (NOVO) Converte o 'sessionHandle' genérico em uma sessão do MongoDB.
        /// </sumario>
        private IClientSessionHandle? GetSession(object? sessionHandle)
        {
            return (IClientSessionHandle?)sessionHandle;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Staff?> GetByIdAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            var session = GetSession(sessionHandle);

            // *** CORREÇÃO: Adicionada verificação de sessão nula ***
            var find = (session != null)
                ? _staff.Find(session, s => s.Id == objectId.ToString())
                : _staff.Find(s => s.Id == objectId.ToString());

            var model = await find.FirstOrDefaultAsync();

            return _mapper.Map<Staff>(model);
        }

        public async Task<Staff?> GetByUsuarioIdAsync(string usuarioId, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            // *** CORREÇÃO: Adicionada verificação de sessão nula ***
            var find = (session != null)
                ? _staff.Find(session, s => s.UsuarioId == usuarioId)
                : _staff.Find(s => s.UsuarioId == usuarioId); // Esta é a linha 59

            var model = await find.FirstOrDefaultAsync();

            return _mapper.Map<Staff>(model);
        }

        public async Task<IReadOnlyList<Staff>> GetByRoleAsync(FuncaoTrabalho role, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);

            // *** CORREÇÃO: Adicionada verificação de sessão nula ***
            var find = (session != null)
                ? _staff.Find(session, s => s.Job == role)
                : _staff.Find(s => s.Job == role);

            var models = await find
                .SortByDescending(s => s.Id)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Staff>>(models);
        }

        /// <sumario>
        /// *** NOVO MÉTODO IMPLEMENTADO ***
        /// Retorna todos os registros de Staff, com suporte a paginação.
        /// </sumario>
        public async Task<IReadOnlyList<Staff>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null)
        {
            int skip = pagina * tamanho;
            var session = GetSession(sessionHandle);

            var find = (session != null)
                ? _staff.Find(session, _ => true)
                : _staff.Find(_ => true);

            var models = await find
                .SortByDescending(s => s.Id) // Ordena pelo ObjectId (data de criação)
                .Skip(skip)
                .Limit(tamanho)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Staff>>(models);
        }

        public async Task AddAsync(Staff staff, object? sessionHandle = null)
        {
            var session = GetSession(sessionHandle);
            var model = _mapper.Map<StaffModel>(staff);

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }

            if (session != null)
                await _staff.InsertOneAsync(session, model);
            else
                await _staff.InsertOneAsync(model);

            _mapper.Map(model, staff);
        }

        public async Task<bool> UpdateAsync(Staff staffMember, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(staffMember.Id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var model = _mapper.Map<StaffModel>(staffMember);

            var result = (session != null)
                ? await _staff.ReplaceOneAsync(session, s => s.Id == objectId.ToString(), model)
                : await _staff.ReplaceOneAsync(s => s.Id == objectId.ToString(), model);

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id, object? sessionHandle = null)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;
            var session = GetSession(sessionHandle);

            var result = (session != null)
                ? await _staff.DeleteOneAsync(session, s => s.Id == objectId.ToString())
                : await _staff.DeleteOneAsync(s => s.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}