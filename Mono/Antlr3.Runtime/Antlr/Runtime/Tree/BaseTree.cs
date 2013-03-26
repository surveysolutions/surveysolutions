namespace Antlr.Runtime.Tree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public abstract class BaseTree : ITree
    {
        protected IList children;

        public BaseTree()
        {
        }

        public BaseTree(ITree node)
        {
        }

        public virtual void AddChild(ITree t)
        {
            if (t != null)
            {
                BaseTree tree = (BaseTree) t;
                if (tree.IsNil)
                {
                    if ((this.children != null) && (this.children == tree.children))
                    {
                        throw new InvalidOperationException("attempt to add child list to itself");
                    }
                    if (tree.children != null)
                    {
                        if (this.children != null)
                        {
                            int count = tree.children.Count;
                            for (int i = 0; i < count; i++)
                            {
                                ITree tree2 = (ITree) tree.Children[i];
                                this.children.Add(tree2);
                                tree2.Parent = this;
                                tree2.ChildIndex = this.children.Count - 1;
                            }
                        }
                        else
                        {
                            this.children = tree.children;
                            this.FreshenParentAndChildIndexes();
                        }
                    }
                }
                else
                {
                    if (this.children == null)
                    {
                        this.children = this.CreateChildrenList();
                    }
                    this.children.Add(t);
                    tree.Parent = this;
                    tree.ChildIndex = this.children.Count - 1;
                }
            }
        }

        public void AddChildren(IList kids)
        {
            for (int i = 0; i < kids.Count; i++)
            {
                ITree t = (ITree) kids[i];
                this.AddChild(t);
            }
        }

        protected internal virtual IList CreateChildrenList()
        {
            return new List<object>();
        }

        public virtual object DeleteChild(int i)
        {
            if (this.children == null)
            {
                return null;
            }
            ITree tree = (ITree) this.children[i];
            this.children.RemoveAt(i);
            this.FreshenParentAndChildIndexes(i);
            return tree;
        }

        public abstract ITree DupNode();
        public virtual void FreshenParentAndChildIndexes()
        {
            this.FreshenParentAndChildIndexes(0);
        }

        public virtual void FreshenParentAndChildIndexes(int offset)
        {
            int childCount = this.ChildCount;
            for (int i = offset; i < childCount; i++)
            {
                ITree child = this.GetChild(i);
                child.ChildIndex = i;
                child.Parent = this;
            }
        }

        public ITree GetAncestor(int ttype)
        {
            ITree tree = this;
            for (tree = tree.Parent; tree != null; tree = tree.Parent)
            {
                if (tree.Type == ttype)
                {
                    return tree;
                }
            }
            return null;
        }

        public IList GetAncestors()
        {
            if (this.Parent == null)
            {
                return null;
            }
            IList list = new List<object>();
            ITree tree = this;
            for (tree = tree.Parent; tree != null; tree = tree.Parent)
            {
                list.Insert(0, tree);
            }
            return list;
        }

        public virtual ITree GetChild(int i)
        {
            if ((this.children != null) && (i < this.children.Count))
            {
                return (ITree) this.children[i];
            }
            return null;
        }

        public bool HasAncestor(int ttype)
        {
            return (this.GetAncestor(ttype) != null);
        }

        public virtual void ReplaceChildren(int startChildIndex, int stopChildIndex, object t)
        {
            IList children;
            if (this.children == null)
            {
                throw new ArgumentException("indexes invalid; no children in list");
            }
            int num = (stopChildIndex - startChildIndex) + 1;
            BaseTree tree = (BaseTree) t;
            if (tree.IsNil)
            {
                children = tree.Children;
            }
            else
            {
                children = new List<object>(1);
                children.Add(tree);
            }
            int count = children.Count;
            int num3 = children.Count;
            int num4 = num - count;
            if (num4 == 0)
            {
                int num5 = 0;
                for (int i = startChildIndex; i <= stopChildIndex; i++)
                {
                    BaseTree tree2 = (BaseTree) children[num5];
                    this.children[i] = tree2;
                    tree2.Parent = this;
                    tree2.ChildIndex = i;
                    num5++;
                }
            }
            else if (num4 > 0)
            {
                for (int j = 0; j < num3; j++)
                {
                    this.children[startChildIndex + j] = children[j];
                }
                int index = startChildIndex + num3;
                for (int k = index; k <= stopChildIndex; k++)
                {
                    this.children.RemoveAt(index);
                }
                this.FreshenParentAndChildIndexes(startChildIndex);
            }
            else
            {
                int num10 = 0;
                while (num10 < num)
                {
                    this.children[startChildIndex + num10] = children[num10];
                    num10++;
                }
                while (num10 < count)
                {
                    this.children.Insert(startChildIndex + num10, children[num10]);
                    num10++;
                }
                this.FreshenParentAndChildIndexes(startChildIndex);
            }
        }

        public virtual void SanityCheckParentAndChildIndexes()
        {
            this.SanityCheckParentAndChildIndexes(null, -1);
        }

        public virtual void SanityCheckParentAndChildIndexes(ITree parent, int i)
        {
            if (parent != this.Parent)
            {
                throw new ArgumentException(string.Concat(new object[] { "parents don't match; expected ", parent, " found ", this.Parent }));
            }
            if (i != this.ChildIndex)
            {
                throw new NotSupportedException(string.Concat(new object[] { "child indexes don't match; expected ", i, " found ", this.ChildIndex }));
            }
            int childCount = this.ChildCount;
            for (int j = 0; j < childCount; j++)
            {
                ((CommonTree) this.GetChild(j)).SanityCheckParentAndChildIndexes(this, j);
            }
        }

        public virtual void SetChild(int i, ITree t)
        {
            if (t != null)
            {
                if (t.IsNil)
                {
                    throw new ArgumentException("Can't set single child to a list");
                }
                if (this.children == null)
                {
                    this.children = this.CreateChildrenList();
                }
                this.children[i] = t;
                t.Parent = this;
                t.ChildIndex = i;
            }
        }

        public abstract override string ToString();
        public virtual string ToStringTree()
        {
            if ((this.children == null) || (this.children.Count == 0))
            {
                return this.ToString();
            }
            StringBuilder builder = new StringBuilder();
            if (!this.IsNil)
            {
                builder.Append("(");
                builder.Append(this.ToString());
                builder.Append(' ');
            }
            for (int i = 0; (this.children != null) && (i < this.children.Count); i++)
            {
                ITree tree = (ITree) this.children[i];
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(tree.ToStringTree());
            }
            if (!this.IsNil)
            {
                builder.Append(")");
            }
            return builder.ToString();
        }

        public virtual int CharPositionInLine
        {
            get
            {
                return 0;
            }
        }

        public virtual int ChildCount
        {
            get
            {
                if (this.children == null)
                {
                    return 0;
                }
                return this.children.Count;
            }
        }

        public virtual int ChildIndex
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public IList Children
        {
            get
            {
                return this.children;
            }
        }

        public virtual bool IsNil
        {
            get
            {
                return false;
            }
        }

        public virtual int Line
        {
            get
            {
                return 0;
            }
        }

        public virtual ITree Parent
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public abstract string Text { get; }

        public abstract int TokenStartIndex { get; set; }

        public abstract int TokenStopIndex { get; set; }

        public abstract int Type { get; }
    }
}

