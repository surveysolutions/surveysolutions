using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Attributes
{
    public class SimpleTokenVerifier : ITokenVerifier
    {
        public SimpleTokenVerifier(string key)
        {
            this.Key = key;
        }

        public string Key { get; private set; }

        public bool IsTokenValid(string token)
        {
            return token == this.Key;
        }
    }
}