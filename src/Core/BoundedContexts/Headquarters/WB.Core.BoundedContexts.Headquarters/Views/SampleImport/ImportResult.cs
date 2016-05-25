using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.SampleImport
{
    public class ImportResult
    {
        public ImportResult(Guid id, bool isCompleted, string errorMessage, string[] header, List<string[]> values)
        {
            this.Id = id;
            this.IsCompleted = isCompleted;
            this.ErrorMessage = errorMessage;
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.Header = header;
                if (values != null)
                    this.Values = values.ToArray();
            }
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
