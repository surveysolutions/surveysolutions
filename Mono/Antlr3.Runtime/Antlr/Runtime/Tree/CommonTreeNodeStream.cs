namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using Antlr.Runtime.Collections;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public class CommonTreeNodeStream : ITreeNodeStream, IIntStream, IEnumerable
    {
        private ITreeAdaptor adaptor;
        protected StackList calls;
        public const int DEFAULT_INITIAL_BUFFER_SIZE = 100;
        protected object down;
        protected object eof;
        public const int INITIAL_CALL_STACK_SIZE = 10;
        protected int lastMarker;
        protected IList nodes;
        protected int p;
        protected internal object root;
        protected ITokenStream tokens;
        protected bool uniqueNavigationNodes;
        protected object up;

        public CommonTreeNodeStream(object tree) : this(new CommonTreeAdaptor(), tree)
        {
        }

        public CommonTreeNodeStream(ITreeAdaptor adaptor, object tree) : this(adaptor, tree, 100)
        {
        }

        public CommonTreeNodeStream(ITreeAdaptor adaptor, object tree, int initialBufferSize)
        {
            this.p = -1;
            this.root = tree;
            this.adaptor = adaptor;
            this.nodes = new List<object>(initialBufferSize);
            this.down = adaptor.Create(2, "DOWN");
            this.up = adaptor.Create(3, "UP");
            this.eof = adaptor.Create(Token.EOF, "EOF");
        }

        protected void AddNavigationNode(int ttype)
        {
            object down = null;
            if (ttype == 2)
            {
                if (this.HasUniqueNavigationNodes)
                {
                    down = this.adaptor.Create(2, "DOWN");
                }
                else
                {
                    down = this.down;
                }
            }
            else if (this.HasUniqueNavigationNodes)
            {
                down = this.adaptor.Create(3, "UP");
            }
            else
            {
                down = this.up;
            }
            this.nodes.Add(down);
        }

        public virtual void Consume()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            this.p++;
        }

        protected void FillBuffer()
        {
            this.FillBuffer(this.root);
            this.p = 0;
        }

        public void FillBuffer(object t)
        {
            bool flag = this.adaptor.IsNil(t);
            if (!flag)
            {
                this.nodes.Add(t);
            }
            int childCount = this.adaptor.GetChildCount(t);
            if (!flag && (childCount > 0))
            {
                this.AddNavigationNode(2);
            }
            for (int i = 0; i < childCount; i++)
            {
                object child = this.adaptor.GetChild(t, i);
                this.FillBuffer(child);
            }
            if (!flag && (childCount > 0))
            {
                this.AddNavigationNode(3);
            }
        }

        public object Get(int i)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            return this.nodes[i];
        }

        public IEnumerator GetEnumerator()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            return new CommonTreeNodeStreamEnumerator(this);
        }

        protected int GetNodeIndex(object node)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            for (int i = 0; i < this.nodes.Count; i++)
            {
                object obj2 = this.nodes[i];
                if (obj2 == node)
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual int Index()
        {
            return this.p;
        }

        public virtual int LA(int i)
        {
            return this.adaptor.GetNodeType(this.LT(i));
        }

        protected object LB(int k)
        {
            if (k == 0)
            {
                return null;
            }
            if ((this.p - k) < 0)
            {
                return null;
            }
            return this.nodes[this.p - k];
        }

        public object LT(int k)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            if (k == 0)
            {
                return null;
            }
            if (k < 0)
            {
                return this.LB(-k);
            }
            if (((this.p + k) - 1) >= this.nodes.Count)
            {
                return this.eof;
            }
            return this.nodes[(this.p + k) - 1];
        }

        public virtual int Mark()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            this.lastMarker = this.Index();
            return this.lastMarker;
        }

        public int Pop()
        {
            int index = (int) this.calls.Pop();
            this.Seek(index);
            return index;
        }

        public void Push(int index)
        {
            if (this.calls == null)
            {
                this.calls = new StackList();
            }
            this.calls.Push(this.p);
            this.Seek(index);
        }

        public virtual void Release(int marker)
        {
        }

        public void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t)
        {
            if (parent != null)
            {
                this.adaptor.ReplaceChildren(parent, startChildIndex, stopChildIndex, t);
            }
        }

        public void Reset()
        {
            this.p = -1;
            this.lastMarker = 0;
            if (this.calls != null)
            {
                this.calls.Clear();
            }
        }

        public void Rewind()
        {
            this.Seek(this.lastMarker);
        }

        public virtual void Rewind(int marker)
        {
            this.Seek(marker);
        }

        public virtual void Seek(int index)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            this.p = index;
        }

        [Obsolete("Please use property Count instead.")]
        public virtual int Size()
        {
            return this.Count;
        }

        public override string ToString()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.nodes.Count; i++)
            {
                object t = this.nodes[i];
                builder.Append(" ");
                builder.Append(this.adaptor.GetNodeType(t));
            }
            return builder.ToString();
        }

        public virtual string ToString(object start, object stop)
        {
            string nodeText;
            Console.Out.WriteLine("ToString");
            if ((start == null) || (stop == null))
            {
                return null;
            }
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            if (start is CommonTree)
            {
                Console.Out.Write("ToString: " + ((CommonTree) start).Token + ", ");
            }
            else
            {
                Console.Out.WriteLine(start);
            }
            if (stop is CommonTree)
            {
                Console.Out.WriteLine(((CommonTree) stop).Token);
            }
            else
            {
                Console.Out.WriteLine(stop);
            }
            if (this.tokens != null)
            {
                int tokenStartIndex = this.adaptor.GetTokenStartIndex(start);
                int tokenStopIndex = this.adaptor.GetTokenStopIndex(stop);
                if (this.adaptor.GetNodeType(stop) == 3)
                {
                    tokenStopIndex = this.adaptor.GetTokenStopIndex(start);
                }
                else if (this.adaptor.GetNodeType(stop) == Token.EOF)
                {
                    tokenStopIndex = this.Count - 2;
                }
                return this.tokens.ToString(tokenStartIndex, tokenStopIndex);
            }
            object t = null;
            int num3 = 0;
            while (num3 < this.nodes.Count)
            {
                t = this.nodes[num3];
                if (t == start)
                {
                    break;
                }
                num3++;
            }
            StringBuilder builder = new StringBuilder();
            for (t = this.nodes[num3]; t != stop; t = this.nodes[num3])
            {
                nodeText = this.adaptor.GetNodeText(t);
                if (nodeText == null)
                {
                    nodeText = " " + this.adaptor.GetNodeType(t);
                }
                builder.Append(nodeText);
                num3++;
            }
            nodeText = this.adaptor.GetNodeText(stop);
            if (nodeText == null)
            {
                nodeText = " " + this.adaptor.GetNodeType(stop);
            }
            builder.Append(nodeText);
            return builder.ToString();
        }

        public string ToTokenString(int start, int stop)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            StringBuilder builder = new StringBuilder();
            for (int i = start; (i < this.nodes.Count) && (i <= stop); i++)
            {
                object treeNode = this.nodes[i];
                builder.Append(" ");
                builder.Append(this.adaptor.GetToken(treeNode));
            }
            return builder.ToString();
        }

        public virtual int Count
        {
            get
            {
                if (this.p == -1)
                {
                    this.FillBuffer();
                }
                return this.nodes.Count;
            }
        }

        public virtual object CurrentSymbol
        {
            get
            {
                return this.LT(1);
            }
        }

        public bool HasUniqueNavigationNodes
        {
            get
            {
                return this.uniqueNavigationNodes;
            }
            set
            {
                this.uniqueNavigationNodes = value;
            }
        }

        public virtual string SourceName
        {
            get
            {
                return this.TokenStream.SourceName;
            }
        }

        public virtual ITokenStream TokenStream
        {
            get
            {
                return this.tokens;
            }
            set
            {
                this.tokens = value;
            }
        }

        public ITreeAdaptor TreeAdaptor
        {
            get
            {
                return this.adaptor;
            }
            set
            {
                this.adaptor = value;
            }
        }

        public virtual object TreeSource
        {
            get
            {
                return this.root;
            }
        }

        protected sealed class CommonTreeNodeStreamEnumerator : IEnumerator
        {
            private object _currentItem;
            private int _index;
            private CommonTreeNodeStream _nodeStream;

            internal CommonTreeNodeStreamEnumerator()
            {
            }

            internal CommonTreeNodeStreamEnumerator(CommonTreeNodeStream nodeStream)
            {
                this._nodeStream = nodeStream;
                this.Reset();
            }

            public bool MoveNext()
            {
                if (this._index >= this._nodeStream.nodes.Count)
                {
                    int num = this._index;
                    this._index++;
                    if (num < this._nodeStream.nodes.Count)
                    {
                        this._currentItem = this._nodeStream.nodes[num];
                    }
                    this._currentItem = this._nodeStream.eof;
                    return true;
                }
                this._currentItem = null;
                return false;
            }

            public void Reset()
            {
                this._index = 0;
                this._currentItem = null;
            }

            public object Current
            {
                get
                {
                    if (this._currentItem == null)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return this._currentItem;
                }
            }
        }
    }
}

