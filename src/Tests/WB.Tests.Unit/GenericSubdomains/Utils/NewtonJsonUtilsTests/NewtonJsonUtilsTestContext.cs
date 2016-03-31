using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class NewtonJsonUtilsTestContext
    {
        public static NewtonJsonSerializer CreateNewtonJsonUtils(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory = null)
        {
            return new NewtonJsonSerializer(jsonSerializerSettingsFactory ?? new JsonSerializerSettingsFactory());
        }
    }
}
