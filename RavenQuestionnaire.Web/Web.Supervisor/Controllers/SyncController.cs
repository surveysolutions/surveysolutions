using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Controllers
{
    public class SyncController : ApiController
    {
        [HandleUIException]
        public bool Handshake(string login, string password, string clientID, string LastSyncID)
        {
            return true;
        }

        [HandleUIException]
        public bool T()
        {
            return true;
        }




    }
}
