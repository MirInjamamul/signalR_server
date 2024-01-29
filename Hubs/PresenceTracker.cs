using chat_server.Models;

namespace chat_server.Hubs
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, int> onlineUsers = new Dictionary<string, int>();

        private static readonly Dictionary<string, string> connectionNickMap = new Dictionary<string, string>();

        static List<UserDetail> ConnectedUser = new List<UserDetail>();

        /*       public Task<bool> NickNameAvailable(string userId)
               {
                   bool available = false;

                   lock (onlineUsers)
                   {
                       if (!onlineUsers.ContainsKey(userId) && userId != "system")
                       {
                           available = true;
                       }
                   }

                   return Task.FromResult(available);
               }

        */

        public UserDetail Login(string connectionId, string userId)
        {
            if (ConnectedUser.Count(x => x.ConnectionId == connectionId) == 0)
            {
                ConnectedUser.Add(new UserDetail
                {
                    ConnectionId = connectionId,
                    UserId = userId
                });
            }

            UserDetail currentUser  = ConnectedUser.Where(u => u.ConnectionId == connectionId).FirstOrDefault();

            return currentUser;
        }

        public UserDetail Logout(string connectionId)
        {
            var item = ConnectedUser.FirstOrDefault(x => x.ConnectionId == connectionId);

            if (item != null)
            { 
                ConnectedUser.Remove(item);
            }

            return item;
        }
        
        public Task SetupNickConnection(string connectionId, string userId)
        {
            lock (connectionNickMap)
            {
                if (!connectionNickMap.ContainsKey(connectionId))
                {
                    connectionNickMap.Add(connectionId, userId);
                }
            }

            return Task.FromResult(0);
        }

        public Task<string> GetConnectionIdByUserId(string userId) 
        {
            lock(connectionNickMap)
            {
                foreach (var item in connectionNickMap)
                {
                    if(item.Value == userId)
                    {
                        return Task.FromResult(item.Key);
                    }
                }
            }


            return null;
            
        }

        public string GetUserId(string connectionId)
        { 
            string userId = ConnectedUser.Where(u => u.ConnectionId == connectionId).Select(u => u.UserId).FirstOrDefault();

            return userId;
        }

        public List<UserDetail> GetUserDetail(string userId)
        {
            List<UserDetail> toUser = ConnectedUser.Where(x => x.UserId == userId).ToList();

            Console.WriteLine($"Looking for  userid {userId}");

            foreach(var item in ConnectedUser) 
            { 
                Console.WriteLine($"Coonected user Connection -> {item.ConnectionId} :: userId {item.UserId}");
            }

            return toUser;
        }


        public Task<ConnectionOpenedResult> ConnectionOpened(string userId)
        {
            var joined = false;
            lock (onlineUsers) 
            { 
                if(onlineUsers.ContainsKey(userId))
                {
                    onlineUsers[userId] += 1;
                }
                else
                {
                    onlineUsers.Add(userId, 1);
                    joined = true;
                }
            }

            return Task.FromResult(new ConnectionOpenedResult { UserJoined = joined});
        }

        public Task<ConnectionClosedResult> ConnectionClosed(string connectionId)
        {
            var left = false;
            lock (onlineUsers)
            {
                if (onlineUsers.ContainsKey(connectionId))
                {
                    onlineUsers[connectionId] -= 1;
                    if (onlineUsers[connectionId] <= 0)
                    {
                        onlineUsers.Remove(connectionId);
                        left = true;
                    }
                }
            }

            lock (connectionNickMap)
            {
                if (connectionNickMap.ContainsKey(connectionId)) 
                { 
                    var nickItem = connectionNickMap[connectionId];
                    connectionNickMap.Remove(nickItem);
                    left = true;
                }
            }

            return Task.FromResult(new ConnectionClosedResult { UserLeft = left});
        }

        public Task<string[]> GetOnlineUsers() 
        { 
            lock(onlineUsers)
            {
                return Task.FromResult(onlineUsers.Keys.ToArray());
            }
        }


    }
}

public class ConnectionOpenedResult
{
    public bool UserJoined { get; set; }
}

public class ConnectionClosedResult
{
    public bool UserLeft { get; set; }
}
