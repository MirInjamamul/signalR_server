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

        [BsonElement("userid")]
        public string UserId { get; set; } = String.Empty;

        [BsonElement("nick")]
        public string NickName { get; set; } = String.Empty;

        [BsonElement("photo")]
        public string Photo { get; set; } = String.Empty;

        [BsonElement("active")]
        public bool IsActive { get; set; }
        [BsonElement("lastonline")]
        public DateTime LastOnline { get; set; }

        [BsonElement("follower")]
        public string[]? Follower { get; set; }

        [BsonElement("block")]
        public string[]? Blocked { get; set; }
    }
}
