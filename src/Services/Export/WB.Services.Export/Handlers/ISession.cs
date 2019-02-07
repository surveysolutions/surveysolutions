using System.Data;

namespace WB.Services.Export.Handlers
{
    public interface ISession
    {
        IDbConnection Connection { get; }
    }
}