using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Media.Intf.Models
{
    public class Midia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } = ObjectId.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId Origem { get; set; }

        [BsonElement("Type")]
        public required string Tipo { get; set; }

        [BsonElement("UrlValue")]
        public required string Url { get; set; }
    }

    public static class MidiaMappers
    {
        public static MidiaDTO ToDto(this Midia dbModel)
        {
            if (dbModel == null) return null;

            return new MidiaDTO
            {

                Id = dbModel.Id.ToString(),
                Origem = dbModel.Origem.ToString(),
                Tipo = dbModel.Tipo,
                Url = dbModel.Url,
            };
        }
        public static Midia ToDbModel(this MidiaDTO dto)
        {
            return new Midia
            {
                Id = string.IsNullOrEmpty(dto.Id) ? ObjectId.Empty : ObjectId.Parse(dto.Id),
                Origem = string.IsNullOrEmpty(dto.Origem) ? ObjectId.Empty : ObjectId.Parse(dto.Origem),
                Tipo = dto.Tipo,
                Url = dto.Url
            };
        }
    }
}