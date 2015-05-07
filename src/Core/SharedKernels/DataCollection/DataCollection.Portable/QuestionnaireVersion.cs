using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.DataCollection
{
    [DataContract]
    [Obsolete("Don't use this class! Don't move it. It is here only for backward compatibility with versions lower then 4.")]
    public sealed class QuestionnaireVersion
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
    }
}