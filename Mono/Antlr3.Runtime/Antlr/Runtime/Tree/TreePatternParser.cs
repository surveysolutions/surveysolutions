namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    public class TreePatternParser
    {
        protected ITreeAdaptor adaptor;
        protected TreePatternLexer tokenizer;
        protected int ttype;
        protected TreeWizard wizard;

        public TreePatternParser(TreePatternLexer tokenizer, TreeWizard wizard, ITreeAdaptor adaptor)
        {
            this.tokenizer = tokenizer;
            this.wizard = wizard;
            this.adaptor = adaptor;
            this.ttype = tokenizer.NextToken();
        }

        public object ParseNode()
        {
            string str = null;
            if (this.ttype == 5)
            {
                this.ttype = this.tokenizer.NextToken();
                if (this.ttype != 3)
                {
                    return null;
                }
                str = this.tokenizer.sval.ToString();
                this.ttype = this.tokenizer.NextToken();
                if (this.ttype != 6)
                {
                    return null;
                }
                this.ttype = this.tokenizer.NextToken();
            }
            if (this.ttype == 7)
            {
                this.ttype = this.tokenizer.NextToken();
                IToken payload = new CommonToken(0, ".");
                TreeWizard.TreePattern pattern = new TreeWizard.WildcardTreePattern(payload);
                if (str != null)
                {
                    pattern.label = str;
                }
                return pattern;
            }
            if (this.ttype != 3)
            {
                return null;
            }
            string tokenName = this.tokenizer.sval.ToString();
            this.ttype = this.tokenizer.NextToken();
            if (tokenName.Equals("nil"))
            {
                return this.adaptor.GetNilNode();
            }
            string text = tokenName;
            string str4 = null;
            if (this.ttype == 4)
            {
                str4 = this.tokenizer.sval.ToString();
                text = str4;
                this.ttype = this.tokenizer.NextToken();
            }
            int tokenType = this.wizard.GetTokenType(tokenName);
            if (tokenType == 0)
            {
                return null;
            }
            object obj2 = this.adaptor.Create(tokenType, text);
            if ((str != null) && (obj2.GetType() == typeof(TreeWizard.TreePattern)))
            {
                ((TreeWizard.TreePattern) obj2).label = str;
            }
            if ((str4 != null) && (obj2.GetType() == typeof(TreeWizard.TreePattern)))
            {
                ((TreeWizard.TreePattern) obj2).hasTextArg = true;
            }
            return obj2;
        }

        public object ParseTree()
        {
            if (this.ttype != 1)
            {
                Console.Out.WriteLine("no BEGIN");
                return null;
            }
            this.ttype = this.tokenizer.NextToken();
            object t = this.ParseNode();
            if (t != null)
            {
                while (((this.ttype == 1) || (this.ttype == 3)) || ((this.ttype == 5) || (this.ttype == 7)))
                {
                    if (this.ttype == 1)
                    {
                        object child = this.ParseTree();
                        this.adaptor.AddChild(t, child);
                    }
                    else
                    {
                        object obj4 = this.ParseNode();
                        if (obj4 == null)
                        {
                            return null;
                        }
                        this.adaptor.AddChild(t, obj4);
                    }
                }
                if (this.ttype != 2)
                {
                    Console.Out.WriteLine("no END");
                    return null;
                }
                this.ttype = this.tokenizer.NextToken();
                return t;
            }
            return null;
        }

        public object Pattern()
        {
            if (this.ttype == 1)
            {
                return this.ParseTree();
            }
            if (this.ttype == 3)
            {
                object obj2 = this.ParseNode();
                if (this.ttype == -1)
                {
                    return obj2;
                }
            }
            return null;
        }
    }
}

