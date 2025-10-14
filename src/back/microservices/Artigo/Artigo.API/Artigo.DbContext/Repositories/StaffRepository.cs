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
    /// Implementacao do contrato de persistencia IStaffRepository.
    /// Gerencia a lista de membros da equipe editorial e suas funcoes (JobRole).
    /// Essencial para a camada de Autorizacao.
    /// </sumario>
    public class StaffRepository : IStaffRepository
    {
        private readonly IMongoCollection<StaffModel> _staff;
        private readonly IMapper _mapper;

        public StaffRepository(Artigo.DbContext.Interfaces.IMongoDbContext dbContext, IMapper mapper)
        {
            _staff = dbContext.Staffs; // Corrigido para Staffs, conforme IMongoDbContext
            _mapper = mapper;
        }

        // --- Implementação dos Métodos da Interface ---

        public async Task<Staff?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var model = await _staff
                .Find(s => s.Id == objectId.ToString())
                .FirstOrDefaultAsync();

            return _mapper.Map<Staff>(model);
        }

        public async Task<Staff?> GetByUsuarioIdAsync(string usuarioId)
        {
            // Busca o membro da equipe usando o ID de usuário externo
            var model = await _staff
                .Find(s => s.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            return _mapper.Map<Staff>(model);
        }

        // Implementa o contrato IStaffRepository (novo método)
        public async Task<IReadOnlyList<Staff>> GetByRoleAsync(JobRole role)
        {
            var models = await _staff
                .Find(s => s.Job == role)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<Staff>>(models);
        }

        public async Task AddAsync(Staff staff)
        {
            var model = _mapper.Map<StaffModel>(staff);

            // Garante que a ID seja gerada se for nova
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = ObjectId.GenerateNewId().ToString();
            }

            await _staff.InsertOneAsync(model);

            // Atualiza a entidade de domínio com a ID final
            _mapper.Map(model, staff);
        }

        // Implementa o contrato IStaffRepository (UpdateAsync)
        public async Task<bool> UpdateAsync(Staff staffMember)
        {
            if (!ObjectId.TryParse(staffMember.Id, out var objectId)) return false;

            var model = _mapper.Map<StaffModel>(staffMember);

            // Usa ReplaceOneAsync para atualizar todo o documento (incluindo JobRole)
            var result = await _staff.ReplaceOneAsync(
                s => s.Id == objectId.ToString(),
                model
            );

            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return false;

            var result = await _staff.DeleteOneAsync(s => s.Id == objectId.ToString());

            return result.IsAcknowledged && result.DeletedCount == 1;
        }
    }
}