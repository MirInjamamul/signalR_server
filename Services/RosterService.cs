﻿using chat_server.Models;
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

        public void Remove(string id)
        {
            _rosters.DeleteOne(roster => roster.Id == id);
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
