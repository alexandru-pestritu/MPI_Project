namespace DbProvider.Database;

public class DatabaseConnectionDetails : IDatabaseConnectionDetails
{
    public string DataSource { get; }
    public string DatabaseName { get; }
    public string Username { get; }
    public string Password { get; }
    
    public DatabaseConnectionDetails(string dataSource, string databaseName, string username, string password)
    {
        DataSource = dataSource;
        DatabaseName = databaseName;
        Username = username;
        Password = password;
    }
}