using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_count_of_invalid_staticTexts_recursively : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var targetRosterVector = new decimal[0] { };

            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetAllUnderlyingStaticTexts(group.Id) == new List<Guid>
                                                            {
                                                                staticText1Id,
                                                                staticText2Id,
                                                                staticText3Id
                                                            }.ToReadOnlyCollection());
            IPlainQuestionnaireRepository questionnaireRepository = Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new Identity(staticText1Id, targetRosterVector)));
            interview.Apply(Create.Event.StaticTextsDeclaredValid(new Identity(staticText2Id, targetRosterVector)));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new Identity(staticText2Id, targetRosterVector)));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(new Identity(staticText3Id, targetRosterVector)));
        };

        Because of = () =>
            countOfEnabledInvalidStaticTexts = interview.CountInvalidStaticTextsInGroupRecursively(group);

        It should_reduce_roster_vector_to_find_target_static_texts = () =>
            countOfEnabledInvalidStaticTexts.ShouldEqual(3);

        private static StatefulInterview interview;
        private static readonly Guid staticText1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid staticText2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid staticText3Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static int countOfEnabledInvalidStaticTexts;
        private static Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
    }
}