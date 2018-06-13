using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Unit.GenericSubdomains.Utils.PasswordHasherTests
{
    internal class PasswordHasherTestContext
    {
        protected static PasswordHasher CreatePasswordHasher()
        {
            return new PasswordHasher();
        }
    }
}
