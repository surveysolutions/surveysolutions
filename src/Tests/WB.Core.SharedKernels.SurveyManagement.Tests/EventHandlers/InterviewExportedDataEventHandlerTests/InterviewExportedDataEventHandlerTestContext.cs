using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.InterviewExportedDataEventHandlerTests
{
    [Subject(typeof(InterviewExportedDataDenormalizer))]
    internal class InterviewExportedDataEventHandlerTestContext
    {
        protected const string firstLevelkey = "#";

        protected static InterviewExportedDataDenormalizer CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
            Func<QuestionnaireDocument> templateCreationAction,
            Func<InterviewData> dataCreationAction = null, Action<InterviewDataExportView> returnStoredView = null)
        {
            var interviewDataStorageMock = new Mock<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>();
            var questionnaire = templateCreationAction();


            interviewDataStorageMock.Setup(
                x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(
                    () =>
                    {
                        var createInterview = dataCreationAction ?? CreateInterviewData;
                        var interview = createInterview();
                        interview.QuestionnaireId = questionnaire.PublicKey;
                        return new ViewWithSequence<InterviewData>(interview, 1);
                    });


            var questionnaireExportStructureMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure>>();
            var exportViewFactory = new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(), new QuestionnaireRosterStructureFactory());
            questionnaireExportStructureMock.Setup(x => x.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()))
                .Returns(exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1));

            var dataExportService = new Mock<IDataExportService>();

            if (returnStoredView != null)
                dataExportService.Setup(x => x.AddExportedDataByInterview(Moq.It.IsAny<InterviewDataExportView>()))
                    .Callback<InterviewDataExportView>(returnStoredView);

            return new InterviewExportedDataDenormalizer(
                interviewDataStorageMock.Object,
                questionnaireExportStructureMock.Object, dataExportService.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string,Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var question in variableNameAndQuestionId)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value, QuestionType = QuestionType.Numeric});
            }

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }

        protected static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() {InterviewId = Guid.NewGuid()};
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            return interviewData;
        }

        protected static InterviewData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interviewData = CreateInterviewData();
            foreach (var questionsWithAnswer in questionsWithAnswers)
            {
                var question =
                    interviewData.Levels["#"].GetOrCreateQuestion(questionsWithAnswer);
                question.Answer = "some answer";
            }
            return interviewData;
        }

        protected static IPublishedEvent<InterviewApprovedByHQ> CreatePublishableEvent()
        {
            var publishableEventMock = new Mock<IPublishedEvent<InterviewApprovedByHQ>>();
            publishableEventMock.Setup(x => x.Payload).Returns(new InterviewApprovedByHQ(Guid.NewGuid(),""));
            return publishableEventMock.Object;
        }


        protected static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid[] levelVector)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelVector.SequenceEqual(levelVector));
        }
    }

    public static class ShouldExtensions
    {
        public static void ShouldQuestionHasOneNotEmptyAnswer(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldContain(q => q.QuestionId == questionId);
            var answers = questions.First(q => q.QuestionId == questionId).Answers;
            answers.ShouldContain(a => !string.IsNullOrEmpty(a));
        }

        public static void ShouldQuestionHasNoAnswers(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldNotContain(q => q.QuestionId == questionId && q.Answers.Any(a=>!string.IsNullOrEmpty(a)));
        }
    }

}