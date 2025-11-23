using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Usuario.Intf.Models
{
    // Esta classe customiza a serialização do ObjectId do MongoDB,
    // garantindo que ele seja tratado como uma string hexadecimal no JSON.
    // O 'public' é essencial para que a anotação [JsonConverter] possa acessá-la.
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        // Método 'Read': Usado na entrada (JSON que o cliente envia).
        // Converte a string de volta para o tipo ObjectId, de forma segura.
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { } value)
            {
                throw new JsonException($"Esperado string para ObjectId, mas veio {reader.TokenType}");
            }

            if (ObjectId.TryParse(value, out var objectId))
            {
                return objectId;
            }

            throw new JsonException($"Valor '{value}' não é um ObjectId válido.");
        }

        // Método 'Write': Usado na saída (JSON que a API envia ao cliente).
        // Converte o ObjectId para uma string hexadecimal no JSON de resposta.
        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}