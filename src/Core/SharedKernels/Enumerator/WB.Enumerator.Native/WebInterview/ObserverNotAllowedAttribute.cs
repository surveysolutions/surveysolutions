using System;

namespace WB.Enumerator.Native.WebInterview
{
    public class ObserverNotAllowedAttribute : Attribute
    {
        public static string Id = "ObserverNowAllowed";

        public override object TypeId => Id;
    }
}
