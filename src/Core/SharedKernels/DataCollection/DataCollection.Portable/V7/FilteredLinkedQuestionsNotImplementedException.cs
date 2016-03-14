using System;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public class FilteredLinkedQuestionsNotImplementedException : NotImplementedException
    {
        public FilteredLinkedQuestionsNotImplementedException()
        {
        }

        public FilteredLinkedQuestionsNotImplementedException(string message) : base(message)
        {
        }

        public FilteredLinkedQuestionsNotImplementedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}