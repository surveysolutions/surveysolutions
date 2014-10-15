using System;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    [DataContract]
    public class QuestionnaireVersion : IComparable<QuestionnaireVersion>
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Patch { get; set; }

        public QuestionnaireVersion(int major, int minor, int patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        protected bool Equals(QuestionnaireVersion other)
        {
            return this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Major;
                hashCode = (hashCode * 397) ^ this.Minor;
                hashCode = (hashCode * 397) ^ this.Patch;
                return hashCode;
            }
        }

        public int CompareTo(QuestionnaireVersion other)
        {
            if (other == null)
                return 1;
            if (Major == other.Major)
            {
                if (Minor == other.Minor)
                {
                    if (Patch == other.Patch) 
                        return 0;
                    if (Patch < other.Patch)
                        return -1;
                    return 1;
                }
                if (Minor < other.Minor)
                    return -1;
                return 1;
            }
            if (Major < other.Major)
            {
                return -1;
            }
            return 1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionnaireVersion)obj);
        }

        public static bool operator ==(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return !(left == right);
        }

        public static bool operator >(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return left.CompareTo(right) < 0;
        }
    }
}