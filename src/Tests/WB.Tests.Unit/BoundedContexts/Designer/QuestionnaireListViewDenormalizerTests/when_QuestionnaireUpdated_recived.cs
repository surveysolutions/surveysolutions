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
    internal class when_QuestionnaireUpdated_recived : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireUpdatedEvent = CreatePublishedEvent(questionnaireId,
                new QuestionnaireUpdated() { IsPublic = true, Title = "nastya" }, DateTime.Now);
            questionnaireListViewItemInMemoryWriter.Store(
                new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, false),
                questionnaireId.FormatGuid());
            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
            questionnaireListViewItemDenormalizer.Handle(questionnaireUpdatedEvent);

        It should_update_title_of_questionnaire = () =>
            Result.Title.ShouldEqual(questionnaireUpdatedEvent.Payload.Title);

        It should_update_LastEntryDate_of_questionnaire = () =>
           Result.LastEntryDate.ShouldEqual(questionnaireUpdatedEvent.EventTimeStamp);

        It should_update_IsPublic_of_questionnaire = () =>
            Result.IsPublic.ShouldEqual(questionnaireUpdatedEvent.Payload.IsPublic);

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;
        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter = Stub.ReadSideRepository<QuestionnaireListViewItem>();
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static IPublishedEvent<QuestionnaireUpdated> questionnaireUpdatedEvent;
        private static QuestionnaireListViewItem Result
        {
            get { return questionnaireListViewItemInMemoryWriter.GetById(questionnaireId.FormatGuid()); }
        }
    }
}
