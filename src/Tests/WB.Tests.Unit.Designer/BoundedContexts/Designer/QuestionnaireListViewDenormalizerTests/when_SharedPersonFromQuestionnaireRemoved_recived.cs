using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    internal class when_SharedPersonFromQuestionnaireRemoved_recived : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            sharedPersonFromQuestionnaireRemovedEvent = CreatePublishedEvent(questionnaireId,
                new SharedPersonFromQuestionnaireRemoved() { PersonId = Guid.NewGuid() }, DateTime.Now);

            var questionnaireListViewItem = new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, false);
            questionnaireListViewItem.SharedPersons.Add(sharedPersonFromQuestionnaireRemovedEvent.Payload.PersonId);
            questionnaireListViewItemInMemoryWriter.Store(
                questionnaireListViewItem,
                questionnaireId.FormatGuid());
            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
            questionnaireListViewItemDenormalizer.Handle(sharedPersonFromQuestionnaireRemovedEvent);

        It should_update_SharedPersons_list_of_questionnaire = () =>
            Result.SharedPersons.ShouldNotContain(sharedPersonFromQuestionnaireRemovedEvent.Payload.PersonId);

        It should_update_LastEntryDate_of_questionnaire = () =>
           Result.LastEntryDate.ShouldEqual(sharedPersonFromQuestionnaireRemovedEvent.EventTimeStamp);

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;
        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter = Stub.ReadSideRepository<QuestionnaireListViewItem>();
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static IPublishedEvent<SharedPersonFromQuestionnaireRemoved> sharedPersonFromQuestionnaireRemovedEvent;
        private static QuestionnaireListViewItem Result
        {
            get { return questionnaireListViewItemInMemoryWriter.GetById(questionnaireId.FormatGuid()); }
        }
    }
}
