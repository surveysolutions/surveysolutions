using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class RosterScope : IEnumerable<Guid>
    {
        private int? cachedHashCode = null;
        public static readonly RosterScope Empty = new Guid[] { };

        private readonly ReadOnlyCollection<Guid> coordinates;

        public RosterScope(IEnumerable<Guid> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

            this.coordinates = new ReadOnlyCollection<Guid>(new List<Guid>(coordinates));
        }

        public IReadOnlyCollection<Guid> Coordinates => this.coordinates;

        public override string ToString() => $"<{string.Join("-", this.Coordinates)}>";

        #region Backward compatibility with decimal[]

        private Guid[] array;

        private Guid[] Array => this.array ?? (this.array = this.Coordinates.ToArray());

        IEnumerator<Guid> IEnumerable<Guid>.GetEnumerator() => ((IEnumerable<Guid>)this.Array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Array.GetEnumerator();

        public int Length => this.Array.Length;

        public Guid this[int index] => this.Array[index];

        public static implicit operator Guid[] (RosterScope rosterVector) => rosterVector.Array;

        public static implicit operator RosterScope(Guid[] array) => new RosterScope(array);

        public bool Identical(RosterScope other)
        {
            if (other == null) return false;

            if ((this.Length == 0 && other.Length == 0) || ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.Length != other.Length)
            {
                return false;
            }

            return this.Array.SequenceEqual(other.Array);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() == typeof(RosterScope))
                return this.Identical((RosterScope)obj);

            if (obj.GetType() == typeof(Guid[]))
                return this.Identical((Guid[])obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (this.cachedHashCode.HasValue)
                return this.cachedHashCode.Value;

            int hc = this.coordinates.Count;

            for (int i = 0; i < this.coordinates.Count; i++)
            {
                int hashCode = this.coordinates[i].GetHashCode();
                hc = unchecked(hc * 13 + hashCode);
            }

            this.cachedHashCode = hc;

            return this.cachedHashCode.Value;
        }

        public static bool operator ==(RosterScope a, RosterScope b)
            => ReferenceEquals(a, b)
               || (a?.Equals(b) ?? false);

        public static bool operator !=(RosterScope a, RosterScope b)
            => !(a == b);

        public bool IsParentScopeFor(RosterScope rosterScope)
        {
            return rosterScope.Length > this.Length && this.SequenceEqual(rosterScope.Take(this.Length));
        }

        public bool IsChildScopeFor(RosterScope rosterScope)
        {
            return rosterScope.Length < this.Length && rosterScope.SequenceEqual(this.Take(rosterScope.Length));
            
        }

        public bool IsSameOrChildScopeFor(RosterScope rosterScope)
        {
            return this.Equals(rosterScope) || this.IsChildScopeFor(rosterScope);
        }

        public bool IsSameOrParentScopeFor(RosterScope rosterScope)
        {
            return this.Equals(rosterScope) || this.IsParentScopeFor(rosterScope);
        }
    }
}