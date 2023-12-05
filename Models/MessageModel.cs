﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;
using static chat_server.Utils.Util;

namespace chat_server.Models
{
    public class MessageModel
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderUserName { get; set; } =string.Empty;
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

    }

    public class FileMessageModel
    {
        public MessageModel Message { get; set; } = new MessageModel();
        public string Url { get; set; } = string.Empty;

    }

    public class StoryReactionModel
    {
        public MessageModel Message { get; set; } = new MessageModel();
        public string StoryLink { get; set; } = string.Empty;

        public StoryType StoryType { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class OfflineMessageModel
    {
        [BsonIgnoreIfDefault]
        [BsonId(IdGenerator = typeof(ObjectIDGenerator))]
        public ObjectId _id {  get; set; }
        public MessageModel Message { get; set; } = new MessageModel();
        public DateTime TimeStamp { get; set; }
    }

}
