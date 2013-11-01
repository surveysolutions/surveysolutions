namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class MissingTokenException : MismatchedTokenException
    {
        private object inserted;

        public MissingTokenException()
        {
        }

        public MissingTokenException(int expecting, IIntStream input, object inserted) : base(expecting, input)
        {
            this.inserted = inserted;
        }

        public override string ToString()
        {
            if ((this.inserted != null) && (base.token != null))
            {
                return string.Concat(new object[] { "MissingTokenException(inserted ", this.inserted, " at ", base.token.Text, ")" });
            }
            if (base.token != null)
            {
                return ("MissingTokenException(at " + base.token.Text + ")");
            }
            return "MissingTokenException";
        }

        public object Inserted
        {
            get
            {
                return this.inserted;
            }
            set
            {
                this.inserted = value;
            }
        }

        public int MissingType
        {
            get
            {
                return base.Expecting;
            }
        }
    }
}

