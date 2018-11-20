using System;

namespace WB.Core.Infrastructure.Versions
{
    public interface IProductVersion
    {
        string ToString();
        Version GetVersion();
        int GetBildNumber();
    }
}
