using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class ImportResult
    {
        public ImportResult(Guid id, bool isCompleted, string errorMessage, string[] header, List<string[]> values)
        {
            Id = id;
            IsCompleted = isCompleted;
            ErrorMessage = errorMessage;
            Header = header;
            if (values != null)
                Values = values.ToArray();
        }

        public Guid Id { get; private set; }
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; }
        public string[] Header { get; private set; }
        public string[][] Values { get; private set; }

        public bool IsSuccessed {
            get { return string.IsNullOrEmpty(this.ErrorMessage); }
        }
    }
}
