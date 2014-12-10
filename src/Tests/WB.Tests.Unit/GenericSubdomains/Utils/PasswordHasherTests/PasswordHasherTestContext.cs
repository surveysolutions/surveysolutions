using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;

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
