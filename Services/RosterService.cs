using chat_server.Models;
using MongoDB.Driver;

namespace chat_server.Services
{
    public class RosterService : IRosterService
    {
        private readonly IMongoCollection<Roster> _rosters;
        public RosterService(IRosterStoreDatabaseSettings settings, IMongoClient mongoClient) 
        { 
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _rosters = database.GetCollection<Roster>(settings.RosterCollectionName);
        }
        public Roster Create(Roster roster)
        {
            _rosters.InsertOne(roster);
            return roster;
        }

        public List<Roster> Get()
        {
            return _rosters.Find(roster => true).ToList();
        }

        public Roster Get(string id)
        {
            return _rosters.Find(roster => roster.UserId == id).FirstOrDefault();
        }

        public List<Roster> GetOnlineRoster(List<Follower> followers)
        {
            var _onlineRoster = _rosters.Find(roster => followers.Any(follower => follower.UserId == roster.UserId) && roster.IsActive).ToList();

            // Return first 12 or fewer if not avaialble

            return _onlineRoster.Take(12).ToList();
        }

        public List<Roster> GetSuggestionRoster(List<Follower> followers)
        {
            var _onlineRoster = _rosters.Find(roster => followers.Any(follower => follower.UserId == roster.UserId)).ToList();

            Random random = new Random();

            return _onlineRoster.OrderBy(x => random.Next()).Take(10).ToList();
        }

        public List<Roster> GetLastOnlineRoster(string[] userId)
        {
            var _lastOnlineRoster = _rosters.Find(roster => userId.Contains(roster.UserId)).ToList();

            return _lastOnlineRoster;
        }

        public void Remove(string id)
        {
            _rosters.DeleteOne(roster => roster.UserId == id);
        }

        public void Update(string id, Roster roster)
        {
            _rosters.ReplaceOne(roster => roster.UserId == id, roster);
        }

        public void UpdateFollower(string userId, Roster roster)
        {
            _rosters.ReplaceOne(roster => roster.UserId == userId, roster);
        }
    }
}
