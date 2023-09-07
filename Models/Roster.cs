using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace chat_server.Models
{
    [BsonIgnoreExtraElements]
    public class Roster
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = String.Empty;

        public string UserId { get; set; } = String.Empty;

        [BsonElement("active")]
        public bool IsActive { get; set; }

        [BsonElement("follower")]
        public string[]? Follower { get; set; }
    }
}
