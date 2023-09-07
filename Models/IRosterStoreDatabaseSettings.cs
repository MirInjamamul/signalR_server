namespace chat_server.Models
{
    public interface IRosterStoreDatabaseSettings
    {
        string RosterCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }

    }
}
