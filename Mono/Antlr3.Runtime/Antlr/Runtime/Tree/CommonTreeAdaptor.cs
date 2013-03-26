namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    public class CommonTreeAdaptor : BaseTreeAdaptor
    {
        public override object Create(IToken payload)
        {
            return new CommonTree(payload);
        }

        public override IToken CreateToken(IToken fromToken)
        {
            return new CommonToken(fromToken);
        }

        public override IToken CreateToken(int tokenType, string text)
        {
            return new CommonToken(tokenType, text);
        }

        public override object DupNode(object t)
        {
            if (t == null)
            {
                return null;
            }
            return ((ITree) t).DupNode();
        }

        public override object GetChild(object t, int i)
        {
            if (t == null)
            {
                return null;
            }
            return ((ITree) t).GetChild(i);
        }

        public override int GetChildCount(object t)
        {
            if (t == null)
            {
                return 0;
            }
            return ((ITree) t).ChildCount;
        }

        public override int GetChildIndex(object t)
        {
            if (t == null)
            {
                return 0;
            }
            return ((ITree) t).ChildIndex;
        }

        public override string GetNodeText(object t)
        {
            if (t == null)
            {
                return null;
            }
            return ((ITree) t).Text;
        }

        public override int GetNodeType(object t)
        {
            if (t == null)
            {
                return 0;
            }
            return ((ITree) t).Type;
        }

        public override object GetParent(object t)
        {
            if (t == null)
            {
                return null;
            }
            return ((ITree) t).Parent;
        }

        public override IToken GetToken(object treeNode)
        {
            if (treeNode is CommonTree)
            {
                return ((CommonTree) treeNode).Token;
            }
            return null;
        }

        public override int GetTokenStartIndex(object t)
        {
            if (t == null)
            {
                return -1;
            }
            return ((ITree) t).TokenStartIndex;
        }

        public override int GetTokenStopIndex(object t)
        {
            if (t == null)
            {
                return -1;
            }
            return ((ITree) t).TokenStopIndex;
        }

        public override void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t)
        {
            if (parent != null)
            {
                ((ITree) parent).ReplaceChildren(startChildIndex, stopChildIndex, t);
            }
        }

        public override void SetChildIndex(object t, int index)
        {
            if (t == null)
            {
                ((ITree) t).ChildIndex = index;
            }
        }

        public override void SetParent(object t, object parent)
        {
            if (t == null)
            {
                ((ITree) t).Parent = (ITree) parent;
            }
        }

        public override void SetTokenBoundaries(object t, IToken startToken, IToken stopToken)
        {
            if (t != null)
            {
                int tokenIndex = 0;
                int num2 = 0;
                if (startToken != null)
                {
                    tokenIndex = startToken.TokenIndex;
                }
                if (stopToken != null)
                {
                    num2 = stopToken.TokenIndex;
                }
                ((ITree) t).TokenStartIndex = tokenIndex;
                ((ITree) t).TokenStopIndex = num2;
            }
        }
    }
}

