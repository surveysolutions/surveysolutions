using System;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    [DataContract]
    public sealed class ExpressionsEngineVersion : IComparable<ExpressionsEngineVersion>
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Patch { get; set; }

        public ExpressionsEngineVersion(int major, int minor, int patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public static bool TryParse(string input, out ExpressionsEngineVersion result)
        {
            int major, minor, patch;

            result = new ExpressionsEngineVersion(0,0,0);

            String[] parsedComponents = input.Split('.');

            int parsedComponentsLength = parsedComponents.Length;
            if (parsedComponentsLength != 3) 
            {
                return false;
            }

            if (!Int32.TryParse(parsedComponents[0], out major))
            {
                return false;
            }

            if (!Int32.TryParse(parsedComponents[1], out minor))
            {
                return false;
            }

            if (!Int32.TryParse(parsedComponents[2], out patch))
            {
                return false;
            }

            result = new ExpressionsEngineVersion(major, minor, patch);

            return true;
        }

        public bool Equals(ExpressionsEngineVersion other)
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
                hashCode = (hashCode * 397) ^ this.Minor;
                hashCode = (hashCode * 397) ^ this.Patch;
                return hashCode;
            }
        }


        public int CompareTo(ExpressionsEngineVersion expressionsEngineVersion)
        {
            if (expressionsEngineVersion == null)
                return 1;

            if (this.Major != expressionsEngineVersion.Major)
                if (this.Major > expressionsEngineVersion.Major)
                    return 1;
                else
                    return -1;

            if (this.Minor != expressionsEngineVersion.Minor)
                if (this.Minor > expressionsEngineVersion.Minor)
                    return 1;
                else
                    return -1;

            if (this.Patch != expressionsEngineVersion.Patch)
                if (this.Patch > expressionsEngineVersion.Patch)
                    return 1;
                else
                    return -1;

            return 0;
        }

        public override bool Equals(object obj)
        {
            ExpressionsEngineVersion other = obj as ExpressionsEngineVersion;
            if (other == null)
                return false;

            return this.Major == other.Major && 
                   this.Minor == other.Minor && 
                   this.Patch == other.Patch;
        }

        public static bool operator ==(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            return !(left == right);
        }

        public static bool operator <(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) <= 0;
        }


        public static bool operator >(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            return right < left;
        }

        public static bool operator >=(ExpressionsEngineVersion left, ExpressionsEngineVersion right)
        {
            return right <= left;
        }

    }
}