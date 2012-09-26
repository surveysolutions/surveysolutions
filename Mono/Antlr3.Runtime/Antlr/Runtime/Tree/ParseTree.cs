namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;
    using System.Collections;
    using System.Text;

    public class ParseTree : BaseTree
    {
        public IList hiddenTokens;
        public object payload;

        public ParseTree(object label)
        {
            this.payload = label;
        }

        public void _ToStringLeaves(StringBuilder buf)
        {
            if (this.payload is IToken)
            {
                buf.Append(this.ToStringWithHiddenTokens());
            }
            else
            {
                for (int i = 0; (base.children != null) && (i < base.children.Count); i++)
                {
                    ((ParseTree) base.children[i])._ToStringLeaves(buf);
                }
            }
        }

        public override ITree DupNode()
        {
            return null;
        }

        public string ToInputString()
        {
            StringBuilder buf = new StringBuilder();
            this._ToStringLeaves(buf);
            return buf.ToString();
        }

        public override string ToString()
        {
            if (!(this.payload is IToken))
            {
                return this.payload.ToString();
            }
            IToken payload = (IToken) this.payload;
            if (payload.Type == Token.EOF)
            {
                return "<EOF>";
            }
            return payload.Text;
        }

        public string ToStringWithHiddenTokens()
        {
            StringBuilder builder = new StringBuilder();
            if (this.hiddenTokens != null)
            {
                for (int i = 0; i < this.hiddenTokens.Count; i++)
                {
                    IToken token = (IToken) this.hiddenTokens[i];
                    builder.Append(token.Text);
                }
            }
            string str = this.ToString();
            if (str != "<EOF>")
            {
                builder.Append(str);
            }
            return builder.ToString();
        }

        public override string Text
        {
            get
            {
                return this.ToString();
            }
        }

        public override int TokenStartIndex
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public override int TokenStopIndex
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public override int Type
        {
            get
            {
                return 0;
            }
        }
    }
}

