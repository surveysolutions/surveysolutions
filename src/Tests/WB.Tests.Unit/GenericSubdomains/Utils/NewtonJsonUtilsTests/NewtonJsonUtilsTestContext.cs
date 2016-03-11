using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class NewtonJsonUtilsTestContext
    {
        public static NewtonJsonSerializer CreateNewtonJsonUtils(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory = null, Dictionary<string, string> assemblyReplacementMapping = null)
        {
            return assemblyReplacementMapping == null
                ? new NewtonJsonSerializer(jsonSerializerSettingsFactory ?? new JsonSerializerSettingsFactory())
                : new NewtonJsonSerializer(jsonSerializerSettingsFactory ?? new JsonSerializerSettingsFactory(),
                    assemblyReplacementMapping ?? new Dictionary<string, string>());
        }
    }
}
