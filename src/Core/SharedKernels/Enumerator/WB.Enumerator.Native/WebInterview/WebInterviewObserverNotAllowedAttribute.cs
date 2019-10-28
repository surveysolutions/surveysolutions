using System;

namespace WB.Enumerator.Native.WebInterview
{
    public class WebInterviewObserverNotAllowedAttribute : Attribute
    {
        public static string Id = "ObserverNowAllowed";

        public override object TypeId => Id;
    }
}
