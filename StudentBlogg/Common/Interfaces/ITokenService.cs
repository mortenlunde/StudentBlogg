namespace StudentBlogg.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid userId, string username);
}