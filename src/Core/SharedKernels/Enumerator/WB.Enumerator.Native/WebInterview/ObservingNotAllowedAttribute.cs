using System;

namespace WB.Enumerator.Native.WebInterview
{
    public class ObservingNotAllowedAttribute : Attribute
    {
        public static string Id = "ObservingNowAllowed";
        public override object TypeId => Id;
    }
}
