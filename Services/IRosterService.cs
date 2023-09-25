using chat_server.Models;

namespace chat_server.Services
{
    public interface IRosterService
    {
        List<Roster> Get();
        Roster Get(string userId);
        List<Roster> GetOnlineRoster(string[] followerId);
        List<Roster> GetSuggestionRoster(string[] followerId);
        List<Roster> GetLastOnlineRoster(string[] userId);
        Roster Create(Roster roster);
        void Update(string id, Roster roster);
        void UpdateFollower(string userId, Roster roster);
        void Remove(string id);
    }
}
