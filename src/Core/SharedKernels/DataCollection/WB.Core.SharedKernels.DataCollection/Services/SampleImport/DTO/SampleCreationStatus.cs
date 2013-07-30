using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Services.SampleImport.DTO
{
    public  class SampleCreationStatus
    {
        public SampleCreationStatus()
        {
        }
        public SampleCreationStatus(Guid id)
        {
            Id = id;
        }

        public Guid Id { get;  set; }
        public bool IsCompleted { get;  set; }
        public string ErrorMessage { get;  set; }
        public string StatusMessage { get;  set; }

        public bool IsSuccessed
        {
            get { return string.IsNullOrEmpty(this.ErrorMessage); }
        }

        public void CompleteProcess()
        {
            this.IsCompleted = true;
        }

        public void SetStatusMessage(string message)
        {
            this.StatusMessage = message;
        }

        public void SetErrorMessage(string message)
        {
            this.ErrorMessage = message;
            this.IsCompleted = true;
        }
    }
}
