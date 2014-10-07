using System.Web.Helpers;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiValidationAntiForgeryTokenVerifier : ITokenVerifier
    {
        public bool IsTokenValid(string token)
        {
            var isTokenValid = false;

            try
            {
                var cookieToken = string.Empty;
                var formToken = string.Empty;

                var tokens = token.Split(':');
                if (tokens.Length == 2)
                {
                    cookieToken = tokens[0].Trim();
                    formToken = tokens[1].Trim();
                }

                AntiForgery.Validate(cookieToken, formToken);

                isTokenValid = true;
            }
            catch
            {
                
            }

            return isTokenValid;
        }
    }
}
