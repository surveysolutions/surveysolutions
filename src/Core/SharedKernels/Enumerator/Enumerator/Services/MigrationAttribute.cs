using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(long version, string description = null)
        {
            Version = version;
            Description = description;
        }
        public long Version { get; }
        public string Description { get; }
    }
}
