using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Services.SampleImport.DTO
{
    public  class SampleCreationStatus
    {
        public SampleCreationStatus(Guid id, bool isCompleted, string errorMessage)
        {
            Id = id;
            IsCompleted = isCompleted;
            ErrorMessage = errorMessage;
        }

        public Guid Id { get; private set; }
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool IsSuccessed
        {
            get { return string.IsNullOrEmpty(this.ErrorMessage); }
        }

        public void CompleteProcess()
        {
            this.IsCompleted = true;
        }

        public void SetErrorMessage(string message)
        {
            this.ErrorMessage = message;
            this.IsCompleted = true;
        }
    }
}
