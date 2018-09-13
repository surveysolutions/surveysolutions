using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    [Obsolete()]
    public enum QuestionSubtype
    {
        MultyOption_YesNo = 1,
        MultyOption_Linked = 2,
        MultyOption_Ordered = 3,
        MultyOption_YesNoOrdered = 4,

        DateTime_Timestamp = 5,

        SingleOption_Linked = 6,
    }
}
