using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class RosterVector : IEnumerable<decimal>
    {
        private int? cachedHashCode = null;
        public static readonly RosterVector Empty = new decimal[]{};

        private readonly ReadOnlyCollection<decimal> coordinates;

        public RosterVector(IEnumerable<decimal> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

            this.coordinates = new ReadOnlyCollection<decimal>(new List<decimal>(coordinates));
        }

        public IReadOnlyCollection<decimal> Coordinates => this.coordinates;

        public override string ToString() => $"<{string.Join("-", this.Coordinates)}>";

        public RosterVector Shrink(int targetLength)
        {
            if (targetLength == 0)
                return Empty;

            if (targetLength == this.Length)
                return this;

            if (targetLength > this.Length)
                throw new ArgumentException($"Cannot shrink roster vector {this} with length {this.Length} to bigger length {targetLength}.");

            return this.Coordinates.Take(targetLength).ToArray();
        }

        public RosterVector ExtendWithOneCoordinate(decimal coordinate)
        {
            return new List<decimal>(this.Coordinates) { coordinate }.ToArray();
        }

        #region Backward compatibility with decimal[]

        private decimal[] array;

        private decimal[] Array => this.array ?? (this.array = this.Coordinates.ToArray());

        IEnumerator<decimal> IEnumerable<decimal>.GetEnumerator() => ((IEnumerable<decimal>)this.Array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Array.GetEnumerator();

        public int Length => this.Array.Length;

        public decimal this[int index] => this.Array[index];

        public static implicit operator decimal[](RosterVector rosterVector) => rosterVector.Array;

        public static implicit operator RosterVector(decimal[] array) => new RosterVector(array);

        public bool Identical(RosterVector other)
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

            if (obj.GetType() == typeof(RosterVector))
                return this.Identical((RosterVector) obj);

            if (obj.GetType() == typeof(decimal[]))
                return this.Identical((decimal[])obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (!this.cachedHashCode.HasValue)
            {
                unchecked
                {
                    int hc = this.Coordinates.Count;

                    foreach (var coordinate in this.Coordinates)
                    {
                        hc = unchecked(hc*13 + coordinate.GetHashCode());
                    }

                    this.cachedHashCode = hc;
                }
            }

            return this.cachedHashCode.Value;
        }

        public static bool operator ==(RosterVector a, RosterVector b)
            => ReferenceEquals(a, b)
            || (a?.Equals(b) ?? false);

        public static bool operator !=(RosterVector a, RosterVector b)
            => !(a == b);
    }
}