using System;

namespace Main.Core.Entities.SubEntities
{
    [Obsolete("old class which is left only for backward compatibility")]
    public struct QuestionIdAndVariableName
    {
        public QuestionIdAndVariableName(Guid id, string variableName)
        {
            this.Id = id;
            this.VariableName = variableName;
        }

        public Guid Id;
        public string VariableName;

        public override bool Equals(object? obj)
        {
            return obj is QuestionIdAndVariableName questionIdAndVariableName && this == questionIdAndVariableName;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(QuestionIdAndVariableName x, QuestionIdAndVariableName y)
        {
            if (x.Id == y.Id && x.VariableName == y.VariableName)
                return true;
            return false;
        }

        public static bool operator !=(QuestionIdAndVariableName x, QuestionIdAndVariableName y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Id, VariableName);
        }

        /// <remarks>Is needed for Newtonsoft JSON.</remarks>
        public static explicit operator QuestionIdAndVariableName(string b)
        {
            return Parse(b);
        }

        public static QuestionIdAndVariableName Parse(string value)
        {
            var items = value.Split(',');
            return new QuestionIdAndVariableName(Guid.Parse(items[0]), items[1]);
        }
    }
}
