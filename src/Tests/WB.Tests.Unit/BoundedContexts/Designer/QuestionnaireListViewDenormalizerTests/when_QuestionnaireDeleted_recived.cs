using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
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
