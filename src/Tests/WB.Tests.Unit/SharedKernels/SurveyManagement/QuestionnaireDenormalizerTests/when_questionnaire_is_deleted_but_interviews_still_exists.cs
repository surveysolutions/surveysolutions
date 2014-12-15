using System;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.DenormalizerStorage;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    public class when_questionnaire_is_deleted_but_interviews_still_exists : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            assemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();
            questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireVersion = 5;

            IReadSideRepositoryWriter<InterviewSummary> interviews = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviews.Store(Create.InterviewSummary(questionnaireId, questionnaireVersion), "1");

            denormalizer = CreateDenormalizer(
                interviews: interviews,
                assemblyFileAccessor: assemblyFileAccessor.Object);
        };

        Because of = () => denormalizer.Handle(Create.Event.QuestionnaireDeleted(questionnaireId: questionnaireId, version: questionnaireVersion));

        It should_not_remove_assembly_if_interviews_still_exist_in_the_system = () => 
            assemblyFileAccessor.Verify(x => x.RemoveAssembly(questionnaireId, questionnaireVersion), Times.Never);

        static QuestionnaireDenormalizer denormalizer;
        static Mock<IQuestionnaireAssemblyFileAccessor> assemblyFileAccessor;
        static Guid questionnaireId;
        static long questionnaireVersion;
    }
}

