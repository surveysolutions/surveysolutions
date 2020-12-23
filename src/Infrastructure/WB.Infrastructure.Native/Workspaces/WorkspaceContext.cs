using System;
using System.Diagnostics;

#nullable enable
namespace WB.Infrastructure.Native.Workspaces
{
    [DebuggerDisplay("{Name} - {DisplayName}")]
    public class WorkspaceContext
    {
        public WorkspaceContext(string name, string displayName, DateTime? disabledAtUtc = null)
        {
            Name = name;
            DisplayName = displayName;
            DisabledAtUtc = disabledAtUtc;
        }

        public DateTime? DisabledAtUtc { get; }

        public string Name { get; }
        public string DisplayName { get; }
        public string? PathBase { get; set; }

        public string SchemaName => $"{SchemaPrefix}{Name}";

        public const string SchemaPrefix = "ws_";

        protected bool Equals(WorkspaceContext other)
        {
            return Name == other.Name && DisplayName == other.DisplayName;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WorkspaceContext) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, DisplayName);
        }
    }
}
