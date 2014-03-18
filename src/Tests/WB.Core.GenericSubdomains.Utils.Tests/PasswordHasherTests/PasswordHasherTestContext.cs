using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;

namespace WB.Core.GenericSubdomains.Utils.Tests.PasswordHasherTests
{
    internal class PasswordHasherTestContext
    {
        protected static PasswordHasher CreatePasswordHasher()
        {
            return new PasswordHasher();
        }
    }
}
