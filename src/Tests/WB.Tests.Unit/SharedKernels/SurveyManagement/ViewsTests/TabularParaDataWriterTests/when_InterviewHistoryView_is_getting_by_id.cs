using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.TabularParaDataWriterTests
{
    internal class when_InterviewHistoryView_is_getting_by_id : TabularParaDataWriterTestContext
    {
        Establish context = () =>
        {
            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(new InterviewSummary()
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    InterviewId = interviewId
                });
            
            tabularParaDataAccessor = CreateTabularParaDataWriter(interviewSummaryWriter: interviewSummaryWriterMock.Object);
        };

        Because of = () =>
            result = tabularParaDataAccessor.GetById(interviewId.FormatGuid());

        It should_return_view_with_QuestionnaireId_equal_to_questionnaireId = () =>
            result.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_return_view_with_QuestionnaireVersion_equal_to_questionnaireVersion = () =>
          result.QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        It should_return_view_with_InterviewId_equal_to_interviewId = () =>
          result.InterviewId.ShouldEqual(interviewId);

        It should_return_view_with_empty_records_list = () =>
            result.Records.ShouldBeEmpty();

        private static TabularParaDataAccessor tabularParaDataAccessor;
        private static InterviewHistoryView result;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static long questionnaireVersion = 2;
    }
}
