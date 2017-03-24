using System;
using System.Text;
using WB.UI.Designer.Api.Attributes;

namespace WB.Tests.Unit.Designer.Applications.AttributesTests
{
    internal class AttributesTestContext
    {
        public static ApiBasicAuthAttribute CreateApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials = null, bool onlyAllowedAddresses = false)
        {
            return new ApiBasicAuthAttribute(validateUserCredentials ?? ((s, s1) => true), onlyAllowedAddresses);
        }
        
        public static string EncodeToBase64(string value)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}
