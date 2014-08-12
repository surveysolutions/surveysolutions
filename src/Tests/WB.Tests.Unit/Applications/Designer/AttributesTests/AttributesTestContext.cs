using System;
using System.Text;
using WB.UI.Designer.Api.Attributes;

namespace WB.Tests.Unit.Applications.Designer.AttributesTests
{
    public class AttributesTestContext
    {
        public static ApiBasicAuthAttribute CreateApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials = null)
        {
            return new ApiBasicAuthAttribute(validateUserCredentials ?? ((s, s1) => true) );
        }
        
        public static string EncodeToBase64(string value)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}
