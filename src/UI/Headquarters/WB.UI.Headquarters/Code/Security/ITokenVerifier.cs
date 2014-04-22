namespace WB.UI.Headquarters.API.Attributes
{
    public interface ITokenVerifier
    {
        bool IsTokenValid(string token);
    }
}