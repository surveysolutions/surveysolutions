namespace WB.Core.GenericSubdomains.Rest
{
    public interface IRestServiceWrapperFactory
    {
        IRestServiceWrapper CreateRestServiceWrapper(string baseAddress, bool acceptUnsignedCertificate = false);
    }
}
