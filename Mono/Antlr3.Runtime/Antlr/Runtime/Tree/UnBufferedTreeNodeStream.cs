namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using Antlr.Runtime.Collections;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public class UnBufferedTreeNodeStream : ITreeNodeStream, IIntStream
    {
        protected int absoluteNodeIndex;
        private ITreeAdaptor adaptor;
        protected internal int currentChildIndex;
        private ITree currentEnumerationNode;
        protected internal object currentNode;
        protected object down;
        protected object eof;
        protected internal int head;
        protected internal StackList indexStack;
        public const int INITIAL_LOOKAHEAD_BUFFER_SIZE = 5;
        protected int lastMarker;
        protected internal object[] lookahead;
        protected int markDepth;
        protected IList markers;
        protected internal StackList nodeStack;
        protected internal object previousNode;
        protected internal object root;
        protected internal int tail;
        protected ITokenStream tokens;
        protected bool uniqueNavigationNodes;
        protected object up;

        public UnBufferedTreeNodeStream(object tree) : this(new CommonTreeAdaptor(), tree)
        {
        }

        public UnBufferedTreeNodeStream(ITreeAdaptor adaptor, object tree)
        {
            this.nodeStack = new StackList();
            this.indexStack = new StackList();
            this.lookahead = new object[5];
            this.root = tree;
            this.adaptor = adaptor;
            this.Reset();
            this.down = adaptor.Create(2, "DOWN");
            this.up = adaptor.Create(3, "UP");
            this.eof = adaptor.Create(Token.EOF, "EOF");
        }

        protected internal virtual void AddLookahead(object node)
        {
            this.lookahead[this.tail] = node;
            this.tail = (this.tail + 1) % this.lookahead.Length;
            if (this.tail == this.head)
            {
                object[] destinationArray = new object[2 * this.lookahead.Length];
                int length = this.lookahead.Length - this.head;
                Array.Copy(this.lookahead, this.head, destinationArray, 0, length);
                Array.Copy(this.lookahead, 0, destinationArray, length, this.tail);
                this.lookahead = destinationArray;
                this.head = 0;
                this.tail += length;
            }
        }

        protected internal virtual void AddNavigationNode(int ttype)
        {
            object node = null;
            if (ttype == 2)
            {
                if (this.HasUniqueNavigationNodes)
                {
                    node = this.adaptor.Create(2, "DOWN");
                }
                else
                {
                    node = this.down;
                }
            }
            else if (this.HasUniqueNavigationNodes)
            {
                node = this.adaptor.Create(3, "UP");
            }
            else
            {
                node = this.up;
            }
            this.AddLookahead(node);
        }

        public virtual void Consume()
        {
            this.fill(1);
            this.absoluteNodeIndex++;
            this.previousNode = this.lookahead[this.head];
            this.head = (this.head + 1) % this.lookahead.Length;
        }

        protected internal virtual void fill(int k)
        {
            int lookaheadSize = this.LookaheadSize;
            for (int i = 1; i <= (k - lookaheadSize); i++)
            {
                this.MoveNext();
            }
        }

        public virtual object Get(int i)
        {
            throw new NotSupportedException("stream is unbuffered");
        }

        protected internal virtual object handleRootNode()
        {
            object currentNode = this.currentNode;
            this.currentChildIndex = 0;
            if (this.adaptor.IsNil(currentNode))
            {
                return this.VisitChild(this.currentChildIndex);
            }
            this.AddLookahead(currentNode);
            if (this.adaptor.GetChildCount(this.currentNode) == 0)
            {
                this.currentNode = null;
            }
            return currentNode;
        }

        public virtual int Index()
        {
            return (this.absoluteNodeIndex + 1);
        }

        public virtual int LA(int i)
        {
            object t = (ITree) this.LT(i);
            if (t == null)
            {
                return 0;
            }
            return this.adaptor.GetNodeType(t);
        }

        public virtual object LT(int k)
        {
            if (k == -1)
            {
                return this.previousNode;
            }
            if (k < 0)
            {
                throw new ArgumentNullException("tree node streams cannot look backwards more than 1 node", "k");
            }
            if (k == 0)
            {
                return Antlr.Runtime.Tree.Tree.INVALID_NODE;
            }
            this.fill(k);
            return this.lookahead[((this.head + k) - 1) % this.lookahead.Length];
        }

        public virtual int Mark()
        {
            if (this.markers == null)
            {
                this.markers = new List<object>();
                this.markers.Add(null);
            }
            this.markDepth++;
            TreeWalkState state = null;
            if (this.markDepth >= this.markers.Count)
            {
                state = new TreeWalkState();
                this.markers.Add(state);
            }
            else
            {
                state = (TreeWalkState) this.markers[this.markDepth];
            }
            state.absoluteNodeIndex = this.absoluteNodeIndex;
            state.currentChildIndex = this.currentChildIndex;
            state.currentNode = this.currentNode;
            state.previousNode = this.previousNode;
            state.nodeStackSize = this.nodeStack.Count;
            state.indexStackSize = this.indexStack.Count;
            int lookaheadSize = this.LookaheadSize;
            int index = 0;
            state.lookahead = new object[lookaheadSize];
            int k = 1;
            while (k <= lookaheadSize)
            {
                state.lookahead[index] = this.LT(k);
                k++;
                index++;
            }
            this.lastMarker = this.markDepth;
            return this.markDepth;
        }

        public virtual bool MoveNext()
        {
            if (this.currentNode == null)
            {
                this.AddLookahead(this.eof);
                this.currentEnumerationNode = null;
                return false;
            }
            if (this.currentChildIndex == -1)
            {
                this.currentEnumerationNode = (ITree) this.handleRootNode();
                return true;
            }
            if (this.currentChildIndex < this.adaptor.GetChildCount(this.currentNode))
            {
                this.currentEnumerationNode = (ITree) this.VisitChild(this.currentChildIndex);
                return true;
            }
            this.WalkBackToMostRecentNodeWithUnvisitedChildren();
            if (this.currentNode != null)
            {
                this.currentEnumerationNode = (ITree) this.VisitChild(this.currentChildIndex);
                return true;
            }
            return false;
        }

        public virtual void Release(int marker)
        {
            this.markDepth = marker;
            this.markDepth--;
        }

        public void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t)
        {
            throw new NotSupportedException("can't do stream rewrites yet");
        }

        public virtual void Reset()
        {
            this.currentNode = this.root;
            this.previousNode = null;
            this.currentChildIndex = -1;
            this.absoluteNodeIndex = -1;
            this.head = this.tail = 0;
        }

        public void Rewind()
        {
            this.Rewind(this.lastMarker);
        }

        public virtual void Rewind(int marker)
        {
            if (this.markers != null)
            {
                TreeWalkState state = (TreeWalkState) this.markers[marker];
                this.absoluteNodeIndex = state.absoluteNodeIndex;
                this.currentChildIndex = state.currentChildIndex;
                this.currentNode = state.currentNode;
                this.previousNode = state.previousNode;
                this.nodeStack.Capacity = state.nodeStackSize;
                this.indexStack.Capacity = state.indexStackSize;
                this.head = this.tail = 0;
                while (this.tail < state.lookahead.Length)
                {
                    this.lookahead[this.tail] = state.lookahead[this.tail];
                    this.tail++;
                }
                this.Release(marker);
            }
        }

        public virtual void Seek(int index)
        {
            if (index < this.Index())
            {
                throw new ArgumentOutOfRangeException("can't seek backwards in node stream", "index");
            }
            while (this.Index() < index)
            {
                this.Consume();
            }
        }

        [Obsolete("Please use property Count instead.")]
        public virtual int Size()
        {
            return this.Count;
        }

        public override string ToString()
        {
            return this.ToString(this.root, null);
        }

        public virtual string ToString(object start, object stop)
        {
            if (start == null)
            {
                return null;
            }
            if (this.tokens != null)
            {
                int tokenStartIndex = this.adaptor.GetTokenStartIndex(start);
                int tokenStopIndex = this.adaptor.GetTokenStopIndex(stop);
                if ((stop != null) && (this.adaptor.GetNodeType(stop) == 3))
                {
                    tokenStopIndex = this.adaptor.GetTokenStopIndex(start);
                }
                else
                {
                    tokenStopIndex = this.Count - 1;
                }
                return this.tokens.ToString(tokenStartIndex, tokenStopIndex);
            }
            StringBuilder buf = new StringBuilder();
            this.ToStringWork(start, stop, buf);
            return buf.ToString();
        }

        protected internal virtual void ToStringWork(object p, object stop, StringBuilder buf)
        {
            if (!this.adaptor.IsNil(p))
            {
                string nodeText = this.adaptor.GetNodeText(p);
                if (nodeText == null)
                {
                    nodeText = " " + this.adaptor.GetNodeType(p);
                }
                buf.Append(nodeText);
            }
            if (p != stop)
            {
                int childCount = this.adaptor.GetChildCount(p);
                if ((childCount > 0) && !this.adaptor.IsNil(p))
                {
                    buf.Append(" ");
                    buf.Append(2);
                }
                for (int i = 0; i < childCount; i++)
                {
                    object child = this.adaptor.GetChild(p, i);
                    this.ToStringWork(child, stop, buf);
                }
                if ((childCount > 0) && !this.adaptor.IsNil(p))
                {
                    buf.Append(" ");
                    buf.Append(3);
                }
            }
        }

        protected internal virtual object VisitChild(int child)
        {
            object node = null;
            this.nodeStack.Push(this.currentNode);
            this.indexStack.Push(child);
            if ((child == 0) && !this.adaptor.IsNil(this.currentNode))
            {
                this.AddNavigationNode(2);
            }
            this.currentNode = this.adaptor.GetChild(this.currentNode, child);
            this.currentChildIndex = 0;
            node = this.currentNode;
            this.AddLookahead(node);
            this.WalkBackToMostRecentNodeWithUnvisitedChildren();
            return node;
        }

        protected internal virtual void WalkBackToMostRecentNodeWithUnvisitedChildren()
        {
            while ((this.currentNode != null) && (this.currentChildIndex >= this.adaptor.GetChildCount(this.currentNode)))
            {
                this.currentNode = this.nodeStack.Pop();
                if (this.currentNode == null)
                {
                    return;
                }
                this.currentChildIndex = (int) this.indexStack.Pop();
                this.currentChildIndex++;
                if (this.currentChildIndex >= this.adaptor.GetChildCount(this.currentNode))
                {
                    if (!this.adaptor.IsNil(this.currentNode))
                    {
                        this.AddNavigationNode(3);
                    }
                    if (this.currentNode == this.root)
                    {
                        this.currentNode = null;
                    }
                }
            }
        }

        public virtual int Count
        {
            get
            {
                CommonTreeNodeStream stream = new CommonTreeNodeStream(this.root);
                return stream.Count;
            }
        }

        public virtual object Current
        {
            get
            {
                return this.currentEnumerationNode;
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

        protected int LookaheadSize
        {
            get
            {
                if (this.tail >= this.head)
                {
                    return (this.tail - this.head);
                }
                return ((this.lookahead.Length - this.head) + this.tail);
            }
        }

        public string SourceName
        {
            get
            {
                return this.TokenStream.SourceName;
            }
        }

        public ITokenStream TokenStream
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
        }

        public virtual object TreeSource
        {
            get
            {
                return this.root;
            }
        }

        protected class TreeWalkState
        {
            protected internal int absoluteNodeIndex;
            protected internal int currentChildIndex;
            protected internal object currentNode;
            protected internal int indexStackSize;
            protected internal object[] lookahead;
            protected internal int nodeStackSize;
            protected internal object previousNode;
        }
    }
}

