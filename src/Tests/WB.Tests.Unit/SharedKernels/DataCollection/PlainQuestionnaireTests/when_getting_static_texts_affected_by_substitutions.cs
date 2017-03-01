using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_static_texts_affected_by_substitutions : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitleid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetStaticTextId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Entity.NumericIntegerQuestion(rosterSizeId),
                    Create.Entity.Roster(rosterSizeQuestionId: rosterSizeId,
                        rosterTitleQuestionId: rosterTitleid,
                        children: new List<IComposite>
                        {
                            Create.Entity.TextQuestion(questionId: rosterTitleid),
                            Create.Entity.StaticText(publicKey: substitutionTargetStaticTextId, text: "with %rostertitle%")
                        })
                });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire);
        };  

        Because of = () => affectedStaticTexts = plainQuestionnaire.GetSubstitutedStaticTexts(rosterTitleid);

        It should_find_roster_title_substitutions = () => affectedStaticTexts.ShouldContain(substitutionTargetStaticTextId);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid substitutionTargetStaticTextId;
        private static Guid rosterTitleid;
        private static IEnumerable<Guid> affectedStaticTexts;
    }
}