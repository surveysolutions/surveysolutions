using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.WebTester.Services
{
    public class CategoricalOptionsFilter
    {
        public CategoricalOptionsFilter(Identity questionIdentity, int itemsCount, CategoricalOption[] unfilteredOptionsForQuestion)
        {
            QuestionIdentity = questionIdentity;
            ItemsCount = itemsCount;
            UnfilteredOptionsForQuestion = unfilteredOptionsForQuestion;
        }
        
        public Identity QuestionIdentity { get; set; }
        public int ItemsCount { get; set; }
        public CategoricalOption[] UnfilteredOptionsForQuestion { get;  set; }
    }
}