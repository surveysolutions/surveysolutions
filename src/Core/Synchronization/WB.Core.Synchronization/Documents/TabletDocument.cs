using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.Documents
{
    public class TabletDocument : IView
    {
        public Guid DeviceId { get; set; }

        public string AndroidId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public List<Guid> Users { get; set; }
    }
}