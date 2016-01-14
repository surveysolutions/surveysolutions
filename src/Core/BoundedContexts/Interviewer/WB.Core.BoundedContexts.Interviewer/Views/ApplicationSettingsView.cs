﻿using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class ApplicationSettingsView : IPlainStorageEntity
    {
        public string Id { get; set; }
        public string Endpoint { get; set; }
        public int HttpResponseTimeoutInSec { get; set; }
        public int GpsResponseTimeoutInSec { get; set; }
        public int CommunicationBufferSize { get; set; }
    }
}
