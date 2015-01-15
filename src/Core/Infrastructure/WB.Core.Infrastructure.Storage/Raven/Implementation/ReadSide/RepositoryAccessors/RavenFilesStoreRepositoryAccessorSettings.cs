using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenFilesStoreRepositoryAccessorSettings
    {
        public RavenFilesStoreRepositoryAccessorSettings(string url, Action<string> additionalEventChecker)
        {
            this.Url = url;
            this.AdditionalEventChecker = additionalEventChecker;
        }

        public string Url { get; private set; }
        public Action<string> AdditionalEventChecker { get; private set; }
    }
}
