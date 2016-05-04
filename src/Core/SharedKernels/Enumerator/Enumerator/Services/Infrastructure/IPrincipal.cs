using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IPrincipal
    {
        bool IsAuthenticated { get; }
        IUserIdentity CurrentUserIdentity { get; }
        Task<bool> SignInAsync(string userName, string password, bool staySignedIn);
        Task SignOutAsync();
    }
}
