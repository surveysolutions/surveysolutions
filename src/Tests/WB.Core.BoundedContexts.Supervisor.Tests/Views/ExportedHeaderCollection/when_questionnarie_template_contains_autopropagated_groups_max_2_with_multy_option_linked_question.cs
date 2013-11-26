using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollection
{
    [Subject(typeof (WB.Core.BoundedContexts.Supervisor.Views.DataExport.ExportedHeaderCollection))]
    internal class when_questionnarie_template_contains_autopropagated_groups_max_2_with_multy_option_linked_question
    {
        private Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);


            var autoPropagateQuestionId = Guid.NewGuid();
            linkedQuestionId = Guid.NewGuid();
            referencedQuestionId = Guid.NewGuid();
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = Guid.NewGuid() };
            questionnaireDocument.Children.Add(new AutoPropagateQuestion("i am auto propagate")
            {
                PublicKey = autoPropagateQuestionId,
                MaxValue = 2
            });
            referenceInfoForLinkedQuestions = new ReferenceInfoForLinkedQuestions(questionnaireDocument.PublicKey, 1,
                new Dictionary<Guid, ReferenceInfoByQuestion>
                {
                    { linkedQuestionId, new ReferenceInfoByQuestion(autoPropagateQuestionId, referencedQuestionId) }
                });

            headerCollection =
                new Supervisor.Views.DataExport.ExportedHeaderCollection(referenceInfoForLinkedQuestions, questionnaireDocument);
            
        };

        private Because of = () =>
            headerCollection.Add(new MultyOptionsQuestion() { LinkedToQuestionId = referencedQuestionId,PublicKey = linkedQuestionId, QuestionType = QuestionType.MultyOption});

        private It should_create_header_with_2_cooulumn = () =>
            headerCollection[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        private Cleanup stuff = () =>
        {

        };

        private static WB.Core.BoundedContexts.Supervisor.Views.DataExport.ExportedHeaderCollection headerCollection;
        private static QuestionnaireDocument questionnaireDocument;
        private static ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
    }
}
