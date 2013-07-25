using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class TempFileImportData:IView
    {
        public TempFileImportData(Guid publicKey, Guid templateId, string[][] values, string[] header)
        {
            PublicKey = publicKey;
            TemplateId = templateId;
            Values = values;
            Header = header;
        }

        public TempFileImportData()
        {
        }

        public Guid PublicKey { get;  set; }
        public Guid TemplateId { get;  set; }
        public string[][] Values { get;  set; }
        public string[] Header { get;  set; }
    }
}
