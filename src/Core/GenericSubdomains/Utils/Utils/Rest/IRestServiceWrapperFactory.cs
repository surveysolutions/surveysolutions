namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public interface IRestServiceWrapperFactory
    {
        IRestServiceWrapper CreateRestServiceWrapper(string baseAddress, bool acceptUnsignedCertificate = false);
    }
}
