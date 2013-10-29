namespace Antlr.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ANTLRStringStream : ICharStream, IIntStream
    {
        protected internal int charPositionInLine;
        protected internal char[] data;
        protected int lastMarker;
        protected internal int line;
        protected internal int markDepth;
        protected internal IList markers;
        protected int n;
        protected string name;
        protected internal int p;

        protected ANTLRStringStream()
        {
            this.line = 1;
        }

        public ANTLRStringStream(string input)
        {
            this.line = 1;
            this.data = input.ToCharArray();
            this.n = input.Length;
        }

        public ANTLRStringStream(char[] data, int numberOfActualCharsInArray)
        {
            this.line = 1;
            this.data = data;
            this.n = numberOfActualCharsInArray;
        }

        public virtual void Consume()
        {
            if (this.p < this.n)
            {
                this.charPositionInLine++;
                if (this.data[this.p] == '\n')
                {
                    this.line++;
                    this.charPositionInLine = 0;
                }
                this.p++;
            }
        }

        public virtual int Index()
        {
            return this.p;
        }

        public virtual int LA(int i)
        {
            if (i == 0)
            {
                return 0;
            }
            if (i < 0)
            {
                i++;
                if (((this.p + i) - 1) < 0)
                {
                    return -1;
                }
            }
            if (((this.p + i) - 1) >= this.n)
            {
                return -1;
            }
            return this.data[(this.p + i) - 1];
        }

        public virtual int LT(int i)
        {
            return this.LA(i);
        }

        public virtual int Mark()
        {
            if (this.markers == null)
            {
                this.markers = new List<object>();
                this.markers.Add(null);
            }
            this.markDepth++;
            CharStreamState state = null;
            if (this.markDepth >= this.markers.Count)
            {
                state = new CharStreamState();
                this.markers.Add(state);
            }
            else
            {
                state = (CharStreamState) this.markers[this.markDepth];
            }
            state.p = this.p;
            state.line = this.line;
            state.charPositionInLine = this.charPositionInLine;
            this.lastMarker = this.markDepth;
            return this.markDepth;
        }

        public virtual void Release(int marker)
        {
            this.markDepth = marker;
            this.markDepth--;
        }

        public virtual void Reset()
        {
            this.p = 0;
            this.line = 1;
            this.charPositionInLine = 0;
            this.markDepth = 0;
        }

        public virtual void Rewind()
        {
            this.Rewind(this.lastMarker);
        }

        public virtual void Rewind(int m)
        {
            CharStreamState state = (CharStreamState) this.markers[m];
            this.Seek(state.p);
            this.line = state.line;
            this.charPositionInLine = state.charPositionInLine;
            this.Release(m);
        }

        public virtual void Seek(int index)
        {
            if (index <= this.p)
            {
                this.p = index;
            }
            else
            {
                while (this.p < index)
                {
                    this.Consume();
                }
            }
        }

        [Obsolete("Please use property Count instead.")]
        public virtual int Size()
        {
            return this.Count;
        }

        public virtual string Substring(int start, int stop)
        {
            return new string(this.data, start, (stop - start) + 1);
        }

        public virtual int CharPositionInLine
        {
            get
            {
                return this.charPositionInLine;
            }
            set
            {
                this.charPositionInLine = value;
            }
        }

        public virtual int Count
        {
            get
            {
                return this.n;
            }
        }

        public virtual int Line
        {
            get
            {
                return this.line;
            }
            set
            {
                this.line = value;
            }
        }

        public virtual string SourceName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

