using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    [Ignore("KP-7362")]
    internal class when_answer_is_roster_title_and_removed_because_of_filtering : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                var triggerQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                Guid rosterTitleQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                Guid questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var rosterTitleQuestionIdentity = Create.Identity(rosterTitleQuestionId,
                    Create.RosterVector(0));
                var rosterIdentity = Create.Identity(rosterId, Create.RosterVector(0));

                var singleOptionQuestion = Create.SingleQuestion(
                    id: rosterTitleQuestionId,
                    optionsFilter: "trigger == 1",
                    options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "Option 1"),
                        Create.Option(value: "2", text: "Option 2"),
                    });

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(id: triggerQuestionId,
                        variable: "trigger"),
                    Create.Roster(id: rosterId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: triggerQuestionId,
                        rosterTitleQuestionId: rosterTitleQuestionId,
                        children: new IComposite[]
                        {
                            singleOptionQuestion
                        })
                    );

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaire);

                var interview = SetupInterview(questionnaire, precompiledState: interviewState);

                interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Today, 1);
                interview.AnswerSingleOptionQuestion(userId, rosterTitleQuestionIdentity.Id, rosterTitleQuestionIdentity.RosterVector, DateTime.Today, 1);

                var result = new InvokeResults();
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 2);

                    result.RosterTitleHasChanged = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => 
                        x.ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterIdentity.Id));
                    result.RosterTitleBecomesEmpty = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => x.ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterIdentity.Id && c.Title == string.Empty));
                }

                return result;
            });

        It should_raise_roster_titles_changed_for_roster = () => results.RosterTitleHasChanged.ShouldBeTrue();
        
        It should_calculate_new_roster_title_as_empty_string = () => results.RosterTitleBecomesEmpty.ShouldBeTrue();

        static readonly Guid userId = Guid.NewGuid();

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool RosterTitleHasChanged { get; set; }
            public bool RosterTitleBecomesEmpty { get; set; }
        }
    }
}