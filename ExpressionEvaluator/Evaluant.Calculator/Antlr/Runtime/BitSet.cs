namespace Antlr.Runtime
{
    using System;
    using System.Collections;
    using System.Text;

    public class BitSet
    {
        protected internal ulong[] bits;
        protected internal const int BITS = 0x40;
        protected internal const int LOG_BITS = 6;
        protected internal static readonly int MOD_MASK = 0x3f;

        public BitSet() : this(0x40)
        {
        }

        public BitSet(ulong[] bits_)
        {
            this.bits = bits_;
        }

        public BitSet(IList items) : this(0x40)
        {
            for (int i = 0; i < items.Count; i++)
            {
                int el = (int) items[i];
                this.Add(el);
            }
        }

        public BitSet(int nbits)
        {
            this.bits = new ulong[((nbits - 1) >> 6) + 1];
        }

        public virtual void Add(int el)
        {
            int index = WordNumber(el);
            if (index >= this.bits.Length)
            {
                this.GrowToInclude(el);
            }
            this.bits[index] |= BitMask(el);
        }

        private static ulong BitMask(int bitNumber)
        {
            int num = bitNumber & MOD_MASK;
            return (((ulong) 1L) << num);
        }

        public virtual object Clone()
        {
            BitSet set;
            try
            {
                set = (BitSet) base.MemberwiseClone();
                set.bits = new ulong[this.bits.Length];
                Array.Copy(this.bits, 0, set.bits, 0, this.bits.Length);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Unable to clone BitSet", exception);
            }
            return set;
        }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is BitSet))
            {
                return false;
            }
            BitSet set = (BitSet) other;
            int num = Math.Min(this.bits.Length, set.bits.Length);
            for (int i = 0; i < num; i++)
            {
                if (this.bits[i] != set.bits[i])
                {
                    return false;
                }
            }
            if (this.bits.Length > num)
            {
                for (int j = num + 1; j < this.bits.Length; j++)
                {
                    if (this.bits[j] != 0L)
                    {
                        return false;
                    }
                }
            }
            else if (set.bits.Length > num)
            {
                for (int k = num + 1; k < set.bits.Length; k++)
                {
                    if (set.bits[k] != 0L)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual void GrowToInclude(int bit)
        {
            ulong[] destinationArray = new ulong[Math.Max(this.bits.Length << 1, this.NumWordsToHold(bit))];
            Array.Copy(this.bits, 0, destinationArray, 0, this.bits.Length);
            this.bits = destinationArray;
        }

        public virtual int LengthInLongWords()
        {
            return this.bits.Length;
        }

        public virtual bool Member(int el)
        {
            if (el < 0)
            {
                return false;
            }
            int index = WordNumber(el);
            if (index >= this.bits.Length)
            {
                return false;
            }
            return ((this.bits[index] & BitMask(el)) != 0L);
        }

        public virtual int NumBits()
        {
            return (this.bits.Length << 6);
        }

        private int NumWordsToHold(int el)
        {
            return ((el >> 6) + 1);
        }

        public static BitSet Of(int el)
        {
            BitSet set = new BitSet(el + 1);
            set.Add(el);
            return set;
        }

        public static BitSet Of(int a, int b)
        {
            BitSet set = new BitSet(Math.Max(a, b) + 1);
            set.Add(a);
            set.Add(b);
            return set;
        }

        public static BitSet Of(int a, int b, int c)
        {
            BitSet set = new BitSet();
            set.Add(a);
            set.Add(b);
            set.Add(c);
            return set;
        }

        public static BitSet Of(int a, int b, int c, int d)
        {
            BitSet set = new BitSet();
            set.Add(a);
            set.Add(b);
            set.Add(c);
            set.Add(d);
            return set;
        }

        public virtual BitSet Or(BitSet a)
        {
            if (a == null)
            {
                return this;
            }
            BitSet set = (BitSet) this.Clone();
            set.OrInPlace(a);
            return set;
        }

        public virtual void OrInPlace(BitSet a)
        {
            if (a != null)
            {
                if (a.bits.Length > this.bits.Length)
                {
                    this.SetSize(a.bits.Length);
                }
                for (int i = Math.Min(this.bits.Length, a.bits.Length) - 1; i >= 0; i--)
                {
                    this.bits[i] |= a.bits[i];
                }
            }
        }

        public virtual void Remove(int el)
        {
            int index = WordNumber(el);
            if (index < this.bits.Length)
            {
                this.bits[index] &= ~BitMask(el);
            }
        }

        private void SetSize(int nwords)
        {
            ulong[] destinationArray = new ulong[nwords];
            int length = Math.Min(nwords, this.bits.Length);
            Array.Copy(this.bits, 0, destinationArray, 0, length);
            this.bits = destinationArray;
        }

        public virtual int[] ToArray()
        {
            int[] numArray = new int[this.Count];
            int num = 0;
            for (int i = 0; i < (this.bits.Length << 6); i++)
            {
                if (this.Member(i))
                {
                    numArray[num++] = i;
                }
            }
            return numArray;
        }

        public virtual ulong[] ToPackedArray()
        {
            return this.bits;
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public virtual string ToString(string[] tokenNames)
        {
            StringBuilder builder = new StringBuilder();
            string str = ",";
            bool flag = false;
            builder.Append('{');
            for (int i = 0; i < (this.bits.Length << 6); i++)
            {
                if (this.Member(i))
                {
                    if ((i > 0) && flag)
                    {
                        builder.Append(str);
                    }
                    if (tokenNames != null)
                    {
                        builder.Append(tokenNames[i]);
                    }
                    else
                    {
                        builder.Append(i);
                    }
                    flag = true;
                }
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static int WordNumber(int bit)
        {
            return (bit >> 6);
        }

        public virtual int Count
        {
            get
            {
                int num = 0;
                for (int i = this.bits.Length - 1; i >= 0; i--)
                {
                    ulong num3 = this.bits[i];
                    if (num3 != 0L)
                    {
                        for (int j = 0x3f; j >= 0; j--)
                        {
                            if ((num3 & (((ulong) 1L) << j)) != 0L)
                            {
                                num++;
                            }
                        }
                    }
                }
                return num;
            }
        }

        public virtual bool Nil
        {
            get
            {
                for (int i = this.bits.Length - 1; i >= 0; i--)
                {
                    if (this.bits[i] != 0L)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

