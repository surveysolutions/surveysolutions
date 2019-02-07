using System.Data;

namespace WB.Services.Export.Handlers
{
    public class Session : ISession
    {
        public IDbConnection Connection { get; set; }
    }
}