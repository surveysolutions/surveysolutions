namespace Antlr.Runtime.Collections
{
    using System;
    using System.Collections.Generic;

    public class StackList : List<object>
    {
        public object Peek()
        {
            return base[base.Count - 1];
        }

        public object Pop()
        {
            object obj2 = base[base.Count - 1];
            base.RemoveAt(base.Count - 1);
            return obj2;
        }

        public void Push(object item)
        {
            base.Add(item);
        }
    }
}

