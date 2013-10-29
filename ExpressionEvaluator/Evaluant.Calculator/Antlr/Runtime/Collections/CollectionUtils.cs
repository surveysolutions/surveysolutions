namespace Antlr.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Text;

    public class CollectionUtils
    {
        public static string DictionaryToString(IDictionary dict)
        {
            StringBuilder builder = new StringBuilder();
            if (dict != null)
            {
                builder.Append("{");
                int num = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    if (num > 0)
                    {
                        builder.Append(", ");
                    }
                    if (entry.Value is IDictionary)
                    {
                        builder.AppendFormat("{0}={1}", entry.Key.ToString(), DictionaryToString((IDictionary) entry.Value));
                    }
                    else if (entry.Value is IList)
                    {
                        builder.AppendFormat("{0}={1}", entry.Key.ToString(), ListToString((IList) entry.Value));
                    }
                    else
                    {
                        builder.AppendFormat("{0}={1}", entry.Key.ToString(), entry.Value.ToString());
                    }
                    num++;
                }
                builder.Append("}");
            }
            else
            {
                builder.Insert(0, "null");
            }
            return builder.ToString();
        }

        public static string ListToString(IList coll)
        {
            StringBuilder builder = new StringBuilder();
            if (coll != null)
            {
                builder.Append("[");
                for (int i = 0; i < coll.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }
                    object obj2 = coll[i];
                    if (obj2 == null)
                    {
                        builder.Append("null");
                    }
                    else if (obj2 is IDictionary)
                    {
                        builder.Append(DictionaryToString((IDictionary) obj2));
                    }
                    else if (obj2 is IList)
                    {
                        builder.Append(ListToString((IList) obj2));
                    }
                    else
                    {
                        builder.Append(obj2.ToString());
                    }
                }
                builder.Append("]");
            }
            else
            {
                builder.Insert(0, "null");
            }
            return builder.ToString();
        }
    }
}

