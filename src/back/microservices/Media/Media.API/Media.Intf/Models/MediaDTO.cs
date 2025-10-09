using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Media.Intf.Models
{
    public class MidiaDTO
    {
        [BsonElement("String")]
        public required string Id { get; set; }

        [BsonElement("String")]
        public required string Origem { get; set; }

        [BsonElement("Type")]
        public required string Tipo { get; set; }

        [BsonElement("UrlValue")]
        public required string Url { get; set; }
    }
       
}