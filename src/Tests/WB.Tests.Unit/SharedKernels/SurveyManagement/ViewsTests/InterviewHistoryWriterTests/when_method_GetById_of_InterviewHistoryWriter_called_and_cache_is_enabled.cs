using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewHistoryWriterTests
{
    internal class when_method_GetById_of_InterviewHistoryWriter_called_and_cache_is_enabled : InterviewHistoryWriterTestContext
    {
        Establish context = () =>
        {
            interviewHistoryWriter = CreateInterviewHistoryWriter();
            interviewHistoryWriter.EnableCache();
            view = CreateInterviewHistoryView();
            interviewHistoryWriter.Store(view, view.InterviewId.FormatGuid());
        };

        Because of = () =>
            result = interviewHistoryWriter.GetById(view.InterviewId.FormatGuid());

        It should_return_cached_view = () =>
            result.ShouldBeTheSameAs(view);

        private static InterviewHistoryWriter interviewHistoryWriter;
        private static InterviewHistoryView view;
        private static InterviewHistoryView result;
    }
}
