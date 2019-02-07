using System.Data;
using System.Data.Common;

namespace WB.Services.Export.Handlers
{
    public class Session : ISession
    {
        public DbConnection Connection { get; set; }
    }
}
