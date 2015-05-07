using System;

namespace WB.Core.SharedKernels.DataCollection.Views
{
    public class DeviceInfo
    {
        public virtual UserDocument User { get; set; }

        public virtual int Id { get; set; }

        public virtual DateTime Date { get; set; }

        public virtual string DeviceId { get; set; }
    }
}