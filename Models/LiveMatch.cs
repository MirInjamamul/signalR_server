namespace chat_server.Models
{
    public class LiveMatch
    {
        public string SenderId { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string RoomId { get; set; }
        public string Message { get; set; }
        public bool IsSearchPool { get; set; } = false;
    }
}
