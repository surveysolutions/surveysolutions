using System.Data;
using System.Data.Common;

namespace WB.Services.Export.Storage
{
    public interface ISession
    {
        DbConnection Connection { get; }
    }
}
