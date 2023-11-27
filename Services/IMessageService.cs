using chat_server.Models;

namespace chat_server.Services
{
    public interface IMessageService
    {
        void InsertOne(OfflineMessageModel offlineMessage);

        List<OfflineMessageModel> GetOfflineMessageByUserAsync(string userId);

        void deleteMessage(string userId);
    }
}
