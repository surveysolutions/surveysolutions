using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class RosterVector : IEnumerable<decimal>
    {
        public static readonly RosterVector Empty = new decimal[]{};

        private readonly ReadOnlyCollection<decimal> coordinates;

        public RosterVector(IEnumerable<decimal> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException("coordinates");

            this.coordinates = new ReadOnlyCollection<decimal>(new List<decimal>(coordinates));
        }

        public IReadOnlyCollection<decimal> Coordinates
        {
            get { return this.coordinates; }
        }

        public override string ToString()
        {
            return string.Format("<{0}>", string.Join("-", this.Coordinates));
        }

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

        private decimal[] Array
        {
            get { return this.array ?? (this.array = this.Coordinates.ToArray()); }
        }

        IEnumerator<decimal> IEnumerable<decimal>.GetEnumerator()
        {
            return ((IEnumerable<decimal>)this.Array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Array.GetEnumerator();
        }

        public int Length
        {
            get { return this.Array.Length; }
        }

        public decimal this[int index]
        {
            get { return this.Array[index]; }
        }

        public static implicit operator decimal[](RosterVector rosterVector)
        {
            return rosterVector.Array;
        }

        public static implicit operator RosterVector(decimal[] array)
        {
            return new RosterVector(array);
        }

        public bool Identical(RosterVector other)
        {
            if (other == null) return false;

            if (this.Length == 0 && other.Length == 0 || ReferenceEquals(this, other))
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
            unchecked
            {
                int hc = this.Coordinates.Count;

                foreach (var coordinate in this.Coordinates)
                {
                    hc = unchecked(hc * 13 + coordinate.GetHashCode());
                }

                return hc;
            }
        }
    }
}