using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WB.Services.Export.Interview.Entities
{
     [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class RosterVector
    {
        private int? cachedHashCode;
        public static readonly RosterVector Empty = new RosterVector(Array.Empty<int>());
        private readonly int[] coordinates;

        public RosterVector(IEnumerable<int> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

            var asArray = coordinates as int[];
            this.coordinates = asArray ?? coordinates.ToArray();
        }

        public IReadOnlyCollection<int> Coordinates => this.coordinates;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() == typeof(RosterVector))
                return this.Identical((RosterVector)obj);

            if (obj.GetType() == typeof(int[]))
                return this.coordinates.SequenceEqual((int[])obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (!this.cachedHashCode.HasValue)
            {
                var hc = this.coordinates.Length;

                for (var i = 0; i < this.coordinates.Length; i++)
                {
                    var hashCode = this.coordinates[i].GetHashCode();
                    hc = unchecked(hc * 13 + hashCode);
                }

                this.cachedHashCode = hc;
            }

            return this.cachedHashCode.Value;
        }

        public static bool operator ==(RosterVector a, RosterVector b)
            => ReferenceEquals(a, b)
                || (a?.Equals(b) ?? false);

        public static bool operator !=(RosterVector a, RosterVector b)
            => !(a == b);

        public override string ToString()
        {
            if (this.coordinates.Length > 0)
                return $"_{string.Join("-", this.coordinates.Select(c => c))}";
            return string.Empty;
        }

        public int Length => this.Coordinates.Count;

        public bool Identical(RosterVector other)
        {
            if (other == null) return false;

            if (this.Length == 0 && other.Length == 0 || ReferenceEquals(this, other))
                return true;

            if (this.Length != other.Length)
                return false;

            return this.coordinates.SequenceEqual(other.coordinates);
        }

        public bool Identical(RosterVector other, int otherLength)
        {
            if (other == null) return false;

            if (this.Length == 0 && otherLength == 0 || ReferenceEquals(this, other))
                return true;
            
            return SequenceEqual(this.coordinates, other.coordinates, otherLength);
        }


        private bool SequenceEqual(int[] source, int[] target, int targetLength)
        {
            if (source.Length == targetLength)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (target[i] == source[i]) continue;

                    return false;
                }

                return true;
            }

            return false;
        }

        public int this[int index] => this.coordinates[index];
    }
}
