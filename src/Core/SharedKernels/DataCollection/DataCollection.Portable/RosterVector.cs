using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class RosterVector : IEnumerable<int>, IEquatable<RosterVector>
    {
        private int? cachedHashCode;
        public static readonly RosterVector Empty = new int[] { };
        private readonly int[] coordinates;
        private decimal[] coordinatesAsDecimals;

        public static RosterVector Convert(object obj)
        {
            var answerAsRosterVector = obj as RosterVector;
            if (answerAsRosterVector != null)
            {
                return answerAsRosterVector;
            }
            
            var answerAsDecimalArray = obj as decimal[];
            if (answerAsDecimalArray != null)
                return new RosterVector(answerAsDecimalArray);

            var answerAsIntArray = obj as int[];
            if (answerAsIntArray != null)
                return new RosterVector(answerAsIntArray);

            throw new ArgumentException(nameof(obj));
        }

        public static RosterVector[] ConvertToArray(object obj)
        {
            switch (obj)
            {
                case RosterVector[] answerAsRosterVector:
                    return answerAsRosterVector;
                case int[][] answerAsIntArrayArray:
                    return answerAsIntArrayArray.Select(intArray => (RosterVector)intArray).ToArray();
                case decimal[][] answerAsDecimalArrayArray:
                    return answerAsDecimalArrayArray.Select(decimalArray => (RosterVector)decimalArray).ToArray();
                case IReadOnlyCollection<RosterVector> readOnlyCollection:
                    return readOnlyCollection.ToArray();
                default:
                    throw new ArgumentException(nameof(obj));
            }
        }

        public RosterVector(IEnumerable<decimal> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            this.coordinates = coordinates.Select(System.Convert.ToInt32).ToArray();
        }

        public RosterVector(IEnumerable<int> coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

            var asArray = coordinates as int[];
            this.coordinates = asArray ?? coordinates.ToArray();
        }

        public IReadOnlyCollection<int> Coordinates => this.coordinates;

        [Obsolete("version 5.19. started transition to ints as vector. should be removed later")]
        public decimal[] CoordinatesAsDecimals => this.coordinatesAsDecimals ??= this.coordinates.Select(System.Convert.ToDecimal).ToArray();

        public bool Equals(RosterVector other)
        {
            if (other == null)
                return false;
            
            return this.Identical(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() == typeof(RosterVector))
                return this.Identical((RosterVector)obj);

            if (obj.GetType() == typeof(int[]))
                return this.coordinates.SequenceEqual((int[])obj);

            if (obj.GetType() == typeof(decimal[]))
                return this.CoordinatesAsDecimals.SequenceEqual((decimal[])obj);

            return false;
        }

        public RosterVector ExtendWithOneCoordinate(int coordinate)
        {
            return this.coordinates.ExtendWithOneItem(coordinate);
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

        public static RosterVector Parse(string value)
        {
            value = value.Trim('_');

            if (string.IsNullOrWhiteSpace(value))
            {
                return Empty;
            }

            return new RosterVector(value.ParseMinusDelimitedIntArray());
        }

        public RosterVector Replace(int index, int replaceTo)
        {
            var clonedRosterVector = (int[])this.coordinates.Clone();
            clonedRosterVector[index] = replaceTo;

            return clonedRosterVector;
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
                case 0:
                    return new RosterVector(System.Array.Empty<int>());
                case 1:
                    return new RosterVector(new[] {this.coordinates[0]});
                case 2:
                    return new RosterVector(new[] { this.coordinates[0], this.coordinates[1] });
                case 3:
                    return new RosterVector(new[] { this.coordinates[0], this.coordinates[1], this.coordinates[2] });
            }

            return this.coordinates.Take(targetLength).ToArray();
        }

        public string AsString() => string.Join("-", this.coordinates.Select(c => c));

        public override string ToString()
        {
            if (this.coordinates.Length > 0)
                return $"_{AsString()}";
            return string.Empty;
        }

        #region Backward compatibility with int[]

        public int[] Array => this.coordinates;

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => ((IEnumerable<int>)this.Array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Array.GetEnumerator();

        public int Length => this.Array.Length;

        public int this[int index] => this.Array[index];
        public static implicit operator int[] (RosterVector rosterVector) => rosterVector.Array;
        public static implicit operator RosterVector(int[] array) => new RosterVector(array);

        public bool Identical(RosterVector other)
        {
            if (other == null) return false;

            if (this.coordinates.Length != other.coordinates.Length)
                return false;

            if (this.coordinates.Length == 0 && other.coordinates.Length == 0 || ReferenceEquals(this, other))
                return true;

            return this.coordinates.SequenceEqual(other.coordinates);
        }

        public bool Identical(RosterVector other, int otherLength)
        {
            if (other == null) return false;

            if (this.coordinates.Length == 0 && otherLength == 0 || ReferenceEquals(this, other))
                return true;
            
            return ArrayExtensions.SequenceEqual(this.coordinates, other.coordinates, otherLength);
        }

        #endregion

        #region Backward compatibility with decimal[]
        public static implicit operator decimal[] (RosterVector rosterVector) => rosterVector.CoordinatesAsDecimals;
        public static implicit operator RosterVector(decimal[] array) => new RosterVector(array);
        #endregion
    }
}
