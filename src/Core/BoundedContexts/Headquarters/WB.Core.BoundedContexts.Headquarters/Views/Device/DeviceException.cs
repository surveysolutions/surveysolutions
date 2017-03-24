using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Device
{
    public class DeviceException
    {
        public int Id { get; set; }
        public Guid InterviewerId { get; set; }
        public string DeviceId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime ExceptionDate { get; set; }
    }
}