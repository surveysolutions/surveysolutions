namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    [Serializable]
    public class CommonTree : BaseTree
    {
        public int childIndex;
        public CommonTree parent;
        public int startIndex;
        public int stopIndex;
        protected IToken token;

        public CommonTree()
        {
            this.startIndex = -1;
            this.stopIndex = -1;
            this.childIndex = -1;
        }

        public CommonTree(IToken t)
        {
            this.startIndex = -1;
            this.stopIndex = -1;
            this.childIndex = -1;
            this.token = t;
        }

        public CommonTree(CommonTree node) : base(node)
        {
            this.startIndex = -1;
            this.stopIndex = -1;
            this.childIndex = -1;
            this.token = node.token;
            this.startIndex = node.startIndex;
            this.stopIndex = node.stopIndex;
        }

        public override ITree DupNode()
        {
            return new CommonTree(this);
        }

        public void SetUnknownTokenBoundaries()
        {
            if (base.children == null)
            {
                if ((this.startIndex < 0) || (this.stopIndex < 0))
                {
                    this.startIndex = this.stopIndex = this.token.TokenIndex;
                }
            }
            else
            {
                for (int i = 0; i < base.children.Count; i++)
                {
                    ((CommonTree) base.children[i]).SetUnknownTokenBoundaries();
                }
                if (((this.startIndex < 0) || (this.stopIndex < 0)) && (base.children.Count > 0))
                {
                    CommonTree tree = (CommonTree) base.children[0];
                    CommonTree tree2 = (CommonTree) base.children[base.children.Count - 1];
                    this.startIndex = tree.TokenStartIndex;
                    this.stopIndex = tree2.TokenStopIndex;
                }
            }
        }

        public override string ToString()
        {
            if (this.IsNil)
            {
                return "nil";
            }
            if (this.Type == 0)
            {
                return "<errornode>";
            }
            if (this.token == null)
            {
                return null;
            }
            return this.token.Text;
        }

        public override int CharPositionInLine
        {
            get
            {
                if ((this.token != null) && (this.token.CharPositionInLine != -1))
                {
                    return this.token.CharPositionInLine;
                }
                if (this.ChildCount > 0)
                {
                    return this.GetChild(0).CharPositionInLine;
                }
                return 0;
            }
        }

        public override int ChildIndex
        {
            get
            {
                return this.childIndex;
            }
            set
            {
                this.childIndex = value;
            }
        }

        public override bool IsNil
        {
            get
            {
                return (this.token == null);
            }
        }

        public override int Line
        {
            get
            {
                if ((this.token != null) && (this.token.Line != 0))
                {
                    return this.token.Line;
                }
                if (this.ChildCount > 0)
                {
                    return this.GetChild(0).Line;
                }
                return 0;
            }
        }

        public override ITree Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = (CommonTree) value;
            }
        }

        public override string Text
        {
            get
            {
                if (this.token == null)
                {
                    return null;
                }
                return this.token.Text;
            }
        }

        public virtual IToken Token
        {
            get
            {
                return this.token;
            }
        }

        public override int TokenStartIndex
        {
            get
            {
                if ((this.startIndex == -1) && (this.token != null))
                {
                    return this.token.TokenIndex;
                }
                return this.startIndex;
            }
            set
            {
                this.startIndex = value;
            }
        }

        public override int TokenStopIndex
        {
            get
            {
                if ((this.stopIndex == -1) && (this.token != null))
                {
                    return this.token.TokenIndex;
                }
                return this.stopIndex;
            }
            set
            {
                this.stopIndex = value;
            }
        }

        public override int Type
        {
            get
            {
                if (this.token == null)
                {
                    return 0;
                }
                return this.token.Type;
            }
        }
    }
}

