namespace chat_server.Models
{
    public class RequestModel
    {

    }

    public class FollowerModel
    {
        public string FollowerId { get; set; } = string.Empty;
    }

    public class NickModel
    {
        public string NickName { get; set; } = string.Empty;
    }

    public class PhotoModel
    {
        public string Photo { get; set; } = string.Empty;
    }
}
