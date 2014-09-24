namespace WB.UI.Shared.Web.Filters
{
    public interface ITokenVerifier
    {
        bool IsTokenValid(string token);
    }
}