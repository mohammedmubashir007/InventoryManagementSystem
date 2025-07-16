namespace AuthService.Interfaces
{
    public interface ITokenService
    {
        string GetToken(string username, string email);
    }
}