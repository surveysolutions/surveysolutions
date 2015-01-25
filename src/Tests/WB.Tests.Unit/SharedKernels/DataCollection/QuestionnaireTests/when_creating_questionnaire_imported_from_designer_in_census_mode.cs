extern alias datacollection;
using System;
using System.Linq;
using datacollection::Main.Core.Events.Questionnaire;
using Machine.Specifications;
using Main.Core.Documents;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
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
            questionnaire = new Questionnaire(Guid.NewGuid(), questionnaireDocument, true, "assembly body");

        It should_raise_1_TemplateImported_event = () =>
          eventContext.GetEvents<TemplateImported>().Count(@event => @event.Source == questionnaireDocument && @event.AllowCensusMode && @event.Version == 1).ShouldEqual(1);

        It should_raise_1_QuestionnaireAssemblyImported_event = () =>
          eventContext.GetEvents<QuestionnaireAssemblyImported>().Count(@event => @event.Version == 1).ShouldEqual(1);

        private static Questionnaire questionnaire;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static EventContext eventContext;
    }
}
