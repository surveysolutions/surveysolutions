namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class CommonToken : IToken
    {
        protected internal int channel;
        protected internal int charPositionInLine;
        protected internal int index;
        [NonSerialized]
        protected internal ICharStream input;
        protected internal int line;
        protected internal int start;
        protected internal int stop;
        protected internal string text;
        protected internal int type;

        public CommonToken(IToken oldToken)
        {
            this.charPositionInLine = -1;
            this.index = -1;
            this.text = oldToken.Text;
            this.type = oldToken.Type;
            this.line = oldToken.Line;
            this.index = oldToken.TokenIndex;
            this.charPositionInLine = oldToken.CharPositionInLine;
            this.channel = oldToken.Channel;
            if (oldToken is CommonToken)
            {
                this.start = ((CommonToken) oldToken).start;
                this.stop = ((CommonToken) oldToken).stop;
            }
        }

        public CommonToken(int type)
        {
            this.charPositionInLine = -1;
            this.index = -1;
            this.type = type;
        }

        public CommonToken(int type, string text)
        {
            this.charPositionInLine = -1;
            this.index = -1;
            this.type = type;
            this.channel = 0;
            this.text = text;
        }

        public CommonToken(ICharStream input, int type, int channel, int start, int stop)
        {
            this.charPositionInLine = -1;
            this.index = -1;
            this.input = input;
            this.type = type;
            this.channel = channel;
            this.start = start;
            this.stop = stop;
        }

        public override string ToString()
        {
            string str = "";
            if (this.channel > 0)
            {
                str = ",channel=" + this.channel;
            }
            string text = this.Text;
            if (text != null)
            {
                text = text.Replace("\n", @"\\n").Replace("\r", @"\\r").Replace("\t", @"\\t");
            }
            else
            {
                text = "<no text>";
            }
            return string.Concat(new object[] { 
                "[@", this.TokenIndex, ",", this.start, ":", this.stop, "='", text, "',<", this.type, ">", str, ",", this.line, ":", this.CharPositionInLine, 
                "]"
             });
        }

        public virtual int Channel
        {
            get
            {
                return this.channel;
            }
            set
            {
                this.channel = value;
            }
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

        public virtual ICharStream InputStream
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
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

        public virtual int StartIndex
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }

        public virtual int StopIndex
        {
            get
            {
                return this.stop;
            }
            set
            {
                this.stop = value;
            }
        }

        public virtual string Text
        {
            get
            {
                if (this.text == null)
                {
                    if (this.input == null)
                    {
                        return null;
                    }
                    this.text = this.input.Substring(this.start, this.stop);
                }
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        public virtual int TokenIndex
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        public virtual int Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

