namespace WB.Core.GenericSubdomains.Utils.Implementation.Crypto
{
    internal interface ICrypto : IHash, IBlockHash
    {
    }

    internal interface ICryptoNotBuildIn : ICrypto
    {
    }

    internal interface IBlockHash
    {
    }
}
