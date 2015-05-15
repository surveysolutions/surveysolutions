using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Utils;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    internal class when_SharedPersonToQuestionnaireAdded_received : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            sharedPersonToQuestionnaireAddedEvent = CreatePublishedEvent(questionnaireId,
                new SharedPersonToQuestionnaireAdded(){PersonId = Guid.NewGuid()}, DateTime.Now);
            questionnaireListViewItemInMemoryWriter.Store(
                new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, false),
                questionnaireId.FormatGuid());
            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
            questionnaireListViewItemDenormalizer.Handle(sharedPersonToQuestionnaireAddedEvent);

        It should_update_SharedPersons_list_of_questionnaire = () =>
            Result.SharedPersons.ShouldContain(sharedPersonToQuestionnaireAddedEvent.Payload.PersonId);

        It should_update_LastEntryDate_of_questionnaire = () =>
           Result.LastEntryDate.ShouldEqual(sharedPersonToQuestionnaireAddedEvent.EventTimeStamp);

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;
        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter = Stub.ReadSideRepository<QuestionnaireListViewItem>();
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static IPublishedEvent<SharedPersonToQuestionnaireAdded> sharedPersonToQuestionnaireAddedEvent;
        private static QuestionnaireListViewItem Result
        {
            get { return questionnaireListViewItemInMemoryWriter.GetById(questionnaireId.FormatGuid()); }
        }
    }
}
