using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_checking_should_static_text_be_hidden_if_it_is_disabled : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.StaticText(staticTextId: staticTextId),
            });

            plainQuestionnaire = Create.PlainQuestionnaire(document: questionnaireDocument);
        };

        Because of = () =>
            result = plainQuestionnaire.ShouldBeHiddenIfDisabled(staticTextId);

        It should_return_false = () =>
            result.ShouldEqual(false);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid staticTextId = Guid.Parse("11111111111111111111111111111111");
        private static bool result;
    }

    internal class when_getting_questions_affected_by_substitutions
    {
        Establish context = () =>
        {
            var rosterSizeId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitleid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.NumericIntegerQuestion(rosterSizeId),
                    Create.Roster(rosterSizeQuestionId: rosterSizeId,
                        rosterTitleQuestionId: rosterTitleid,
                        children: new List<IComposite>
                        {
                            Create.TextQuestion(questionId: rosterTitleid),
                            Create.TextQuestion(questionId: substitutionTargetQuestionId, text: "with %rostertitle%")
                        })
                });

            plainQuestionnaire = Create.PlainQuestionnaire(document: questionnaire);
        };  

        Because of = () => affectedQuestions = plainQuestionnaire.GetSubstitutedQuestions(rosterTitleid);

        It should_find_roster_title_substitutions = () => affectedQuestions.ShouldContain(substitutionTargetQuestionId);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid substitutionTargetQuestionId;
        private static Guid rosterTitleid;
        private static IEnumerable<Guid> affectedQuestions;
    }
}

