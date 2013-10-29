namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class ClassicToken : IToken
    {
        protected internal int channel;
        protected internal int charPositionInLine;
        protected internal int index;
        protected internal int line;
        protected internal string text;
        protected internal int type;

        public ClassicToken(IToken oldToken)
        {
            this.text = oldToken.Text;
            this.type = oldToken.Type;
            this.line = oldToken.Line;
            this.charPositionInLine = oldToken.CharPositionInLine;
            this.channel = oldToken.Channel;
        }

        public ClassicToken(int type)
        {
            this.type = type;
        }

        public ClassicToken(int type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public ClassicToken(int type, string text, int channel)
        {
            this.type = type;
            this.text = text;
            this.channel = channel;
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
            return string.Concat(new object[] { "[@", this.TokenIndex, ",'", text, "',<", this.type, ">", str, ",", this.line, ":", this.CharPositionInLine, "]" });
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
                return null;
            }
            set
            {
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

        public virtual string Text
        {
            get
            {
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

