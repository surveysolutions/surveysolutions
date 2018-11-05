using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WB.Services.Export.Interview.Entities
{
     [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class RosterVector : IEnumerable<int>
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

        public IEnumerator<int> GetEnumerator()
        {
            return Coordinates.GetEnumerator();
        }

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
        
        public int Last()
        {
            return this.coordinates[this.Length - 1];
        }

        public RosterVector Take(int targetLength)
        {
            if (targetLength > this.coordinates.Length)
            {
                return this;
            }

            return this.Shrink(targetLength);
        }

        public RosterVector Shrink(int targetLength)
        {
            if (targetLength == 0)
                return Empty;

            if (targetLength == this.Length)
                return this;

            if (targetLength > this.Length)
                throw new ArgumentException(
                    $"Cannot shrink roster vector {this} with length {this.Length} to bigger length {targetLength}.");

            switch (targetLength)
            {
                case 1:
                    return new RosterVector(new[] {this.coordinates[0]});
                case 2:
                    return new RosterVector(new[] { this.coordinates[0], this.coordinates[1] });
                case 3:
                    return new RosterVector(new[] { this.coordinates[0], this.coordinates[1], this.coordinates[2] });
            }

            return new RosterVector(this.coordinates.Take(targetLength));
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public static implicit operator RosterVector(int[] array) => new RosterVector(array);
    }
}
