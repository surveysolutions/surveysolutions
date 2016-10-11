using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [Subject(typeof(Questionnaire))]
    internal class QuestionnaireTestsContext
    {
        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static T GetLastEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Last(e => e.Payload is T).Payload;
        }

        public static IEnumerable<T> GetSpecificEvents<T>(EventContext eventContext) where T : class
        {
            return eventContext.Events.Where(evnt => evnt.Payload is T).Select(evnt => (T)evnt.Payload);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(IEnumerable<IComposite> children = null, Guid? createdBy = null)
        {
            var questionnaire = new QuestionnaireDocument();

            if (children != null)
            {
                questionnaire.Children.AddRange(children);
            }

            questionnaire.CreatedBy = createdBy;

            return questionnaire;
        }


        public static Questionnaire CreateQuestionnaire(Guid responsibleId)
        {
            var questionnaire = Create.Questionnaire();

            questionnaire.CreateQuestionnaire(publicKey: Guid.NewGuid(), title: "title", createdBy: responsibleId, isPublic: false);
            
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestion(Guid? questionId = null, Guid? responsibleId = null, Guid? questionnaireId = null)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: questionId ?? Guid.NewGuid(), responsibleId: responsibleId ?? Guid.NewGuid(), questionnaireId : questionnaireId);
        }

        public static Questionnaire CreateQuestionnaire(Guid responsibleId, Guid? questionnaireId = null, string text = "text of questionnaire",
            IExpressionProcessor expressionProcessor = null)
        {
            var questionnaire = Create.Questionnaire(expressionProcessor: expressionProcessor);

            questionnaire.CreateQuestionnaire(publicKey: questionnaireId ?? Guid.NewGuid(), title: text, createdBy: responsibleId, isPublic: false);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: questionKey,
                responsibleId: responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneGroup(Guid responsibleId, Guid? questionnaireId = null, Guid? groupId = null, bool isRoster = false,
            IExpressionProcessor expressionProcessor = null)
        {
            Questionnaire questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId ?? Guid.NewGuid(), text: "Title", responsibleId: responsibleId, expressionProcessor: expressionProcessor);

            groupId = groupId ?? Guid.NewGuid();
            questionnaire.AddGroup(groupId.Value,
                responsibleId: responsibleId,
                title: "New group"
            );

            if (isRoster)
            {
                questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, groupId.Value));
            }

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithRosterGroupAndQuestion(Guid rosterGroupId,  Guid rosterSizeQuestionId, Guid responsibleId, Guid? mainChapterId = null)
        {
            Questionnaire questionnaire = CreateQuestionnaire(questionnaireId: Guid.NewGuid(), text: "Title", responsibleId: responsibleId);

            Guid chapterId = mainChapterId ?? Guid.NewGuid();
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId, title:"New section" );
            AddQuestion(questionnaire, rosterSizeQuestionId, chapterId, responsibleId, QuestionType.MultyOption, "rosterSizeQuestion",
                new[] { new Option(Guid.NewGuid(), "1", "opt1"), new Option(Guid.NewGuid(), "2", "opt2") });
            AddGroup(questionnaire, rosterGroupId, chapterId, "", responsibleId, rosterSizeQuestionId, isRoster: true);
            return questionnaire;
        }


        public static Questionnaire CreateQuestionnaireWithOneRosterGroup(Guid groupId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, isRoster: true,
                responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneNotRosterGroup(Guid groupId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, isRoster:false,
                responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithRosterGroupAndQuestionAndQuestionInRoster(Guid questionId,
            Guid responsibleId)
        {
            Guid rosterGroupId = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithRosterGroupAndQuestion(rosterGroupId, responsibleId: responsibleId,
                rosterSizeQuestionId: Guid.NewGuid());
            AddQuestion(questionnaire, questionId, rosterGroupId, responsibleId, QuestionType.Text, "question");
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneGroupAndQuestionInIt(Guid questionId, Guid responsibleId, Guid? questionnaireId = null, Guid? groupId = null, QuestionType questionType = QuestionType.Text, 
                                                                                   bool? isInteger = null, string alias = "text")
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, questionnaireId, groupId.Value, isRoster: false);
            
            AddQuestion(questionnaire, questionId, groupId.Value, responsibleId, questionType, alias,
                                               (questionType == QuestionType.MultyOption || questionType == QuestionType.SingleOption)?
                                                  new Option[2]{new Option(Guid.NewGuid(),"1", "1"),new Option(Guid.NewGuid(), "2", "2") }:
                                                  new Option[0]);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoQuestions(Guid secondQuestionId, Guid responsibleId)
        {
            var groupId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(Guid.NewGuid(), groupId, responsibleId, "Title", "text", null, false, QuestionScope.Interviewer, "", "", "", "", mask:null);
            questionnaire.AddTextQuestion(secondQuestionId, groupId, responsibleId, "Title","name", null , false, QuestionScope.Interviewer, "", "", "", "", mask: null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoGroups(Guid firstGroup, Guid secondGroup, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: firstGroup, responsibleId: responsibleId, isRoster: false);

            questionnaire.AddGroupAndMoveIfNeeded(secondGroup,
                responsibleId: responsibleId, title: "Second group", variableName: null, rosterSizeQuestionId: null, description: null, 
                condition: null, hideIfDisabled: false, parentGroupId: null, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, 
                rosterFixedTitles: null, rosterTitleQuestionId: null);

            return questionnaire;
        }

        public static void AddGroup(Questionnaire questionnaire, Guid groupId, Guid? parentGroupId, string condition, Guid responsibleId,
            Guid? rosterSizeQuestionId = null, bool isRoster = false, RosterSizeSourceType rosterSizeSource = RosterSizeSourceType.Question,
           FixedRosterTitleItem[] rosterFixedTitles = null, Guid? rosterTitleQuestionId = null, bool hideIfDisabled = false)
        {
            questionnaire.AddGroupAndMoveIfNeeded(groupId,
                responsibleId: responsibleId, title: "New group", variableName: null, rosterSizeQuestionId: rosterSizeQuestionId, description: null,
                condition: condition, hideIfDisabled: hideIfDisabled, parentGroupId: parentGroupId, isRoster: isRoster, rosterSizeSource: rosterSizeSource,
                rosterFixedTitles: rosterFixedTitles,
                rosterTitleQuestionId: rosterTitleQuestionId);
        }

        public static Questionnaire CreateQuestionnaireWithRosterGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneRosterGroup(groupId: autoGroupPublicKey,
                responsibleId: responsibleId);

            questionnaire.AddGroupAndMoveIfNeeded(secondGroup, responsibleId, "Second group",null, null, null, null, false, null, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithRosterGroupAndQuestionAndAndRegularGroupAndQuestionsInThem(
          Guid rosterId, Guid nonRosterGroupId, Guid autoQuestionId, Guid questionId, Guid responsibleId,
          QuestionType firstQuestionType, QuestionType secondQuestionType = QuestionType.Text)
        {
            var rosterSizeQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithRosterGroupAndQuestion(rosterId, rosterSizeQuestionId, responsibleId, chapterId);

            AddGroup(questionnaire, nonRosterGroupId, chapterId, "", responsibleId, null);

            AddQuestion(questionnaire,
                    questionId: autoQuestionId,
                    groupId: rosterId,
                    questionType: secondQuestionType,
                    variableName: "auto",
                    responsible: responsibleId);

            AddQuestion(questionnaire, questionId, nonRosterGroupId, responsibleId, firstQuestionType, "manual", new[] { new Option(Guid.NewGuid(), "1", "first title"), new Option(Guid.NewGuid(), "2", "second title") });
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
            Guid rosterId, Guid secondGroup, Guid autoQuestionId, Guid questionId, Guid responsibleId,
            QuestionType questionType, QuestionType autoQuestionType = QuestionType.Text)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithRosterGroupAndRegularGroup(rosterId, secondGroup, responsibleId);

            AddQuestion(questionnaire,
                    questionId: autoQuestionId,
                    groupId : rosterId,
                    questionType : autoQuestionType,
                    variableName : "auto",
                    condition : string.Empty,
                    validation : string.Empty,
                    responsible : responsibleId
                );
            AddQuestion(questionnaire, questionId, secondGroup, responsibleId, questionType, "manual",
                new[] { new Option(Guid.NewGuid(), "1", "title") });
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId, Guid responsibleId)
        {
            var secondGroup = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(firstGroup: firstGroup,
                secondGroup: secondGroup, responsibleId: responsibleId);
            AddQuestion(questionnaire, autoQuestoinId, secondGroup, responsibleId, QuestionType.AutoPropagate,"auto");
            return questionnaire;
        }

        public static void AddQuestion(Questionnaire questionnaire, Guid questionId, Guid groupId, Guid responsible, QuestionType questionType, 
            string variableName, Option[] options = null, string condition = "", string validation = "")
        {
            if (IsNumericQuestion(questionType))
            {
                questionnaire.AddNumericQuestion(questionId, groupId, responsible,  "Title", variableName);
                return;
            }

            questionnaire.AddTextQuestion(
                questionId,
                groupId,
                responsible,
                "Title",
                variableName, null,
                false,
                QuestionScope.Interviewer,
                condition,
                validation,
                "",
                "", null);
        }

        public static bool AreOptionsRequiredByQuestionType(QuestionType type)
        {
            return type == QuestionType.MultyOption || type == QuestionType.SingleOption;
        }

        public static bool IsNumericQuestion(QuestionType type)
        {
            return type == QuestionType.Numeric || type == QuestionType.AutoPropagate;
        }

        public static Option[] CreateTwoOptions()
        {
            return new[]
                {
                    new Option(Guid.Parse("00000000-1111-0000-1111-000000000000"), "-1", "No"),
                    new Option(Guid.Parse("00000000-2222-0000-2222-000000000000"), "42", "Yes"),
                };
        }

        public static Questionnaire CreateQuestionnaireWithRegularGroupAndRegularGroupInIt(Guid groupId, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneNotRosterGroup(groupId: groupId,
                responsibleId: responsibleId);

            questionnaire.AddGroupAndMoveIfNeeded(Guid.NewGuid(),
                responsibleId: responsibleId, title: "New group", variableName: null, rosterSizeQuestionId: null, description: null, 
                condition: null, hideIfDisabled: false,
                parentGroupId: groupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithChapterWithRegularAndRosterGroup(Guid rosterGroupId, Guid regularGroupId, Guid responsibleId)
        {
            var chapterId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: chapterId, responsibleId: responsibleId);

            Guid rosterSizeQuestionId = Guid.NewGuid();
            questionnaire.AddGroupAndMoveIfNeeded(regularGroupId, responsibleId: responsibleId, title: "regularGroup", variableName: null, rosterSizeQuestionId: null,
                description: null, condition: null, hideIfDisabled: false, parentGroupId: chapterId, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);
            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId, regularGroupId,responsibleId, 
                new[] { new Option(Guid.NewGuid(), "1", "opt1"), new Option(Guid.NewGuid(), "2", "opt2") }, "rosterSizeQuestion",
                "rosterSizeQuestion", null, QuestionScope.Interviewer, "", "", "", "", null, false, null, false);
            questionnaire.AddGroupAndMoveIfNeeded(rosterGroupId, responsibleId: responsibleId, title: "autoPropagateGroup", variableName: null,
                rosterSizeQuestionId: rosterSizeQuestionId, description: null, condition: null, hideIfDisabled: false, parentGroupId: chapterId, isRoster: true,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithNesingAndLastGroup(int depth, Guid dippestGroupId, Guid responsibleId)
        {
            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            if (depth > 0)
            {
                Guid parentId = depth == 1 ? dippestGroupId : Guid.NewGuid();
                questionnaire.AddGroup(parentId, responsibleId: responsibleId, title: "New section");

                for (int i = 0; i < depth - 1; i++)
                {
                    var groupId = (i == depth - 2) ? dippestGroupId : Guid.NewGuid();

                    AddGroup(questionnaire, groupId, parentId, "", responsibleId, null);
                    
                    parentId = groupId;
                }
            }

            return questionnaire;
        }
    }
}