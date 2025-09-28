using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Media.Intf.Models
{
    internal class Midia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId Origem { get; set; }

        [BsonElement("String")]
        public required string Tipo { get; set; }
        [JsonIgnore]

        [BsonElement("String")]
        public required string Url { get; set; }
    }
}
