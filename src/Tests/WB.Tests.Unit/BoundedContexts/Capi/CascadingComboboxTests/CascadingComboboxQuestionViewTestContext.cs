using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    internal class CascadingComboboxQuestionViewTestContext
    {
        protected static CascadingComboboxQuestionViewModel CreateCascadingComboboxQuestionViewModel(Func<decimal[], object, IEnumerable<AnswerViewModel>> getAnswerOptions)
        {
            return new CascadingComboboxQuestionViewModel(
                new InterviewItemId(Guid.NewGuid()),
                new ValueVector<Guid>(), 
                "Hello",
                getAnswerOptions,
                true,
                "Instructions",
                null,
                true,
                false,
                null,
                "Validation Message",
                "variable",
                new string[0]);
        }
    }
}
