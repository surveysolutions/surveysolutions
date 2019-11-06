using System.ComponentModel.DataAnnotations;

namespace WB.UI.Shared.Web.DataAnnotations
{
    public class ServerSideOnlyRegularExpressionAttribute: RegularExpressionAttribute
    {
        public ServerSideOnlyRegularExpressionAttribute(string pattern)
            : base(pattern)
        {
        }
    }
}
