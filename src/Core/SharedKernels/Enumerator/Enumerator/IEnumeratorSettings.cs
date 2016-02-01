﻿using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator
{
    public interface IEnumeratorSettings : IRestServiceSettings
    {
        int GpsReceiveTimeoutSec { get; }
        double GpsDesiredAccuracy { get; }
    }
}