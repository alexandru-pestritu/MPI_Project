namespace DbProvider.Database;

public interface IDatabaseConnectionDetails
{
    public string DataSource { get; }
    public string DatabaseName { get; }
    public string Username { get; }
    public string Password { get; }
}