namespace chat_server.Models
{
    public class RosterStoreDatabaseSettings : IRosterStoreDatabaseSettings
    {
        public string RosterCollectionName { get; set; } = String.Empty;
        public string ConnectionString { get ; set ; } = String.Empty;
        public string DatabaseName { get; set; } = String.Empty;
    }

    public class MessageDatabaseSettings : IMessageStoreDatabaseSettings
    {
        public string OfflineMessageCollectionName { get; set; } = String.Empty;
        public string ConnectionString { get; set; } = String.Empty;
        public string DatabaseName { get; set; } = String.Empty;
    }
}
