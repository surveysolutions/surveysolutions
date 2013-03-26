namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class MismatchedRangeException : RecognitionException
    {
        private int a;
        private int b;

        public MismatchedRangeException()
        {
        }

        public MismatchedRangeException(int a, int b, IIntStream input) : base(input)
        {
            this.a = a;
            this.b = b;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "MismatchedNotSetException(", this.UnexpectedType, " not in [", this.a, ",", this.b, "])" });
        }

        public int A
        {
            get
            {
                return this.a;
            }
            set
            {
                this.a = value;
            }
        }

        public int B
        {
            get
            {
                return this.b;
            }
            set
            {
                this.b = value;
            }
        }
    }
}

