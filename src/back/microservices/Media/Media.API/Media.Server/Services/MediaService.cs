using Media.DbContext;
using MongoDB.Bson;
using MongoDB.Driver;
using Media.Intf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Media.Server.Services
{
    public class MediaService(MongoDbContext context)
    {
        private readonly MongoDbContext _context = context;

        public virtual async Task<List<Media>> GetMediaAsync() =>
            await _context.Medias.Find(_ => true).ToListAsync();

        public virtual async Task<Media> GetMediaAsync(int id) =>
            await _context.Medias.Find(x => x.Id == id).FirstOrDefaultAsync();

        public virtual async Task<Media> CreateMediaAsync(Media newMedia)
        {
            await _context.Medias.InsertOneAsync(newMedia);
            return newMedia;
        }

        public virtual async Task<bool> UpdateMediaAsync(int id, Url updatedUrl)
        {
            var filter = Builders<Media>.Filter.Eq(q => q.Id, id);
            var update = Builders<Media>.Update.Set(q => q.Url, updatedUrl);

            var result = await _context.Medias.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public virtual async Task DeleteMediaAsync(int id) =>
            await _context.Medias.DeleteOneAsync(x => x.Id == id);
    }
}
