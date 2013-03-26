namespace Antlr.Runtime
{
    using Antlr.Runtime.Collections;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public class CommonTokenStream : ITokenStream, IIntStream
    {
        protected int channel;
        protected IDictionary channelOverrideMap;
        protected bool discardOffChannelTokens;
        protected HashList discardSet;
        protected int lastMarker;
        protected int p;
        protected IList tokens;
        protected ITokenSource tokenSource;

        public CommonTokenStream()
        {
            this.p = -1;
            this.channel = 0;
            this.tokens = new List<object>(500);
        }

        public CommonTokenStream(ITokenSource tokenSource) : this()
        {
            this.tokenSource = tokenSource;
        }

        public CommonTokenStream(ITokenSource tokenSource, int channel) : this(tokenSource)
        {
            this.channel = channel;
        }

        public virtual void Consume()
        {
            if (this.p < this.tokens.Count)
            {
                this.p++;
                this.p = this.SkipOffTokenChannels(this.p);
            }
        }

        public virtual void DiscardOffChannelTokens(bool discardOffChannelTokens)
        {
            this.discardOffChannelTokens = discardOffChannelTokens;
        }

        public virtual void DiscardTokenType(int ttype)
        {
            if (this.discardSet == null)
            {
                this.discardSet = new HashList();
            }
            this.discardSet.Add(ttype.ToString(), ttype);
        }

        protected virtual void FillBuffer()
        {
            int num = 0;
            for (IToken token = this.tokenSource.NextToken(); (token != null) && (token.Type != -1); token = this.tokenSource.NextToken())
            {
                bool flag = false;
                if (this.channelOverrideMap != null)
                {
                    object obj2 = this.channelOverrideMap[token.Type];
                    if (obj2 != null)
                    {
                        token.Channel = (int) obj2;
                    }
                }
                if ((this.discardSet != null) && this.discardSet.Contains(token.Type.ToString()))
                {
                    flag = true;
                }
                else if (this.discardOffChannelTokens && (token.Channel != this.channel))
                {
                    flag = true;
                }
                if (!flag)
                {
                    token.TokenIndex = num;
                    this.tokens.Add(token);
                    num++;
                }
            }
            this.p = 0;
            this.p = this.SkipOffTokenChannels(this.p);
        }

        public virtual IToken Get(int i)
        {
            return (IToken) this.tokens[i];
        }

        public virtual IList GetTokens()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            return this.tokens;
        }

        public virtual IList GetTokens(int start, int stop)
        {
            return this.GetTokens(start, stop, (BitSet) null);
        }

        public virtual IList GetTokens(int start, int stop, BitSet types)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            if (stop >= this.tokens.Count)
            {
                stop = this.tokens.Count - 1;
            }
            if (start < 0)
            {
                start = 0;
            }
            if (start > stop)
            {
                return null;
            }
            IList list = new List<object>();
            for (int i = start; i <= stop; i++)
            {
                IToken token = (IToken) this.tokens[i];
                if ((types == null) || types.Member(token.Type))
                {
                    list.Add(token);
                }
            }
            if (list.Count == 0)
            {
                list = null;
            }
            return list;
        }

        public virtual IList GetTokens(int start, int stop, IList types)
        {
            return this.GetTokens(start, stop, new BitSet(types));
        }

        public virtual IList GetTokens(int start, int stop, int ttype)
        {
            return this.GetTokens(start, stop, BitSet.Of(ttype));
        }

        public virtual int Index()
        {
            return this.p;
        }

        public virtual int LA(int i)
        {
            return this.LT(i).Type;
        }

        protected virtual IToken LB(int k)
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            if (k == 0)
            {
                return null;
            }
            if ((this.p - k) < 0)
            {
                return null;
            }
            int p = this.p;
            for (int i = 1; i <= k; i++)
            {
                p = this.SkipOffTokenChannelsReverse(p - 1);
            }
            if (p < 0)
            {
                return null;
            }
            return (IToken) this.tokens[p];
        }

        public virtual IToken LT(int k)
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
            if (((this.p + k) - 1) >= this.tokens.Count)
            {
                return Token.EOF_TOKEN;
            }
            int p = this.p;
            for (int i = 1; i < k; i++)
            {
                p = this.SkipOffTokenChannels(p + 1);
            }
            if (p >= this.tokens.Count)
            {
                return Token.EOF_TOKEN;
            }
            return (IToken) this.tokens[p];
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

        public virtual void Release(int marker)
        {
        }

        public virtual void Reset()
        {
            this.p = 0;
            this.lastMarker = 0;
        }

        public virtual void Rewind()
        {
            this.Seek(this.lastMarker);
        }

        public virtual void Rewind(int marker)
        {
            this.Seek(marker);
        }

        public virtual void Seek(int index)
        {
            this.p = index;
        }

        public virtual void SetTokenTypeChannel(int ttype, int channel)
        {
            if (this.channelOverrideMap == null)
            {
                this.channelOverrideMap = new Hashtable();
            }
            this.channelOverrideMap[ttype] = channel;
        }

        [Obsolete("Please use the property Count instead.")]
        public virtual int Size()
        {
            return this.Count;
        }

        protected virtual int SkipOffTokenChannels(int i)
        {
            int count = this.tokens.Count;
            while ((i < count) && (((IToken) this.tokens[i]).Channel != this.channel))
            {
                i++;
            }
            return i;
        }

        protected virtual int SkipOffTokenChannelsReverse(int i)
        {
            while ((i >= 0) && (((IToken) this.tokens[i]).Channel != this.channel))
            {
                i--;
            }
            return i;
        }

        public override string ToString()
        {
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            return this.ToString(0, this.tokens.Count - 1);
        }

        public virtual string ToString(IToken start, IToken stop)
        {
            if ((start != null) && (stop != null))
            {
                return this.ToString(start.TokenIndex, stop.TokenIndex);
            }
            return null;
        }

        public virtual string ToString(int start, int stop)
        {
            if ((start < 0) || (stop < 0))
            {
                return null;
            }
            if (this.p == -1)
            {
                this.FillBuffer();
            }
            if (stop >= this.tokens.Count)
            {
                stop = this.tokens.Count - 1;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = start; i <= stop; i++)
            {
                IToken token = (IToken) this.tokens[i];
                builder.Append(token.Text);
            }
            return builder.ToString();
        }

        public virtual int Count
        {
            get
            {
                return this.tokens.Count;
            }
        }

        public virtual string SourceName
        {
            get
            {
                return this.TokenSource.SourceName;
            }
        }

        public virtual ITokenSource TokenSource
        {
            get
            {
                return this.tokenSource;
            }
            set
            {
                this.tokenSource = value;
                this.tokens.Clear();
                this.p = -1;
                this.channel = 0;
            }
        }
    }
}

