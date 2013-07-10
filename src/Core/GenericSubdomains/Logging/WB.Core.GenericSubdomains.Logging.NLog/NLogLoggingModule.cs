using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    public class NLogLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().ToMethod(x => LogManager.GetLogger(x.Request.ParentRequest.Service));
        }
    }
}
