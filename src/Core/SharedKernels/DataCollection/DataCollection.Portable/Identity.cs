using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection
{
    /// <summary>
    /// Full identity of group or question: id and roster vector.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Identity: IEquatable<Identity>
    {
        private int? hashCode;

        public bool Equals(Identity other)
        {
            if (other == null)
                return false;
            
            return this.Id == other.Id && this.RosterVector.Identical(other.RosterVector);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Identity) obj);
        }

        public override int GetHashCode()
        {
            this.hashCode ??= this.Id.GetHashCode() ^ this.RosterVector.GetHashCode();

            return this.hashCode.Value;
        }

        public Guid Id { get; set; }

        public RosterVector RosterVector { get; set; }

        // ReSharper disable once UnusedMember.Local # used by NHibernate
        private Identity()
        {
            this.RosterVector = RosterVector.Empty;
        }

        public Identity(Guid id, RosterVector rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector ?? RosterVector.Empty;
        }

        public static Identity Create(Guid id, RosterVector rosterVector) => new Identity(id, rosterVector);

        public static Identity Parse(string value)
        {
            var id = Guid.Parse(value.Substring(0, 32));
            var rosterVector = RosterVector.Parse(value.Substring(32));

            return Create(id, rosterVector);
        }

        public static bool TryParse(string value, out Identity identity)
        {
            try
            {
                identity = Parse(value);
                return true;
            }
            catch
            {
                identity = null;
                return false;
            }
        }

        public bool Equals(Guid id, RosterVector rosterVector) => id == this.Id && this.RosterVector.Identical(rosterVector);

        public bool Equals(Guid id, RosterVector rosterVector, int length) => id == this.Id && this.RosterVector.Identical(rosterVector, length);

        public static bool operator ==(Identity a, Identity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Identity a, Identity b) => !(a == b);

        public override string ToString() => $"{this.Id.FormatGuid()}{this.RosterVector}";
    }
}
