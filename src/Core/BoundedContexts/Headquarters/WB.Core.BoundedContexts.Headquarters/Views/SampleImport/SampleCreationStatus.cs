namespace WB.Core.BoundedContexts.Headquarters.Views.SampleImport
{
    public  class SampleCreationStatus
    {
        public SampleCreationStatus()
        {
        }
        public SampleCreationStatus(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
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
