﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_questions_affected_by_substitutions
    {
        Establish context = () =>
        {
            var rosterSizeId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitleid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Other.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Other.NumericIntegerQuestion(rosterSizeId),
                    Create.Other.Roster(rosterSizeQuestionId: rosterSizeId,
                        rosterTitleQuestionId: rosterTitleid,
                        children: new List<IComposite>
                        {
                            Create.Other.TextQuestion(questionId: rosterTitleid),
                            Create.Other.TextQuestion(questionId: substitutionTargetQuestionId, text: "with %rostertitle%")
                        })
                });

            plainQuestionnaire = Create.Other.PlainQuestionnaire(document: questionnaire);
        };  

        Because of = () => affectedQuestions = plainQuestionnaire.GetSubstitutedQuestions(rosterTitleid);

        It should_find_roster_title_substitutions = () => affectedQuestions.ShouldContain(substitutionTargetQuestionId);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid substitutionTargetQuestionId;
        private static Guid rosterTitleid;
        private static IEnumerable<Guid> affectedQuestions;
    }
}

