namespace Antlr.Runtime.Tree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public abstract class RewriteRuleElementStream<T>
    {
        protected ITreeAdaptor adaptor;
        protected int cursor;
        protected bool dirty;
        protected string elementDescription;
        protected IList<T> elements;
        protected T singleElement;

        public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription)
        {
            this.elementDescription = elementDescription;
            this.adaptor = adaptor;
        }

        public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, T oneElement) : this(adaptor, elementDescription)
        {
            this.Add(oneElement);
        }

        public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, IList<T> elements) : this(adaptor, elementDescription)
        {
            this.singleElement = default(T);
            this.elements = elements;
        }

        [Obsolete("This constructor is for internal use only and might be phased out soon. Use instead the one with IList<T>.")]
        public RewriteRuleElementStream(ITreeAdaptor adaptor, string elementDescription, IList elements) : this(adaptor, elementDescription)
        {
            this.singleElement = default(T);
            this.elements = new List<T>();
            if (elements != null)
            {
                foreach (T local in elements)
                {
                    this.elements.Add(local);
                }
            }
        }

        protected object _Next()
        {
            int count = this.Count;
            if (count == 0)
            {
                throw new RewriteEmptyStreamException(this.elementDescription);
            }
            if (this.cursor >= count)
            {
                if (count != 1)
                {
                    throw new RewriteCardinalityException(this.elementDescription);
                }
                return this.ToTree(this.singleElement);
            }
            if (this.singleElement != null)
            {
                this.cursor++;
                return this.ToTree(this.singleElement);
            }
            return this.ToTree(this.elements[this.cursor++]);
        }

        public void Add(T el)
        {
            if (el != null)
            {
                if (this.elements != null)
                {
                    this.elements.Add(el);
                }
                else if (this.singleElement == null)
                {
                    this.singleElement = el;
                }
                else
                {
                    this.elements = new List<T>(5);
                    this.elements.Add(this.singleElement);
                    this.singleElement = default(T);
                    this.elements.Add(el);
                }
            }
        }

        public bool HasNext()
        {
            return (((this.singleElement != null) && (this.cursor < 1)) || ((this.elements != null) && (this.cursor < this.elements.Count)));
        }

        public virtual object NextTree()
        {
            return this._Next();
        }

        public virtual void Reset()
        {
            this.cursor = 0;
            this.dirty = true;
        }

        [Obsolete("Please use property Count instead.")]
        public int Size()
        {
            return this.Count;
        }

        protected virtual object ToTree(T el)
        {
            return el;
        }

        public int Count
        {
            get
            {
                if (this.singleElement != null)
                {
                    return 1;
                }
                if (this.elements != null)
                {
                    return this.elements.Count;
                }
                return 0;
            }
        }

        public string Description
        {
            get
            {
                return this.elementDescription;
            }
        }
    }
}

