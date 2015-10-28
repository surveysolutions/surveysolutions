using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.TabularParaDataWriterTests
{
    internal class WhenMethodGetByIdOfTabularParaDataWriterCalledAndCacheIsEnabled : TabularParaDataWriterTestContext
    {
        Establish context = () =>
        {
            _tabularParaDataWriter = CreateTabularParaDataWriter();
            view = CreateInterviewHistoryView();
            _tabularParaDataWriter.Store(view, view.InterviewId.FormatGuid());
        };

        Because of = () =>
            result = _tabularParaDataWriter.GetById(view.InterviewId.FormatGuid());

        It should_return_cached_view = () =>
            result.ShouldBeTheSameAs(view);

        private static TabularParaDataWriter _tabularParaDataWriter;
        private static InterviewHistoryView view;
        private static InterviewHistoryView result;
    }
}
