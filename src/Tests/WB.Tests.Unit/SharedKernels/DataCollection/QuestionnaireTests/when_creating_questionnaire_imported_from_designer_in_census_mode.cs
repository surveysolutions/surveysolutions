using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_creating_questionnaire_imported_from_designer_in_census_mode : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            questionnaire=new Questionnaire(Guid.NewGuid(),questionnaireDocument,true);

        It should_raise_1_TemplateImported_event = () =>
          eventContext.GetEvents<TemplateImported>().Count(@event => @event.Source == questionnaireDocument && @event.AllowCensusMode).ShouldEqual(1);

        private static Questionnaire questionnaire;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static EventContext eventContext;
    }
}
