using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public struct ItemPublicKey
    {
        public ItemPublicKey(Guid publicKey, int[] propagationVector)
        {
            PublicKey = publicKey;
            PropagationVector = propagationVector;
        }
        public ItemPublicKey(Guid publicKey)
        {
            PublicKey = publicKey;
            PropagationVector = new int[0];
        }
        public readonly Guid PublicKey;
        public readonly int[] PropagationVector;

        public bool CompareWithVector(int[] vector)
        {
            if (PropagationVector.Length != vector.Length)
                return false;
            return PropagationVector.All(vector.Contains);
        }

        public bool IsTopLevel {
            get { return this.PropagationVector.Length == 0; }
        }

        public override bool Equals(object obj)
        {
            return obj is ItemPublicKey && this == (ItemPublicKey)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ItemPublicKey x, ItemPublicKey y)
        {
            if (x.PublicKey != y.PublicKey)
                return false;
            return x.CompareWithVector(y.PropagationVector);
        }

        public static bool operator !=(ItemPublicKey x, ItemPublicKey y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            if (!IsTopLevel)
            {
                string vector = string.Join(",", PropagationVector);
                return string.Format("{0},{1}", vector, PublicKey);
            }
            return PublicKey.ToString();
        }

        public static explicit operator ItemPublicKey(string b)  // explicit byte to digit conversion operator
        {
            return Parse(b);
        }

        public static ItemPublicKey Parse(string value)
        {
            if (value.Contains(','))
            {
                var items = value.Split(',');
                var vector = new int[items.Length - 1];
                for (int i = 0; i < items.Length - 1; i++)
                {
                    vector[i] = int.Parse(items[i]);
                }
                return new ItemPublicKey(Guid.Parse(items[items.Length - 1]), vector);
            }
            return new ItemPublicKey(Guid.Parse(value));
        }
    }

}