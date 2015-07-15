using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_fixed_roster_and_roster_size_numeric_question_inside_it : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group("fixed roster")
                {
                    PublicKey = fixedRosteId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] {"fixed1", "fixed2"},
                    Children = new List<IComposite>
                    {
                        new NumericQuestion("roster size question")
                        {
                            PublicKey = rosterSizeQuestionId,
                            QuestionType = QuestionType.Numeric,
                            IsInteger = true
                        },
                        new Group("nested roster")
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = rosterSizeQuestionId
                        }
                    }
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
        };

        Because of = () => 
            exception = Catch.Exception(() => new Interview(new Guid(), new Guid(), questionnaireId, new Dictionary<Guid, object>(),
                DateTime.Now));

        It should_exception_be_null = () =>
            exception.ShouldBeNull();

        private static Guid fixedRosteId = Guid.Parse("11111111111111111111111111111111");
        private static Guid nestedRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid questionnaireId = Guid.Parse("44444444444444444444444444444444");
        private static Exception exception;
    }
}
