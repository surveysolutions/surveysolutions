using System.Data;
using System.Data.Common;

namespace WB.Services.Export.Storage
{
    public class Session : ISession
    {
        public DbConnection Connection { get; set; }
    }
}
