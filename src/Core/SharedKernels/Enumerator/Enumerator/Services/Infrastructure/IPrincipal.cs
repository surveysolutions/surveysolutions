namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IPrincipal
    {
        bool IsAuthenticated { get; }
        IUserIdentity CurrentUserIdentity { get; }
        bool SignIn(string userName, string password, bool staySignedIn);
        bool SignInWithHash(string userName, string passwordHash, bool staySignedIn);
        void SignOut();
        bool SignIn(string userId, bool staySignedIn);
    }
}
