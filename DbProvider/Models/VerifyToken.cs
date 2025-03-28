namespace DbProvider.Models;

public class VerifyToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public int TokenType { get; set; }

    public VerifyToken(int id, int userId, string token, int tokenType)
    {
        Id = id;
        UserId = userId;
        Token = token;
        TokenType = tokenType;
    }
}