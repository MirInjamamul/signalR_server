namespace chat_server.Models
{
    public class Follower
    {
        public string UserId { get; set; } = String.Empty;
        public bool IsFriend { get; set; } = false;
    }
}
