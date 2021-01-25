using System.DirectoryServices.Protocols;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    public class MockDataProtectorProvider : IDataProtectionProvider
    {
        public IDataProtector CreateProtector(string purpose)
        {
            return new MockDataProtector();
        }

        class MockDataProtector : IDataProtector
        {
            public IDataProtector CreateProtector(string purpose)
            {
                return this;
            }

            public byte[] Protect(byte[] plaintext)
            {
                return plaintext;
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                return protectedData;
            }
        }
    }
}
