using System;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Utils;

public static class BrokenFileHelper
{
    public static string GetBrokenFileName(Guid userId, string originalFileName)
    {
        var utcNow = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");
        var newFileName = $"{userId.FormatGuid()}#{utcNow}#{originalFileName}";
        return newFileName;
    }
}
