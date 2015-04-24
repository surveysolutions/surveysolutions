using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.Documents
{
    public class TabletDocument : IView
    {
        public virtual string Id { get; set; }

        public virtual Guid DeviceId { get; set; }

        public virtual string AndroidId { get; set; }

        public virtual DateTime RegistrationDate { get; set; }
    }
}