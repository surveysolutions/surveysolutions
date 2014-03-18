namespace WB.Core.GenericSubdomains.Utils.Crypto
{
    public interface ICrypto : IHash, IBlockHash
    {
    }

    public interface ICryptoNotBuildIn : ICrypto
    {
    }

    public interface IBlockHash
    {
    }
}
