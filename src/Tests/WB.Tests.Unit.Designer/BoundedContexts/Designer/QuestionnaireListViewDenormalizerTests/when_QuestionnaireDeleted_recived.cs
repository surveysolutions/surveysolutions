using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    internal class when_QuestionnaireDeleted_recived : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireListViewItemInMemoryWriter.Store(
                new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, false),
                questionnaireId.FormatGuid());
            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
            questionnaireListViewItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId,new QuestionnaireDeleted()));

        It should_mark_questionnaire_as_deleted = () =>
            Result.IsDeleted.ShouldBeTrue();

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;
        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter = Stub.ReadSideRepository<QuestionnaireListViewItem>();
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireListViewItem Result
        {
            get { return questionnaireListViewItemInMemoryWriter.GetById(questionnaireId.FormatGuid()); }
        }
    }
}
