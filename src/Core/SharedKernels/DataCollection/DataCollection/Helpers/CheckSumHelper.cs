using System;
using System.Security.Cryptography;

namespace WB.Core.SharedKernels.DataCollection.Helpers;

public static class CheckSumHelper
{
    public static string GetMd5Cache(byte[] content)
    {
        using var crypto = MD5.Create();
        var hash = crypto.ComputeHash(content);
        var hashString = BitConverter.ToString(hash).Replace("-", "");
        return hashString;
    }
    
    public static string GetSha1Cache(byte[] content)
    {
        using var crypto = SHA1.Create();
        var hash = crypto.ComputeHash(content);
        var hashString = BitConverter.ToString(hash).Replace("-", "");
        return hashString;
    }
}
