using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }

        public static bool operator ==(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return !(left == right);
        }

        public static bool operator <(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            if ((object) left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            if ((object) left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) <= 0;
        }


        public static bool operator >(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return right < left;
        }

        public static bool operator >=(QuestionnaireVersion left, QuestionnaireVersion right)
        {
            return right <= left;
        }
        
        public bool Equals(QuestionnaireVersion other)
        {
            if (other == null)
                return false;

            return this.Major == other.Major &&
                   this.Minor == other.Minor &&
                   this.Patch == other.Patch;
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
                hashCode = (hashCode*397) ^ this.Minor;
                hashCode = (hashCode*397) ^ this.Patch;
                return hashCode;
            }
        }


        public int CompareTo(QuestionnaireVersion questionnaireVersion)
        {
            if (questionnaireVersion == null)
                return 1;

            if (this.Major != questionnaireVersion.Major)
                if (this.Major > questionnaireVersion.Major)
                    return 1;
                else
                    return -1;

            if (this.Minor != questionnaireVersion.Minor)
                if (this.Minor > questionnaireVersion.Minor)
                    return 1;
                else
                    return -1;

            if (this.Patch != questionnaireVersion.Patch)
                if (this.Patch > questionnaireVersion.Patch)
                    return 1;
                else
                    return -1;

            return 0;
        }

        public override bool Equals(object obj)
        {
            QuestionnaireVersion other = obj as QuestionnaireVersion;
            if (other == null)
                return false;

            return this.Major == other.Major &&
                   this.Minor == other.Minor &&
                   this.Patch == other.Patch;
        }
    }
}