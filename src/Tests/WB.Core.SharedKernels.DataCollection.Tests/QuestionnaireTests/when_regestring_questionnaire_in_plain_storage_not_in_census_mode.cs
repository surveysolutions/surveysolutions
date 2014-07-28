using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_regestring_questionnaire_in_plain_storage_not_in_census_mode : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };
            plainQuestionnaireRepositoryMock=new Mock<IPlainQuestionnaireRepository>();
            plainQuestionnaireRepositoryMock.Setup(x => x.GetQuestionnaireDocument(questionnaireId, 1)).Returns(questionnaireDocument);

            Mock.Get(ServiceLocator.Current)
            .Setup(locator => locator.GetInstance<IPlainQuestionnaireRepository>())
            .Returns(plainQuestionnaireRepositoryMock.Object);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            questionnaire = new Questionnaire(questionnaireId,1, false);

        It should_raise_1_PlainQuestionnaireRegistered_event = () =>
          eventContext.GetEvents<PlainQuestionnaireRegistered>().Count(@event => @event.Version == 1 && !@event.AllowCensusMode).ShouldEqual(1);

        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static EventContext eventContext;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
    }
}
