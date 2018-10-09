namespace WB.Services.Scheduler.Model.Events
{
    public class UpdateDataEvent : JobEvent
    {
        public string Key { get; }
        public object Value { get; }

        public UpdateDataEvent(long jobId, string key, object value) : base(jobId)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{nameof(UpdateDataEvent)} [{Key}]={Value}";
        }
    }
}
