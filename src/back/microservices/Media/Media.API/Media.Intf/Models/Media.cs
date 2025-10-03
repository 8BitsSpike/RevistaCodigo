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
}