namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
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
