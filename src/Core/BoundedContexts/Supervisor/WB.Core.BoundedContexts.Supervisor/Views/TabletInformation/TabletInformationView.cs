﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.TabletInformation
{
    public class TabletInformationView
    {
        public TabletInformationView(string packageName, string androidId, string registrationId, DateTime creationDate, long size)
        {
            this.Size = size;
            this.CreationDate = creationDate;
            this.PackageName = packageName;
            this.AndroidId = androidId;
            this.RegistrationId = registrationId;
        }

        public string PackageName { get;private set; }
        public string AndroidId { get;private set; }
        public string RegistrationId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public long Size { get; private set; }
    }
}
