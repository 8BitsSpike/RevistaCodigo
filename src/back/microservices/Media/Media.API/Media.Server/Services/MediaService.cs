using Media.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using Media.Intf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq; 

namespace Media.Server.Services
{
   

public class MediaService(MongoDbContext context)
    {
        private readonly MongoDbContext _context = context;

        public virtual async Task<List<MidiaDTO>> GetMediaAsync() =>
            (await _context.Midias.Find(_ => true).ToListAsync())
            .Select(m => m.ToDto()) 
            .ToList();
        public virtual async Task<MidiaDTO?> GetMediaAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return null;
            }
            var result = await _context.Midias.Find(x => x.Id == objectId).FirstOrDefaultAsync();
            return result.ToDto();
        }

        public virtual async Task<Midia> GetMediaAsync(ObjectId id) =>
            await _context.Midias.Find(x => x.Id == id).FirstOrDefaultAsync();


        public virtual async Task<Midia> CreateMediaAsync(Midia newMidia)
        {
            if (newMidia.Id == ObjectId.Empty)
            {
                newMidia.Id = ObjectId.GenerateNewId();
            }
            await _context.Midias.InsertOneAsync(newMidia);
            return newMidia;
        }

        public virtual async Task UpdateMediaAsync(string id, Midia updatedMidia)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                throw new ArgumentException("Id em formato inválido.", nameof(id));
            }

            updatedMidia.Id = objectId;

            await _context.Midias.ReplaceOneAsync(x => x.Id == objectId, updatedMidia);
        }

        public virtual async Task DeleteMediaAsync(string id)
        {

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return;
            }
            await _context.Midias.DeleteOneAsync(x => x.Id == objectId);
        }
    }
}