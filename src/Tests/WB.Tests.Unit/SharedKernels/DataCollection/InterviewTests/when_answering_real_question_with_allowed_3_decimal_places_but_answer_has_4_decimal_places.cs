using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_real_question_with_allowed_3_decimal_places_but_answer_has_4_decimal_places : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            realQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.HasQuestion(realQuestionId) == true
                                                        && _.GetMaxRosterRowCount() == Constants.MaxRosterRowCount
                                                        && _.IsQuestionInteger(realQuestionId)==false
                                                        && _.GetCountOfDecimalPlacesAllowedByQuestion(realQuestionId)==3
                                                        && _.GetQuestionType(realQuestionId) == QuestionType.Numeric
                                                        );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);


            interview = CreateInterview(questionnaireId: questionnaireId);

        };

        Cleanup stuff = () =>
        {
        };

        Because of = () => expectedException = Catch.Exception(() =>
            interview.AnswerNumericRealQuestion(userId, realQuestionId, new decimal[] { }, DateTime.Now, (decimal)0.1234));

        It should_raise_AnswerDeclaredValid_event_with_QuestionId_equal_to_mandatoryQuestionId = () =>
            expectedException.ShouldBeOfType(typeof (InterviewException));

        private static Exception expectedException;
        private static Guid realQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
