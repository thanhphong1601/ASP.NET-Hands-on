namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken);
        string IssueJwtAdminAsync(string username);
    }
}
