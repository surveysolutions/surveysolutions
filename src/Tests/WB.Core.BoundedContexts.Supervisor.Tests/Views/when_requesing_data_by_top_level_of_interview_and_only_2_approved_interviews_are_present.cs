using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;


namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    [Subject(typeof(InterviewDataExportFactory))]
    internal class when_requesing_data_by_top_level_of_interview_and_only_2_approved_interviews_are_present
    {
        private Establish context = () =>
        {
            interviewDataExportFactory = CreateInterviewDataExportFactory(2);
        };

        Because of = () =>
           result = interviewDataExportFactory.Load(new InterviewDataExportInputModel(Guid.NewGuid(), 1, null));

        private It should_records_count_equals_2 = () =>
            result.Records.Length.ShouldEqual(2);

        private It should_first_record_id_equals_0 = () =>
            result.Records[0].RecordId.ShouldEqual(0);

        private It should_second_record_id_equals_1 = () =>
            result.Records[1].RecordId.ShouldEqual(1);

        private static InterviewDataExportFactory interviewDataExportFactory;
        private static InterviewDataExportView result;

        public void Load_When_2_approved_interviews_and_top_level_is_requested_Then_2_records_returned()
        {
            //arrange

            InterviewDataExportFactory target = CreateInterviewDataExportFactory(2);

            //act

            InterviewDataExportView result = target.Load(new InterviewDataExportInputModel(Guid.NewGuid(), 1, null));

            //assert
            Assert.That(result.Records.Length, Is.EqualTo(2));
            Assert.That(result.Records[0].RecordId, Is.EqualTo(0));
            Assert.That(result.Records[1].RecordId, Is.EqualTo(1));
        }

        protected static InterviewDataExportFactory CreateInterviewDataExportFactory(int interviewCount = 0)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var interviewSummaryStorageMock = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();

            interviewSummaryStorageMock.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, InterviewSummary[]>>()))
                .Returns(CreateListOfApprovedInterviews(interviewCount));

            var interviewDataStorageMock = new Mock<IReadSideRepositoryReader<InterviewData>>();
            interviewDataStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>())).Returns(CreateInterviewData);

            var questionnaireStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnaireDocumentVersioned() { Questionnaire = new QuestionnaireDocument() });

            var propagationStructureStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure>>();
            propagationStructureStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnairePropagationStructure());

            var linkedQuestionStructureStorageMock = new Mock<IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions>>();
            linkedQuestionStructureStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns<Guid, long>(
                    (questionnaireId, version) => new ReferenceInfoForLinkedQuestions(questionnaireId, version, new Dictionary<Guid, ReferenceInfoByQuestion>()));

            return new InterviewDataExportFactory(interviewDataStorageMock.Object,
                interviewSummaryStorageMock.Object,
                questionnaireStorageMock.Object,
                propagationStructureStorageMock.Object,
                linkedQuestionStructureStorageMock.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            var questionnaire = new QuestionnaireDocument();
            return questionnaire;
        }

        protected static InterviewSummary[] CreateListOfApprovedInterviews(int count)
        {
            var interviewsSummary = new InterviewSummary[count];

            for (int i = 0; i < count; i++)
            {
                interviewsSummary[i] = new InterviewSummary();
            }

            return interviewsSummary;
        }

        protected static InterviewData CreateInterviewData()
        {
            var interview = new InterviewData();
            interview.InterviewId = Guid.NewGuid();
            interview.Levels.Add("#", new InterviewLevel(interview.InterviewId, new int[0]));
            return interview;
        }
    }
}
