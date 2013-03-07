namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;
    using System.Collections;

    public abstract class BaseTreeAdaptor : ITreeAdaptor
    {
        protected IDictionary treeToUniqueIDMap;
        protected int uniqueNodeID = 1;

        protected BaseTreeAdaptor()
        {
        }

        public virtual void AddChild(object t, object child)
        {
            if ((t != null) && (child != null))
            {
                ((ITree) t).AddChild((ITree) child);
            }
        }

        public virtual object BecomeRoot(IToken newRoot, object oldRoot)
        {
            return this.BecomeRoot(this.Create(newRoot), oldRoot);
        }

        public virtual object BecomeRoot(object newRoot, object oldRoot)
        {
            ITree child = (ITree) newRoot;
            ITree t = (ITree) oldRoot;
            if (oldRoot == null)
            {
                return newRoot;
            }
            if (child.IsNil)
            {
                int childCount = child.ChildCount;
                if (childCount == 1)
                {
                    child = child.GetChild(0);
                }
                else if (childCount > 1)
                {
                    throw new SystemException("more than one node as root (TODO: make exception hierarchy)");
                }
            }
            child.AddChild(t);
            return child;
        }

        public abstract object Create(IToken param1);
        public virtual object Create(int tokenType, IToken fromToken)
        {
            fromToken = this.CreateToken(fromToken);
            fromToken.Type = tokenType;
            return (ITree) this.Create(fromToken);
        }

        public virtual object Create(int tokenType, string text)
        {
            IToken token = this.CreateToken(tokenType, text);
            return (ITree) this.Create(token);
        }

        public virtual object Create(int tokenType, IToken fromToken, string text)
        {
            fromToken = this.CreateToken(fromToken);
            fromToken.Type = tokenType;
            fromToken.Text = text;
            return (ITree) this.Create(fromToken);
        }

        public abstract IToken CreateToken(IToken fromToken);
        public abstract IToken CreateToken(int tokenType, string text);
        public virtual object DeleteChild(object t, int i)
        {
            return ((ITree) t).DeleteChild(i);
        }

        public abstract object DupNode(object param1);
        public virtual object DupTree(object tree)
        {
            return this.DupTree(tree, null);
        }

        public virtual object DupTree(object t, object parent)
        {
            if (t == null)
            {
                return null;
            }
            object obj2 = this.DupNode(t);
            this.SetChildIndex(obj2, this.GetChildIndex(t));
            this.SetParent(obj2, parent);
            int childCount = this.GetChildCount(t);
            for (int i = 0; i < childCount; i++)
            {
                object child = this.GetChild(t, i);
                object obj4 = this.DupTree(child, t);
                this.AddChild(obj2, obj4);
            }
            return obj2;
        }

        public virtual object ErrorNode(ITokenStream input, IToken start, IToken stop, RecognitionException e)
        {
            return new CommonErrorNode(input, start, stop, e);
        }

        public virtual object GetChild(object t, int i)
        {
            return ((ITree) t).GetChild(i);
        }

        public virtual int GetChildCount(object t)
        {
            return ((ITree) t).ChildCount;
        }

        public abstract int GetChildIndex(object t);
        public virtual object GetNilNode()
        {
            return this.Create(null);
        }

        public virtual string GetNodeText(object t)
        {
            return ((ITree) t).Text;
        }

        public virtual int GetNodeType(object t)
        {
            return ((ITree) t).Type;
        }

        public abstract object GetParent(object t);
        public abstract IToken GetToken(object treeNode);
        public abstract int GetTokenStartIndex(object t);
        public abstract int GetTokenStopIndex(object t);
        public int GetUniqueID(object node)
        {
            if (this.treeToUniqueIDMap == null)
            {
                this.treeToUniqueIDMap = new Hashtable();
            }
            object obj2 = this.treeToUniqueIDMap[node];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int uniqueNodeID = this.uniqueNodeID;
            this.treeToUniqueIDMap[node] = uniqueNodeID;
            this.uniqueNodeID++;
            return uniqueNodeID;
        }

        public virtual bool IsNil(object tree)
        {
            return ((ITree) tree).IsNil;
        }

        public abstract void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t);
        public virtual object RulePostProcessing(object root)
        {
            ITree child = (ITree) root;
            if ((child != null) && child.IsNil)
            {
                if (child.ChildCount == 0)
                {
                    return null;
                }
                if (child.ChildCount == 1)
                {
                    child = child.GetChild(0);
                    child.Parent = null;
                    child.ChildIndex = -1;
                }
            }
            return child;
        }

        public virtual void SetChild(object t, int i, object child)
        {
            ((ITree) t).SetChild(i, (ITree) child);
        }

        public abstract void SetChildIndex(object t, int index);
        public virtual void SetNodeText(object t, string text)
        {
            throw new NotImplementedException("don't know enough about Tree node");
        }

        public virtual void SetNodeType(object t, int type)
        {
            throw new NotImplementedException("don't know enough about Tree node");
        }

        public abstract void SetParent(object t, object parent);
        public abstract void SetTokenBoundaries(object param1, IToken param2, IToken param3);
    }
}

