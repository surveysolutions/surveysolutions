using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    internal class when_NewQuestionnaireCreated_recived : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
            questionnaireListViewItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new NewQuestionnaireCreated() { CreatedBy = Guid.NewGuid() }));

        It should_create_new_instance_of_QuestionnaireListViewItem = () =>
            Result.ShouldNotBeNull();

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;
        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter = Stub.ReadSideRepository<QuestionnaireListViewItem>();
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

        private static QuestionnaireListViewItem Result
        {
            get { return questionnaireListViewItemInMemoryWriter.GetById(questionnaireId.FormatGuid()); }
        }
    }
}
