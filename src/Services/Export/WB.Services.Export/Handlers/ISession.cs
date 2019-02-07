using System.Data;
using System.Data.Common;

namespace WB.Services.Export.Handlers
{
    public interface ISession
    {
        DbConnection Connection { get; }
    }
}
