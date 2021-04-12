using System;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NonWorkspacedAttribute : Attribute
    {
        
    }
}