using System;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RetryFailedJobAttribute : Attribute
    {

    }
}