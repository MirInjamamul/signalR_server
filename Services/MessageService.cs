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
            var filter = Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId);

            _offlineMessage.DeleteMany(filter);
        }

        public List<OfflineMessageModel> GetOfflineMessageByUserAsync(string userId)
        {
            var filter = Builders<OfflineMessageModel>.Filter.Eq(x => x.Message.To, userId);
            var offlineMessages = _offlineMessage.Find(filter).ToList();

            return offlineMessages;
        }

        public void InsertOne(OfflineMessageModel offlineMessage)
        {
            _offlineMessage.InsertOne(offlineMessage);
        }
    }
}
