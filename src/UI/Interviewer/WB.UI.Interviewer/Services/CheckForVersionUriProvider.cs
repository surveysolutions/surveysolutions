﻿using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Services
{
    public class CheckForVersionUriProvider : ICheckVersionUriProvider
    {
        public string CheckVersionUrl { get; } = "api/interviewer/";
    }
}