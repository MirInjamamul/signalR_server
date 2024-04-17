using chat_server.Models;
using MongoDB.Driver;

namespace chat_server.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<OfflineMessageModel> _offlineMessage;

        public MessageService(IMessageStoreDatabaseSettings settings, IMongoClient mongoClient)
        { 
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _offlineMessage = database.GetCollection<OfflineMessageModel>(settings.OfflineMessageCollectionName);
        }

        public void deleteMessage(string userId)
        {
            var filter = Builders<OfflineMessageModel>.Filter.And(
                Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId),
                Builders<OfflineMessageModel>.Filter.Eq(x => x.IsOfflineMessage, true)
            );

            var update = Builders<OfflineMessageModel>.Update.Set(x => x.IsOfflineMessage, false);

            _offlineMessage.UpdateMany(filter, update);
        }

        public List<OfflineMessageModel> GetOfflineMessageByUserAsync(string userId)
        {
            var filter = Builders<OfflineMessageModel>.Filter.And(
                Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId),
                Builders<OfflineMessageModel>.Filter.Eq(x => x.IsOfflineMessage, true)
                ); 
           // Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId);
            var offlineMessages = _offlineMessage.Find(filter).ToList();

            return offlineMessages;
        }

        public List<OfflineMessageModel> GetBackupMessageByUser(string userId)
        {
            var filter = Builders<OfflineMessageModel>.Filter.And(
                Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId),
                Builders<OfflineMessageModel>.Filter.Eq(x => x.IsOfflineMessage, false)
            ) | Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.SenderId, userId);
            var offlineMessages = _offlineMessage.Find(filter).ToList();

            return offlineMessages;
        }

        public void InsertOne(OfflineMessageModel offlineMessage)
        {
            _offlineMessage.InsertOne(offlineMessage);
        }
    }
}
