namespace chat_server.Models
{
    public class OnlineRoster
    {
        public string UserId { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
