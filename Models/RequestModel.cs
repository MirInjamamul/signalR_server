namespace chat_server.Models
{
    public class RequestModel
    {

    }

    public class FollowerModel
    {
        public string FollowerId { get; set; } = string.Empty;
        public bool FollowBack { get; set; } = false;
    }

    public class NickModel
    {
        public string NickName { get; set; } = string.Empty;
    }

    public class PhotoModel
    {
        public string Photo { get; set; } = string.Empty;
    }

    public class PresenceModel
    {
        public bool Presence { get; set; } = true;
    }

    public class UserIdModel 
    { 
        public string[] UserId { get; set; }
    }

    public class BlockIdModel 
    {
        public string UserId { get; set; } = string.Empty;
        public string BlockId { get; set; } = string.Empty;
    }

    public class BlockedResponseModel
    {
        public bool IsBlocked { get; set; }
    }
}
