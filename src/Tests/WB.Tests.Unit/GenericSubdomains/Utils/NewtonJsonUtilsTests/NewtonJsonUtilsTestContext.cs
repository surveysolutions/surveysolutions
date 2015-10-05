using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class NewtonJsonUtilsTestContext
    {
        public static NewtonJsonUtils CreateNewtonJsonUtils(Dictionary<string, string> assemblyReplacementMapping=null)
        {
            return assemblyReplacementMapping == null
                ? new NewtonJsonUtils()
                : new NewtonJsonUtils(assemblyReplacementMapping);
        }
    }
}
