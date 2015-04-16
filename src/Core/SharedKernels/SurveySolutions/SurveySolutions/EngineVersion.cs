using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.SurveySolutions
{
    [DataContract]
    public sealed class EngineVersion : IComparable<EngineVersion>
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Patch { get; set; }

        public EngineVersion(int major, int minor, int patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public static bool TryParse(string input, out EngineVersion result)
        {
            int major, minor, patch;

            result = new EngineVersion(0,0,0);

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

            result = new EngineVersion(major, minor, patch);

            return true;
        }

        public bool Equals(EngineVersion other)
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


        public int CompareTo(EngineVersion engineVersion)
        {
            if (engineVersion == null)
                return 1;

            if (this.Major != engineVersion.Major)
                if (this.Major > engineVersion.Major)
                    return 1;
                else
                    return -1;

            if (this.Minor != engineVersion.Minor)
                if (this.Minor > engineVersion.Minor)
                    return 1;
                else
                    return -1;

            if (this.Patch != engineVersion.Patch)
                if (this.Patch > engineVersion.Patch)
                    return 1;
                else
                    return -1;

            return 0;
        }

        public override bool Equals(object obj)
        {
            EngineVersion other = obj as EngineVersion;
            if (other == null)
                return false;

            return this.Major == other.Major && 
                   this.Minor == other.Minor && 
                   this.Patch == other.Patch;
        }

        public static bool operator ==(EngineVersion left, EngineVersion right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(EngineVersion left, EngineVersion right)
        {
            return !(left == right);
        }

        public static bool operator <(EngineVersion left, EngineVersion right)
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(EngineVersion left, EngineVersion right)
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return left.CompareTo(right) <= 0;
        }


        public static bool operator >(EngineVersion left, EngineVersion right)
        {
            return right < left;
        }

        public static bool operator >=(EngineVersion left, EngineVersion right)
        {
            return right <= left;
        }

    }
}