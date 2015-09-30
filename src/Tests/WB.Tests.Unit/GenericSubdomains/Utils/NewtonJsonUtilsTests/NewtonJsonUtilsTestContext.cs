using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class NewtonJsonUtilsTestContext
    {
        public static NewtonJsonSerializer CreateNewtonJsonUtils(Dictionary<string, string> assemblyReplacementMapping=null)
        {
            return assemblyReplacementMapping == null
                ? new NewtonJsonSerializer()
                : new NewtonJsonSerializer(assemblyReplacementMapping);
        }
    }
}
