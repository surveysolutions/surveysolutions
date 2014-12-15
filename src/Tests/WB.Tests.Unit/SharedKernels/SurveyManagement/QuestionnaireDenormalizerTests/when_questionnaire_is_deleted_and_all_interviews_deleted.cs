using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    public class when_questionnaire_is_deleted_and_all_interviews_deleted : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            assemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();
            questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireVersion = 5;

            denormalizer = CreateDenormalizer(
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

