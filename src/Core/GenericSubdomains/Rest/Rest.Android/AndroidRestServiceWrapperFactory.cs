using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.GenericSubdomains.Rest.Android
{
    internal class AndroidRestServiceWrapperFactory : IRestServiceWrapperFactory
    {
        private readonly IJsonUtils jsonUtils;

        public AndroidRestServiceWrapperFactory(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
        }

        public IRestServiceWrapper CreateRestServiceWrapper(string baseAddress, bool acceptUnsignedCertificate = false)
        {
            return new AndroidRestServiceWrapper(baseAddress, this.jsonUtils, acceptUnsignedCertificate);
        }
    }
}